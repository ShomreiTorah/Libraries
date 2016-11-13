using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using ShomreiTorah.Common;

namespace ShomreiTorah.Data.UI.Controls {
	///<summary>A <see cref="TreeView"/> that binds a pair of <see cref="TextEdit"/>s to the tree of standard <see cref="PledgeType"/>s.</summary>
	public class PledgeTypeTree : TreeView {
		///<summary>Creates a <see cref="PledgeTypeTree"/> bound to the <see cref="PledgeType"/>s from <see cref="Config"/>.</summary>
		public PledgeTypeTree() {
			foreach (var type in Names.PledgeTypes) {
				var node = Nodes.Add(type.Name);
				foreach (var subtype in type.Subtypes)
					node.Nodes.Add(subtype);
			}
			this.HideSelection = false;
		}

		// Don't serialize nodes from the designer's config.
		///<summary>Gets the nodes displayed in the tree.</summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new TreeNodeCollection Nodes {
			get { return base.Nodes; }
		}

		private TextEdit typeField, subtypeField;

		///<summary>Gets or sets the textbox bound to the Type field.</summary>
		[Description("Gets or sets the textbox bound to the Type field.")]
		[Category("Data")]
		public TextEdit TypeField {
			get { return typeField; }
			set {
				if (TypeField != null) TypeField.EditValueChanged -= TypeChanged;
				typeField = value;
				if (TypeField != null) TypeField.EditValueChanged += TypeChanged;
			}
		}
		///<summary>Gets or sets the textbox bound to the SubType field.</summary>
		[Description("Gets or sets the textbox bound to the SubType field.")]
		[Category("Data")]
		public TextEdit SubTypeField {
			get { return subtypeField; }
			set {
				if (SubTypeField != null) SubTypeField.EditValueChanged -= TypeChanged;
				subtypeField = value;
				if (SubTypeField != null) SubTypeField.EditValueChanged += TypeChanged;
			}
		}

		private void TypeChanged(object sender, EventArgs e) {
			var node = Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Text.Equals(TypeField.Text, StringComparison.OrdinalIgnoreCase));

			if (node != null && node.Nodes.Count > 0)
				node = node.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Text.Equals(SubTypeField.Text, StringComparison.OrdinalIgnoreCase))
					?? node;
			else if (!String.IsNullOrEmpty(SubTypeField.Text))
				node = null;

			SelectedNode = node;
		}

		///<summary>Raises the <see cref="TreeView.AfterSelect"/> event.</summary>
		protected override void OnAfterSelect(TreeViewEventArgs e) {
			base.OnAfterSelect(e);
			if (e.Action == TreeViewAction.Collapse || e.Action == TreeViewAction.Expand || e.Action == TreeViewAction.Unknown)
				return;
			if (e.Node == null)
				return;

			if (e.Node.Parent == null) {
				TypeField.Text = e.Node.Text;
				SubTypeField.Text = "";
			} else {
				TypeField.Text = e.Node.Parent.Text;
				SubTypeField.Text = e.Node.Text;
			}

			e.Node.Expand();
		}
	}
}
