using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Xrm.Sdk;
using PP.Signicat.WebApi.Models.SignicatHandlers;

namespace PP.Signicat.WebApi.Models
{
    internal class Helpers
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public byte[] ReadToEnd(System.IO.Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        /// <summary>
        /// Determins regarding wich entity is the documentsigning belongs to.
        /// </summary>
        /// <param name="documentsigning"></param>
        /// <param name="service"></param>
        /// <returns>EntityReference of the regading field.</returns>
        internal EntityReference GetRegarding(Entity documentsigning, IOrganizationService service)
        {
            EntityReference valueRef = null;

            if (documentsigning.Contains("pp_accountid"))
                valueRef = documentsigning.Attributes["pp_accountid"] as EntityReference;
            if (documentsigning.Contains("pp_opportunityid"))
                valueRef = documentsigning.Attributes["pp_opportunityid"] as EntityReference;
            if (documentsigning.Contains("pp_salesorderid"))
                valueRef = documentsigning.Attributes["pp_salesorderid"] as EntityReference;
            if (documentsigning.Contains("pp_quoteid"))
                valueRef = documentsigning.Attributes["pp_quoteid"] as EntityReference;
            if (documentsigning.Contains("pp_incidentid"))
                valueRef = documentsigning.Attributes["pp_incidentid"] as EntityReference;
            if (documentsigning.Contains("pp_contractid"))
                valueRef = documentsigning.Attributes["pp_contractid"] as EntityReference;

            return valueRef;
        }

        /// <summary>
        /// Strips the url and gets the documentname at the 8th prosition.
        /// </summary>
        /// <param name="signicatUrl"></param>
        /// <returns>The name of the document.</returns>
        internal string GetNameFromUrl(string signicatUrl)
        {
            string[] splicedUrl = signicatUrl.Split('/');
            var name = "";
            if (splicedUrl.Count() > 0)
                name = splicedUrl[8];
            return name;
        }

        public static SigningInfo GetSignInfo(HttpRequest currentRequest)
        {
            var signingInfo = new SigningInfo();
            signingInfo.customerOrg = currentRequest.Params["CustomerOrg"];
            signingInfo.authMetod = Convert.ToInt32(currentRequest.Params["Authmetod"]);
            signingInfo.notifyMe = Convert.ToBoolean(currentRequest.Params["NotifyMe"]);
            signingInfo.senderMail = currentRequest.Params["SenderEmail"];
            signingInfo.LCID = Convert.ToInt32(currentRequest.Params["lcid"]);
            signingInfo.signMethod = Convert.ToInt32(currentRequest.Params["SigningMetod"]);
            signingInfo.SendSMS = Convert.ToBoolean(currentRequest.Params["SendSMS"]);
            signingInfo.SMSText = currentRequest.Params["SMSText"];
            signingInfo.isInk = Convert.ToBoolean(currentRequest.Params["Ink"]);
            signingInfo.signingMetodText = "ink";

            if (!string.IsNullOrWhiteSpace(currentRequest.Params["Daystolive"]))
                signingInfo.daysToLive = Convert.ToInt32(currentRequest.Params["Daystolive"]);
            else
                signingInfo.daysToLive = 60;

            return signingInfo;
        }

        public static string HandleFileName(string docName)
        {
            //"\"Orderd Varelinjer.pdf\""
            char[] MyChar = { '\"' };
            if (docName.Contains("\""))
            {
                docName = docName.TrimStart(MyChar);
                docName = docName.TrimEnd(MyChar);
            }
            return docName;
        }
    }

    public class ResultObject
    {
        public Guid Id { get; set; }
        public string name { get; set; }
        public string rquestid { get; set; }
        public string sdsurl { get; set; }
        public string resulturl { get; set; }
        public string padesurl { get; set; }
        public Guid taskId { get; set; }
    }

    public class FileObj
    {
        public string filename { get; set; }
        public byte[] file { get; set; }
    }
}