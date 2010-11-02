namespace ShomreiTorah.Data.UI.Controls {
	partial class PersonEditor {
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.dataLayoutControl1 = new DevExpress.XtraDataLayout.DataLayoutControl();
			this.LastNameTextEdit = new DevExpress.XtraEditors.TextEdit();
			this.bindingSource = new System.Windows.Forms.BindingSource();
			this.contextBinder1 = new ShomreiTorah.Data.UI.Controls.DesignerBinder();
			this.HisNameTextEdit = new DevExpress.XtraEditors.TextEdit();
			this.HerNameTextEdit = new DevExpress.XtraEditors.TextEdit();
			this.FullNameTextEdit = new DevExpress.XtraEditors.TextEdit();
			this.AddressTextEdit = new DevExpress.XtraEditors.TextEdit();
			this.CityTextEdit = new DevExpress.XtraEditors.TextEdit();
			this.StateComboBoxEdit = new DevExpress.XtraEditors.ComboBoxEdit();
			this.ZipTextEdit = new DevExpress.XtraEditors.TextEdit();
			this.PhoneTextEdit = new DevExpress.XtraEditors.TextEdit();
			this.ItemForId = new DevExpress.XtraLayout.LayoutControlItem();
			this.ItemForYKID = new DevExpress.XtraLayout.LayoutControlItem();
			this.ItemForSource = new DevExpress.XtraLayout.LayoutControlItem();
			this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
			this.layoutControlGroup2 = new DevExpress.XtraLayout.LayoutControlGroup();
			this.ItemForPhone = new DevExpress.XtraLayout.LayoutControlItem();
			this.ItemForHisName = new DevExpress.XtraLayout.LayoutControlItem();
			this.ItemForHerName = new DevExpress.XtraLayout.LayoutControlItem();
			this.ItemForLastName = new DevExpress.XtraLayout.LayoutControlItem();
			this.mailingAddressGroup = new DevExpress.XtraLayout.LayoutControlGroup();
			this.ItemForFullName = new DevExpress.XtraLayout.LayoutControlItem();
			this.ItemForAddress = new DevExpress.XtraLayout.LayoutControlItem();
			this.ItemForCity = new DevExpress.XtraLayout.LayoutControlItem();
			this.ItemForState = new DevExpress.XtraLayout.LayoutControlItem();
			this.ItemForZip = new DevExpress.XtraLayout.LayoutControlItem();
			this.editorSettingsApplier1 = new ShomreiTorah.Data.UI.Controls.EditorSettingsApplier();
			((System.ComponentModel.ISupportInitialize)(this.dataLayoutControl1)).BeginInit();
			this.dataLayoutControl1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LastNameTextEdit.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.bindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.HisNameTextEdit.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.HerNameTextEdit.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FullNameTextEdit.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AddressTextEdit.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CityTextEdit.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.StateComboBoxEdit.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ZipTextEdit.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PhoneTextEdit.Properties)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemForId)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemForYKID)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemForSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemForPhone)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemForHisName)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemForHerName)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemForLastName)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.mailingAddressGroup)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemForFullName)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemForAddress)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemForCity)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemForState)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemForZip)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.editorSettingsApplier1)).BeginInit();
			this.SuspendLayout();
			// 
			// dataLayoutControl1
			// 
			this.dataLayoutControl1.Controls.Add(this.LastNameTextEdit);
			this.dataLayoutControl1.Controls.Add(this.HisNameTextEdit);
			this.dataLayoutControl1.Controls.Add(this.HerNameTextEdit);
			this.dataLayoutControl1.Controls.Add(this.FullNameTextEdit);
			this.dataLayoutControl1.Controls.Add(this.AddressTextEdit);
			this.dataLayoutControl1.Controls.Add(this.CityTextEdit);
			this.dataLayoutControl1.Controls.Add(this.StateComboBoxEdit);
			this.dataLayoutControl1.Controls.Add(this.ZipTextEdit);
			this.dataLayoutControl1.Controls.Add(this.PhoneTextEdit);
			this.dataLayoutControl1.DataSource = this.bindingSource;
			this.dataLayoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataLayoutControl1.HiddenItems.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.ItemForId,
            this.ItemForYKID,
            this.ItemForSource});
			this.dataLayoutControl1.Location = new System.Drawing.Point(0, 0);
			this.dataLayoutControl1.Name = "dataLayoutControl1";
			this.dataLayoutControl1.OptionsView.UseSkinIndents = false;
			this.dataLayoutControl1.Root = this.layoutControlGroup1;
			this.dataLayoutControl1.Size = new System.Drawing.Size(257, 209);
			this.dataLayoutControl1.TabIndex = 0;
			this.dataLayoutControl1.Text = "dataLayoutControl1";
			// 
			// LastNameTextEdit
			// 
			this.LastNameTextEdit.DataBindings.Add(new System.Windows.Forms.Binding("EditValue", this.bindingSource, "LastName", true));
			this.editorSettingsApplier1.SetDefaultSettingsMode(this.LastNameTextEdit, ShomreiTorah.Data.UI.Controls.DefaultSettingsMode.Inactive);
			this.LastNameTextEdit.Location = new System.Drawing.Point(56, 35);
			this.LastNameTextEdit.Name = "LastNameTextEdit";
			this.LastNameTextEdit.Size = new System.Drawing.Size(196, 20);
			this.LastNameTextEdit.StyleController = this.dataLayoutControl1;
			this.LastNameTextEdit.TabIndex = 6;
			this.LastNameTextEdit.EditValueChanged += new System.EventHandler(this.SingleNameEdit_EditValueChanged);
			this.LastNameTextEdit.KeyUp += new System.Windows.Forms.KeyEventHandler(this.SingleNameEdit_KeyUp);
			// 
			// bindingSource
			// 
			this.bindingSource.DataMember = "MasterDirectory";
			this.bindingSource.DataSource = this.contextBinder1;
			// 
			// HisNameTextEdit
			// 
			this.HisNameTextEdit.DataBindings.Add(new System.Windows.Forms.Binding("EditValue", this.bindingSource, "HisName", true));
			this.editorSettingsApplier1.SetDefaultSettingsMode(this.HisNameTextEdit, ShomreiTorah.Data.UI.Controls.DefaultSettingsMode.Inactive);
			this.HisNameTextEdit.Location = new System.Drawing.Point(56, 5);
			this.HisNameTextEdit.Name = "HisNameTextEdit";
			this.HisNameTextEdit.Size = new System.Drawing.Size(65, 20);
			this.HisNameTextEdit.StyleController = this.dataLayoutControl1;
			this.HisNameTextEdit.TabIndex = 7;
			this.HisNameTextEdit.EditValueChanged += new System.EventHandler(this.SingleNameEdit_EditValueChanged);
			this.HisNameTextEdit.KeyUp += new System.Windows.Forms.KeyEventHandler(this.SingleNameEdit_KeyUp);
			// 
			// HerNameTextEdit
			// 
			this.HerNameTextEdit.DataBindings.Add(new System.Windows.Forms.Binding("EditValue", this.bindingSource, "HerName", true));
			this.editorSettingsApplier1.SetDefaultSettingsMode(this.HerNameTextEdit, ShomreiTorah.Data.UI.Controls.DefaultSettingsMode.Inactive);
			this.HerNameTextEdit.Location = new System.Drawing.Point(182, 5);
			this.HerNameTextEdit.Name = "HerNameTextEdit";
			this.HerNameTextEdit.Size = new System.Drawing.Size(70, 20);
			this.HerNameTextEdit.StyleController = this.dataLayoutControl1;
			this.HerNameTextEdit.TabIndex = 8;
			this.HerNameTextEdit.EditValueChanged += new System.EventHandler(this.SingleNameEdit_EditValueChanged);
			this.HerNameTextEdit.KeyUp += new System.Windows.Forms.KeyEventHandler(this.SingleNameEdit_KeyUp);
			// 
			// FullNameTextEdit
			// 
			this.FullNameTextEdit.DataBindings.Add(new System.Windows.Forms.Binding("EditValue", this.bindingSource, "FullName", true));
			this.editorSettingsApplier1.SetDefaultSettingsMode(this.FullNameTextEdit, ShomreiTorah.Data.UI.Controls.DefaultSettingsMode.Inactive);
			this.FullNameTextEdit.Location = new System.Drawing.Point(59, 88);
			this.FullNameTextEdit.Name = "FullNameTextEdit";
			this.FullNameTextEdit.Size = new System.Drawing.Size(190, 20);
			this.FullNameTextEdit.StyleController = this.dataLayoutControl1;
			this.FullNameTextEdit.TabIndex = 9;
			// 
			// AddressTextEdit
			// 
			this.AddressTextEdit.DataBindings.Add(new System.Windows.Forms.Binding("EditValue", this.bindingSource, "Address", true));
			this.editorSettingsApplier1.SetDefaultSettingsMode(this.AddressTextEdit, ShomreiTorah.Data.UI.Controls.DefaultSettingsMode.Inactive);
			this.AddressTextEdit.Location = new System.Drawing.Point(59, 118);
			this.AddressTextEdit.Name = "AddressTextEdit";
			this.AddressTextEdit.Size = new System.Drawing.Size(190, 20);
			this.AddressTextEdit.StyleController = this.dataLayoutControl1;
			this.AddressTextEdit.TabIndex = 10;
			// 
			// CityTextEdit
			// 
			this.CityTextEdit.DataBindings.Add(new System.Windows.Forms.Binding("EditValue", this.bindingSource, "City", true));
			this.editorSettingsApplier1.SetDefaultSettingsMode(this.CityTextEdit, ShomreiTorah.Data.UI.Controls.DefaultSettingsMode.Inactive);
			this.CityTextEdit.Location = new System.Drawing.Point(32, 148);
			this.CityTextEdit.Name = "CityTextEdit";
			this.CityTextEdit.Size = new System.Drawing.Size(50, 20);
			this.CityTextEdit.StyleController = this.dataLayoutControl1;
			this.CityTextEdit.TabIndex = 11;
			this.CityTextEdit.EditValueChanged += new System.EventHandler(this.CityTextEdit_EditValueChanged);
			// 
			// StateComboBoxEdit
			// 
			this.StateComboBoxEdit.DataBindings.Add(new System.Windows.Forms.Binding("EditValue", this.bindingSource, "State", true));
			this.editorSettingsApplier1.SetDefaultSettingsMode(this.StateComboBoxEdit, ShomreiTorah.Data.UI.Controls.DefaultSettingsMode.Inactive);
			this.StateComboBoxEdit.Location = new System.Drawing.Point(123, 148);
			this.StateComboBoxEdit.Name = "StateComboBoxEdit";
			this.StateComboBoxEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
			this.StateComboBoxEdit.Properties.DropDownRows = 15;
			this.StateComboBoxEdit.Properties.HighlightedItemStyle = DevExpress.XtraEditors.HighlightStyle.Skinned;
			this.StateComboBoxEdit.Properties.Items.AddRange(new object[] {
            "NJ",
            "NY",
            "AK",
            "AL",
            "AR",
            "AZ",
            "CA",
            "CO",
            "CT",
            "DE",
            "FL",
            "GA",
            "HI",
            "IA",
            "ID",
            "IL",
            "IN",
            "KS",
            "KY",
            "LA",
            "MA",
            "MD",
            "ME",
            "MI",
            "MN",
            "MO",
            "MS",
            "MT",
            "NC",
            "ND",
            "NE",
            "NH",
            "NJ",
            "NM",
            "NV",
            "NY",
            "OH",
            "OK",
            "OR",
            "PA",
            "RI",
            "SC",
            "SD",
            "TN",
            "TX",
            "UT",
            "VA",
            "VT",
            "WA",
            "WI",
            "WV",
            "WY"});
			this.StateComboBoxEdit.Size = new System.Drawing.Size(44, 20);
			this.StateComboBoxEdit.StyleController = this.dataLayoutControl1;
			this.StateComboBoxEdit.TabIndex = 12;
			// 
			// ZipTextEdit
			// 
			this.ZipTextEdit.DataBindings.Add(new System.Windows.Forms.Binding("EditValue", this.bindingSource, "Zip", true));
			this.editorSettingsApplier1.SetDefaultSettingsMode(this.ZipTextEdit, ShomreiTorah.Data.UI.Controls.DefaultSettingsMode.Inactive);
			this.ZipTextEdit.Location = new System.Drawing.Point(196, 148);
			this.ZipTextEdit.Name = "ZipTextEdit";
			this.ZipTextEdit.Properties.Mask.AutoComplete = DevExpress.XtraEditors.Mask.AutoCompleteType.None;
			this.ZipTextEdit.Properties.Mask.EditMask = "\\d{5}";
			this.ZipTextEdit.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.RegEx;
			this.ZipTextEdit.Properties.Mask.ShowPlaceHolders = false;
			this.ZipTextEdit.Size = new System.Drawing.Size(53, 20);
			this.ZipTextEdit.StyleController = this.dataLayoutControl1;
			this.ZipTextEdit.TabIndex = 13;
			this.ZipTextEdit.EditValueChanged += new System.EventHandler(this.ZipTextEdit_EditValueChanged);
			// 
			// PhoneTextEdit
			// 
			this.PhoneTextEdit.DataBindings.Add(new System.Windows.Forms.Binding("EditValue", this.bindingSource, "Phone", true));
			this.editorSettingsApplier1.SetDefaultSettingsMode(this.PhoneTextEdit, ShomreiTorah.Data.UI.Controls.DefaultSettingsMode.Inactive);
			this.PhoneTextEdit.Location = new System.Drawing.Point(56, 181);
			this.PhoneTextEdit.Name = "PhoneTextEdit";
			this.PhoneTextEdit.Properties.Mask.EditMask = "\\(\\d\\d\\d\\) \\d\\d\\d - \\d\\d\\d\\d";
			this.PhoneTextEdit.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.RegEx;
			this.PhoneTextEdit.Properties.Mask.ShowPlaceHolders = false;
			this.PhoneTextEdit.Size = new System.Drawing.Size(196, 20);
			this.PhoneTextEdit.StyleController = this.dataLayoutControl1;
			this.PhoneTextEdit.TabIndex = 14;
			// 
			// ItemForId
			// 
			this.ItemForId.CustomizationFormText = "Id";
			this.ItemForId.Location = new System.Drawing.Point(0, 0);
			this.ItemForId.Name = "ItemForId";
			this.ItemForId.Size = new System.Drawing.Size(0, 0);
			this.ItemForId.Text = "Id";
			this.ItemForId.TextSize = new System.Drawing.Size(50, 20);
			this.ItemForId.TextToControlDistance = 5;
			// 
			// ItemForYKID
			// 
			this.ItemForYKID.CustomizationFormText = "YKID";
			this.ItemForYKID.Location = new System.Drawing.Point(0, 0);
			this.ItemForYKID.Name = "ItemForYKID";
			this.ItemForYKID.Size = new System.Drawing.Size(0, 0);
			this.ItemForYKID.Text = "YKID";
			this.ItemForYKID.TextSize = new System.Drawing.Size(50, 20);
			this.ItemForYKID.TextToControlDistance = 5;
			// 
			// ItemForSource
			// 
			this.ItemForSource.CustomizationFormText = "Source";
			this.ItemForSource.Location = new System.Drawing.Point(0, 0);
			this.ItemForSource.Name = "ItemForSource";
			this.ItemForSource.Size = new System.Drawing.Size(0, 0);
			this.ItemForSource.Text = "Source";
			this.ItemForSource.TextSize = new System.Drawing.Size(50, 20);
			this.ItemForSource.TextToControlDistance = 5;
			// 
			// layoutControlGroup1
			// 
			this.layoutControlGroup1.CustomizationFormText = "layoutControlGroup1";
			this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
			this.layoutControlGroup1.GroupBordersVisible = false;
			this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlGroup2});
			this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
			this.layoutControlGroup1.Name = "layoutControlGroup1";
			this.layoutControlGroup1.OptionsItemText.TextToControlDistance = 1;
			this.layoutControlGroup1.Size = new System.Drawing.Size(257, 209);
			this.layoutControlGroup1.Spacing = new DevExpress.XtraLayout.Utils.Padding(0, 0, 0, 0);
			this.layoutControlGroup1.Text = "layoutControlGroup1";
			this.layoutControlGroup1.TextVisible = false;
			// 
			// layoutControlGroup2
			// 
			this.layoutControlGroup2.AllowDrawBackground = false;
			this.layoutControlGroup2.CustomizationFormText = "autoGeneratedGroup0";
			this.layoutControlGroup2.GroupBordersVisible = false;
			this.layoutControlGroup2.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.ItemForPhone,
            this.ItemForHisName,
            this.ItemForHerName,
            this.ItemForLastName,
            this.mailingAddressGroup});
			this.layoutControlGroup2.Location = new System.Drawing.Point(0, 0);
			this.layoutControlGroup2.Name = "autoGeneratedGroup0";
			this.layoutControlGroup2.OptionsItemText.TextToControlDistance = 5;
			this.layoutControlGroup2.Size = new System.Drawing.Size(257, 209);
			this.layoutControlGroup2.Text = "autoGeneratedGroup0";
			// 
			// ItemForPhone
			// 
			this.ItemForPhone.Control = this.PhoneTextEdit;
			this.ItemForPhone.CustomizationFormText = "Phone";
			this.ItemForPhone.Location = new System.Drawing.Point(0, 176);
			this.ItemForPhone.Name = "ItemForPhone";
			this.ItemForPhone.Size = new System.Drawing.Size(257, 33);
			this.ItemForPhone.Text = "Phone";
			this.ItemForPhone.TextSize = new System.Drawing.Size(50, 13);
			// 
			// ItemForHisName
			// 
			this.ItemForHisName.Control = this.HisNameTextEdit;
			this.ItemForHisName.CustomizationFormText = "His Name";
			this.ItemForHisName.Location = new System.Drawing.Point(0, 0);
			this.ItemForHisName.Name = "ItemForHisName";
			this.ItemForHisName.Size = new System.Drawing.Size(126, 30);
			this.ItemForHisName.Text = "His Name";
			this.ItemForHisName.TextSize = new System.Drawing.Size(50, 13);
			// 
			// ItemForHerName
			// 
			this.ItemForHerName.Control = this.HerNameTextEdit;
			this.ItemForHerName.CustomizationFormText = "Her Name";
			this.ItemForHerName.Location = new System.Drawing.Point(126, 0);
			this.ItemForHerName.Name = "ItemForHerName";
			this.ItemForHerName.Size = new System.Drawing.Size(131, 30);
			this.ItemForHerName.Text = "Her Name";
			this.ItemForHerName.TextSize = new System.Drawing.Size(50, 13);
			// 
			// ItemForLastName
			// 
			this.ItemForLastName.Control = this.LastNameTextEdit;
			this.ItemForLastName.CustomizationFormText = "Last Name";
			this.ItemForLastName.Location = new System.Drawing.Point(0, 30);
			this.ItemForLastName.Name = "ItemForLastName";
			this.ItemForLastName.Size = new System.Drawing.Size(257, 30);
			this.ItemForLastName.Text = "Last Name";
			this.ItemForLastName.TextSize = new System.Drawing.Size(50, 13);
			// 
			// mailingAddressGroup
			// 
			this.mailingAddressGroup.CustomizationFormText = "Mailing Address";
			this.mailingAddressGroup.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.ItemForFullName,
            this.ItemForAddress,
            this.ItemForCity,
            this.ItemForState,
            this.ItemForZip});
			this.mailingAddressGroup.Location = new System.Drawing.Point(0, 60);
			this.mailingAddressGroup.Name = "mailingAddressGroup";
			this.mailingAddressGroup.OptionsItemText.TextToControlDistance = 5;
			this.mailingAddressGroup.Size = new System.Drawing.Size(257, 116);
			this.mailingAddressGroup.Text = "Mailing Address";
			// 
			// ItemForFullName
			// 
			this.ItemForFullName.Control = this.FullNameTextEdit;
			this.ItemForFullName.CustomizationFormText = "Full Name";
			this.ItemForFullName.Location = new System.Drawing.Point(0, 0);
			this.ItemForFullName.Name = "ItemForFullName";
			this.ItemForFullName.Size = new System.Drawing.Size(251, 30);
			this.ItemForFullName.Text = "Full Name";
			this.ItemForFullName.TextSize = new System.Drawing.Size(50, 13);
			// 
			// ItemForAddress
			// 
			this.ItemForAddress.Control = this.AddressTextEdit;
			this.ItemForAddress.CustomizationFormText = "Address";
			this.ItemForAddress.Location = new System.Drawing.Point(0, 30);
			this.ItemForAddress.Name = "ItemForAddress";
			this.ItemForAddress.Size = new System.Drawing.Size(251, 30);
			this.ItemForAddress.Text = "Address";
			this.ItemForAddress.TextSize = new System.Drawing.Size(50, 13);
			// 
			// ItemForCity
			// 
			this.ItemForCity.Control = this.CityTextEdit;
			this.ItemForCity.CustomizationFormText = "City";
			this.ItemForCity.Location = new System.Drawing.Point(0, 60);
			this.ItemForCity.Name = "ItemForCity";
			this.ItemForCity.Size = new System.Drawing.Size(84, 30);
			this.ItemForCity.Text = "City";
			this.ItemForCity.TextAlignMode = DevExpress.XtraLayout.TextAlignModeItem.AutoSize;
			this.ItemForCity.TextSize = new System.Drawing.Size(19, 13);
			this.ItemForCity.TextToControlDistance = 5;
			// 
			// ItemForState
			// 
			this.ItemForState.Control = this.StateComboBoxEdit;
			this.ItemForState.CustomizationFormText = "State";
			this.ItemForState.Location = new System.Drawing.Point(84, 60);
			this.ItemForState.MaxSize = new System.Drawing.Size(108, 24);
			this.ItemForState.MinSize = new System.Drawing.Size(85, 24);
			this.ItemForState.Name = "ItemForState";
			this.ItemForState.Size = new System.Drawing.Size(85, 30);
			this.ItemForState.SizeConstraintsType = DevExpress.XtraLayout.SizeConstraintsType.Custom;
			this.ItemForState.Text = "State";
			this.ItemForState.TextAlignMode = DevExpress.XtraLayout.TextAlignModeItem.AutoSize;
			this.ItemForState.TextSize = new System.Drawing.Size(26, 13);
			this.ItemForState.TextToControlDistance = 5;
			// 
			// ItemForZip
			// 
			this.ItemForZip.Control = this.ZipTextEdit;
			this.ItemForZip.CustomizationFormText = "Zip";
			this.ItemForZip.Location = new System.Drawing.Point(169, 60);
			this.ItemForZip.Name = "ItemForZip";
			this.ItemForZip.Size = new System.Drawing.Size(82, 30);
			this.ItemForZip.Text = "Zip";
			this.ItemForZip.TextAlignMode = DevExpress.XtraLayout.TextAlignModeItem.AutoSize;
			this.ItemForZip.TextSize = new System.Drawing.Size(14, 13);
			this.ItemForZip.TextToControlDistance = 5;
			// 
			// PersonEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.dataLayoutControl1);
			this.Name = "PersonEditor";
			this.Size = new System.Drawing.Size(257, 209);
			((System.ComponentModel.ISupportInitialize)(this.dataLayoutControl1)).EndInit();
			this.dataLayoutControl1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.LastNameTextEdit.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.bindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.HisNameTextEdit.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.HerNameTextEdit.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FullNameTextEdit.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AddressTextEdit.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CityTextEdit.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.StateComboBoxEdit.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ZipTextEdit.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PhoneTextEdit.Properties)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemForId)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemForYKID)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemForSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemForPhone)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemForHisName)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemForHerName)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemForLastName)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.mailingAddressGroup)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemForFullName)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemForAddress)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemForCity)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemForState)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ItemForZip)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.editorSettingsApplier1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private EditorSettingsApplier editorSettingsApplier1;
		private DesignerBinder contextBinder1;
		private DevExpress.XtraDataLayout.DataLayoutControl dataLayoutControl1;
		private DevExpress.XtraEditors.TextEdit LastNameTextEdit;
		private DevExpress.XtraEditors.TextEdit HisNameTextEdit;
		private DevExpress.XtraEditors.TextEdit HerNameTextEdit;
		private DevExpress.XtraEditors.TextEdit FullNameTextEdit;
		private DevExpress.XtraEditors.TextEdit AddressTextEdit;
		private DevExpress.XtraEditors.TextEdit CityTextEdit;
		private DevExpress.XtraEditors.ComboBoxEdit StateComboBoxEdit;
		private DevExpress.XtraEditors.TextEdit ZipTextEdit;
		private DevExpress.XtraEditors.TextEdit PhoneTextEdit;
		private DevExpress.XtraLayout.LayoutControlItem ItemForId;
		private DevExpress.XtraLayout.LayoutControlItem ItemForYKID;
		private DevExpress.XtraLayout.LayoutControlItem ItemForSource;
		private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
		private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup2;
		private DevExpress.XtraLayout.LayoutControlItem ItemForLastName;
		private DevExpress.XtraLayout.LayoutControlItem ItemForHisName;
		private DevExpress.XtraLayout.LayoutControlItem ItemForHerName;
		private DevExpress.XtraLayout.LayoutControlItem ItemForFullName;
		private DevExpress.XtraLayout.LayoutControlItem ItemForAddress;
		private DevExpress.XtraLayout.LayoutControlItem ItemForCity;
		private DevExpress.XtraLayout.LayoutControlItem ItemForState;
		private DevExpress.XtraLayout.LayoutControlItem ItemForZip;
		private DevExpress.XtraLayout.LayoutControlItem ItemForPhone;
		private DevExpress.XtraLayout.LayoutControlGroup mailingAddressGroup;
		private System.Windows.Forms.BindingSource bindingSource;
	}
}
