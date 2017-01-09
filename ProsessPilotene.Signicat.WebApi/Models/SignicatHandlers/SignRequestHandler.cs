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
    public class SignRequestHandler
    {
        public async Task<sdsdocument[]> UploadDocument(List<HttpPostedFile> postedFiles)
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

        public createrequestrequest GetCreateRequest(sdsdocument[] documentInSds, List<ContactInfo> recipients, SigningInfo signingInfo)
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

                if (signingInfo.notifyMe == "true")
                    tasks = AddNotifyMe(signingInfo, tasks);

                var request = new request
                {
                    clientreference = "cliref1",
                    language = "nb",
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

        private task[] AddNotifyMe(SigningInfo signingInfo, task[] tasks)
        {
            try
            {
                var notifyme = new notification
                {
                    header = "Message from Signicat",
                    message = "A document or more are signed and will be available in CRM shortly!",
                    notificationid = "req_com",
                    recipient = signingInfo.senderMail,
                    sender = "noreply@signicat.com",
                    type = notificationtype.EMAIL,
                    schedule = new schedule[]
                    {
                        new schedule
                        {
                            stateis = taskstatus.completed,
                        }
                    }
                };

                for (int i = 0; i < tasks.Length; i++)
                {
                    var tempList = tasks[i].notification.ToList();
                    tempList.Add(notifyme);
                    tasks[i].notification = tempList.ToArray();
                }

                return tasks;
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
                var signatures = GetSignatures(signingInfo.signingMetodText);
                var documentactions = GetDocumentActions(documentInSds, recipients);
                var randomnr = new Random();
                int nr = randomnr.Next(10000);

                var expiration = 0;
                if (signingInfo.daysToLive > 2)
                    expiration = signingInfo.daysToLive - 2;
                else
                    expiration = 0;

                var tasks = new task[recipients.Count];
                for (int i = 0; i < recipients.Count; i++)
                {
                    tasks[i] = new task
                    {
                        id = "task_" + i + "_" + nr, // Any identifier you'd like
                        subjectref = "sub_" + i, // Any identifier you'd like
                        bundle = false,
                        bundleSpecified = true,
                        daystolive = signingInfo.daysToLive,
                        documentaction = documentactions,
                        signature = signatures,
                        subject = new subject
                        {
                            id = "sub_" + i,
                            email = recipients[i].email,
                            mobile = "2222222"//recipients[i].mobile
                            //nationalid = recipients[i].ssn //National id mustbe inserted
                        },
                        notification = new[]
                        {
                            new notification
                            {
                                header = "Message from Signicat",
                                message = "Documents are waiting for you to sign, you have 2 days left! Please find the documents in the original email.",
                                notificationid = "req_exp_" + i,
                                recipient = recipients[i].email,
                                sender = "noreply@signicat.com",
                                type = notificationtype.EMAIL,
                                schedule = new []
                                {
                                    new schedule
                                    {
                                        stateis = taskstatus.created,
                                        waituntil = DateTime.Now.AddDays(expiration),
                                        waituntilSpecified = true
                                    }
                                }
                            },
                            new notification
                            {
                                header = "Message from Signicat",
                                message = "Document signing has been rejected!",
                                notificationid = "req_rej_" + i,
                                recipient = recipients[i].email,
                                sender = "noreply@signicat.com",
                                type = notificationtype.EMAIL,
                                schedule = new schedule[]
                                {
                                    new schedule
                                    {
                                        stateis = taskstatus.rejected,
                                    }
                                }
                            }
                        }
                    };
                }
                return tasks;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private signature[] GetSignatures(string signingInfoSigningMetodText)
        {
            if (signingInfoSigningMetodText != "nbid")
            {
                var signature = new[]
                {
                    new signature
                    {
                        method = new string[]
                        {
                            // The name of the signature method(s)                                            
                            signingInfoSigningMetodText
                        }
                    }
                };
                return signature;
            }


            if (signingInfoSigningMetodText == "nbid")
            {
                var signature = new[]
                {
                    new signature
                    {
                        method = new string[]
                        {
                            // The name of the signature method(s)                                            
                            signingInfoSigningMetodText,
                            "nbid-mobil"
                        }
                    }
                };
                return signature;
            }

            return null;
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