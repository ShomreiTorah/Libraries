using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace ShomreiTorah.Singularity {
	///<summary>Contains tables in a Singularity database.</summary>
	public partial class DataContext : IListSource {
		///<summary>Creates a new DataContext.</summary>
		public DataContext() { Tables = new TableCollection(this); }

		///<summary>Gets the tables in this context.</summary>
		public TableCollection Tables { get; private set; }

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
