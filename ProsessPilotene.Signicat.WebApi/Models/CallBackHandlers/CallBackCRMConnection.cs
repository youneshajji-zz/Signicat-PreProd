using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel.Description;
using System.Web;
using System.Web.Services.Protocols;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;
using PP.Signicat.WebApi.Models.CRMCredentials;

namespace PP.Signicat.WebApi.Models.CallBackHandlers
{
    public class CallBackCRMConnection
    {
        public IOrganizationService GetOrgService(string orgname)
        {
            try
            {
                var customerCredentials = new CustomerCredentialsHandler().GetCredentials(orgname);
                var discoveryUri = customerCredentials.discoveryurl; //ConfigurationManager.AppSettings["DiscoveryUri"];

                IServiceManagement<IDiscoveryService> serviceManagement =
                     ServiceConfigurationFactory.CreateManagement<IDiscoveryService>(
                     new Uri(discoveryUri));
                AuthenticationProviderType endpointType = serviceManagement.AuthenticationType;

                // Set the credentials.
                AuthenticationCredentials authCredentials = GetCredentials(serviceManagement, endpointType, customerCredentials);

                Uri serviceUri = new Uri(customerCredentials.orgurl); //ConfigurationSettings.AppSettings["OrganizationUri"]);

                OrganizationServiceProxy proxy = new OrganizationServiceProxy(serviceUri, null, authCredentials.ClientCredentials, null);
                proxy.EnableProxyTypes();
                IOrganizationService service = (IOrganizationService)proxy;

                return service;
            }
            catch (SoapException ex)
            {
                throw new Exception(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #region Helpers
        /// <summary>
        /// Obtain the AuthenticationCredentials based on AuthenticationProviderType.
        /// </summary>
        /// <param name="service">A service management object.</param>
        /// <param name="endpointType">An AuthenticationProviderType of the CRM environment.</param>
        /// <returns>Get filled credentials.</returns>
        private AuthenticationCredentials GetCredentials<TService>(IServiceManagement<TService> service, AuthenticationProviderType endpointType, CustomerCredentialsModel customerCredentials)
        {
            var _userName = customerCredentials.username; //ConfigurationManager.AppSettings["UserName"];
            var _password = customerCredentials.password; //ConfigurationManager.AppSettings["Password"];
            var _domain = customerCredentials.domain; //ConfigurationManager.AppSettings["Domain"];

            AuthenticationCredentials authCredentials = new AuthenticationCredentials();

            switch (endpointType)
            {
                case AuthenticationProviderType.ActiveDirectory:
                    authCredentials.ClientCredentials.Windows.ClientCredential =
                        new System.Net.NetworkCredential(_userName,
                            _password,
                            _domain);
                    break;
                case AuthenticationProviderType.LiveId:
                    authCredentials.ClientCredentials.UserName.UserName = _userName;
                    authCredentials.ClientCredentials.UserName.Password = _password;
                    authCredentials.SupportingCredentials = new AuthenticationCredentials();
                    authCredentials.SupportingCredentials.ClientCredentials =
                        Microsoft.Crm.Services.Utility.DeviceIdManager.LoadOrRegisterDevice();
                    break;
                default: // For Federated and OnlineFederated environments.                    
                    authCredentials.ClientCredentials.UserName.UserName = _userName;
                    authCredentials.ClientCredentials.UserName.Password = _password;
                    // For OnlineFederated single-sign on, you could just use current UserPrincipalName instead of passing user name and password.
                    // authCredentials.UserPrincipalName = UserPrincipal.Current.UserPrincipalName;  // Windows Kerberos

                    // The service is configured for User Id authentication, but the user might provide Microsoft
                    // account credentials. If so, the supporting credentials must contain the device credentials.
                    if (endpointType == AuthenticationProviderType.OnlineFederation)
                    {
                        IdentityProvider provider = service.GetIdentityProvider(authCredentials.ClientCredentials.UserName.UserName);
                        if (provider != null && provider.IdentityProviderType == IdentityProviderType.LiveId)
                        {
                            authCredentials.SupportingCredentials = new AuthenticationCredentials();
                            authCredentials.SupportingCredentials.ClientCredentials =
                                Microsoft.Crm.Services.Utility.DeviceIdManager.LoadOrRegisterDevice();
                        }
                    }

                    break;
            }

            return authCredentials;
        }

        /// <summary>
        /// Discovers the organizations that the calling user belongs to.
        /// </summary>
        /// <param name="service">A Discovery service proxy instance.</param>
        /// <returns>Array containing detailed information on each organization that 
        /// the user belongs to.</returns>
        public OrganizationDetailCollection DiscoverOrganizations(
            IDiscoveryService service)
        {
            if (service == null) throw new ArgumentNullException("service");
            RetrieveOrganizationsRequest orgRequest = new RetrieveOrganizationsRequest();
            RetrieveOrganizationsResponse orgResponse =
                (RetrieveOrganizationsResponse)service.Execute(orgRequest);

            return orgResponse.Details;
        }

        /// <summary>
        /// Finds a specific organization detail in the array of organization details
        /// returned from the Discovery service.
        /// </summary>
        /// <param name="orgUniqueName">The unique name of the organization to find.</param>
        /// <param name="orgDetails">Array of organization detail object returned from the discovery service.</param>
        /// <returns>Organization details or null if the organization was not found.</returns>
        /// <seealso cref="DiscoveryOrganizations"/>
        public OrganizationDetail FindOrganization(string orgUniqueName,
            OrganizationDetail[] orgDetails)
        {
            if (String.IsNullOrWhiteSpace(orgUniqueName))
                throw new ArgumentNullException("orgUniqueName");
            if (orgDetails == null)
                throw new ArgumentNullException("orgDetails");
            OrganizationDetail orgDetail = null;

            foreach (OrganizationDetail detail in orgDetails)
            {
                if (String.Compare(detail.UniqueName, orgUniqueName,
                    StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    orgDetail = detail;
                    break;
                }
            }
            return orgDetail;
        }

        /// <summary>
        /// Generic method to obtain discovery/organization service proxy instance.
        /// </summary>
        /// <typeparam name="TService">
        /// Set IDiscoveryService or IOrganizationService type to request respective service proxy instance.
        /// </typeparam>
        /// <typeparam name="TProxy">
        /// Set the return type to either DiscoveryServiceProxy or OrganizationServiceProxy type based on TService type.
        /// </typeparam>
        /// <param name="serviceManagement">An instance of IServiceManagement</param>
        /// <param name="authCredentials">The user's Microsoft Dynamics CRM logon credentials.</param>
        /// <returns></returns>
        private TProxy GetProxy<TService, TProxy>(
            IServiceManagement<TService> serviceManagement,
            AuthenticationCredentials authCredentials)
            where TService : class
            where TProxy : ServiceProxy<TService>
        {
            Type classType = typeof(TProxy);

            if (serviceManagement.AuthenticationType !=
                AuthenticationProviderType.ActiveDirectory)
            {
                AuthenticationCredentials tokenCredentials =
                    serviceManagement.Authenticate(authCredentials);
                // Obtain discovery/organization service proxy for Federated, LiveId and OnlineFederated environments. 
                // Instantiate a new class of type using the 2 parameter constructor of type IServiceManagement and SecurityTokenResponse.
                return (TProxy)classType
                    .GetConstructor(new Type[] { typeof(IServiceManagement<TService>), typeof(SecurityTokenResponse) })
                    .Invoke(new object[] { serviceManagement, tokenCredentials.SecurityTokenResponse });
            }

            // Obtain discovery/organization service proxy for ActiveDirectory environment.
            // Instantiate a new class of type using the 2 parameter constructor of type IServiceManagement and ClientCredentials.
            return (TProxy)classType
                .GetConstructor(new Type[] { typeof(IServiceManagement<TService>), typeof(ClientCredentials) })
                .Invoke(new object[] { serviceManagement, authCredentials.ClientCredentials });
        }

        #endregion
    }
}