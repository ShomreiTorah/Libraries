using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;
using ShomreiTorah.Singularity.Dependencies;

namespace ShomreiTorah.Singularity {
	///<summary>A column containing a value that is calculated automatically.</summary>
	public sealed class CalculatedColumn : Column {
		///<summary>An instances used when a calculated column's value has not been calculated.</summary>
		///<remarks>Calculated columns are lazy, and will only run the delegate if the column's
		///value is this object.  The dependency manager will set the column's value to this object
		///whenever a dependency changes, triggering a recalc when the value is next fetched.</remarks>
		internal static readonly object UncalculatedValue = new object();

		readonly Func<Row, object> func;

		internal CalculatedColumn(TableSchema schema, string name, Type dataType, Func<Row, object> func, Dependency dependency)
			: base(schema, name) {
			this.func = func;
			DefaultValue = UncalculatedValue;
			DataType = dataType;
			ReadOnly = true;
			Dependency = dependency;
			Dependency.RowInvalidated += Dependency_RowInvalidated;

			foreach (var table in Schema.Rows.Select(r => r.Table).Distinct()) {
				if (!Dependency.RequiresDataContext || table.Context != null)
					Dependency.Register(table);
			}
		}

		void Dependency_RowInvalidated(object sender, RowEventArgs e) {
			e.Row.InvalidateCalculatedValue(this);
		}

		///<summary>Calculates this column's value in a row.</summary>
		public object CalculateValue(Row row) {
			if (row == null) throw new ArgumentNullException("row");
			if (row.Schema != Schema) throw new ArgumentException("Row must belong to the schema containing this column");
			return func(row);
		}

		///<summary>This method is not supported.</summary>
		public override string ValidateValue(Row row, object value) { throw new NotSupportedException(); }
		///<summary>This method is not supported.</summary>
		public override string ValidateValueType(object value) { throw new NotSupportedException(); }

		///<summary>Gets a dependency object that can track changes to the data that this column depends on.</summary>
		internal Dependency Dependency { get; private set; }
	}
	//The public AddCalculatedColumn take strongly-typed 
	//expression trees with actual types of the value and
	//the row.  The column needs a weakly-typed delegate 
	//to calculate the values, so the generic Add methods
	//generate weakly-typed delegates.
	partial class ColumnCollection {
		///<summary>Adds a calculated column to the schema.</summary>
		///<typeparam name="TValue">The type of the column's value.</typeparam>
		///<param name="name">The name of the column.</param>
		///<param name="expression">An expression used to calculate the column's value.</param>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Expression tree")]
		public CalculatedColumn AddCalculatedColumn<TValue>(string name, Expression<Func<Row, TValue>> expression) {
			if (expression == null) throw new ArgumentNullException("expression");
			var compiled = expression.Compile();

			return AddColumn(new CalculatedColumn(Schema, name, typeof(TValue), row => compiled(row), DependencyParser.GetDependencyTree(Schema, expression)));
		}
		///<summary>Adds a calculated column to the schema.</summary>
		///<typeparam name="TValue">The type of the column's value.</typeparam>
		///<typeparam name="TRow">The strongly-typed row used to calculate the column's value.</typeparam>
		///<param name="name">The name of the column.</param>
		///<param name="expression">An expression used to calculate the column's value.</param>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Expression tree")]
		public CalculatedColumn AddCalculatedColumn<TRow, TValue>(string name, Expression<Func<TRow, TValue>> expression) where TRow : Row {
			if (expression == null) throw new ArgumentNullException("expression");
			var compiled = expression.Compile();

			return AddColumn(new CalculatedColumn(Schema, name, typeof(TValue), row => compiled((TRow)row), DependencyParser.GetDependencyTree(Schema, expression)));
		}
	}
}