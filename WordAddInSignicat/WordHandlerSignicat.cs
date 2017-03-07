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
    public class WordHandlerSignicat
    {
        public async Task<HttpResponseMessage> SendRequest(WordSearchObject searchValues, Document doc, string pdfName,
            string orgName, WordCRMConfig crmconfig, IOrganizationService crm)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    using (var content = new MultipartFormDataContent())
                    {
                        var apiurl = crmconfig.Webapiurl;
                        client.BaseAddress = new Uri(apiurl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/pdf"));

                        StreamReader sr = new StreamReader(doc.Path + "\\" + pdfName);
                        var fileContent = new ByteArrayContent(WordHelpers.ReadToEnd(sr.BaseStream));
                        var orgname = new NameValueHeaderValue("orgname", orgName);
                        var language = new NameValueHeaderValue("language", searchValues.language.ToString());
                        var method = new NameValueHeaderValue("method", crmconfig.Wordsigningmethod.ToString());
                        //pdfName = Convert.ToBase64String(Encoding.UTF8.GetBytes(pdfName));

                        fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = pdfName,
                            Parameters = { orgname, language, method }
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
