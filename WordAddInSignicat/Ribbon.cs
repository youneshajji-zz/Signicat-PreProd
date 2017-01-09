using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Microsoft.Office.Tools.Ribbon;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Office.Interop;
using System.Windows.Forms;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Word;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow.Activities;
using Microsoft.Xrm.Tooling.Connector;
using Task = System.Threading.Tasks.Task;

namespace WordAddInSignicat
{
    public partial class Ribbon
    {
        private void Ribbon_Load(object sender, RibbonUIEventArgs e)
        {

        }

        private void buttonPDF_Click(object sender, RibbonControlEventArgs e)
        {
            Document document = Globals.ThisAddIn.Application.ActiveDocument;

            var fullname = document.Name.Split('.');
            var pdfName = fullname[0] + ".pdf";

            document.ExportAsFixedFormat(
                 Path.Combine(document.Path, pdfName),
                 WdExportFormat.wdExportFormatPDF,
                 OpenAfterExport: true);
        }

        private void buttonSignicat_Click(object sender, RibbonControlEventArgs e)
        {

            var account = "accountnumber";
            var order = "ordernumber";
            var quote = "quotenumber";
            var incident = "ticketnumber";
            var contract = "contractnumber";
            var recievermail = "recievermailaddress";

            Document document = Globals.ThisAddIn.Application.ActiveDocument;
            //HeaderFooter objHeader = document.Sections[1].Headers[WdHeaderFooterIndex.wdHeaderFooterPrimary];
            Object start = document.Content.Start;
            Object end = document.Content.End;
            //document.Range(ref start, ref end).Select();

            var selectedText = Globals.ThisAddIn.Application.ActiveDocument.Range(start, end).Text;

            //var headerText = objHeader.Range.Text;
            //headerText = headerText.Replace("\r\a", "").Replace("\r", "").Replace("\a", "");
            //var headerTextSplit = headerText.Split(' ');

            selectedText = selectedText.Replace("\r\a", "").Replace("\r", "").Replace("\a", "");
            var docText = selectedText.Split(' ');
            string searchnumber, searchentity, searchnumberfield, senderfrom, recieverto;
            searchnumber = searchentity = searchnumberfield = senderfrom = recieverto = "";

            for (int i = 0; i < docText.Length; i++)
            {
                if (docText[i].ToLower().Contains(recievermail))
                    recieverto = docText[i + 1];
            }

            for (int i = 0; i < docText.Length; i++)
            {
                if (docText[i].ToLower().Contains(account))
                {
                    searchnumber = docText[i + 1];
                    searchentity = "account";
                    searchnumberfield = "accountnumber";
                    break;
                }
                if (docText[i].ToLower().Contains(order))
                {
                    searchnumber = docText[i + 1];
                    searchentity = "salesorder";
                    searchnumberfield = "ordernumber";
                    break;
                }
                if (docText[i].ToLower().Contains(quote))
                {
                    searchnumber = docText[i + 1];
                    searchentity = "quote";
                    searchnumberfield = "quotenumber";
                    break;
                }
                if (docText[i].ToLower().Contains(incident))
                {
                    searchnumber = docText[i + 1];
                    searchentity = "incident";
                    searchnumberfield = "ticketnumber";
                    break;
                }
                if (docText[i].ToLower().Contains(contract))
                {
                    searchnumber = docText[i + 1];
                    searchentity = "contract";
                    searchnumberfield = "contractnumber";
                    break;
                }
            }

            for (int i = 0; i < docText.Length; i++)
            {
                if (docText[i].ToLower().Contains("saveat"))
                {
                    searchentity = docText[i + 1];
                    searchnumber = docText[i + 2];
                    if (searchentity == "account")
                        searchnumberfield = searchentity + "accountnumber";
                    if (searchentity == "salesorder")
                        searchnumberfield = "ordernumber";
                    if (searchentity == "quote")
                        searchnumberfield = "quotenumber";
                    if (searchentity == "incident")
                        searchnumberfield = "ticketnumber";
                    if (searchentity == "contract")
                        searchnumberfield = "contractnumber";
                    break;
                }
            }

            SearchObject searchValues = new SearchObject();
            searchValues.searchentity = searchentity;
            searchValues.searchnumber = searchnumber;
            searchValues.searchnumberfield = searchnumberfield;
            searchValues.recievermail = recieverto;

            if (string.IsNullOrWhiteSpace(searchentity) || string.IsNullOrWhiteSpace(searchnumber) || string.IsNullOrWhiteSpace(searchnumberfield))
            {
                MessageBox.Show("The Word template is not well constructed, please klikk help for more details!", "Document Signing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            HandlerSigning.SendDocumentForSigning(searchValues);
        }

        private void btnSaveToSP_Click(object sender, RibbonControlEventArgs e)
        {
            var account = "accountnumber";
            var order = "ordernumber";
            var quote = "quotenumber";
            var incident = "ticketnumber";
            var contract = "contractnumber";

            Document document = Globals.ThisAddIn.Application.ActiveDocument;
            Object start = document.Content.Start;
            Object end = document.Content.End;

            var selectedText = Globals.ThisAddIn.Application.ActiveDocument.Range(start, end).Text;
            
            selectedText = selectedText.Replace("\r\a", "").Replace("\r", "").Replace("\a", "");
            var docText = selectedText.Split(' ');
            string searchnumber, searchentity, searchnumberfield;
            searchnumber = searchentity = searchnumberfield =  "";
            
            for (int i = 0; i < docText.Length; i++)
            {
                if (docText[i].ToLower().Contains(account))
                {
                    searchnumber = docText[i + 1];
                    searchentity = "account";
                    searchnumberfield = "accountnumber";
                    break;
                }
                if (docText[i].ToLower().Contains(order))
                {
                    searchnumber = docText[i + 1];
                    searchentity = "salesorder";
                    searchnumberfield = "ordernumber";
                    break;
                }
                if (docText[i].ToLower().Contains(quote))
                {
                    searchnumber = docText[i + 1];
                    searchentity = "quote";
                    searchnumberfield = "quotenumber";
                    break;
                }
                if (docText[i].ToLower().Contains(incident))
                {
                    searchnumber = docText[i + 1];
                    searchentity = "incident";
                    searchnumberfield = "ticketnumber";
                    break;
                }
                if (docText[i].ToLower().Contains(contract))
                {
                    searchnumber = docText[i + 1];
                    searchentity = "contract";
                    searchnumberfield = "contractnumber";
                    break;
                }
            }

            for (int i = 0; i < docText.Length; i++)
            {
                if (docText[i].ToLower().Contains("saveat"))
                {
                    searchentity = docText[i + 1];
                    searchnumber = docText[i + 2];
                    if (searchentity == "account")
                        searchnumberfield = searchentity + "accountnumber";
                    if (searchentity == "salesorder")
                        searchnumberfield = "ordernumber";
                    if (searchentity == "quote")
                        searchnumberfield = "quotenumber";
                    if (searchentity == "incident")
                        searchnumberfield = "ticketnumber";
                    if (searchentity == "contract")
                        searchnumberfield = "contractnumber";
                    break;
                }
            }

            SearchObject searchValues = new SearchObject();
            searchValues.searchentity = searchentity;
            searchValues.searchnumber = searchnumber;
            searchValues.searchnumberfield = searchnumberfield;

            if (string.IsNullOrWhiteSpace(searchentity) || string.IsNullOrWhiteSpace(searchnumber) || string.IsNullOrWhiteSpace(searchnumberfield))
            {
                MessageBox.Show("The Word template is not well constructed, please klikk help for more details!", "Document Signing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            HandlerSP.SendDocumentToSP(searchValues);
        }

        private void buttonSign_Click(object sender, RibbonControlEventArgs e)
        {
            var account = "accountnumber";
            var order = "ordernumber";
            var quote = "quotenumber";
            var incident = "ticketnumber";
            var contract = "contractnumber";
            var recievermail = "recievermailaddress";

            Document document = Globals.ThisAddIn.Application.ActiveDocument;
            //HeaderFooter objHeader = document.Sections[1].Headers[WdHeaderFooterIndex.wdHeaderFooterPrimary];
            Object start = document.Content.Start;
            Object end = document.Content.End;
            //document.Range(ref start, ref end).Select();

            var selectedText = Globals.ThisAddIn.Application.ActiveDocument.Range(start, end).Text;

            //var headerText = objHeader.Range.Text;
            //headerText = headerText.Replace("\r\a", "").Replace("\r", "").Replace("\a", "");
            //var headerTextSplit = headerText.Split(' ');

            selectedText = selectedText.Replace("\r\a", "").Replace("\r", "").Replace("\a", "");
            var docText = selectedText.Split(' ');
            string searchnumber, searchentity, searchnumberfield, senderfrom, recieverto;
            searchnumber = searchentity = searchnumberfield = senderfrom = recieverto = "";

            for (int i = 0; i < docText.Length; i++)
            {
                if (docText[i].ToLower().Contains(recievermail))
                    recieverto = docText[i + 1];
            }

            for (int i = 0; i < docText.Length; i++)
            {
                if (docText[i].ToLower().Contains(account))
                {
                    searchnumber = docText[i + 1];
                    searchentity = "account";
                    searchnumberfield = "accountnumber";
                    break;
                }
                if (docText[i].ToLower().Contains(order))
                {
                    searchnumber = docText[i + 1];
                    searchentity = "salesorder";
                    searchnumberfield = "ordernumber";
                    break;
                }
                if (docText[i].ToLower().Contains(quote))
                {
                    searchnumber = docText[i + 1];
                    searchentity = "quote";
                    searchnumberfield = "quotenumber";
                    break;
                }
                if (docText[i].ToLower().Contains(incident))
                {
                    searchnumber = docText[i + 1];
                    searchentity = "incident";
                    searchnumberfield = "ticketnumber";
                    break;
                }
                if (docText[i].ToLower().Contains(contract))
                {
                    searchnumber = docText[i + 1];
                    searchentity = "contract";
                    searchnumberfield = "contractnumber";
                    break;
                }
            }

            for (int i = 0; i < docText.Length; i++)
            {
                if (docText[i].ToLower().Contains("saveat"))
                {
                    searchentity = docText[i + 1];
                    searchnumber = docText[i + 2];
                    if (searchentity == "account")
                        searchnumberfield = searchentity + "accountnumber";
                    if (searchentity == "salesorder")
                        searchnumberfield = "ordernumber";
                    if (searchentity == "quote")
                        searchnumberfield = "quotenumber";
                    if (searchentity == "incident")
                        searchnumberfield = "ticketnumber";
                    if (searchentity == "contract")
                        searchnumberfield = "contractnumber";
                    break;
                }
            }

            SearchObject searchValues = new SearchObject();
            searchValues.searchentity = searchentity;
            searchValues.searchnumber = searchnumber;
            searchValues.searchnumberfield = searchnumberfield;
            searchValues.recievermail = recieverto;

            if (string.IsNullOrWhiteSpace(searchentity) || string.IsNullOrWhiteSpace(searchnumber) || string.IsNullOrWhiteSpace(searchnumberfield))
            {
                MessageBox.Show("The Word template is not well constructed, please klikk help for more details!", "Document Signing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            HandlerSigning.SendDocumentForSigning(searchValues);
        }
    }
}
