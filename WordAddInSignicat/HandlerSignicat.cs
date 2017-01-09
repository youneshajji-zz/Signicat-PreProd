using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Word;
using WordAddInSignicat.SignicatPreProdService;
using Microsoft.Xrm.Sdk;

namespace WordAddInSignicat
{
    public class HandlerSignicat
    {
        public async Task<HttpResponseMessage> SendRequest(Document doc, string pdfName, string orgName, IOrganizationService crm)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    using (var content = new MultipartFormDataContent())
                    {
                        var apiurl = HandlerCRM.GetSettingKeyValue(crm, "webapiurl");
                        client.BaseAddress = new Uri(apiurl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/pdf"));

                        StreamReader sr = new StreamReader(doc.Path + "\\" + pdfName);
                        var fileContent = new ByteArrayContent(Helpers.ReadToEnd(sr.BaseStream));
                        var param = new NameValueHeaderValue("orgname", orgName);
                        fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = pdfName,
                            Parameters = { param }
                        };
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                        content.Add(fileContent);

                        var response = client.PostAsync("api/SignRequest/PostWord", content).Result;
                        sr.Close();
                        return response;
                    }
                }
            }

            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.ExpectationFailed);
            }
        }
    }
}
