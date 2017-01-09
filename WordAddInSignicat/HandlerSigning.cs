using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Office.Interop.Word;
using Task = System.Threading.Tasks.Task;

namespace WordAddInSignicat
{
    public static class HandlerSigning
    {
        internal static async Task SendDocumentForSigning(SearchObject searchValues)
        {
            Document document = Globals.ThisAddIn.Application.ActiveDocument;

            try
            {
                var crm = ConnectToCrm.ConnectToMSCRM();
                if (crm == null)
                    return;

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

                var signicatHandler = new HandlerSignicat();
                var responseMessage = await signicatHandler.SendRequest(document, pdfName, orgName, crm);
                var stringResult = await responseMessage.Content.ReadAsStringAsync();
                var result = stringResult.Split('"');
                var sdsurl = result[1];

                var entity = HandlerCRM.FindRecordByNumber(searchValues, crm);
                if (entity == null)
                    MessageBox.Show("Could not find a record with number: " + searchValues.searchnumber + ". The signing package will not be connected to any record!", "Document Signing", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                var documentsigningid = HandlerCRM.CreatDocumentSigningInCRM(sdsurl, fullPath, fullname[0], entity, searchValues, crm);
                if (documentsigningid != Guid.Empty)
                {
                    HandlerCRM.SendEmail(sdsurl, fullname[0], entity, documentsigningid, searchValues, crm);
                    MessageBox.Show("The document is sent for signing, visit the log in CRM for updates!", "Document Signing", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                File.Delete(fullPath);


            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot create document in signicat: " + ex.Message);
            }
        }
    }
}
