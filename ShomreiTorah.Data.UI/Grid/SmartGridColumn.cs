using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.XtraGrid.Columns;
using System.ComponentModel;
using DevExpress.XtraEditors.Repository;
using System.Windows.Forms;

namespace ShomreiTorah.Data.UI.Grid {
	///<summary>A grid column that automatically reads column settings from metadata.</summary>
	public partial class SmartGridColumn : GridColumn {
		///<summary>Copies properties from another column to this instance.</summary>
		protected override void Assign(GridColumn column) {
			base.Assign(column);
			var col = column as SmartGridColumn;
			if (col != null) {
				ShowEditorOnMouseDown = col.ShowEditorOnMouseDown;
			}
		}

		internal RepositoryItem DefaultEditor { get; set; }
		///<summary>Sets the default editor for this column.</summary>
		public void SetDefaultEditor(RepositoryItem editor) {
			if (DefaultEditor == ColumnEditor)
				ColumnEditor = editor;
			DefaultEditor = editor;	//Must be set after if statement
		}

		bool ShouldSerializeColumnEditor() { return ColumnEdit != DefaultEditor; }
		void ResetColumnEditor() { ColumnEdit = DefaultEditor; }

		///<summary>Gets or sets the repository item specifying the editor used to edit a column's cell values.</summary>
		[Category("Data")]
		[Description("Gets or sets the repository item specifying the editor used to edit a column's cell values.")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[TypeConverter("DevExpress.XtraGrid.TypeConverters.ColumnEditConverter, " + AssemblyInfo.SRAssemblyGridDesign)]
		[Editor("DevExpress.XtraGrid.Design.ColumnEditEditor, " + AssemblyInfo.SRAssemblyGridDesign, typeof(System.Drawing.Design.UITypeEditor))]
		public RepositoryItem ColumnEditor {
			get { return base.ColumnEdit; }
			set { base.ColumnEdit = value; }
		}

		///<summary>Gets or sets the repository item specifying the editor used to edit a column's cell values.</summary>
		///<remarks>
		///	In order to use ShouldSerialize & Reset, there cannot be a DefaultValueAttribute.
		/// Since attributes are inherited and cannot be removed from a base class,  I need a
		/// new property with a different name.
		///</remarks>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public new RepositoryItem ColumnEdit {
			get { return ColumnEditor; }
			set { ColumnEditor = value; }
		}
	}
}
