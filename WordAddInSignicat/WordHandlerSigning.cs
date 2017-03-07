using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Office.Interop.Word;
using Microsoft.Xrm.Sdk;
using Task = System.Threading.Tasks.Task;

namespace WordAddInSignicat
{
    public static class WordHandlerSigning
    {
        internal static async Task SendDocumentForSigning(WordSearchObject searchValues, WordCRMConfig crmconfig, IOrganizationService crm)
        {
            Document document = Globals.ThisAddIn.Application.ActiveDocument;

            try
            {
                var fullname = document.Name.Split('.');
                var pdfName = fullname[0] + ".pdf";
                var fullPath = document.Path + "\\" + pdfName;

                if (File.Exists(fullPath))
                    File.Delete(fullPath);

                document.ExportAsFixedFormat(
                    Path.Combine(document.Path, pdfName),
                    WdExportFormat.wdExportFormatPDF,
                    OpenAfterExport: false);

                var orgName = ConfigurationManager.AppSettings["OrgName"];

                var signicatHandler = new WordHandlerSignicat();
                var responseMessage = await signicatHandler.SendRequest(searchValues, document, pdfName, orgName,
                    crmconfig, crm);
                var stringResult = await responseMessage.Content.ReadAsStringAsync();
                var result = stringResult.Split('"');
                var sdsurl = result[1];

                var entity = WordHandlerCRM.FindRecordByNumber(searchValues, crm);
                if (entity == null)
                {
                    if (searchValues.language == 1044)
                        MessageBox.Show(Resources.ResourceWordNb.recordnotfound + searchValues.searchnumber + Resources.ResourceWordNb.unassociated, Resources.ResourceWordNb.documentsinging, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    else
                        MessageBox.Show(Resources.ResourceWordEn.recordnotfound + searchValues.searchnumber + Resources.ResourceWordEn.unassociated, Resources.ResourceWordEn.documentsinging, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                var documentsigningid = WordHandlerCRM.CreatDocumentSigningInCRM(sdsurl, fullPath, fullname[0], entity,
                    searchValues, crmconfig, crm);
                if (documentsigningid != Guid.Empty)
                {
                    WordHandlerCRM.SendEmail(sdsurl, fullname[0], entity, documentsigningid, searchValues, crmconfig, crm);
                    if (searchValues.language == 1044)
                        MessageBox.Show(Resources.ResourceWordNb.documentsent, Resources.ResourceWordNb.documentsinging, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show(Resources.ResourceWordEn.documentsent, Resources.ResourceWordEn.documentsinging, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                File.Delete(fullPath);


            }
            catch (Exception ex)
            {
                if (searchValues.language == 1044)
                    MessageBox.Show(Resources.ResourceWordNb.cannotcreateinsignicat + ex.Message);
                else
                    MessageBox.Show(Resources.ResourceWordEn.cannotcreateinsignicat + ex.Message);
            }
        }
    }
}
