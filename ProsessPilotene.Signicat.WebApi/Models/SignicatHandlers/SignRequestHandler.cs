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

                //if (signingInfo.notifyMe == 1)
                //    tasks = AddNotifyMe(signingInfo, tasks);

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

        private task[] AddNotifyMe(SigningInfo signingInfo, task[] tasks)
        {
            try
            {
                var message = Resources.Resourceeng.signicatmessage;
                var header = Resources.Resourceeng.signicatsigned;

                if (signingInfo.LCID == 1044)
                {
                    message = Resources.Resourcenb.signicatmessage;
                    header = Resources.Resourcenb.signicatsigned;
                }

                for (int i = 0; i < tasks.Length; i++)
                {
                    var notifyme = new notification
                    {
                        header = header,
                        message = message,
                        notificationid = "req_com_" + i,
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
                var message = Resources.Resourceeng.signicatmessage;
                var header1 = Resources.Resourceeng.signicatexpiration;
                var header2 = Resources.Resourceeng.signicatrejected;

                if (signingInfo.LCID == 1044)
                {
                    message = Resources.Resourcenb.signicatmessage;
                    header1 = Resources.Resourcenb.signicatexpiration;
                    header2 = Resources.Resourcenb.signicatrejected;
                }

                var signatures = GetSignatures(signingInfo.signingMetodText);
                var authSignatures = GetAuthSignatures(signingInfo.signingMetodText);
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
                        bundle = true,
                        //bundleSpecified = true,
                        daystolive = signingInfo.daysToLive,
                        documentaction = documentactions,
                        signature = signatures,
                        authenticationbasedsignature = authSignatures,
                        subject = new subject
                        {
                            id = "sub_" + i,
                            email = recipients[i].email,
                            mobile = !string.IsNullOrWhiteSpace(recipients[i].mobile) ? recipients[i].mobile : ""
                            //nationalid = recipients[i].ssn //National id mustbe inserted
                        },
                        notification = new[]
                        {
                            new notification
                            {
                                    header = Resources.Resourcenb.signicatsigned,
                                    message = message,
                                    notificationid = "req_com_" + i,
                                    recipient = signingInfo.senderMail,
                                    sender = "noreply@signicat.com",
                                    type = notificationtype.EMAIL,
                                    schedule = new []
                                {
                                    new schedule
                                    {
                                        stateis = taskstatus.completed,
                                    }
                                }
                            },
                            new notification
                            {
                                header = message,
                                message = header1,
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
                                header = message,
                                message = header2,
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

        private authenticationbasedsignature[] GetAuthSignatures(string signingInfoSigningMetodText)
        {
            if (signingInfoSigningMetodText != "nbid" && signingInfoSigningMetodText != "handwritten")
            {
                var authsignature = new[]
                {
                    new authenticationbasedsignature()
                    {
                                        method = new method[]
                                        {
                                            new method
                                            {
                                                handwritten = true,
                                               Value = signingInfoSigningMetodText
                                            }
                                        }
                    }
                };
                return authsignature;
            }

            ////if (signingInfoSigningMetodText == "nbid")
            ////{
            ////    var authsignature = new[]
            ////    {
            ////        new authenticationbasedsignature
            ////        {
            ////                            method = new method[]
            ////                            {
            ////                                new method
            ////                                {
            ////                                    handwritten = true,
            ////                                   Value = "nbid"
            ////                                },
            ////                                new method
            ////                                {
            ////                                   Value = "nbid-mobil"
            ////                                }
            ////                            }
            ////        }
            ////    };
            //    return authsignature;
            //}

            return null;
        }

        private signature[] GetSignatures(string signingInfoSigningMetodText)
        {
            if (signingInfoSigningMetodText == "nbid")
            {
                var signature = new[]
                {
                    new signature
                    {
                        responsive = true,
                                        method = new []
                                        {
                                            new method
                                            {
                                               Value = "nbid-sign"
                                            },
                                            new method
                                            {
                                               Value = "nbid-mobil-sign"
                                            }
                                        }
                    }
                };
                return signature;
            }

            if (signingInfoSigningMetodText == "handwritten")
            {
                var signature = new[]
                {
                    new signature
                    {
                        responsive = true,
                                        method = new method[]
                                        {
                                            new method
                                            {
                                               handwritten = true
                                            }
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