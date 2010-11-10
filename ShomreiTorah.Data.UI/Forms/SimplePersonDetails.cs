using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using ShomreiTorah.Singularity;

namespace ShomreiTorah.Data.UI.Forms {
	///<summary>A form which displays details about a person.</summary>
	public partial class SimplePersonDetails : XtraForm {
		readonly Person person;
		readonly TypedTable<Person> table;
		///<summary>Creates a SimplePersonDetails form that displays the specified person.</summary>
		public SimplePersonDetails(Person person) {
			InitializeComponent();

			this.person = person;
			table = person.Table;
			if (table != null) {
				table.ValueChanged += table_ValueChanged;
				table.RowRemoved += table_RowRemoved;
			}
			editor.Person = person;
			UpdateDetails();
		}

		//These events can be fired from the ThreadPool when
		//refreshing data if the row changed  on the server.
		void table_ValueChanged(object sender, ValueChangedEventArgs<Person> e) {
			if (e.Row == person)
				TryInvoke(UpdateDetails);
		}
		void table_RowRemoved(object sender, RowListEventArgs<Person> e) {
			if (e.Row == person)
				TryInvoke(Close);
		}

		///<summary>Releases the unmanaged resources used by the SimplePersonDetails and optionally releases the managed resources.</summary>
		///<param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				if (table != null) {
					table.ValueChanged -= table_ValueChanged;
					table.RowRemoved -= table_RowRemoved;
				}
				if (components != null) components.Dispose();
			}
			base.Dispose(disposing);
		}

		void TryInvoke(Action method) {
			if (IsHandleCreated && InvokeRequired)
				BeginInvoke(method);
			else
				method();
		}

		///<summary>Processes a command key.</summary>
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
			if (keyData == Keys.Escape) {
				Close();
				return false;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}
		void UpdateDetails() {
			Text = person.VeryFullName;
			map.Text = person.FullName;
			map.AddressString = person.Address + Environment.NewLine + person.City + ", " + person.State + "  " + person.Zip;

			detailText.Text = person.ToFullString();
		}

		private void showEditor_Click(object sender, EventArgs e) {
			detailText.Hide();
			editorPanel.Show();
			AcceptButton = closeEditor;
		}

		private void closeEditor_Click(object sender, EventArgs e) {
			UpdateDetails();
			editorPanel.Hide();
			detailText.Show();
			AcceptButton = null;
		}
	}
}