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
	public sealed partial class CalculatedColumn : Column {
		///<summary>An instance used when a calculated column's value has not been calculated.</summary>
		///<remarks>Calculated columns are lazy, and will only run the delegate if the column's
		///value equals this object.  The dependency manager will set the column's value to this
		///object whenever a dependency changes, triggering a recalc when the value is next fetched.
		///
		///Calculated columns are not supported in detached rows; any calculated column in a
		///detached row will equal the column's DefaultValue property.</remarks>
		internal static readonly object UncalculatedValue = new object();

		readonly Func<Row, object> func;

		//The dependency is only created when first needed to
		//allow for cyclic dependencies during schema creation
		//This is required for typed rows that have calculated
		//columns which use child rows, since the columns are
		//added by the static ctor.
		internal CalculatedColumn(TableSchema schema, string name, Type dataType, Func<Row, object> func, Func<Dependency> dependencyGenerator)
			: base(schema, name) {
			CanValidate = false;
			this.func = func;
			DataType = dataType;
			ReadOnly = true;

			this.dependencyGenerator = dependencyGenerator;

			//TODO: Loop over empty tables
			foreach (var table in Schema.Rows.Select(r => r.Table).Distinct()) {
				if (!Dependency.RequiresDataContext || table.Context != null)
					Dependency.Register(table);
			}

			if (dataType.IsValueType)
				base.DefaultValue = Activator.CreateInstance(dataType);
			// Otherwise, keep it null.  Either way, don't run our
			// setter, since the column hasn't been added yet.
		}

		Func<Dependency> dependencyGenerator;
		Dependency dependency;
		///<summary>Gets a dependency object that can track changes to the data that this column depends on.</summary>
		internal Dependency Dependency {
			get {
				if (dependency == null) {
					dependency = dependencyGenerator();
					dependency.RowInvalidated += Dependency_RowInvalidated;
					dependencyGenerator = null;
				}
				return dependency;
			}
		}

		void Dependency_RowInvalidated(object sender, RowEventArgs e) {
			e.Row.InvalidateCalculatedValue(this);
		}

		///<summary>Gets the value that will be used for this column in a detached row.</summary>
		///<remarks>By default, this will be equal to the default value for the column's type.</remarks>
		public override object DefaultValue {
			get { return base.DefaultValue; }
			set { base.DefaultValue = value; Schema.EachRow(r => r.ToggleCalcColDefault(this)); }
		}
		///<summary>Calculates this column's value in a row.</summary>
		public object CalculateValue(Row row) {
			if (row == null) throw new ArgumentNullException("row");
			if (row.Schema != Schema) throw new ArgumentException("Row must belong to the schema containing this column");
			return func(row);
		}

		internal override void OnRowAdded(Row row) { row.ToggleCalcColDefault(this); }
		internal override void OnRowRemoved(Row row) { row.ToggleCalcColDefault(this); }

		///<summary>This method is not supported.</summary>
		public override string ValidateValue(Row row, object value) { throw new NotSupportedException(); }
		///<summary>This method is not supported.</summary>
		public override string ValidateValueType(object value) { throw new NotSupportedException(); }

	}
	//The public AddCalculatedColumn take strongly-typed
	//expression trees with actual types of the value and
	//the row.  The column needs a weakly-typed delegate
	//to calculate the values, so the generic Add methods
	//generate weakly-typed delegates.  Variance will not
	//help for typed rows.
	partial class ColumnCollection {
		///<summary>Adds a calculated column to the schema.</summary>
		///<typeparam name="TValue">The type of the column's value.</typeparam>
		///<param name="name">The name of the column.</param>
		///<param name="expression">An expression used to calculate the column's value.</param>
		public CalculatedColumn AddCalculatedColumn<TValue>(string name, Expression<Func<Row, TValue>> expression) {
			if (expression == null) throw new ArgumentNullException("expression");
			return AddCalculatedColumn<Row, TValue>(name, expression);
		}
		///<summary>Adds a calculated column to the schema.</summary>
		///<typeparam name="TValue">The type of the column's value.</typeparam>
		///<typeparam name="TRow">The strongly-typed row used to calculate the column's value.</typeparam>
		///<param name="name">The name of the column.</param>
		///<param name="expression">An expression used to calculate the column's value.</param>
		public CalculatedColumn AddCalculatedColumn<TRow, TValue>(string name, Expression<Func<TRow, TValue>> expression) where TRow : Row {
			if (expression == null) throw new ArgumentNullException("expression");
			var compiled = expression.Compile();

			return AddColumn(new CalculatedColumn(Schema, name, typeof(TValue), row => compiled((TRow)row), () => DependencyParser.GetDependencyTree(Schema, expression)));
		}
	}
}