namespace ShomreiTorah.Singularity.Tests.DataBinding {
	partial class DxForm {
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			this.numbersBindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
			this.numbersDataSource1 = new ShomreiTorah.Singularity.Tests.DataBinding.NumbersDataSource();
			this.dataSet1 = new System.Data.DataSet();
			this.dataTable1 = new System.Data.DataTable();
			this.dataColumn1 = new System.Data.DataColumn();
			this.dataColumn2 = new System.Data.DataColumn();
			this.dataColumn3 = new System.Data.DataColumn();
			this.dataTable2 = new System.Data.DataTable();
			this.dataColumn4 = new System.Data.DataColumn();
			this.dataColumn5 = new System.Data.DataColumn();
			this.dataColumn6 = new System.Data.DataColumn();
			this.dataTable3 = new System.Data.DataTable();
			this.dataColumn7 = new System.Data.DataColumn();
			this.dataColumn8 = new System.Data.DataColumn();
			this.dataColumn9 = new System.Data.DataColumn();
			this.table1BindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.gridControl1 = new DevExpress.XtraGrid.GridControl();
			this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
			this.colValue = new DevExpress.XtraGrid.Columns.GridColumn();
			this.colNote = new DevExpress.XtraGrid.Columns.GridColumn();
			((System.ComponentModel.ISupportInitialize)(this.numbersBindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataSet1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataTable1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataTable2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataTable3)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.table1BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
			this.SuspendLayout();
			// 
			// numbersBindingSource
			// 
			this.numbersBindingSource.DataMember = "Numbers";
			this.numbersBindingSource.DataSource = this.bindingSource1;
			this.numbersBindingSource.Position = 0;
			// 
			// bindingSource1
			// 
			this.bindingSource1.DataSource = this.numbersDataSource1;
			this.bindingSource1.Position = 0;
			// 
			// dataSet1
			// 
			this.dataSet1.DataSetName = "NewDataSet";
			this.dataSet1.Tables.AddRange(new System.Data.DataTable[] {
            this.dataTable1,
            this.dataTable2,
            this.dataTable3});
			// 
			// dataTable1
			// 
			this.dataTable1.Columns.AddRange(new System.Data.DataColumn[] {
            this.dataColumn1,
            this.dataColumn2,
            this.dataColumn3});
			this.dataTable1.TableName = "Table1";
			// 
			// dataColumn1
			// 
			this.dataColumn1.ColumnName = "Column1";
			// 
			// dataColumn2
			// 
			this.dataColumn2.ColumnName = "Column2";
			// 
			// dataColumn3
			// 
			this.dataColumn3.ColumnName = "Column3";
			// 
			// dataTable2
			// 
			this.dataTable2.Columns.AddRange(new System.Data.DataColumn[] {
            this.dataColumn4,
            this.dataColumn5,
            this.dataColumn6});
			this.dataTable2.TableName = "Table2";
			// 
			// dataColumn4
			// 
			this.dataColumn4.ColumnName = "Column1";
			// 
			// dataColumn5
			// 
			this.dataColumn5.ColumnName = "Column2";
			// 
			// dataColumn6
			// 
			this.dataColumn6.ColumnName = "Column3";
			// 
			// dataTable3
			// 
			this.dataTable3.Columns.AddRange(new System.Data.DataColumn[] {
            this.dataColumn7,
            this.dataColumn8,
            this.dataColumn9});
			this.dataTable3.TableName = "Table3";
			// 
			// dataColumn7
			// 
			this.dataColumn7.ColumnName = "Column1";
			// 
			// dataColumn8
			// 
			this.dataColumn8.ColumnName = "Column2";
			// 
			// dataColumn9
			// 
			this.dataColumn9.ColumnName = "Column3";
			// 
			// table1BindingSource
			// 
			this.table1BindingSource.DataSource = this.bindingSource1;
			this.table1BindingSource.Position = 0;
			// 
			// gridControl1
			// 
			this.gridControl1.DataSource = this.numbersBindingSource;
			this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridControl1.Location = new System.Drawing.Point(0, 0);
			this.gridControl1.MainView = this.gridView1;
			this.gridControl1.Name = "gridControl1";
			this.gridControl1.Size = new System.Drawing.Size(765, 433);
			this.gridControl1.TabIndex = 0;
			this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
			// 
			// gridView1
			// 
			this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colValue,
            this.colNote});
			this.gridView1.GridControl = this.gridControl1;
			this.gridView1.Name = "gridView1";
			// 
			// colValue
			// 
			this.colValue.FieldName = "Value";
			this.colValue.Name = "colValue";
			this.colValue.Visible = true;
			this.colValue.VisibleIndex = 0;
			// 
			// colNote
			// 
			this.colNote.FieldName = "Note";
			this.colNote.Name = "colNote";
			this.colNote.Visible = true;
			this.colNote.VisibleIndex = 1;
			// 
			// DxForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(765, 433);
			this.Controls.Add(this.gridControl1);
			this.Name = "DxForm";
			this.Text = "DxForm";
			((System.ComponentModel.ISupportInitialize)(this.numbersBindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataSet1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataTable1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataTable2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataTable3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.table1BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private NumbersDataSource numbersDataSource1;
		private System.Windows.Forms.BindingSource numbersBindingSource;
		private System.Windows.Forms.BindingSource bindingSource1;
		private System.Data.DataSet dataSet1;
		private System.Data.DataTable dataTable1;
		private System.Data.DataColumn dataColumn1;
		private System.Data.DataColumn dataColumn2;
		private System.Data.DataColumn dataColumn3;
		private System.Data.DataTable dataTable2;
		private System.Data.DataColumn dataColumn4;
		private System.Data.DataColumn dataColumn5;
		private System.Data.DataColumn dataColumn6;
		private System.Data.DataTable dataTable3;
		private System.Data.DataColumn dataColumn7;
		private System.Data.DataColumn dataColumn8;
		private System.Data.DataColumn dataColumn9;
		private System.Windows.Forms.BindingSource table1BindingSource;
		private DevExpress.XtraGrid.GridControl gridControl1;
		private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
		private DevExpress.XtraGrid.Columns.GridColumn colValue;
		private DevExpress.XtraGrid.Columns.GridColumn colNote;
	}
}