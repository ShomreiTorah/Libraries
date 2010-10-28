using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Collections;

namespace ShomreiTorah.Singularity.DataBinding {
	///<summary>A base class for a wrapper around a DataContext for use in a designer.</summary>
	public abstract class BindableDataContextBase : Component, IListSource  {
		///<summary>Gets the DataContext instance to bind to at runtime.</summary>
		protected abstract DataContext FindDataContext();

		DataContext dataContext;
		///<summary>Gets the typed DataContext wrapped by this instance.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DataContext DataContext {
			get {
				if (dataContext == null)
					dataContext = FindDataContext();
				return dataContext;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Data-binding support")]
		bool IListSource.ContainsListCollection { get { return true; } }
		[SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Data-binding support")]
		IList IListSource.GetList() { return DataContext.GetList(); }
	}
}
