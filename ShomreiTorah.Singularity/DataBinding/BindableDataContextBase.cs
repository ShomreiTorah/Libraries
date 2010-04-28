using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Collections;

namespace ShomreiTorah.Singularity.DataBinding {
	///<summary>A base class for a wrapper around a typed DataContext for use in a designer.</summary>
	public abstract class BindableDataContextBase<TDataContext> : Component, IListSource where TDataContext : DataContext, new() {

		///<summary>Gets the DataContext instance to bind to at runtime.</summary>
		///<remarks>This method will not be called at design-time.</remarks>
		protected abstract TDataContext FindDataContext();

		TDataContext dataContext;
		///<summary>Gets the typed DataContext wrapped by this instance.</summary>
		public TDataContext DataContext {
			get {
				if (dataContext == null)
					dataContext = DesignMode ? new TDataContext() : FindDataContext();
				return dataContext;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Data-binding support")]
		bool IListSource.ContainsListCollection { get { return true; } }
		[SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Data-binding support")]
		IList IListSource.GetList() { return DataContext.GetList(); }
	}
}
