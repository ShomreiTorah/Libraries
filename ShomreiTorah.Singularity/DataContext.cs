using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using ShomreiTorah.Common;

namespace ShomreiTorah.Singularity {
	///<summary>Contains tables in a Singularity database.</summary>
	public partial class DataContext : IListSource {
		///<summary>Creates a new DataContext.</summary>
		public DataContext() { Tables = new TableCollection(this); }

		///<summary>Gets the tables in this context.</summary>
		public TableCollection Tables { get; private set; }

		///<summary>Gets the typed table in this context that holds the given typed row.</summary>
		///<typeparam name="TRow">The typed rows in the table.</typeparam>
		///<returns>A typed table, or null if this context does not contain the schema.</returns>
		public TypedTable<TRow> Table<TRow>() where TRow : Row {
			if (TypedSchema<TRow>.Instance == null) return null;
			return (TypedTable<TRow>)Tables[TypedSchema<TRow>.Instance];
		}

		///<summary>Marks the table as loading data until the return value is disposed.
		///When the return value is disposed, the LoadCompleted event will be raised,
		///unless the table is still loading data from another call.</summary>
		///<param name="threadContext">An optional SynchronizationContext to
		///raise the LoadCompleted event on.</param>
		public IDisposable BeginLoadData(SynchronizationContext threadContext = null) {
			var tables = Tables.Select(t => t.BeginLoadData(threadContext)).ToList();
			return new Disposable(() => tables.ForEach(t => t.Dispose()));
		}

		[SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Data-binding support")]
		bool IListSource.ContainsListCollection { get { return true; } }
		DataBinding.DataContextBinder binder;
		[SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Data-binding support")]
		IList IListSource.GetList() {
			if (binder == null)
				binder = new DataBinding.DataContextBinder(this);
			return binder;
		}
	}
}
