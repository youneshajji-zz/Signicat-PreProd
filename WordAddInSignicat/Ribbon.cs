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
            searchnumber = searchentity = searchnumberfield = "";

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

            WordSearchObject searchValues = new WordSearchObject();
            searchValues.searchentity = searchentity;
            searchValues.searchnumber = searchnumber;
            searchValues.searchnumberfield = searchnumberfield;

            if (string.IsNullOrWhiteSpace(searchentity) || string.IsNullOrWhiteSpace(searchnumber) ||
                string.IsNullOrWhiteSpace(searchnumberfield))
            {
                if (searchValues.language == 1044)
                    MessageBox.Show(Resources.ResourceWordNb.wordtemplaterror, Resources.ResourceWordNb.documentsinging,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show(Resources.ResourceWordEn.wordtemplaterror, Resources.ResourceWordEn.documentsinging,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //WordHandlerSP.SendDocumentToSP(searchValues);
        }

        private void buttonSign_Click(object sender, RibbonControlEventArgs e)
        {
            try
            {
                var crm = WordConnectToCrm.ConnectToMSCRM();
                if (crm == null)
                    return;

                WordSearchObject searchValues = new WordSearchObject();
                WordCRMConfig crmconfig = WordHandlerCRM.GetCRMConfig(crm);
                searchValues.language = WordHandlerCRM.RetrieveUserUiLanguageCode(crm, crmconfig.Worduser.Id);

                var accountfield = (!string.IsNullOrWhiteSpace(crmconfig.AccountNrField)) ? crmconfig.AccountNrField.Split(',') : new[] { "accountnumber" };
                var orderfield = (!string.IsNullOrWhiteSpace(crmconfig.OrderNrField)) ? crmconfig.OrderNrField.Split(',') : new[] { "ordernumber" };
                var quotefield = (!string.IsNullOrWhiteSpace(crmconfig.QuoteNrField)) ? crmconfig.QuoteNrField.Split(',') : new[] { "quotenumber" };
                var incidentfield = (!string.IsNullOrWhiteSpace(crmconfig.IncidentNrField)) ? crmconfig.IncidentNrField.Split(',') : new[] { "ticketnumber"};
                var contractfield = (!string.IsNullOrWhiteSpace(crmconfig.ContractNrField)) ? crmconfig.ContractNrField.Split(',') : new[] { "contractnumber" };
                var emailfield = (!string.IsNullOrWhiteSpace(crmconfig.EmailField)) ? crmconfig.EmailField.Split(',') : new[] { "recievermailaddress" };

                var account = accountfield; // new[] { "accountnumber", "kundenr" };
                var order = orderfield; //new[] { "ordernumber", "ordrenummer"};
                var quote = quotefield; //new[] { "quotenumber"};
                var incident = incidentfield; //new[] { "ticketnumber"};
                var contract = contractfield; //new[] { "contractnumber"};
                var recievermail = emailfield; //new[] { "recievermailaddress", "e-post" };


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
                    //if (docText[i].ToLower().Contains(recievermail))
                    if (recievermail.Any(docText[i].ToLower().Contains))
                        recieverto = docText[i + 1];
                }

                for (int i = 0; i < docText.Length; i++)
                {
                    //if (docText[i].ToLower().Contains(account))
                    if (account.Any(docText[i].ToLower().Contains))
                    {
                        searchnumber = docText[i + 1];
                        searchentity = "account";
                        searchnumberfield = "accountnumber";
                        break;
                    }
                    //if (docText[i].ToLower().Contains(order))
                    if (order.Any(docText[i].ToLower().Contains))
                    {
                        searchnumber = docText[i + 1];
                        searchentity = "salesorder";
                        searchnumberfield = "ordernumber";
                        break;
                    }
                    //if (docText[i].ToLower().Contains(quote))
                    if (quote.Any(docText[i].ToLower().Contains))
                    {
                        searchnumber = docText[i + 1];
                        searchentity = "quote";
                        searchnumberfield = "quotenumber";
                        break;
                    }
                    //if (docText[i].ToLower().Contains(incident))
                    if (incident.Any(docText[i].ToLower().Contains))
                    {
                        searchnumber = docText[i + 1];
                        searchentity = "incident";
                        searchnumberfield = "ticketnumber";
                        break;
                    }
                    //if (docText[i].ToLower().Contains(contract))
                    if (contract.Any(docText[i].ToLower().Contains))
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

                searchValues.searchentity = searchentity;
                searchValues.searchnumber = searchnumber;
                searchValues.searchnumberfield = searchnumberfield;
                searchValues.recievermail = recieverto;

                if (string.IsNullOrWhiteSpace(searchentity) || string.IsNullOrWhiteSpace(searchnumber) || string.IsNullOrWhiteSpace(searchnumberfield))
                {
                    if (searchValues.language == 1044)
                        MessageBox.Show(Resources.ResourceWordNb.wordtemplaterror, Resources.ResourceWordNb.documentsinging, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                        MessageBox.Show(Resources.ResourceWordEn.wordtemplaterror, Resources.ResourceWordEn.documentsinging, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                WordHandlerSigning.SendDocumentForSigning(searchValues, crmconfig, crm);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Signicat Sign", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
