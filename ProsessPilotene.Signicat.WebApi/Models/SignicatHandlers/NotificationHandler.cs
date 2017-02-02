using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PP.Signicat.WebApi.SignicatPreProd;

namespace PP.Signicat.WebApi.Models.SignicatHandlers
{
    internal class NotificationHandler
    {
        internal notification[] AddNotifications(ContactInfo recipeint, SigningInfo signingInfo, int i)
        {
            var header = Resources.Resourceeng.signicatmessage;
            var message1 = Resources.Resourceeng.signicatexpiration;
            var message2 = Resources.Resourceeng.signicatrejected;
            var message3 = Resources.Resourceeng.signicatexpired;

            if (signingInfo.LCID == 1044)
            {
                header = Resources.Resourcenb.signicatmessage;
                message1 = Resources.Resourcenb.signicatexpiration;
                message2 = Resources.Resourcenb.signicatrejected;
                message3 = Resources.Resourcenb.signicatexpired;
            }


            var callbackNotificationUrl = "https://prosesspilotenesignicatwebapi-preprod.azurewebsites.net:443/api/Callback/GetSigning?orgname=" + signingInfo.customerOrg;
            var callbackExpUrl = "https://prosesspilotenesignicatwebapi-preprod.azurewebsites.net:443/api/Callback/DeactivateSigning?orgname=" + signingInfo.customerOrg;

            var expiration = 0;
            if (signingInfo.daysToLive > 2)
                expiration = signingInfo.daysToLive - 2;
            else
                expiration = 0;

            return new[]
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
                            },
                new notification
                            {
                                notificationid = "req_expurl_" + i,
                                recipient = callbackExpUrl,
                                message = "callbackurl",
                                type = notificationtype.URL,
                                schedule = new[]
                                {
                                    new schedule
                                    {
                                        stateis = taskstatus.expired
                                    }
                                }
                            },
                new notification
                            {
                                header = header,
                                message = message1,
                                notificationid = "req_exp_" + i,
                                recipient = recipeint.email,
                                sender = "noreply@signicat.com",
                                type = notificationtype.EMAIL,
                                schedule = new[]
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
                                header = header,
                                message = message2,
                                notificationid = "req_rej_" + i,
                                recipient = signingInfo.senderMail,
                                sender = "noreply@signicat.com",
                                type = notificationtype.EMAIL,
                                schedule = new schedule[]
                                {
                                    new schedule
                                    {
                                        stateis = taskstatus.rejected,
                                    }
                                }
                            },
                new notification
                            {
                                header = header,
                                message = message3,
                                notificationid = "req_expd_" + i,
                                recipient = signingInfo.senderMail,
                                sender = "noreply@signicat.com",
                                type = notificationtype.EMAIL,
                                schedule = new schedule[]
                                {
                                    new schedule
                                    {
                                        stateis = taskstatus.expired,
                                    }
                                }
                            }
            };
        }

        internal void AddSmsNotification(createrequestresponse response, createrequestrequest request, SigningInfo signingInfo, int i, string url, string phonenr)
        {
            using (var client = new DocumentEndPointClient())
            {

                var smsnotify = new notification
                {
                    notificationid = "send_sms_" + i,
                    type = notificationtype.SMS,
                    recipient = phonenr,
                    message = signingInfo.SMSText + " " + url
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

        internal task[] AddNotifyMe(SigningInfo signingInfo, task[] tasks)
        {
            try
            {
                var header = Resources.Resourceeng.signicatmessage;
                var message = Resources.Resourceeng.signicatsigned;

                if (signingInfo.LCID == 1044)
                {
                    header = Resources.Resourcenb.signicatmessage;
                    message = Resources.Resourcenb.signicatsigned;
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
    }
}