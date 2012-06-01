using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LINQPad;
using ShomreiTorah.Singularity;

namespace ShomreiTorah.Singularity {
	partial class Row : ICustomMemberProvider {
		IEnumerable<string> LINQPad.ICustomMemberProvider.GetNames() {
			return Schema.Columns.Select(c => c.Name)
				.Concat(Schema.ChildRelations.Select(r => r.Name));
		}
		IEnumerable<Type> LINQPad.ICustomMemberProvider.GetTypes() {
			return Schema.Columns.Select(c => c.DataType)
				.Concat(Schema.ChildRelations.Select(r => typeof(ICollection<Row>)));
		}
		IEnumerable<object> LINQPad.ICustomMemberProvider.GetValues() {
			return Schema.Columns.Select(c => this[c])
				.Concat(Schema.ChildRelations.Select(r => (object)this.ChildRows(r)));
		}
	}

	class RowWrapper : ICustomMemberProvider {
		public RowWrapper(Row row) { this.row = row; }
		readonly Row row;

		IEnumerable<string> LINQPad.ICustomMemberProvider.GetNames() { return row.Schema.Columns.Select(c => c.Name); }
		IEnumerable<Type> LINQPad.ICustomMemberProvider.GetTypes() { return row.Schema.Columns.Select(c => c.DataType); }
		IEnumerable<object> LINQPad.ICustomMemberProvider.GetValues() { return row.Schema.Columns.Select(c => row[c]); }
	}
	partial class Table : ICustomMemberProvider {
		IEnumerable<string> ICustomMemberProvider.GetNames() {
			yield return "Rows";
			yield return "Schema";
		}

		IEnumerable<Type> ICustomMemberProvider.GetTypes() {
			yield return typeof(IEnumerable<RowWrapper>);
			yield return typeof(TableSchema);
		}

		IEnumerable<object> ICustomMemberProvider.GetValues() {
			yield return Rows.Select(r => new RowWrapper(r));
			yield return Schema;
		}
	}
	partial class FilteredTable<TRow> : ICustomMemberProvider {
		IEnumerable<string> ICustomMemberProvider.GetNames() {
			yield return "Rows";
			yield return "Schema";
		}

		IEnumerable<Type> ICustomMemberProvider.GetTypes() {
			yield return typeof(IEnumerable<RowWrapper>);
			yield return typeof(TableSchema);
		}

		IEnumerable<object> ICustomMemberProvider.GetValues() {
			yield return Rows.Select(r => new RowWrapper(r));
			yield return Table.Schema;
		}
	}
}

namespace LINQPad {
	public interface ICustomMemberProvider {
		// Each of these methods must return a sequence
		// with the same number of elements:
		IEnumerable<string> GetNames();
		IEnumerable<Type> GetTypes();
		IEnumerable<object> GetValues();
	}
}