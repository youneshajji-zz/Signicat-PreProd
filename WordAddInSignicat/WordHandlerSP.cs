using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Word;
using Microsoft.SharePoint.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using File = System.IO.File;
using List = Microsoft.Office.Interop.Word.List;

namespace WordAddInSignicat
{
    public static class WordHandlerSP
    {
        private static string folderRelativeUrl = "";
        public static void SendDocumentToSP(WordSearchObject searchValues)
        {
            try
            {
                SaveInSP(searchValues);
            }
            catch (Exception ex)
            {
                if (searchValues.language == 1044)
                    MessageBox.Show(Resources.ResourceWordNb.cannotsavetosp + ex.Message, Resources.ResourceWordNb.docsp,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show(Resources.ResourceWordNb.cannotsavetosp + ex.Message, Resources.ResourceWordEn.docsp,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private static void SaveInSP(WordSearchObject searchValues)
        {
            try
            {
                var crm = WordConnectToCrm.ConnectToMSCRM();
                if (crm == null)
                    return;

                var entity = WordHandlerCRM.FindRecordByNumber(searchValues, crm);
                if (entity == null)
                {
                    if (searchValues.language == 1044)
                        MessageBox.Show(Resources.ResourceWordNb.recordnotfound + searchValues.searchnumber + ".",
                            Resources.ResourceWordNb.docsp, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    else
                        MessageBox.Show(Resources.ResourceWordEn.recordnotfound + searchValues.searchnumber + ".",
               Resources.ResourceWordEn.docsp, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                Save(entity, searchValues, crm);
            }

            catch (Exception ex)
            {
                if (searchValues.language == 1044)
                    MessageBox.Show(Resources.ResourceWordNb.cannotsavetosp, Resources.ResourceWordNb.docsp, MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                else
                    MessageBox.Show(Resources.ResourceWordEn.cannotsavetosp, Resources.ResourceWordEn.docsp, MessageBoxButtons.OK,
               MessageBoxIcon.Warning);
            }

        }

        private static void Save(Entity Entity, WordSearchObject searchValues, IOrganizationService service)
        {
            try
            {
                var spUsername = ""; //WordHandlerCRM.GetSettingKeyValue(service, "spuser");
                var spPassword = ""; //WordHandlerCRM.GetSettingKeyValue(service, "sppassword");
                var currentSharePointFolderEntity = GetDocumentLocation(Entity.ToEntityReference(), "regardingobjectid", service);
                var sharePointUrl = GetDefaultSPSiteUrlFromCRMSiteCollectionEntity(searchValues, service);
                Document document = Globals.ThisAddIn.Application.ActiveDocument;

                var fullname = document.Name.Split('.');
                var pdfName = fullname[0] + ".pdf";
                var fullPath = document.Path + "\\" + pdfName;

                document.ExportAsFixedFormat(
                 Path.Combine(document.Path, pdfName),
                 WdExportFormat.wdExportFormatPDF,
                 OpenAfterExport: false);

                HandleRelativeUrl(currentSharePointFolderEntity, service);

                var urlSplit = folderRelativeUrl.Split('/').ToList();
                var listTitle = urlSplit.First();
                var folderName = urlSplit[1];

                using (ClientContext clientContext = new ClientContext(sharePointUrl))
                {
                    SecureString passWord = new SecureString();
                    foreach (char c in spPassword.ToCharArray()) passWord.AppendChar(c);
                    clientContext.Credentials = new SharePointOnlineCredentials(spUsername, passWord);
                    Web web = clientContext.Web;
                    clientContext.Load(web);
                    clientContext.Load(web.Lists);
                    clientContext.ExecuteQuery();

                    var accountList = web.Lists.GetByTitle(listTitle);
                    clientContext.Load(accountList);
                    clientContext.ExecuteQuery();


                    var folder = accountList.RootFolder.Folders.GetByUrl(folderName);

                    FileCreationInformation newFile = new FileCreationInformation();
                    newFile.Content = File.ReadAllBytes(document.Path + "\\" + pdfName);
                    newFile.Url = sharePointUrl + "/" + folderRelativeUrl + pdfName;
                    newFile.Overwrite = true;

                    var uploadFile = folder.Files.Add(newFile);
                    clientContext.ExecuteQuery();

                    if (searchValues.language == 1044)
                        MessageBox.Show(Resources.ResourceWordNb.docsavedsp, Resources.ResourceWordNb.docsp,
                        MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    else
                        MessageBox.Show(Resources.ResourceWordEn.docsavedsp, Resources.ResourceWordEn.docsp,
           MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }

                File.Delete(fullPath);
            }
            catch (Exception ex)
            {
                if (searchValues.language == 1044)
                    MessageBox.Show(Resources.ResourceWordNb.cannotfindsetting + ex.Message, Resources.ResourceWordNb.datafromcrm,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show(Resources.ResourceWordEn.cannotfindsetting + ex.Message, Resources.ResourceWordEn.datafromcrm,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void HandleRelativeUrl(Entity parentLibrary, IOrganizationService service)
        {
            if (!string.Compare(parentLibrary.LogicalName, "sharepointsite", StringComparison.CurrentCultureIgnoreCase).Equals(0))
                folderRelativeUrl = parentLibrary.Attributes["relativeurl"] as string + "/" + folderRelativeUrl;

            var parentLocationRef = parentLibrary.Attributes["parentsiteorlocation"] as EntityReference;
            var parentLoc = GetDocumentLocation(parentLocationRef, "sharepointdocumentlocationid", service);
            if (parentLoc != null)
                HandleRelativeUrl(parentLoc, service);
        }


        private static Entity GetDocumentLocation(EntityReference regardingRef, string searchField, IOrganizationService crm)
        {
            OrganizationServiceContext orgContext = new OrganizationServiceContext(crm);

            var documents = from doc in orgContext.CreateQuery("sharepointdocumentlocation")

                            where ((EntityReference)doc[searchField]).Id.Equals(regardingRef.Id)

                            select doc;
            var document = documents.FirstOrDefault();
            return document;
        }

        internal static string GetDefaultSPSiteUrlFromCRMSiteCollectionEntity(WordSearchObject searchValues, IOrganizationService crm)
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
                    if (defaultSharePointSite.Attributes.Contains("parentsite") &&
                        defaultSharePointSite.Attributes.Contains("relativeurl"))
                    {
                        Entity parentSiteOfDefaultSite = crm.Retrieve("sharepointsite",
                            ((EntityReference)defaultSharePointSite["parentsite"]).Id, new ColumnSet(true));
                        return (string)parentSiteOfDefaultSite["absoluteurl"] + "/" +
                               defaultSharePointSite.GetAttributeValue<string>("relativeurl");
                    }
                    else
                    {
                        return defaultSharePointSite.GetAttributeValue<string>("absoluteurl");
                    }
                }
                // no SharePoint Sites defined in CRM
                if (searchValues.language == 1044)
                    MessageBox.Show(Resources.ResourceWordNb.spsitenotfound, Resources.ResourceWordNb.datafromcrm,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show(Resources.ResourceWordEn.spsitenotfound, Resources.ResourceWordEn.datafromcrm,
               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("CrmMethods -> GetDefaultSPSite (" + ex.Message + ")", "Getting data from CRM",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            return null;
        }

    }
}
