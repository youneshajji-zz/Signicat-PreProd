using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PP.Signicat.Plugins
{
    public class pp_signicatdocurl : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService =
               (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // The InputParameters collection contains all the data passed in the message request.
            if (context.InputParameters.Contains("Target") &&
        context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.
                //Entity entity = (Entity)context.InputParameters["Target"];
                Entity entity = context.PostEntityImages["PostImage"];

                // Verify that the target entity represents an account.
                // If not, this plug-in was not registered correctly.
                if (entity.LogicalName != "pp_signicatdocurl")
                    return;

                // Obtain the organization service reference.
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                OptionSetValue statusCode = entity.Attributes["statuscode"] as OptionSetValue;
                if (statusCode.Value == 778380000)
                {
                    try
                    {
                        CheckOverAllStatus(entity, service);
                    }
                    catch (FaultException<OrganizationServiceFault> ex)
                    {
                        throw new InvalidPluginExecutionException("An error occurred in the plug-in: pp_signicatdocurl. " + ex.Message, ex);
                    }

                    catch (Exception ex)
                    {
                        tracingService.Trace("Plugin error: {0}", ex.ToString());
                        throw;
                    }
                }
            }
        }

        private void CheckOverAllStatus(Entity entity, IOrganizationService service)
        {
            try
            {
                var results = GetSignicatDocUrls(entity, service); //if all tasks are completed, the return is null
                if (results.Entities.Count == 0)
                {
                    var docsignid = (EntityReference)entity.Attributes["pp_documentsigningid"];
                    
                    var newentity = new Entity("pp_documentsigning");
                    newentity.Id = docsignid.Id;
                    newentity["statuscode"] = new OptionSetValue(778380001); //Signed
                    service.Update(newentity);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }

        private EntityCollection GetSignicatDocUrls(Entity entity, IOrganizationService service)
        {
            try
            {
                var docsignid = (EntityReference)entity.Attributes["pp_documentsigningid"];

                QueryExpression query = new QueryExpression("pp_signicatdocurl");
                query.ColumnSet = new ColumnSet(true);
                query.Criteria.AddCondition("pp_documentsigningid", ConditionOperator.Equal, docsignid.Id);
                query.Criteria.AddCondition("statuscode", ConditionOperator.NotEqual, 778380000); //Signed
                var result = service.RetrieveMultiple(query);
                return result;
            }
            catch (Exception ex)
            {                
                throw new InvalidPluginExecutionException(ex.Message);
            }

        }
    }

}
