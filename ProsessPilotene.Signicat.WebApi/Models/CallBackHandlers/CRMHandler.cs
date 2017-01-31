using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Services.Protocols;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Office.Interop.Word;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Sdk.Query;
using PP.Signicat.CredentialManager.Models;

namespace PP.Signicat.WebApi.Models.CallBackHandlers
{
    internal class CRMHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="orgname"></param>
        /// <returns></returns>
        internal IOrganizationService ConnectToCRM(string orgname)
        {
            return new CRMConnection().GetOrgService(orgname);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="taskid"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        internal Entity GetDocumentSigning(string requestid, string taskid, IOrganizationService service)
        {
            try
            {
                QueryExpression query = new QueryExpression("pp_documentsigning");
                query.ColumnSet = new ColumnSet("pp_saveindocumentlocation", "pp_saveonlymerged", "pp_accountid",
                    "pp_opportunityid", "pp_salesorderid", "pp_quoteid", "pp_incidentid", "pp_contractid",
                    "pp_sendcopy", "ownerid", "pp_signing");
                query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, 778380000); //Sent
                query.Criteria.AddCondition("pp_requestid", ConditionOperator.Equal, requestid);
                var result = service.RetrieveMultiple(query);

                if (result.Entities.Count == 1)
                    return result.Entities[0];
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="docsignRef"></param>
        /// <param name="status"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        internal EntityCollection GetDocumentSiginingTasks(EntityReference docsignRef, int status,
            IOrganizationService service)
        {
            try
            {
                QueryExpression query = new QueryExpression("pp_signicatdocurl");
                query.ColumnSet = new ColumnSet("pp_sdsurl", "pp_name", "pp_documentsigningid", "pp_customerid",
                    "pp_customerid");
                query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, status);
                query.Criteria.AddCondition("pp_documentsigningid", ConditionOperator.Equal, docsignRef.Id);
                var result = service.RetrieveMultiple(query);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="statuscode"></param>
        /// <param name="service"></param>
        internal void ChangeDocumentSigningTaskStatus(Guid id, int statuscode, IOrganizationService service)
        {
            try
            {
                var entity = new Entity("pp_signicatdocurl");
                entity.Id = id;
                entity["statuscode"] = new OptionSetValue(statuscode);
                service.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="documentsigningRef"></param>
        /// <param name="documentsigningtaskRef"></param>
        /// <param name="resulturl"></param>
        /// <param name="name"></param>
        /// <param name="service"></param>
        internal Entity CreateSignicatResult(EntityReference documentsigningRef, EntityReference documentsigningtaskRef,
            string resulturl, string name, IOrganizationService service)
        {
            try
            {
                var padesUrl = new SignicatHandler().CreatePades(null, resulturl);

                var entity = new Entity("pp_signicatresulturl");
                entity["pp_resulturl"] = resulturl;
                if (padesUrl != null)
                    entity["pp_xpadesurl"] = padesUrl;
                entity["pp_name"] = name;
                entity["pp_documentsignid"] = documentsigningRef;
                if (documentsigningtaskRef != null)
                    entity["pp_signicatdocurlid"] = documentsigningtaskRef;
                var entityid = service.Create(entity);
                entity.Id = entityid;

                return entity;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encodedData"></param>
        /// <param name="name"></param>
        /// <param name="entityid"></param>
        /// <param name="entityname"></param>
        /// <param name="service"></param>
        internal HttpStatusCode CreateAnnotations(string signicatUrl, string name, string subject, Guid entityid,
            string entityname, int lcid, IOrganizationService service)
        {
            try
            {
                var fileobj = new SignicatHandler().ReadAsyncFile(signicatUrl);
                string encodedData = Convert.ToBase64String(fileobj.Result.file);

                Random rnd = new Random();
                int docnr = rnd.Next(100, 500);

                Entity Annotation = new Entity("annotation");
                Annotation.Attributes["objectid"] = new EntityReference(entityname, entityid);
                Annotation.Attributes["objecttypecode"] = entityname;
                if (lcid == 1044)
                    Annotation.Attributes["subject"] = "Signert: " + subject;
                else
                    Annotation.Attributes["subject"] = "Signed: " + subject;
                Annotation.Attributes["documentbody"] = encodedData;
                Annotation.Attributes["mimetype"] = @"application/pdf";

                if (lcid == 1044)
                    Annotation.Attributes["notetext"] = Resources.Resourcenb.notetext;
                else
                    Annotation.Attributes["notetext"] = Resources.Resourceeng.notetext;
                Annotation.Attributes["filename"] = name + ".pdf";

                service.Create(Annotation);
                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                return HttpStatusCode.BadRequest;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskid"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        internal string GetSDSUrlFromTask(Guid taskid, IOrganizationService service)
        {
            try
            {
                QueryExpression query = new QueryExpression("pp_signicatdocurl");
                query.ColumnSet = new ColumnSet("pp_sdsurl");
                query.Criteria.AddCondition("pp_signicatdocurlid", ConditionOperator.Equal, taskid);
                var result = service.RetrieveMultiple(query);

                if (result.Entities.Count == 1)
                    return result.Entities[0].Attributes["pp_sdsurl"].ToString();
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="docsignid"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        internal EntityCollection GetAllSigningResults(Guid docsignid, IOrganizationService service)
        {
            try
            {
                QueryExpression query = new QueryExpression("pp_signicatresulturl"); //Signing tasks
                query.ColumnSet = new ColumnSet("pp_resulturl", "pp_xpadesurl", "pp_name", "pp_signicatdocurlid");
                query.Criteria.AddCondition("pp_documentsignid", ConditionOperator.Equal, docsignid);
                query.Criteria.AddCondition("statuscode", ConditionOperator.NotEqual, 2); //Not incative
                var result = service.RetrieveMultiple(query);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        internal string GetSettingKeyValue(IOrganizationService service, string key)
        {
            try
            {
                var query = new QueryExpression("pp_signicatsettings");
                query.ColumnSet = new ColumnSet("pp_value");
                query.Criteria.AddCondition("pp_name", ConditionOperator.Equal, key);
                var results = service.RetrieveMultiple(query);

                if (results.Entities.Count == 1)
                    return results.Entities[0]["pp_value"].ToString();
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        internal void SendSignedMergedCopy(EntityReference docsignRef, EntityReference senderRef,
            string padesurl, string name, int lcid, IOrganizationService service)
        {
            try
            {
                var tasks = GetDocumentSiginingTasks(docsignRef, 778380000, service);

                foreach (var task in tasks.Entities)
                {
                    var customer = (EntityReference) task.Attributes["pp_customerid"];
                    SendEmail(docsignRef, senderRef, padesurl, name, customer, lcid, service);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        internal void SendEmail(EntityReference docsignRef, EntityReference senderRef, string padesurl,
            string name, EntityReference customer, int lcid, IOrganizationService service)
        {
            try
            {
                Entity fromParty = new Entity("activityparty");
                fromParty["partyid"] = new EntityReference("systemuser", senderRef.Id);

                Entity toParty = new Entity("activityparty");
                toParty["partyid"] = new EntityReference(customer.LogicalName, customer.Id);

                var listfrom = new List<Entity>() {fromParty};
                var listto = new List<Entity>() {toParty};

                // Create an e-mail message.
                Entity email = new Entity("email");
                email["to"] = new EntityCollection(listto);
                email["from"] = new EntityCollection(listfrom);

                if (lcid == 1044)
                    email["subject"] = Resources.Resourcenb.emailsubject + ": " + name;
                else
                    email["subject"] = Resources.Resourceeng.emailsubject + ": " + name;

                email["directioncode"] = true;
                email["regardingobjectid"] = new EntityReference(docsignRef.LogicalName, docsignRef.Id);

                if (lcid == 1044)
                    email["description"] = Resources.Resourcenb.emaildescription + ": " + name;
                else
                    email["description"] = Resources.Resourceeng.emaildescription + ": " + name;

                var _emailId = service.Create(email);

                // Create email attachment
                var fileobj = new SignicatHandler().ReadAsyncFile(padesurl);
                string encodedData = Convert.ToBase64String(fileobj.Result.file);

                Entity _emailAttachment = new Entity("activitymimeattachment");
                _emailAttachment["objectid"] = new EntityReference(email.LogicalName, _emailId);
                _emailAttachment["objecttypecode"] = email.LogicalName;
                _emailAttachment["subject"] = name;
                _emailAttachment["body"] = encodedData;
                _emailAttachment["filename"] = name + ".pdf";


                var _emailAttachmentId = service.Create(_emailAttachment);

                // Use the SendEmail message to send an e-mail message.
                SendEmailRequest sendEmailreq = new SendEmailRequest
                {
                    EmailId = _emailId,
                    TrackingToken = "",
                    IssueSend = true
                };

                SendEmailResponse sendEmailresp = (SendEmailResponse) service.Execute(sendEmailreq);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        internal void DeactivateSignicatResults(IOrderedEnumerable<IGrouping<string, ResultObject>> groups,
            IOrganizationService service)
        {
            foreach (var group in groups)
            {
                var tempList = group.ToList();
                if (tempList.Count > 1)
                {
                    foreach (var resultObject in tempList)
                    {
                        SetStateRequest setStateRequest = new SetStateRequest()
                        {
                            EntityMoniker = new EntityReference
                            {
                                Id = resultObject.Id,
                                LogicalName = "pp_signicatresulturl",
                            },
                            State = new OptionSetValue(1),
                            Status = new OptionSetValue(2)
                        };
                        service.Execute(setStateRequest);
                    }

                }
            }
        }

        internal int RetrieveUserUiLanguageCode(IOrganizationService service, Guid userId)
        {
            var userSettingsQuery = new QueryExpression("usersettings");
            userSettingsQuery.ColumnSet.AddColumns("uilanguageid", "systemuserid");
            userSettingsQuery.Criteria.AddCondition("systemuserid", ConditionOperator.Equal, userId);
            var userSettings = service.RetrieveMultiple(userSettingsQuery);
            if (userSettings.Entities.Count > 0)
            {
                return (int) userSettings.Entities[0]["uilanguageid"];
            }
            return 0;
        }

        internal void CreateTransaction(string orgname, int method, EntityReference senderRef,
            IOrganizationService service)
        {
            try
            {
                var subscription = new SubscriptionHandler().GetSubscription("signicatcrm", orgname);
                if (subscription == null)
                    return;
                
                var period = DateTime.Now.Month.ToString() + "-" + DateTime.Now.Year.ToString();
                var transaction = new TransactionHandler().GetTransaction(orgname, period);

                if (transaction != null)
                {
                    var crmunique = GetUniqueSignersCount(senderRef, service);

                    var total = transaction.countertotal;
                    var bankid = transaction.counterbankid;
                    var npid = transaction.counternpid;
                    var social = transaction.countersocial;
                    var handwritten = transaction.counterhandwritten;
                    var unique = transaction.counteruniqueusers;

                    var newtransaction = new TransactionModel();
                    newtransaction.subscription = transaction.subscription;
                    newtransaction.period = transaction.period;
                    newtransaction.countertotal = total + 1;
                    newtransaction.counteruniqueusers = crmunique;

                    if (method == 1) //Bankid
                        newtransaction.counterbankid = bankid + 1;
                    if (method == 2) //npid
                        newtransaction.counternpid = npid + 1;
                    if (method == 3) //Social
                        newtransaction.countersocial = social + 1;
                    if (method == 4) //Handwritten
                        newtransaction.counterhandwritten = handwritten + 1;

                    new TransactionHandler().UpdateTransactions(newtransaction);
                }

                if (transaction == null)
                {
                    var newtransaction = new TransactionModel();
                    newtransaction.period = period;
                    newtransaction.subscription = orgname;
                    newtransaction.customer = subscription.customer;
                    newtransaction.countertotal = 1;
                    newtransaction.counteruniqueusers = 1;

                    if (method == 1) //Bankid
                        newtransaction.counterbankid = 1;
                    if (method == 2) //npid
                        newtransaction.counternpid = 1;
                    if (method == 3) //Social
                        newtransaction.countersocial = 1;
                    if (method == 4) //Handwritten
                        newtransaction.counterhandwritten = 1;

                    new TransactionHandler().CreateTransactions(newtransaction);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private int GetUniqueSignersCount(EntityReference senderRef, IOrganizationService service)
        {
            try
            {
                var query = new QueryExpression("pp_documentsigning");
                query.ColumnSet = new ColumnSet(false);
                query.Criteria.AddCondition("ownerid", ConditionOperator.Equal, senderRef.Id);
                query.Distinct = true;

                var results = service.RetrieveMultiple(query);
                return results.Entities.Count;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        internal void DeactivateTask(string requestId, string taskId, IOrganizationService service)
        {
            try
            {
                QueryExpression query = new QueryExpression("pp_signicatdocurl");
                query.ColumnSet = new ColumnSet(false);
                query.Criteria.AddCondition("new_name", ConditionOperator.Equal, taskId);
                query.Criteria.AddCondition("pp_sdsurl", ConditionOperator.Contains, requestId);
                var result = service.RetrieveMultiple(query);

                if (result.Entities.Count == 1)
                {
                    var task = result.Entities[0].ToEntityReference();
                    SetStateRequest setStateRequest = new SetStateRequest()
                    {
                        EntityMoniker = new EntityReference
                        {
                            Id = task.Id,
                            LogicalName = task.LogicalName,
                        },
                        State = new OptionSetValue(1),
                        Status = new OptionSetValue(2)
                    };
                    service.Execute(setStateRequest);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}