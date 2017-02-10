using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using PP.Signicat.WebApi.SignicatPreProdPkg;
using PP.Signicat.WebApi.SignicatPreProd;

namespace PP.Signicat.WebApi.Models.CallBackHandlers
{
    internal class CallBackSignicatHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        internal taskstatusinfo GetSignicatTaskInfo(string requestId, string taskId)
        {
            try
            {
                var taskstatusinfo = GetStatus(requestId);
                var result = taskstatusinfo.Where(item => item.taskid == taskId).SingleOrDefault();
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
        /// <param name="tempList"></param>
        /// <returns></returns>
        internal string CreatePades(List<ResultObject> tempList, string resulturl)
        {
            try
            {
                using (var client = new PackagingEndPointClient())
                {
                    var request = new createpackagerequest();

                    if (tempList != null)
                    {
                        var docids = new documentid[tempList.Count];
                        for (int i = 0; i < tempList.Count; i++)
                        {
                            if (tempList[i].resulturl.Contains("sds")) //If it contains sds then it is allready a merged pades
                                return null;

                            docids[i] = new documentid
                            {
                                uridocumentid = tempList[i].resulturl
                            };
                        }

                        request = new createpackagerequest
                        {
                            service = "prosesspilotene",
                            password = "Bond007",
                            version = "4",
                            packagingmethod = "pades",
                            validationpolicy = "ltvsdo-validator",
                            Items = docids,
                            sendresulttoarchive = false
                        };
                    }

                    if (resulturl != null)
                    {
                        if (resulturl.Contains("sds")) //If it contains sds then it is allready a merged pades
                            return null;

                        request = new createpackagerequest
                        {
                            service = "prosesspilotene",
                            password = "Bond007",
                            version = "4",
                            packagingmethod = "pades",
                            validationpolicy = "ltvsdo-validator",
                            Items = new documentid[]
                            {
                                new documentid
                                {
                                    uridocumentid = resulturl
                                }
                            },
                            sendresulttoarchive = false
                        };
                    }


                    var createPackageResponse = client.createpackage(request);

                    if (createPackageResponse.id == null)
                        return null;

                    string padesDocumentId = createPackageResponse.id;
                    string padesDownloadUrl = "https://preprod.signicat.com/doc/prosesspilotene/sds/" + padesDocumentId;
                    // if you set sendresulttoarchive=true, the url must also be updated:
                    //string padesDownloadUrl = "https://preprod.signicat.com/doc/prosesspilotene/archive/" + padesDocumentId;

                    return padesDownloadUrl;
                }

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        internal taskstatusinfo[] GetStatus(string requestId)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resulturl"></param>
        /// <returns></returns>
        internal async Task<FileObj> ReadAsyncFile(string resulturl)
        {
            var httpClientHandler = new HttpClientHandler { Credentials = new NetworkCredential("prosesspilotene", "Bond007") };
            using (var client = new HttpClient(httpClientHandler))
            {
                HttpResponseMessage response = await client.GetAsync(resulturl, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
                byte[] pdf = await response.Content.ReadAsByteArrayAsync();

                var fileObj = new FileObj();
                fileObj.file = pdf;

                return fileObj;
            }
        }
    }
}