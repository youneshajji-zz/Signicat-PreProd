using Microsoft.Crm.Sdk.Messages;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WordAddInSignicat
{
    public static class WordHandlerCRM
    {
        internal static Entity FindRecordByNumber(WordSearchObject searchValues, IOrganizationService crm)
        {
            try
            {
                var query = new QueryExpression(searchValues.searchentity);
                query.ColumnSet = new ColumnSet(true);
                query.Criteria.AddCondition(searchValues.searchnumberfield, ConditionOperator.Equal, searchValues.searchnumber);
                var results = crm.RetrieveMultiple(query);

                if (results.Entities.Count == 1)
                    return results.Entities[0];
                return null;
            }
            catch (Exception ex)
            {
                if (searchValues.language == 1044)
                    MessageBox.Show(Resources.ResourceWordNb.cannotfindsearch + ex.Message, Resources.ResourceWordNb.documentsinging, MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show(Resources.ResourceWordEn.cannotfindsearch + ex.Message, Resources.ResourceWordEn.documentsinging, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        internal static Guid CreatDocumentSigningInCRM(string sdsurl, string docPath, string docName, Entity entity,
            WordSearchObject searchValues, WordCRMConfig crmconfig, IOrganizationService crm)
        {
            try
            {
                var parts = sdsurl.Split('=');
                var taskid = parts.Last();
                var requestid = GetRequestId(sdsurl);

                var saveinsp = crmconfig.Wordsaveinsp;


                //Create signing package
                var documentsigning = new Entity("pp_documentsigning");
                documentsigning["pp_name"] = "Word dokument: " + docName;
                documentsigning["pp_requestid"] = requestid;
                documentsigning["pp_signing"] = new OptionSetValue(crmconfig.Wordsigningmethod); 

                if (saveinsp)
                    documentsigning["pp_saveindocumentlocation"] = true;

                if (entity != null)
                {
                    var regardingidfield = "pp_" + entity.LogicalName + "id";
                    documentsigning[regardingidfield] = new EntityReference(entity.LogicalName, entity.Id);
                }

                var documentsigningid = crm.Create(documentsigning);

                //Save doc as note in singing package
                SavePDFInCRM(searchValues, docName, docPath, documentsigningid, crm);

                //Create signing task
                var documenttask = new Entity("pp_signicatdocurl");
                documenttask["pp_name"] = taskid;
                documenttask["pp_sdsurl"] = sdsurl;
                if (documentsigningid != Guid.Empty)
                    documenttask["pp_documentsigningid"] = new EntityReference("pp_documentsigning", documentsigningid);
                documenttask["statuscode"] = new OptionSetValue(1);
                var documenttaskid = crm.Create(documenttask);

                CreateOrAddContact(searchValues.recievermail, documenttaskid, documentsigningid, crm);

                return documentsigningid;
            }
            catch (Exception ex)
            {
                if (searchValues.language == 1044)
                    MessageBox.Show(Resources.ResourceWordNb.cannotcreateinsignicat + ex.Message, Resources.ResourceWordNb.documentsinging, MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show(Resources.ResourceWordEn.cannotcreateinsignicat + ex.Message, Resources.ResourceWordEn.documentsinging, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return Guid.Empty;
            }
        }

        private static string GetRequestId(string sdsurl)
        {
            int from = sdsurl.IndexOf("=") + 1;
            int to = sdsurl.LastIndexOf("&");

            return sdsurl.Substring(from, to - from);
        }

        private static void CreateOrAddContact(string recievermail, Guid documenttaskid, Guid documentsigningid, IOrganizationService crm)
        {
            var accountRelationShip = "pp_pp_documentsigning_account";
            var contcatRelationShip = "pp_pp_documentsigning_contact";
            var docSignRef = new EntityReference("pp_documentsigning", documentsigningid);

            if (documenttaskid == Guid.Empty)
                return;

            var customerRef = GetCustomerByEmail(recievermail, crm);

            if (customerRef == null)
            {
                var contact = new Entity("contact");
                contact["fullname"] = recievermail;
                contact["emailaddress1"] = recievermail;
                var contcatid = crm.Create(contact);
                customerRef = new EntityReference("contact", contcatid);
            }

            var entity = new Entity("pp_signicatdocurl");
            entity.Id = documenttaskid;
            entity["pp_customerid"] = customerRef;
            crm.Update(entity);

            if (customerRef.LogicalName == "account")
                crm.Associate(customerRef.LogicalName, customerRef.Id, new Relationship(accountRelationShip), new EntityReferenceCollection() { docSignRef });
            if (customerRef.LogicalName == "contact")
                crm.Associate(customerRef.LogicalName, customerRef.Id, new Relationship(contcatRelationShip), new EntityReferenceCollection() { docSignRef });

        }

        private static EntityReference GetCustomerByEmail(string recievermail, IOrganizationService crm)
        {
            var queryContact = new QueryExpression("contact");
            queryContact.ColumnSet = new ColumnSet(false);
            queryContact.Criteria.AddCondition("emailaddress1", ConditionOperator.Equal, recievermail);
            var contactResult = crm.RetrieveMultiple(queryContact);

            foreach (var contactResultEntity in contactResult.Entities)
                return contactResultEntity.ToEntityReference();

            var queryAccount = new QueryExpression("account");
            queryAccount.ColumnSet = new ColumnSet(false);
            queryAccount.Criteria.AddCondition("emailaddress1", ConditionOperator.Equal, recievermail);
            var accountResult = crm.RetrieveMultiple(queryAccount);

            foreach (var accountResultEntity in accountResult.Entities)
                return accountResultEntity.ToEntityReference();
            return null;
        }

        internal static void SavePDFInCRM(WordSearchObject searchValues, string docName, string docPath, Guid documentsigningid, IOrganizationService crm)
        {
            try
            {
                byte[] pdf = File.ReadAllBytes(docPath);
                string encodedData = Convert.ToBase64String(pdf);

                Entity Annotation = new Entity("annotation");
                Annotation.Attributes["objectid"] = new EntityReference("pp_documentsigning", documentsigningid);
                Annotation.Attributes["objecttypecode"] = "pp_documentsigning";
                Annotation.Attributes["subject"] = "Original: " + docName;
                Annotation.Attributes["documentbody"] = encodedData;
                Annotation.Attributes["mimetype"] = @"application/pdf";
                if (searchValues.language == 1044)
                    Annotation.Attributes["notetext"] = Resources.ResourceWordNb.documentsentforsinging;
                else
                    Annotation.Attributes["notetext"] = Resources.ResourceWordEn.documentsentforsinging;
                Annotation.Attributes["filename"] = docName + ".pdf";

                crm.Create(Annotation);
            }
            catch (Exception ex)
            {
                if (searchValues.language == 1044)
                    MessageBox.Show(Resources.ResourceWordNb.cannotcreatenote + ex.Message, Resources.ResourceWordNb.documentsinging, MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show(Resources.ResourceWordEn.cannotcreatenote + ex.Message, Resources.ResourceWordEn.documentsinging, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        internal static void SendEmail(string sdsurl, string docName, Entity entity, Guid documentsigningid,
            WordSearchObject searchValues, WordCRMConfig crmconfig, IOrganizationService crm)
        {
            try
            {
                var linkBtnImg = "https://ppsignicatresources.blob.core.windows.net/signicatlinkbutton/SignicatLinkButton.png";
                var emailLogoImg = "https://ppsignicatresources.blob.core.windows.net/signicatlinkbutton/SignicatMailLogo.png";

                var url = "";
                if (searchValues.language == 1044)
                    url = "<br/><a href='" + sdsurl + "'><img alt='Click her' src='" + linkBtnImg + "'></a>";
                else
                    url = "<br/><a href='" + sdsurl + "'><img alt='Click here' src='" + linkBtnImg + "'></a>";

                // Create an e-mail message.
                Entity email = new Entity("email");
                
                if (crmconfig.Worduser == null)
                {
                    if (searchValues.language == 1044)
                        MessageBox.Show(Resources.ResourceWordNb.cannotfindsender);
                    else
                        MessageBox.Show(Resources.ResourceWordEn.cannotfindsender);
                }

                if (crmconfig.Worduser != null)
                {
                    Entity fromParty = new Entity("activityparty");
                    fromParty["partyid"] = crmconfig.Worduser;
                    var listfrom = new List<Entity>() { fromParty };
                    email["from"] = new EntityCollection(listfrom);
                }

                Entity toParty = new Entity("activityparty");
                if (string.IsNullOrWhiteSpace(searchValues.recievermail) && entity != null)
                    toParty["partyid"] = new EntityReference(entity.LogicalName, entity.Id);
                else if (!string.IsNullOrWhiteSpace(searchValues.recievermail))
                    toParty["addressused"] = searchValues.recievermail;
                var listto = new List<Entity>() { toParty };
                email["to"] = new EntityCollection(listto);

                var subject = "";
                var description = "";
                if (searchValues.language == 1044)
                {
                    subject = Resources.ResourceWordNb.documentsinging + ": " + docName;
                    description = Resources.ResourceWordNb.clicktosign;
                }
                else
                {
                    subject = Resources.ResourceWordEn.documentsinging + ": " + docName;
                    description = Resources.ResourceWordEn.clicktosign;
                }

                email["subject"] = subject;
                email["directioncode"] = true;
                email["regardingobjectid"] = new EntityReference("pp_documentsigning", documentsigningid);

                if (description != null)
                {
                    var newText = "<img src='" + emailLogoImg + "'/><br/>";
                    //newText += description;
                    newText += url + "<br/><br/><br/>";
                    newText += "<br/> With Regards / Med vennlig Hilsen<br/>";
                    newText += crmconfig.Wordusername;

                    email["description"] = newText;
                }

                //SendEmailFromTemplate(email, documentsigningid, sdsurl, crm);
                //return;

                var _emailId = crm.Create(email);

                // Use the SendEmail message to send an e-mail message.
                SendEmailRequest sendEmailreq = new SendEmailRequest
                {
                    EmailId = _emailId,
                    TrackingToken = "",
                    IssueSend = true
                };

                SendEmailResponse sendEmailresp = (SendEmailResponse)crm.Execute(sendEmailreq);
            }
            catch (Exception ex)
            {
                if (searchValues.language == 1044)
                    MessageBox.Show(Resources.ResourceWordNb.emailerror + ex.Message, "Email sending", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show(Resources.ResourceWordEn.emailerror + ex.Message, "Email sending", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void SendEmailFromTemplate(Entity email, Guid documentsigningid, string sdsurl, IOrganizationService crm)
        {
            try
            {
                var _templateId = new Guid("");

                QueryExpression queryBuildInTemplates = new QueryExpression
                {
                    EntityName = "template",
                    ColumnSet = new ColumnSet("templateid", "templatetypecode"),
                    Criteria = new FilterExpression()
                };
                queryBuildInTemplates.Criteria.AddCondition("templatetypecode",
                    ConditionOperator.Equal, "contact");
                EntityCollection templateEntityCollection = crm.RetrieveMultiple(queryBuildInTemplates);

                if (templateEntityCollection.Entities.Count > 0)
                {
                    _templateId = (Guid)templateEntityCollection.Entities[0].Attributes["templateid"];
                }
                else
                {
                    MessageBox.Show("Standard Email Templates are missing: ", "Email sending", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // Create the request
                SendEmailFromTemplateRequest emailUsingTemplateReq = new SendEmailFromTemplateRequest
                {
                    Target = email,

                    // Use a built-in Email Template of type "contact".
                    TemplateId = _templateId,

                    // The regarding Id is required, and must be of the same type as the Email Template.
                    RegardingId = documentsigningid,
                    RegardingType = "pp_documentsigning"
                };

                SendEmailFromTemplateResponse emailUsingTemplateResp = (SendEmailFromTemplateResponse)crm.Execute(emailUsingTemplateReq);

                // Verify that the e-mail has been created
                var _emailId = emailUsingTemplateResp.Id;
                if (!_emailId.Equals(Guid.Empty))
                {
                    MessageBox.Show("Successfully sent an e-mail message using the template.", "Email sending", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred in sending email from template: " + ex.Message, "Email sending", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        internal static int RetrieveUserUiLanguageCode(IOrganizationService service, Guid userId)
        {
            var userSettingsQuery = new QueryExpression("usersettings");
            userSettingsQuery.ColumnSet.AddColumns("uilanguageid", "systemuserid");
            userSettingsQuery.Criteria.AddCondition("systemuserid", ConditionOperator.Equal, userId);
            var userSettings = service.RetrieveMultiple(userSettingsQuery);
            if (userSettings.Entities.Count > 0)
            {
                return (int)userSettings.Entities[0]["uilanguageid"];
            }
            return 1033;
        }

        internal static WordCRMConfig GetCRMConfig(IOrganizationService crm)
        {
            try
            {
                var query = new QueryExpression("pp_signicatconfig");
                query.ColumnSet = new ColumnSet(true);
                var results = crm.RetrieveMultiple(query);

                if (results.Entities.Count == 1)
                {
                    var entity = results.Entities[0];

                    var config = new WordCRMConfig();
                    config.Worduser = entity.GetAttributeValue<EntityReference>("pp_wordsignuser");
                    config.Entitylogicalnames = entity.GetAttributeValue<string>("pp_entitylogicalnames");
                    config.SPUser = entity.GetAttributeValue<string>("pp_spuser");
                    config.SPpassword = entity.GetAttributeValue<string>("pp_sppassord");
                    config.Webapiurl = entity.GetAttributeValue<string>("pp_webapiurl");
                    config.Wordsaveinsp = entity.GetAttributeValue<bool>("pp_wordsaveinsp");
                    config.Wordsigningmethod = entity.GetAttributeValue<OptionSetValue>("pp_signing").Value;

                    var senderRef = crm.Retrieve("systemuser", config.Worduser.Id, new ColumnSet("fullname"));
                    config.Wordusername = senderRef.GetAttributeValue<string>("fullname");

                    config.AccountNrField = entity.GetAttributeValue<string>("pp_accountnumberfield");
                    config.OrderNrField = entity.GetAttributeValue<string>("pp_ordernumberfield");
                    config.IncidentNrField = entity.GetAttributeValue<string>("pp_incidentnumberfield");
                    config.QuoteNrField = entity.GetAttributeValue<string>("pp_quotenumberfield");
                    config.ContractNrField = entity.GetAttributeValue<string>("pp_contractnumberfield");
                    config.EmailField = entity.GetAttributeValue<string>("pp_emailfield");

                    return config;
                }
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot find value in Signicat config in CRM: " + ex.Message, "Document Signing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}
