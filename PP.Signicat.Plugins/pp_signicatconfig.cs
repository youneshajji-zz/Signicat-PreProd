using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace PP.Signicat.Plugins
{
    public class pp_signicatconfig : IPlugin
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
                Entity entity = (Entity)context.InputParameters["Target"];

                // Verify that the target entity represents an account.
                // If not, this plug-in was not registered correctly.
                if (entity.LogicalName != "pp_signicatconfig")
                    return;

                // Obtain the organization service reference.
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    var exists = GetExistingsSignicatConfigs(service);

                    if (exists)
                    {
                        throw new InvalidPluginExecutionException("There is allready a Signicat condig entry, delete or deactivate the existing entry before creating a new one!");
                    }
                }
                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in the plug-in: pp_signicatconfig. " + ex.Message, ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("Plugin error: {0}", ex.ToString());
                    throw;
                }
            }
        }

        private bool GetExistingsSignicatConfigs(IOrganizationService service)
        {
            try
            {
                QueryExpression query = new QueryExpression("pp_signicatconfig");
                query.ColumnSet = new ColumnSet(false);
                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0); //active
                var result = service.RetrieveMultiple(query);

                if (result.Entities.Count > 0)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }

        }
    }
}
