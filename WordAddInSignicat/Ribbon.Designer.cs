namespace WordAddInSignicat
{
    partial class Ribbon : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public Ribbon()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Ribbon));
            this.tab1 = this.Factory.CreateRibbonTab();
            this.tabSignicat = this.Factory.CreateRibbonTab();
            this.groupSignicat = this.Factory.CreateRibbonGroup();
            this.buttonSignicat = this.Factory.CreateRibbonButton();
            this.groupPdf = this.Factory.CreateRibbonGroup();
            this.buttonPDF = this.Factory.CreateRibbonButton();
            this.SharePointGroup = this.Factory.CreateRibbonGroup();
            this.btnSaveToSP = this.Factory.CreateRibbonButton();
            this.SignicatRibbon = this.Factory.CreateRibbonTab();
            this.Signing = this.Factory.CreateRibbonGroup();
            this.buttonSign = this.Factory.CreateRibbonButton();
            this.tab1.SuspendLayout();
            this.tabSignicat.SuspendLayout();
            this.groupSignicat.SuspendLayout();
            this.groupPdf.SuspendLayout();
            this.SharePointGroup.SuspendLayout();
            this.SignicatRibbon.SuspendLayout();
            this.Signing.SuspendLayout();
            this.SuspendLayout();
            // 
            // tab1
            // 
            this.tab1.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.tab1.Label = "TabAddIns";
            this.tab1.Name = "tab1";
            // 
            // tabSignicat
            // 
            this.tabSignicat.Groups.Add(this.groupSignicat);
            this.tabSignicat.Groups.Add(this.groupPdf);
            this.tabSignicat.Groups.Add(this.SharePointGroup);
            this.tabSignicat.Label = "ProsessPilotene";
            this.tabSignicat.Name = "tabSignicat";
            // 
            // groupSignicat
            // 
            this.groupSignicat.Items.Add(this.buttonSignicat);
            this.groupSignicat.Label = "Signing";
            this.groupSignicat.Name = "groupSignicat";
            // 
            // buttonSignicat
            // 
            this.buttonSignicat.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.buttonSignicat.Image = ((System.Drawing.Image)(resources.GetObject("buttonSignicat.Image")));
            this.buttonSignicat.Label = "Sign";
            this.buttonSignicat.Name = "buttonSignicat";
            this.buttonSignicat.ScreenTip = "Sign";
            this.buttonSignicat.ShowImage = true;
            this.buttonSignicat.SuperTip = resources.GetString("buttonSignicat.SuperTip");
            this.buttonSignicat.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonSignicat_Click);
            // 
            // groupPdf
            // 
            this.groupPdf.Items.Add(this.buttonPDF);
            this.groupPdf.Label = "PDF";
            this.groupPdf.Name = "groupPdf";
            // 
            // buttonPDF
            // 
            this.buttonPDF.Image = ((System.Drawing.Image)(resources.GetObject("buttonPDF.Image")));
            this.buttonPDF.Label = "Convert";
            this.buttonPDF.Name = "buttonPDF";
            this.buttonPDF.ScreenTip = "Convert";
            this.buttonPDF.ShowImage = true;
            this.buttonPDF.SuperTip = "Convert this document to PDF, the pdf will be saved in the same directory.";
            this.buttonPDF.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonPDF_Click);
            // 
            // SharePointGroup
            // 
            this.SharePointGroup.Items.Add(this.btnSaveToSP);
            this.SharePointGroup.Label = "SharePoint";
            this.SharePointGroup.Name = "SharePointGroup";
            // 
            // btnSaveToSP
            // 
            this.btnSaveToSP.Image = ((System.Drawing.Image)(resources.GetObject("btnSaveToSP.Image")));
            this.btnSaveToSP.Label = "Save";
            this.btnSaveToSP.Name = "btnSaveToSP";
            this.btnSaveToSP.ShowImage = true;
            this.btnSaveToSP.SuperTip = "Click to save this document as pdf in thedocument location of the specified entit" +
    "y.";
            this.btnSaveToSP.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.btnSaveToSP_Click);
            // 
            // SignicatRibbon
            // 
            this.SignicatRibbon.Groups.Add(this.Signing);
            this.SignicatRibbon.Label = "Signicat";
            this.SignicatRibbon.Name = "SignicatRibbon";
            // 
            // Signing
            // 
            this.Signing.Items.Add(this.buttonSign);
            this.Signing.Label = "Signing";
            this.Signing.Name = "Signing";
            // 
            // buttonSign
            // 
            this.buttonSign.Image = ((System.Drawing.Image)(resources.GetObject("buttonSign.Image")));
            this.buttonSign.Label = "Sign";
            this.buttonSign.Name = "buttonSign";
            this.buttonSign.ShowImage = true;
            this.buttonSign.SuperTip = resources.GetString("buttonSign.SuperTip");
            this.buttonSign.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonSign_Click);
            // 
            // Ribbon
            // 
            this.Name = "Ribbon";
            this.RibbonType = "Microsoft.Word.Document";
            this.Tabs.Add(this.tab1);
            this.Tabs.Add(this.tabSignicat);
            this.Tabs.Add(this.SignicatRibbon);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.Ribbon_Load);
            this.tab1.ResumeLayout(false);
            this.tab1.PerformLayout();
            this.tabSignicat.ResumeLayout(false);
            this.tabSignicat.PerformLayout();
            this.groupSignicat.ResumeLayout(false);
            this.groupSignicat.PerformLayout();
            this.groupPdf.ResumeLayout(false);
            this.groupPdf.PerformLayout();
            this.SharePointGroup.ResumeLayout(false);
            this.SharePointGroup.PerformLayout();
            this.SignicatRibbon.ResumeLayout(false);
            this.SignicatRibbon.PerformLayout();
            this.Signing.ResumeLayout(false);
            this.Signing.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tab1;
        internal Microsoft.Office.Tools.Ribbon.RibbonTab tabSignicat;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup groupSignicat;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonSignicat;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup groupPdf;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonPDF;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup SharePointGroup;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton btnSaveToSP;
        internal Microsoft.Office.Tools.Ribbon.RibbonTab SignicatRibbon;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup Signing;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton buttonSign;
    }

    partial class ThisRibbonCollection
    {
        internal Ribbon Ribbon
        {
            get { return this.GetRibbon<Ribbon>(); }
        }
    }
}
