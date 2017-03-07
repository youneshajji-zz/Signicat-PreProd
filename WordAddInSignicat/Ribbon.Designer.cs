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
            this.SignicatRibbon = this.Factory.CreateRibbonTab();
            this.Signing = this.Factory.CreateRibbonGroup();
            this.buttonSign = this.Factory.CreateRibbonButton();
            this.tab1.SuspendLayout();
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
            this.buttonSign.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.buttonSign.Image = ((System.Drawing.Image)(resources.GetObject("buttonSign.Image")));
            this.buttonSign.Label = "Sign";
            this.buttonSign.Name = "buttonSign";
            this.buttonSign.ScreenTip = "Sign";
            this.buttonSign.ShowImage = true;
            this.buttonSign.SuperTip = resources.GetString("buttonSign.SuperTip");
            this.buttonSign.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.buttonSign_Click);
            // 
            // Ribbon
            // 
            this.Name = "Ribbon";
            this.RibbonType = "Microsoft.Word.Document";
            this.Tabs.Add(this.tab1);
            this.Tabs.Add(this.SignicatRibbon);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.Ribbon_Load);
            this.tab1.ResumeLayout(false);
            this.tab1.PerformLayout();
            this.SignicatRibbon.ResumeLayout(false);
            this.SignicatRibbon.PerformLayout();
            this.Signing.ResumeLayout(false);
            this.Signing.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tab1;
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
