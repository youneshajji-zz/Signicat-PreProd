using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PP.Signicat.UnisolatedPlugins
{
    public class pp_documentsigning : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is EntityReference)
            {
                EntityReference entityRef = (EntityReference)context.InputParameters["Target"];

                if (entityRef.LogicalName != "pp_documentsigning")
                    return;

                // Obtain the organization service reference.
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                if (context.MessageName.ToUpper() == "DELETE")
                {
                    var signingtasks = GetRelatedSigningTask(entityRef.Id, service);
                    var signingresults = GetRelatedSigningResults(entityRef.Id, service);

                    try
                    {
                        foreach (var item in signingtasks.Entities)
                        {
                            service.Delete(item.LogicalName, item.Id);
                        }

                        foreach (var item in signingresults.Entities)
                        {
                            service.Delete(item.LogicalName, item.Id);
                        }
                    }

                    catch (FaultException<OrganizationServiceFault> ex)
                    {
                        throw new InvalidPluginExecutionException("An error occurred in the plug-in(delete): pp_documentsigning. " + ex.Message, ex);
                    }

                    catch (Exception ex)
                    {
                        tracingService.Trace("Plugin error (delete): {0}", ex.ToString());
                        throw;
                    }
                }
            }

            // The InputParameters collection contains all the data passed in the message request.
            if (context.InputParameters.Contains("Target") &&
    context.InputParameters["Target"] is Entity)
            {

                // Obtain the target entity from the input parameters.
                //Entity entity = (Entity)context.InputParameters["Target"];
                Entity entity = context.PostEntityImages["PostImage"];

                // Verify that the target entity represents an account.
                // If not, this plug-in was not registered correctly.
                if (entity.LogicalName != "pp_documentsigning")
                    return;

                // Obtain the organization service reference.
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                if (context.MessageName.ToUpper() == "UPDATE")
                {
                    OptionSetValue statusCode = entity.Attributes["statuscode"] as OptionSetValue;
                    var user = context.InitiatingUserId;
                    var lcid = RetrieveUserUiLanguageCode(service, user);

                    if (statusCode.Value != 778380000)
                        return;
                    bool sendmail = (bool)entity.Attributes["pp_sendemail"];
                    if (sendmail)
                    {
                        try
                        {
                            var signicatTasks = GetRelatedSigningTask(entity.Id, service);
                            foreach (var task in signicatTasks.Entities)
                            {
                                SendEmail(entity, task, lcid, service);
                            }
                            return;
                        }

                        catch (FaultException<OrganizationServiceFault> ex)
                        {
                            throw new InvalidPluginExecutionException("An error occurred in the plug-in: pp_documentsigning. " + ex.Message, ex);
                        }

                        catch (Exception ex)
                        {
                            tracingService.Trace("Plugin error: {0}", ex.ToString());
                            throw;
                        }
                    }
                }
            }
        }

        private EntityCollection GetRelatedSigningResults(Guid id, IOrganizationService service)
        {
            var query = new QueryExpression("pp_signicatresulturl");
            query.ColumnSet = new ColumnSet(false);
            query.Criteria.AddCondition("pp_documentsignid", ConditionOperator.Equal, id);
            return service.RetrieveMultiple(query);

        }

        private EntityCollection GetRelatedSigningTask(Guid id, IOrganizationService service)
        {
            var query = new QueryExpression("pp_signicatdocurl");
            query.ColumnSet = new ColumnSet(new string[] { "pp_sdsurl", "pp_customerid" });
            query.Criteria.AddCondition("pp_documentsigningid", ConditionOperator.Equal, id);
            return service.RetrieveMultiple(query);
        }

        private void SendEmail(Entity documentsigning, Entity task, int lcid, IOrganizationService service)
        {
            try
            {
                var linkBtnImg = "https://ppsignicatresources.blob.core.windows.net/signicatlinkbutton/SignicatLinkButton.png";
                var emailLogoImg = "https://ppsignicatresources.blob.core.windows.net/signicatlinkbutton/SignicatMailLogo.png";

                //var contcatRef = (EntityReference)task.Attributes["pp_contactid"];
                //var accountRef = (EntityReference)task.Attributes["pp_accountid"];
                var customerRef = (EntityReference)task.Attributes["pp_customerid"];
                var link = task.Attributes["pp_sdsurl"].ToString();
                var url = "";
                if (lcid == 1033)
                    url = "<br/><a href='" + link + "'><img alt='Click here' src='" + linkBtnImg + "'></a>";
                else if (lcid == 1044)
                    url = "<br/><a href='" + link + "'><img alt='Klikk her' src='" + linkBtnImg + "'></a>";
                else
                    url = "<br/><a href='" + link + "'><img alt='Click here' src='" + linkBtnImg + "'></a>";


                //var _userId = GetSenderId(documentsigning, service);
                var senderRef = (EntityReference)documentsigning["ownerid"];
                //throw new InvalidPluginExecutionException("A: " + senderRef.Id);

                Entity fromParty = new Entity("activityparty");
                fromParty["partyid"] = new EntityReference("systemuser", senderRef.Id);

                Entity toParty = new Entity("activityparty");
                toParty["partyid"] = new EntityReference(customerRef.LogicalName, customerRef.Id);

                var listfrom = new List<Entity>() { fromParty };
                var listto = new List<Entity>() { toParty };
                var subject = documentsigning["pp_subject"].ToString();
                var description = documentsigning["pp_message"].ToString();

                // Create an e-mail message.
                Entity email = new Entity("email");
                email["to"] = new EntityCollection(listto);
                email["from"] = new EntityCollection(listfrom);
                email["subject"] = subject;
                email["directioncode"] = true;
                email["regardingobjectid"] = new EntityReference(documentsigning.LogicalName, documentsigning.Id);

                if (description != null)
                {
                    var newText = "<img src='" + emailLogoImg + "'/><br/><br/>";
                    //var regex = new Regex(@"(\r\r\n\n|\r\r|\n\n)+");
                    //newText = regex.Replace(description, "<br />" + "<br />");
                    //regex = new Regex(@"(\r\n|\r|\n)+");
                    //newText = regex.Replace(newText, "<br />" + "<br />");                    
                    newText += description;
                    newText += "<br/><br/><br/>" + url + "<br/><br/><br/>";
                    if (lcid == 1033)
                        newText += "<br/> With Regards<br/>";
                    else if (lcid == 1044)
                        newText += "<br/> Med vennlig Hilsen<br/>";
                    else
                        newText += "<br/> With Regards<br/>";
                    newText += senderRef.Name;

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
                throw new InvalidPluginExecutionException("An error occurred in sending email.", ex);
            }
        }

        //From the SDK: http://msdn.microsoft.com/en-us/library/hh670609.aspx
        private static int RetrieveUserUiLanguageCode(IOrganizationService service, Guid userId)
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

    public class FileObj
    {
        public string filename { get; set; }
        public byte[] file { get; set; }
    }

}
