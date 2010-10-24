using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections;
using ShomreiTorah.Common;

namespace ShomreiTorah.Singularity.DataBinding {
	///<summary>Implemented by PropertyDescriptors that return lists to expose the lists' properties.  This is called by ITypedList implementations.</summary>
	interface ITypedListPropertyProvider {
		string ChildListName { get; }
		PropertyDescriptorCollection GetItemProperties();
	}
	static class TypedListUtils {
		public static PropertyDescriptorCollection CreatePropertyDescriptors(this Table table) {
			var descriptors = new List<PropertyDescriptor>(table.Schema.Columns.Count + table.Schema.ChildRelations.Count);

			descriptors.AddRange(table.Schema.Columns.Select(c => new ColumnPropertyDescriptor(c)));

			//Only return child relations for schemas
			//that are available in the DataContext. 
			//This allows programs to use typed rows 
			//without using every child table.
			if (table.Context != null) {
				descriptors.AddRange(table.Schema.ChildRelations
					.Where(cr => table.Context.Tables[cr.ChildSchema] != null)
					.Select(c => new ChildRelationPropertyDescriptor(c, table.Context)));
			}

			return new PropertyDescriptorCollection(descriptors.ToArray(), true);
		}

		public static PropertyDescriptorCollection CreatePropertyDescriptors(this TableSchema schema) {
			var descriptors = new List<PropertyDescriptor>(schema.Columns.Count + schema.ChildRelations.Count);

			descriptors.AddRange(schema.Columns.Select(c => new ColumnPropertyDescriptor(c)));
			//Since there's no table, don't add any child properties.
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
