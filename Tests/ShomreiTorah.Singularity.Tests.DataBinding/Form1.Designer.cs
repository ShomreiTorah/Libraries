namespace ShomreiTorah.Singularity.Tests.DataBinding {
	partial class Form1 {
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
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.column1DataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.column2DataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.column3DataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataSet1 = new System.Data.DataSet();
			this.dataTable1 = new System.Data.DataTable();
			this.dataColumn1 = new System.Data.DataColumn();
			this.dataColumn2 = new System.Data.DataColumn();
			this.dataColumn3 = new System.Data.DataColumn();
			this.dataTable2 = new System.Data.DataTable();
			this.dataColumn4 = new System.Data.DataColumn();
			this.dataColumn5 = new System.Data.DataColumn();
			this.numbersDataSource1 = new ShomreiTorah.Singularity.Tests.DataBinding.NumbersDataSource();
			this.dataGridView2 = new System.Windows.Forms.DataGridView();
			this.numberDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.noteDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridView3 = new System.Windows.Forms.DataGridView();
			this.column1DataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.column2DataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dataGridView4 = new System.Windows.Forms.DataGridView();
			this.baseDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.exponentDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.stringValueDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataSet1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataTable1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataTable2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView3)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView4)).BeginInit();
			this.SuspendLayout();
			// 
			// dataGridView1
			// 
			this.dataGridView1.AutoGenerateColumns = false;
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.column1DataGridViewTextBoxColumn,
            this.column2DataGridViewTextBoxColumn,
            this.column3DataGridViewTextBoxColumn});
			this.dataGridView1.DataMember = "Table1";
			this.dataGridView1.DataSource = this.dataSet1;
			this.dataGridView1.Location = new System.Drawing.Point(0, 0);
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.Size = new System.Drawing.Size(415, 175);
			this.dataGridView1.TabIndex = 0;
			// 
			// column1DataGridViewTextBoxColumn
			// 
			this.column1DataGridViewTextBoxColumn.DataPropertyName = "Column1";
			this.column1DataGridViewTextBoxColumn.HeaderText = "Column1";
			this.column1DataGridViewTextBoxColumn.Name = "column1DataGridViewTextBoxColumn";
			// 
			// column2DataGridViewTextBoxColumn
			// 
			this.column2DataGridViewTextBoxColumn.DataPropertyName = "Column2";
			this.column2DataGridViewTextBoxColumn.HeaderText = "Column2";
			this.column2DataGridViewTextBoxColumn.Name = "column2DataGridViewTextBoxColumn";
			// 
			// column3DataGridViewTextBoxColumn
			// 
			this.column3DataGridViewTextBoxColumn.DataPropertyName = "Column3";
			this.column3DataGridViewTextBoxColumn.HeaderText = "Column3";
			this.column3DataGridViewTextBoxColumn.Name = "column3DataGridViewTextBoxColumn";
			// 
			// dataSet1
			// 
			this.dataSet1.DataSetName = "NewDataSet";
			this.dataSet1.Relations.AddRange(new System.Data.DataRelation[] {
            new System.Data.DataRelation("Relation1", "Table1", "Table2", new string[] {
                        "Column1"}, new string[] {
                        "Column1"}, false)});
			this.dataSet1.Tables.AddRange(new System.Data.DataTable[] {
            this.dataTable1,
            this.dataTable2});
			// 
			// dataTable1
			// 
			this.dataTable1.Columns.AddRange(new System.Data.DataColumn[] {
            this.dataColumn1,
            this.dataColumn2,
            this.dataColumn3});
			this.dataTable1.Constraints.AddRange(new System.Data.Constraint[] {
            new System.Data.UniqueConstraint("Constraint1", new string[] {
                        "Column1"}, false)});
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
            this.dataColumn5});
			this.dataTable2.Constraints.AddRange(new System.Data.Constraint[] {
            new System.Data.ForeignKeyConstraint("Relation1", "Table1", new string[] {
                        "Column1"}, new string[] {
                        "Column1"}, System.Data.AcceptRejectRule.None, System.Data.Rule.Cascade, System.Data.Rule.Cascade)});
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
			// dataGridView2
			// 
			this.dataGridView2.AutoGenerateColumns = false;
			this.dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView2.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.numberDataGridViewTextBoxColumn,
            this.noteDataGridViewTextBoxColumn});
			this.dataGridView2.DataMember = "Numbers";
			this.dataGridView2.DataSource = this.numbersDataSource1;
			this.dataGridView2.Location = new System.Drawing.Point(12, 63);
			this.dataGridView2.Name = "dataGridView2";
			this.dataGridView2.Size = new System.Drawing.Size(350, 500);
			this.dataGridView2.TabIndex = 1;
			// 
			// numberDataGridViewTextBoxColumn
			// 
			this.numberDataGridViewTextBoxColumn.DataPropertyName = "Value";
			this.numberDataGridViewTextBoxColumn.HeaderText = "Number";
			this.numberDataGridViewTextBoxColumn.Name = "numberDataGridViewTextBoxColumn";
			// 
			// noteDataGridViewTextBoxColumn
			// 
			this.noteDataGridViewTextBoxColumn.DataPropertyName = "Note";
			this.noteDataGridViewTextBoxColumn.HeaderText = "Note";
			this.noteDataGridViewTextBoxColumn.Name = "noteDataGridViewTextBoxColumn";
			// 
			// dataGridView3
			// 
			this.dataGridView3.AutoGenerateColumns = false;
			this.dataGridView3.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView3.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.column1DataGridViewTextBoxColumn1,
            this.column2DataGridViewTextBoxColumn1});
			this.dataGridView3.DataMember = "Table1.Relation1";
			this.dataGridView3.DataSource = this.dataSet1;
			this.dataGridView3.Location = new System.Drawing.Point(421, 76);
			this.dataGridView3.Name = "dataGridView3";
			this.dataGridView3.Size = new System.Drawing.Size(278, 154);
			this.dataGridView3.TabIndex = 2;
			// 
			// column1DataGridViewTextBoxColumn1
			// 
			this.column1DataGridViewTextBoxColumn1.DataPropertyName = "Column1";
			this.column1DataGridViewTextBoxColumn1.HeaderText = "Column1";
			this.column1DataGridViewTextBoxColumn1.Name = "column1DataGridViewTextBoxColumn1";
			// 
			// column2DataGridViewTextBoxColumn1
			// 
			this.column2DataGridViewTextBoxColumn1.DataPropertyName = "Column2";
			this.column2DataGridViewTextBoxColumn1.HeaderText = "Column2";
			this.column2DataGridViewTextBoxColumn1.Name = "column2DataGridViewTextBoxColumn1";
			// 
			// dataGridView4
			// 
			this.dataGridView4.AutoGenerateColumns = false;
			this.dataGridView4.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView4.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.baseDataGridViewTextBoxColumn,
            this.exponentDataGridViewTextBoxColumn,
            this.stringValueDataGridViewTextBoxColumn});
			this.dataGridView4.DataMember = "Numbers.Powers";
			this.dataGridView4.DataSource = this.numbersDataSource1;
			this.dataGridView4.Location = new System.Drawing.Point(411, 253);
			this.dataGridView4.Name = "dataGridView4";
			this.dataGridView4.Size = new System.Drawing.Size(365, 298);
			this.dataGridView4.TabIndex = 3;
			// 
			// baseDataGridViewTextBoxColumn
			// 
			this.baseDataGridViewTextBoxColumn.DataPropertyName = "Base";
			this.baseDataGridViewTextBoxColumn.HeaderText = "Base";
			this.baseDataGridViewTextBoxColumn.Name = "baseDataGridViewTextBoxColumn";
			// 
			// exponentDataGridViewTextBoxColumn
			// 
			this.exponentDataGridViewTextBoxColumn.DataPropertyName = "Exponent";
			this.exponentDataGridViewTextBoxColumn.HeaderText = "Exponent";
			this.exponentDataGridViewTextBoxColumn.Name = "exponentDataGridViewTextBoxColumn";
			// 
			// stringValueDataGridViewTextBoxColumn
			// 
			this.stringValueDataGridViewTextBoxColumn.DataPropertyName = "StringValue";
			this.stringValueDataGridViewTextBoxColumn.HeaderText = "StringValue";
			this.stringValueDataGridViewTextBoxColumn.Name = "stringValueDataGridViewTextBoxColumn";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(788, 598);
			this.Controls.Add(this.dataGridView4);
			this.Controls.Add(this.dataGridView3);
			this.Controls.Add(this.dataGridView2);
			this.Controls.Add(this.dataGridView1);
			this.Name = "Form1";
			this.Text = "Form1";
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataSet1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataTable1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataTable2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView4)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private NumbersDataSource numbersDataSource1;
		private System.Windows.Forms.DataGridView dataGridView1;
		private System.Windows.Forms.DataGridViewTextBoxColumn column1DataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn column2DataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn column3DataGridViewTextBoxColumn;
		private System.Data.DataSet dataSet1;
		private System.Data.DataTable dataTable1;
		private System.Data.DataColumn dataColumn1;
		private System.Data.DataColumn dataColumn2;
		private System.Data.DataColumn dataColumn3;
		private System.Data.DataTable dataTable2;
		private System.Data.DataColumn dataColumn4;
		private System.Data.DataColumn dataColumn5;
		private System.Windows.Forms.DataGridView dataGridView2;
		private System.Windows.Forms.DataGridViewTextBoxColumn numberDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn noteDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridView dataGridView3;
		private System.Windows.Forms.DataGridViewTextBoxColumn column1DataGridViewTextBoxColumn1;
		private System.Windows.Forms.DataGridViewTextBoxColumn column2DataGridViewTextBoxColumn1;
		private System.Windows.Forms.DataGridView dataGridView4;
		private System.Windows.Forms.DataGridViewTextBoxColumn baseDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn exponentDataGridViewTextBoxColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn stringValueDataGridViewTextBoxColumn;
	}
}