using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ShomreiTorah.Singularity.DataBinding {
	///<summary>A base class for a wrapper around a typed DataContext for use in a designer.</summary>
	public abstract class BindableDataContextBase<TDataContext> : Component, IListSource where TDataContext : DataContext, new() {

		///<summary>Gets the DataContext instance to bind to at runtime.</summary>
		///<remarks>This method will not be called at design-time.</remarks>
		protected abstract TDataContext FindDataContext();
		bool IListSource.ContainsListCollection { get { return true; } }

		TDataContext dataContext;
		public TDataContext DataContext {
			get {
				if (dataContext == null)
					dataContext = DesignMode ? new TDataContext() : FindDataContext();
				return dataContext;
			}
		}
		System.Collections.IList IListSource.GetList() { return DataContext.Tables; }
	}
}
