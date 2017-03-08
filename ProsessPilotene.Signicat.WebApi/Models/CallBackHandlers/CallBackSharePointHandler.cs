using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security;
using System.Web;
using Microsoft.SharePoint.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using PP.Signicat.WebApi.Models.CRMCredentials;
using File = System.IO.File;

namespace PP.Signicat.WebApi.Models.CallBackHandlers
{
    internal class CallBackSharePointHandler
    {
        private string folderRelativeUrl = "";
        private Entity SharePointSite;
        private int loop = 0;

        /// <summary>
        /// Saving documents into sharepoint online
        /// </summary>
        /// <param name="regardingRef"></param>
        /// <param name="resulturi"></param>
        /// <param name="docName"></param>
        /// <param name="service"></param>
        internal void SaveInSP(EntityReference regardingRef, string resulturi, string docName, string orgname, int lcid, IOrganizationService service)
        {
            try
            {
                folderRelativeUrl = "";
                SharePointSite = null;
                var crmconfig = new CallBackCrmHandler().GetCRMConfig(service);
                var spUsername = crmconfig.SPUser;
                var spPassword = crmconfig.SPpassword;

                if (spPassword == null || spPassword == null)
                {
                    var customerCredentials = new CustomerCredentialsHandler().GetCredentials(orgname);
                    spUsername = customerCredentials.username;
                    spPassword = customerCredentials.password;
                }

                var pdfBytes = new CallBackSignicatHandler().ReadAsyncFile(resulturi);
                var currentSharePointFolderEntity = GetDocumentLocation(regardingRef, "regardingobjectid", service);
                var sharePointUrl = GetDefaultSPSiteUrlFromCRMSiteCollectionEntity(service);

                HandleRelativeUrl(currentSharePointFolderEntity, service);

                var urlSplit = folderRelativeUrl.Split('/').ToList();
                var listTitle = urlSplit.First();
                var folderName = urlSplit[1];
                docName = CleanName(docName);

                using (ClientContext clientContext = new ClientContext(sharePointUrl))
                {
                    SecureString passWord = new SecureString();
                    foreach (char c in spPassword.ToCharArray()) passWord.AppendChar(c);
                    clientContext.Credentials = new SharePointOnlineCredentials(spUsername, passWord);
                    Web web = clientContext.Web;
                    clientContext.Load(web);
                    clientContext.Load(web.Lists);
                    clientContext.ExecuteQuery();

                    MemoryStream mStream = new MemoryStream();
                    mStream.Write(pdfBytes.Result.file, 0, pdfBytes.Result.file.Length);

                    FileCreationInformation flciNewFile = new FileCreationInformation();
                    //using ContentStream property for large files
                    mStream.Seek(0, SeekOrigin.Begin);
                    flciNewFile.ContentStream = mStream;
                    flciNewFile.Url = sharePointUrl + "/" + folderRelativeUrl + docName + ".pdf";
                    flciNewFile.Overwrite = true;

                    List accountList = GetAccountList(listTitle, web, clientContext, crmconfig, service);
                    if (accountList == null)
                        return;

                    var folder = accountList.RootFolder.Folders.GetByUrl(folderName);

                    var uploadFile = folder.Files.Add(flciNewFile);

                    clientContext.Load(uploadFile);
                    clientContext.ExecuteQuery();

                }
            }
            catch (Exception ex)
            {
                return;
                throw new Exception(ex.Message);
            }
        }

        private string CleanName(string docName)
        {
            var mychar = new char[] {'"'};
            if (docName.Contains("\""))
            {
                docName = docName.Replace(@"\", "");
                docName = docName.TrimStart(mychar);
                return docName;
            }
            return docName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listTitle"></param>
        /// <param name="web"></param>
        /// <param name="clientContext"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        private List GetAccountList(string listTitle, Web web, ClientContext clientContext, CRMConfig crmconfig,
            IOrganizationService service)
        {
            try
            {
                List accountList;

                clientContext.Load(web.Lists, lists => lists.Include(list => list.Title).Where(list => list.Title == listTitle));
                clientContext.ExecuteQuery();

                if (web.Lists.Count > 0)
                {
                    accountList = web.Lists.GetByTitle(listTitle);
                    clientContext.Load(accountList);
                    clientContext.ExecuteQuery();
                }
                else
                {
                    if (loop > 1)
                        return null;

                    loop++;
                    listTitle = GetListTitle(crmconfig, listTitle);
                    accountList = GetAccountList(listTitle, web, clientContext, crmconfig, service);
                }

                return accountList;
            }
            catch (Exception ex)
            {
                return null;
                throw new Exception(ex.Message);
            }
        }

        private string GetListTitle(CRMConfig crmconfig, string listTitle)
        {
            try
            {
                switch (listTitle)
                {
                    case "account":
                        return crmconfig.SpAccountRoot;
                    case "opportunity":
                        return crmconfig.SpOpportunityRoot;
                    case "salesorder":
                        return crmconfig.SpOrderRoot;
                    case "incident":
                        return crmconfig.SpIncidentRoot;
                    case "quote":
                        return crmconfig.SpQuoteRoot;
                    case "contract":
                        return crmconfig.SpContractRoot;
                }
                return listTitle;
            }
            catch (Exception ex)
            {
                return listTitle;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentLibrary"></param>
        /// <param name="service"></param>
        private void HandleRelativeUrl(Entity parentLibrary, IOrganizationService service)
        {
            if (
                !string.Compare(parentLibrary.LogicalName, "sharepointsite", StringComparison.CurrentCultureIgnoreCase)
                    .Equals(0))
            {
                var parentFolder = parentLibrary.Attributes["relativeurl"] as string;
                folderRelativeUrl = parentFolder + "/" + folderRelativeUrl;
            }

            var parentLocationRef = parentLibrary.Attributes["parentsiteorlocation"] as EntityReference;
            var parentLoc = GetDocumentLocation(parentLocationRef, "sharepointdocumentlocationid", service);
            if (parentLoc != null)
                HandleRelativeUrl(parentLoc, service);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="regardingRef"></param>
        /// <param name="searchField"></param>
        /// <param name="crm"></param>
        /// <returns>The sharepoint document location with regardingid</returns>
        private Entity GetDocumentLocation(EntityReference regardingRef, string searchField, IOrganizationService crm)
        {
            OrganizationServiceContext orgContext = new OrganizationServiceContext(crm);

            var documents = from doc in orgContext.CreateQuery("sharepointdocumentlocation")

                            where ((EntityReference)doc[searchField]).Id.Equals(regardingRef.Id)

                            select doc;
            var document = documents.FirstOrDefault();
            //if (document == null)
            //    document = CreateNewDocumentLocation(regardingRef, crm);

            return document;
        }

        private Entity CreateNewDocumentLocation(EntityReference regardingRef, IOrganizationService crm)
        {
            var spDocLoc = new Entity("sharepointdocumentlocation");
            spDocLoc["name"] = "SharePoint Document Location";
            spDocLoc["description"] = "SharePoint Document Location record";

            // Set the Sample SharePoint Site created earlier as the parent site.
            spDocLoc["ParentSiteOrLocation"] = new EntityReference(SharePointSite.LogicalName, SharePointSite.Id);
            spDocLoc["RelativeUrl"] = "spdocloc";

            // Associate this document location instance with the Fourth Coffee
            // sample account record.
            spDocLoc["RegardingObjectId"] = new EntityReference(regardingRef.LogicalName, regardingRef.Id);

            var doLocId = crm.Create(spDocLoc);
            spDocLoc.Id = doLocId;
            return spDocLoc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="crm"></param>
        /// <returns></returns>
        internal string GetDefaultSPSiteUrlFromCRMSiteCollectionEntity(IOrganizationService crm)
        {
            try
            {
                ConditionExpression c = new ConditionExpression("isdefault", ConditionOperator.Equal, true);
                FilterExpression f = new FilterExpression(LogicalOperator.And);
                f.Conditions.Add(c);

                QueryExpression q = new QueryExpression("sharepointsite");
                q.Criteria = f;
                q.ColumnSet = new ColumnSet("sharepointsiteid", "name", "absoluteurl", "relativeurl", "isdefault",
                    "parentsite");

                EntityCollection crmSites = crm.RetrieveMultiple(q);
                if (crmSites.Entities.Count > 0)
                {
                    Entity defaultSharePointSite = crmSites.Entities[0];
                    SharePointSite = defaultSharePointSite;
                    if (defaultSharePointSite.Attributes.Contains("parentsite") &&
                        defaultSharePointSite.Attributes.Contains("relativeurl"))
                    {
                        Entity parentSiteOfDefaultSite = crm.Retrieve("sharepointsite",
                            ((EntityReference)defaultSharePointSite["parentsite"]).Id, new ColumnSet("absoluteurl"));
                        return (string)parentSiteOfDefaultSite["absoluteurl"] + "/" +
                               defaultSharePointSite.GetAttributeValue<string>("relativeurl");
                    }
                    else
                    {
                        return defaultSharePointSite.GetAttributeValue<string>("absoluteurl");
                    }
                }
                // no SharePoint Sites defined in CRM
                //Console.WriteLine("CRM does not have any default SharePoint Sites", "Getting data from CRM");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return null;
        }
    }
}