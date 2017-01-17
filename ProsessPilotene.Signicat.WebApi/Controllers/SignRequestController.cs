using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http.Headers;
using System.Web;
using PP.Signicat.WebApi.Models;
using System.Web.Http.Cors;
using Microsoft.Xrm.Sdk.Messages;
using PP.Signicat.WebApi.Models.SignicatHandlers;
using PP.Signicat.WebApi.SignicatPreProd;

namespace PP.Signicat.WebApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class SignRequestController : ApiController
    {
        //GET: api/SignRequest/5 getStatus
        [HttpGet]
        public taskstatusinfo Get(string requestId, string taskId)
        {
            var signHandler = new SignRequestHandler();
            var taskstatusinfo = signHandler.getStatus(requestId);
            var result = taskstatusinfo.Where(item => item.taskid == taskId).SingleOrDefault();
            return result;
        }

        //GET: api/SignRequest/5 get file
        [HttpGet]
        public async Task<byte[]> Get(bool downloadpdf, string url)
        {
            var httpClientHandler = new HttpClientHandler { Credentials = new NetworkCredential("prosesspilotene", "Bond007") };
            using (var client = new HttpClient(httpClientHandler))
            {
                HttpResponseMessage response = await client.GetAsync(url);
                byte[] pdf = await response.Content.ReadAsByteArrayAsync();
                return pdf;
            }
        }

        // POST: api/SignRequest/PostWord
        [ActionName("PostWord")]
        [HttpPost]
        public async Task<List<string>> PostWord()
        {
            var signHereUrlList = new List<string>();

            HttpRequestMessage request1 = this.Request;
            if (request1.Content.IsMimeMultipartContent())
            {
                var wordHandler = new WordSignRequest();
                var provider = new MultipartMemoryStreamProvider();

                await Request.Content.ReadAsMultipartAsync(provider);

                foreach (var item in provider.Contents)
                {
                    var param = item.Headers.ContentDisposition.Parameters;
                    var orgname = param.First(x => x.Name == "orgname").Value;
                    var docName = item.Headers.ContentDisposition.FileName;
                    HttpPostedFile postedFile = HttpContext.Current.Request.Files[docName];

                    if (postedFile != null)
                    {
                        //string orgname = "processpilots";
                        sdsdocument uploadedDocument = await wordHandler.UploadDocument(postedFile, item);
                        var url = wordHandler.CreateSignRequest(uploadedDocument, orgname);
                        signHereUrlList.Add(url);

                        return signHereUrlList;
                    }
                }
            }

            return null;
        }

        // POST: api/SignRequest/PostCRM
        [ActionName("PostCRM")]
        [HttpPost]
        public async Task<List<string>> Post()
        {
            var signHereUrlList = new List<string>();

            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var postedFiles = new List<HttpPostedFile>();
                // Get the uploaded image from the Files collection
                for (int i = 0; i < HttpContext.Current.Request.Files.Count; i++)
                {
                    postedFiles.Add(HttpContext.Current.Request.Files["file" + i]);
                }

                // Get the uploaded recipients
                var recipients = new List<ContactInfo>();
                var postedRecipientEmails = HttpContext.Current.Request.Params["RecipientEmails"];
                var postedRecipientMobiles = HttpContext.Current.Request.Params["RecipientMobiles"];
                //var postedRecipientSSN = HttpContext.Current.Request.Params["RecipientSSN"];

                if (postedRecipientEmails != null)
                {
                    var recipientEmailList = postedRecipientEmails.Split(',');
                    var recipientMobileList = postedRecipientMobiles.Split(',');
                    //var recipientSSNList = postedRecipientSSN.Split(',');

                    for (int i = 0; i < recipientEmailList.Length; i++)
                    {
                        if (recipientEmailList[i] == "null")
                            continue;

                        var contactInfo = new ContactInfo();
                        contactInfo.email = recipientEmailList[i];
                        contactInfo.mobile = recipientMobileList[i];
                        //contactInfo.ssn = recipientSSNList[i];
                        recipients.Add(contactInfo);
                    }
                }

                var signingInfo = new SigningInfo();
                var customerorg = HttpContext.Current.Request.Params["CustomerOrg"];
                signingInfo.authMetod = HttpContext.Current.Request.Params["Authmetod"];
                signingInfo.notifyMe = HttpContext.Current.Request.Params["NotifyMe"];
                signingInfo.senderMail = HttpContext.Current.Request.Params["SenderEmail"];
                var signingMetod = HttpContext.Current.Request.Params["SigningMetod"];
                //var description = HttpContext.Current.Request.Params["Description"];
                var daystolive = HttpContext.Current.Request.Params["Daystolive"];
                var sendSMS = HttpContext.Current.Request.Params["SendSMS"];
                signingInfo.signingMetodText = "nbid";

                if (daystolive == "")
                    signingInfo.daysToLive = 60;
                else
                    signingInfo.daysToLive = Convert.ToInt32(daystolive);

                if (postedFiles.Count > 0)
                {
                    var signHandler = new SignRequestHandler();
                    sdsdocument[] uploadedDocuments = await signHandler.UploadDocument(postedFiles);
                    if (uploadedDocuments == null)
                        return null;

                    if (signingMetod == "1") //BankID
                        signingInfo.signingMetodText = "nbid";

                    else if (signingMetod == "2") //SMS email OTP
                        signingInfo.signingMetodText = "scid-otp";

                    else if (signingMetod == "3") //Social
                        signingInfo.signingMetodText = "social";

                    createrequestrequest request = signHandler.GetCreateRequest(uploadedDocuments, recipients,
                        signingInfo); //easysign -- nbid-sign

                    ////For storing in Signicat archive
                    //((sdsdocument)request.request[0].document[0]).sendtoarchive = true;
                    //((sdsdocument)request.request[0].document[0]).sendtoarchiveSpecified = true;

                    //// Archive signed document
                    //request.request[0].task[0].documentaction[0].sendresulttoarchive = true;
                    //request.request[0].task[0].documentaction[0].sendresulttoarchiveSpecified = true;

                    // You can have request level notifications
                    // for example to notify your system when
                    // a request has been completed
                    for (int i = 0; i < request.request[0].task.Length; i++)
                    {
                        //var callbackOnTaskCompleteUrl = "https://prosesspilotenesignicatwebapi-preprod.azurewebsites.net:443/api/Callback/GetSigning?orgname=" + customerorg + "&requestId=${requestId}&taskId=${taskId}";
                        var callbackOnTaskCompleteUrl = "https://prosesspilotenesignicatwebapi-preprod.azurewebsites.net:443/api/Callback/Landingpage";
                        var callbackNotificationUrl = "https://prosesspilotenesignicatwebapi-preprod.azurewebsites.net:443/api/Callback/GetSigning?orgname=" + customerorg;
                        request.request[0].task[i].ontaskcomplete = callbackOnTaskCompleteUrl;

                        request.request[0].task[i].notification = new[]
                        {
                            new notification
                            {
                                notificationid = "req_callback_" + i,
                                type = notificationtype.URL,
                                recipient = callbackNotificationUrl,
                                message = "callbackurl",
                                schedule = new []
                                {
                                    new schedule
                                    {
                                        stateis = taskstatus.completed
                                    }
                                }
                            }
                        };
                    }


                    if (signingInfo.authMetod == "1") //BankID
                    {
                        //for (int i = 0; i < request.request[0].task.Length; i++)
                        //{
                        //    request.request[0].task[i].authentication = new authentication
                        //    {
                        //        method = new string[] { "nbid", "nbid-mobil" }
                        //    };
                        //}
                    }

                    if (signingInfo.authMetod == "2") //SMS Email OTP
                    {
                        for (int i = 0; i < request.request[0].task.Length; i++)
                        {
                            request.request[0].task[i].authentication = new authentication
                            {
                                method = new string[] { "scid-otp" }
                            };
                        }
                    }

                    if (signingInfo.authMetod == "3") //Social
                    {
                        for (int i = 0; i < request.request[0].task.Length; i++)
                        {
                            request.request[0].task[i].authentication = new authentication
                            {
                                method = new string[] { "social" }
                            };
                        }
                    }

                    //try
                    //{

                    createrequestresponse response;
                    using (var client = new DocumentEndPointClient())
                    {
                        response = client.createRequest(request);
                    }

                    for (int i = 0; i < request.request[0].task.Length; i++)
                    {
                        var url = String.Format(
                            "https://preprod.signicat.com/std/docaction/prosesspilotene?request_id={0}&task_id={1}",
                            response.requestid[0], request.request[0].task[i].id);
                        signHereUrlList.Add(url);

                        var phonenr = request.request[0].task[i].subject.mobile;

                        if (sendSMS == "yes" && !string.IsNullOrWhiteSpace(phonenr))
                        {
                            using (var client = new DocumentEndPointClient())
                            {
                                var smsnotify = new notification
                                {
                                    notificationid = "send_sms_" + i,
                                    type = notificationtype.SMS,
                                    recipient = phonenr,
                                    message = "Signer dette: " + url
                                };

                                var notifyReq = new addnotificationrequest
                                {
                                    service = "prosesspilotene",
                                    notification = smsnotify,
                                    password = "Bond007",
                                    requestid = response.requestid[0],
                                    taskid = request.request[0].task[i].id
                                };


                                client.addNotification(notifyReq);
                            }
                        }
                    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    throw new Exception(ex.Message);
                    //}
                }
            }

            return signHereUrlList;
        }
    }
}
