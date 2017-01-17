using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Results;
using Microsoft.Xrm.Sdk;
using PP.Signicat.WebApi.SignicatPreProd;

namespace PP.Signicat.WebApi.Models.CallBackHandlers
{
    public class CallBackHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskid"></param>
        /// <param name="orgname"></param>
        internal async Task<HttpStatusCode> GetDocumentSigning(string requestid, string taskid, string orgname)
        {
            if (string.IsNullOrEmpty((requestid)))
                return HttpStatusCode.BadRequest;
            try
            {
                var service = new CRMHandler().ConnectToCRM(orgname); //Connect to the customers CRM
                if (service == null)
                    return HttpStatusCode.NotFound;

                var documentsigning = new CRMHandler().GetDocumentSigning(requestid, taskid, service);
                HandleTasks(documentsigning, orgname, service);
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
        /// <param name="documentsigning"></param>
        /// <param name="service"></param>
        private HttpStatusCode HandleTasks(Entity documentsigning, string orgname, IOrganizationService service)
        {
            try
            {
                var documentsigningtasks = new CRMHandler().GetDocumentSiginingTasks(
                    documentsigning.ToEntityReference(), 1, service); //status sent
                var regardingRef = new Helpers().GetRegarding(documentsigning, service);
                var sendcopy = (bool)documentsigning["pp_sendcopy"];
                var saveinsp = (bool)documentsigning["pp_saveindocumentlocation"];
                var senderRef = (EntityReference)documentsigning["ownerid"];

                var lcid = new CRMHandler().RetrieveUserUiLanguageCode(service, senderRef.Id);

                foreach (var documentsigningtask in documentsigningtasks.Entities)
                {
                    var sdsurl = documentsigningtask.Attributes["pp_sdsurl"].ToString();
                    var taskid = documentsigningtask.Attributes["pp_name"].ToString();

                    var subString = sdsurl.Substring(sdsurl.IndexOf('=') + 1);
                    var requestId = subString.Substring(0, subString.IndexOf('&'));

                    var taskinfo = new SignicatHandler().GetSignicatTaskInfo(requestId, taskid);

                    if (taskinfo == null)
                        continue;

                    if (taskinfo.taskstatus == taskstatus.created)
                        continue;

                    if (taskinfo.taskstatus == taskstatus.rejected)
                        new CRMHandler().ChangeDocumentSigningTaskStatus(documentsigningtask.Id, 778380001, service);
                    //Rejected

                    if (taskinfo.taskstatus == taskstatus.completed)
                    {
                        foreach (var documentstatus in taskinfo.documentstatus)
                        {
                            var documentsigningRef = (EntityReference)documentsigningtask["pp_documentsigningid"];
                            var saveonlymerged = (bool)documentsigning["pp_saveonlymerged"];
                            var name = new Helpers().GetNameFromUrl(documentstatus.originaluri);

                            new CRMHandler().CreateSignicatResult(documentsigningRef,
                                documentsigningtask.ToEntityReference(), documentstatus.resulturi, name, service);

                            if (!saveonlymerged)
                            {
                                var title = "Single document";
                                if (lcid == 1044)
                                    title = "Singel dokument";

                                var padesurl = new SignicatHandler().CreatePades(null, documentstatus.resulturi);
                                new CRMHandler().CreateAnnotations(padesurl, name, title,
                                    documentsigningRef.Id, documentsigningRef.LogicalName, lcid, service);

                                if (sendcopy)
                                {
                                    var customer = (EntityReference)documentsigningtask.Attributes["pp_customerid"];
                                    new CRMHandler().SendEmail(documentsigning.ToEntityReference(), senderRef,
                                        padesurl, name, customer, lcid, service);
                                }

                            }
                        }

                        new CRMHandler().ChangeDocumentSigningTaskStatus(documentsigningtask.Id, 778380000,
                            service);
                        //Signed
                    }
                }

                CheckOverAllStatus(documentsigning.ToEntityReference(), regardingRef, saveinsp, sendcopy,
                    senderRef, orgname, lcid, service);
                return HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                return HttpStatusCode.BadRequest;
            }
        }

        /// <summary>
        /// If the documentsigningtasks of a specific documentsigning are signed, then generate the pades document(s)
        /// </summary>
        /// <param name="docsignRef"></param>
        /// <param name="regardingRef"></param>
        /// <param name="saveinsp"></param>
        /// <param name="service"></param>
        /// <returns>List of pades url's</returns>
        private HttpStatusCode CheckOverAllStatus(EntityReference docsignRef, EntityReference regardingRef,
             bool saveinsp, bool sendcopy, EntityReference senderRef, string orgname, int lcid,
            IOrganizationService service)
        {
            try
            {
                var results = new CRMHandler().GetDocumentSiginingTasks(docsignRef, 1, service);
                //if all tasks are completed, the return is null
                if (results.Entities.Count == 0)
                {
                    ConvertSdoDocument(docsignRef, regardingRef, saveinsp, sendcopy, senderRef, orgname,
                        lcid, service);
                    return HttpStatusCode.OK;
                }
                return HttpStatusCode.NoContent;
            }
            catch (Exception ex)
            {
                return HttpStatusCode.BadRequest;
            }
        }

        /// <summary>
        /// Loops throught the signingresults and 
        /// </summary>
        /// <param name="docsignRef"></param>
        /// <param name="regardingRef"></param>
        /// <param name="saveinsp"></param>
        /// <param name="service"></param>
        /// <returns>List of pades url's</returns>
        private void ConvertSdoDocument(EntityReference docsignRef, EntityReference regardingRef,
             bool saveinsp, bool sendcopy, EntityReference senderRef, string orgname, int lcid,
            IOrganizationService service)
        {
            try
            {
                var signingresults = new CRMHandler().GetAllSigningResults(docsignRef.Id, service);
                var docList = new List<ResultObject>();

                foreach (var item in signingresults.Entities)
                {
                    string padesurl = null;
                    var resulturl = item.Attributes["pp_resulturl"].ToString();
                    if (item.Contains("pp_xpadesurl"))
                        padesurl = item.Attributes["pp_xpadesurl"].ToString();
                    var name = item.Attributes["pp_name"].ToString();
                    var taskRef = (EntityReference)item.Attributes["pp_signicatdocurlid"];
                    var sdsurl = new CRMHandler().GetSDSUrlFromTask(taskRef.Id, service);

                    var subString = sdsurl.Substring(sdsurl.IndexOf('=') + 1);
                    var requestid = subString.Substring(0, subString.IndexOf('&'));

                    var obj = new ResultObject
                    {
                        Id = item.Id,
                        name = name,
                        resulturl = resulturl,
                        sdsurl = sdsurl,
                        rquestid = requestid,
                        padesurl = padesurl,
                        taskId = taskRef.Id
                    };
                    docList.Add(obj);
                }

                var groups = docList.GroupBy(x => x.name).OrderBy(x => x.Key);

                var title = "Single document";
                if (lcid == 1044)
                    title = "Singel dokument";

                foreach (var item in groups)
                {
                    var tempList = item.ToList();

                    if (tempList.Count < 2) //If one signer and one document
                    {
                        if (tempList[0].padesurl != null)
                        {
                            new CRMHandler().CreateAnnotations(tempList[0].padesurl, tempList[0].name,
                                title, docsignRef.Id, docsignRef.LogicalName, lcid, service);
                            if (sendcopy)
                                new CRMHandler().SendSignedMergedCopy(docsignRef, senderRef,
                                    tempList[0].padesurl, tempList[0].name, lcid, service);
                        }

                        else
                        {
                            new CRMHandler().CreateAnnotations(tempList[0].resulturl, tempList[0].name,
                                title, docsignRef.Id, docsignRef.LogicalName, lcid, service);
                            if (sendcopy)
                                new CRMHandler().SendSignedMergedCopy(docsignRef, senderRef,
                                    tempList[0].padesurl, tempList[0].name, lcid, service);
                        }


                        if (saveinsp && regardingRef != null)
                        {
                            if (tempList[0].padesurl != null)
                                new SharePointHandler().SaveInSP(regardingRef, tempList[0].padesurl,
                                    tempList[0].name, orgname, service);
                            else
                                new SharePointHandler().SaveInSP(regardingRef, tempList[0].resulturl,
                                    tempList[0].name, orgname, service);
                        }
                        continue;
                    }

                    var padesurl = new SignicatHandler().CreatePades(tempList, null);
                    if (padesurl != null)
                    {
                        var mergedTitle = "Merged document";
                        if (lcid == 1044)
                            mergedTitle = "Sammenslått dokument";

                        new CRMHandler().CreateSignicatResult(docsignRef, null, padesurl, mergedTitle +
                            ": " + tempList[0].name, service);
                        new CRMHandler().CreateAnnotations(padesurl, tempList[0].name, mergedTitle,
                            docsignRef.Id, docsignRef.LogicalName, lcid, service);

                        if (sendcopy)
                            new CRMHandler().SendSignedMergedCopy(docsignRef, senderRef, padesurl,
                                tempList[0].name, lcid, service);

                        if (saveinsp && regardingRef != null)
                            new SharePointHandler().SaveInSP(regardingRef, padesurl, tempList[0].name, orgname, service);
                    }
                }

                //HandlerCRM.DeaktivateSignicatResults(groups, service);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }

}