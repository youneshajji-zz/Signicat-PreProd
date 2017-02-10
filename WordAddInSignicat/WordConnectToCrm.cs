using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace WordAddInSignicat
{
    public static class WordConnectToCrm
    {
        //_userId = new Guid("870BEBB1-8C42-E611-80EA-5065F38BF4F1");
        public static IOrganizationService ConnectToMSCRM()
        {
            try
            {
                var discoveryUri = ConfigurationManager.AppSettings["DiscoveryUri"];

                IServiceManagement<IDiscoveryService> serviceManagement =
                     ServiceConfigurationFactory.CreateManagement<IDiscoveryService>(
                     new Uri(discoveryUri));
                AuthenticationProviderType endpointType = serviceManagement.AuthenticationType;

                // Set the credentials.
                AuthenticationCredentials authCredentials = GetCredentials(serviceManagement, endpointType);

                Uri serviceUri = new Uri(ConfigurationSettings.AppSettings["OrganizationUri"]);

                OrganizationServiceProxy proxy = new OrganizationServiceProxy(serviceUri, null, authCredentials.ClientCredentials, null);
                proxy.EnableProxyTypes();
                IOrganizationService service = (IOrganizationService)proxy;

                return service;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while connecting to CRM " + ex.Message, "CRM connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// Obtain the AuthenticationCredentials based on AuthenticationProviderType.
        /// </summary>
        /// <param name="service">A service management object.</param>
        /// <param name="endpointType">An AuthenticationProviderType of the CRM environment.</param>
        /// <returns>Get filled credentials.</returns>
        private static AuthenticationCredentials GetCredentials<TService>(IServiceManagement<TService> service, AuthenticationProviderType endpointType)
        {
            var _userName = ConfigurationManager.AppSettings["UserName"];
            var _password = ConfigurationManager.AppSettings["Password"];
            var _domain = ConfigurationManager.AppSettings["Domain"];

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

    }
}
