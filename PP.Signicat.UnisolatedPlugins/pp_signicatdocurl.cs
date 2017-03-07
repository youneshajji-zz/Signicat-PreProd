using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow.Activities;

namespace PP.Signicat.UnisolatedPlugins
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
                var user = context.InitiatingUserId;
                var lcid = RetrieveUserUiLanguageCode(service, user);

                if (statusCode.Value == 778380000)
                {
                    try
                    {
                        var name = "name";
                        var signatoryRef = entity.Attributes["pp_customerid"] as EntityReference;
                        if (signatoryRef.LogicalName == "contact")
                            name = "fullname";

                        var signatory = service.Retrieve(signatoryRef.LogicalName, signatoryRef.Id, new ColumnSet(name));
                        var signatoryName = signatory.GetAttributeValue<string>(name);
                        var docsigningRef = entity.GetAttributeValue< EntityReference>("pp_documentsigningid");
                        var docsigning = service.Retrieve(docsigningRef.LogicalName, docsigningRef.Id,
                            new ColumnSet("pp_notify", "ownerid"));
                        var notifyme = (bool)docsigning.Attributes["pp_notify"];

                        if (notifyme)
                            SendNotifyEmail(docsigning, signatoryName, lcid, service);
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

        private void SendNotifyEmail(Entity documentsigning, string signatoryName, int lcid, IOrganizationService service)
        {
            try
            {
                var emailLogoImg = "https://ppsignicatresources.blob.core.windows.net/signicatlinkbutton/SignicatMailLogo.png";

                var recieverRef = (EntityReference)documentsigning["ownerid"];
                var crmConfig = GetCRMConfig(service);

                Entity fromParty = new Entity("activityparty");
                fromParty["partyid"] = new EntityReference("systemuser", crmConfig.SigningUser.Id);

                Entity toParty = new Entity("activityparty");
                toParty["partyid"] = new EntityReference(recieverRef.LogicalName, recieverRef.Id);

                var listfrom = new List<Entity>() { fromParty };
                var listto = new List<Entity>() { toParty };
                var subject = "";
                var description = "";

                if (lcid == 1044)
                    subject = "Et dokument er signert.";
                else if (lcid == 1033)
                    subject = "A document is signed.";
                if (lcid == 1044)
                    description = "Et dokument er signert av " + signatoryName + 
                        " og er tilgjengelig på Dynamics 365.";
                else if (lcid == 1033)
                    description = "A document is signed by " + signatoryName + 
                        " and are availabale in Dynamics 365.";

                // Create an e-mail message.
                Entity email = new Entity("email");
                email["to"] = new EntityCollection(listto);
                email["from"] = new EntityCollection(listfrom);
                email["subject"] = subject;
                email["directioncode"] = true;
                email["regardingobjectid"] = new EntityReference(documentsigning.LogicalName, documentsigning.Id);

                if (!string.IsNullOrWhiteSpace(description))
                {
                    var newText = "";
                    //newText = "<img src='" + emailLogoImg + "'/><br/><br/>";
                    newText += description;
                    newText += "<br/><br/><br/> " + crmConfig.SigningUsername;

                    email["description"] = newText;
                }
                var _emailId = service.Create(email);

                // Use the SendEmail message to send an e-mail message.
                SendEmailRequest sendEmailreq = new SendEmailRequest
                {
                    EmailId = _emailId,
                    TrackingToken = "",
                    IssueSend = true
                };

                SendEmailResponse sendEmailresp = (SendEmailResponse)service.Execute(sendEmailreq);
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in sending notify email.", ex);
            }
        }

        private CRMConfig GetCRMConfig(IOrganizationService service)
        {
            try
            {
                var query = new QueryExpression("pp_signicatconfig");
                query.ColumnSet = new ColumnSet("pp_wordsignuser");
                var results = service.RetrieveMultiple(query);

                if (results.Entities.Count == 1)
                {
                    var entity = results.Entities[0];

                    var config = new CRMConfig();
                    config.SigningUser = entity.GetAttributeValue<EntityReference>("pp_wordsignuser");

                    var senderRef = service.Retrieve("systemuser", config.SigningUser.Id, new ColumnSet("fullname"));
                    config.SigningUsername = senderRef.GetAttributeValue<string>("fullname");

                    return config;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private int RetrieveUserUiLanguageCode(IOrganizationService service, Guid userId)
        {
            var userSettingsQuery = new QueryExpression("usersettings");
            userSettingsQuery.ColumnSet.AddColumns("uilanguageid", "systemuserid");
            userSettingsQuery.Criteria.AddCondition("systemuserid", ConditionOperator.Equal, userId);
            var userSettings = service.RetrieveMultiple(userSettingsQuery);
            if (userSettings.Entities.Count > 0)
            {
                return (int)userSettings.Entities[0]["uilanguageid"];
            }
            return 0;
        }
    }
}
