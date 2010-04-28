using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections;
using ShomreiTorah.Common;

namespace ShomreiTorah.Singularity.DataBinding {
	///<summary>Implemented by PropertyDescriptors that return lists to expose the lists' proeprties.  This is called by ITypedList implementations.</summary>
	interface ITypedListPropertyProvider {
		string ChildListName { get; }
		PropertyDescriptorCollection GetItemProperties();
	}
	static class TypedListUtils {
		public static PropertyDescriptorCollection CreatePropertyDescriptors(this TableSchema schema) {
			var descriptors = new List<PropertyDescriptor>(schema.Columns.Count + schema.ChildRelations.Count);

			descriptors.AddRange(schema.Columns.Select(c => new ColumnPropertyDescriptor(c)));
			descriptors.AddRange(schema.ChildRelations.Select(c => new ChildRelationPropertyDescriptor(c)));

			return new PropertyDescriptorCollection(descriptors.ToArray(), true);
		}


		///<summary>Exposes the GetList method on a class that implements IListSource explicitly.</summary>
		public static IList GetList(this IListSource listSource) { return listSource.GetList(); }

		public static string GetListName(this PropertyDescriptor[] listAccessors, string parentListName) {
			if (listAccessors == null || listAccessors.Length == 0)
				return parentListName;

			return ((ITypedListPropertyProvider)listAccessors.Last()).ChildListName;
		}

		public static PropertyDescriptorCollection GetProperties(this PropertyDescriptor[] listAccessors, Func<PropertyDescriptorCollection> parentProperties) {
			if (listAccessors == null || listAccessors.Length == 0)
				return parentProperties();

			return ((ITypedListPropertyProvider)listAccessors.Last()).GetItemProperties();
		}
	}
}
