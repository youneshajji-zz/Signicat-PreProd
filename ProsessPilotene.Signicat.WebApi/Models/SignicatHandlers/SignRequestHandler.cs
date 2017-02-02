using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Office.Interop.Word;
using PP.Signicat.WebApi.SignicatPreProd;

namespace PP.Signicat.WebApi.Models.SignicatHandlers
{
    internal class SignRequestHandler
    {
        internal async Task<sdsdocument[]> UploadDocument(List<HttpPostedFile> postedFiles)
        {
            try
            {

                var sdsdocuments = new sdsdocument[postedFiles.Count];
                var httpClientHandler = new HttpClientHandler { Credentials = new NetworkCredential("prosesspilotene", "Bond007") };
                using (var client = new HttpClient(httpClientHandler))
                {
                    for (int i = 0; i < postedFiles.Count; i++)
                    {
                        var helpers = new Helpers();
                        string documentId = "";

                        if (postedFiles[i].ContentType == "application/pdf")
                        {
                            var fileBytes = helpers.ReadToEnd(postedFiles[i].InputStream);
                            HttpContent content = new ByteArrayContent(fileBytes);
                            content.Headers.ContentType = new MediaTypeHeaderValue(postedFiles[i].ContentType);
                            HttpResponseMessage response =
                                await client.PostAsync("https://preprod.signicat.com/doc/prosesspilotene/sds", content);
                            documentId = await response.Content.ReadAsStringAsync();
                        }

                        var docName = postedFiles[i].FileName.Split('.');
                        if (string.IsNullOrWhiteSpace(documentId))
                            return null;

                        sdsdocuments[i] = new sdsdocument
                        {
                            id = docName[0],
                            refsdsid = documentId,
                            description = docName[0]
                        };
                    }
                    return sdsdocuments;
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        internal createrequestrequest GetCreateRequest(sdsdocument[] documentInSds, List<ContactInfo> recipients, SigningInfo signingInfo)
        {
            try
            {
                request[] req = new[] { CreateRequest(documentInSds, recipients, signingInfo) };

                var request = new createrequestrequest
                {
                    password = "Bond007",
                    service = "prosesspilotene",
                    request = req
                };
                return request;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private request CreateRequest(sdsdocument[] documentInSds, List<ContactInfo> recipients, SigningInfo signingInfo)
        {
            try
            {
                var tasks = GetTasks(documentInSds, recipients, signingInfo);
                var documents = GetDocuments(documentInSds, recipients);

                if (signingInfo.notifyMe == 1)
                    tasks = new NotificationHandler().AddNotifyMe(signingInfo, tasks);

                var lang = "en";
                if (signingInfo.LCID == 1044)
                    lang = "nb";

                var request = new request
                {
                    clientreference = "cliref1",
                    language = lang,
                    profile = "default",
                    //subject = GetSubjects(recipients),
                    task = tasks,
                    document = documents
                };

                return request;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private task[] GetTasks(sdsdocument[] documentInSds, List<ContactInfo> recipients, SigningInfo signingInfo)
        {
            try
            {

                var callbackOnTaskCompleteUrl = "https://prosesspilotenesignicatwebapi-preprod.azurewebsites.net:443/api/Callback/Landingpage?lcid=" + signingInfo.LCID;
                var signatures = new SignatureHandler().GetSignatures(signingInfo);
                var authSignatures = new SignatureHandler().GetAuthSignatures(signingInfo);
                var documentactions = GetDocumentActions(documentInSds, recipients);
                var randomnr = new Random();
                int nr = randomnr.Next(10000);

                var tasks = new task[recipients.Count];
                for (int i = 0; i < recipients.Count; i++)
                {
                    var notifications = new NotificationHandler().AddNotifications(recipients[i], signingInfo, i);
                    tasks[i] = new task
                    {
                        id = "task_" + i + "_" + nr, // Any identifier you'd like
                        subjectref = "sub_" + i, // Any identifier you'd like
                        bundle = true,
                        //bundleSpecified = true,
                        daystolive = signingInfo.daysToLive,
                        documentaction = documentactions,
                        //signature = signatures,
                        authenticationbasedsignature = authSignatures,
                        ontaskcomplete = callbackOnTaskCompleteUrl,
                        subject = new subject
                        {
                            id = "sub_" + i,
                            email = recipients[i].email,
                            mobile = !string.IsNullOrWhiteSpace(recipients[i].mobile) ? recipients[i].mobile : ""
                            //nationalid = recipients[i].ssn //National id mustbe inserted
                        },
                        notification = notifications
                    };
                }
                return tasks;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

       

        private document[] GetDocuments(sdsdocument[] documentInSds, List<ContactInfo> recipients)
        {
            try
            {
                var documents = new document[documentInSds.Length];
                for (int i = 0; i < documentInSds.Length; i++)
                {
                    var doc = new sdsdocument
                    {
                        id = documentInSds[i].id,
                        //id = "doc_" + i,
                        refsdsid = documentInSds[i].refsdsid,
                        description = documentInSds[i].description
                    };
                    documents[i] = doc;
                }
                return documents;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private documentaction[] GetDocumentActions(sdsdocument[] documentInSds, List<ContactInfo> recipients)
        {
            try
            {

                var documentactions = new documentaction[documentInSds.Length];
                for (int i = 0; i < documentInSds.Length; i++)
                {
                    var docact = new documentaction
                    {
                        sendresulttoarchive = false,
                        sendresulttoarchiveSpecified = false,
                        optional = false,
                        type = documentactiontype.sign,
                        //documentref = "doc_" + i
                        documentref = documentInSds[i].id
                    };
                    documentactions[i] = docact;
                }
                return documentactions;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        internal taskstatusinfo[] getStatus(string requestId)
        {
            try
            {

                using (var client = new DocumentEndPointClient())
                {
                    var request = new getstatusrequest
                    {
                        password = "Bond007",
                        service = "prosesspilotene",
                        requestid = new string[]
                        {
                       requestId
                        }
                    };

                    var taskStatusInfo = client.getStatus(request);

                    return taskStatusInfo;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
       
    }
}