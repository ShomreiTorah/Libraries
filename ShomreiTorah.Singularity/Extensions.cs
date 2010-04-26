using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShomreiTorah.Singularity {
	///<summary>Contains extension methods.</summary>
	public static class Extensions {
		///<summary>Sorts a set of schemas so that each schema is returned after all of its dependencies.</summary>
		///<remarks>If the schemas contain a cyclic dependency, an exception will be thrown.</remarks>
		public static IEnumerable<TableSchema> SortDependencies(this IEnumerable<TableSchema> items) { return items.SortDependencies(ts => ts); }
		///<summary>Sorts a set of schemas so that each schema is returned after all of its dependencies.</summary>
		///<remarks>If the schemas contain a cyclic dependency, an exception will be thrown.</remarks>
		public static IEnumerable<T> SortDependencies<T>(this IEnumerable<T> items, Func<T, TableSchema> schemaSelector) {
			if (items == null) throw new ArgumentNullException("items");
			if (schemaSelector == null) throw new ArgumentNullException("schemaSelector");

			var pendingItems = items.ToList();
			var processedSchemas = new HashSet<TableSchema>();

			var returnedTables = new List<T>();

			while (pendingItems.Any()) {
				returnedTables.Clear();

				foreach (var item in pendingItems) {
					var schema = schemaSelector(item);

					if (!schema.Columns.OfType<ForeignKeyColumn>().Select(fkc => fkc.ForeignSchema).Except(processedSchemas).Any()) {
						returnedTables.Add(item);
						processedSchemas.Add(schema);

						yield return item;
					}
				}

				if (returnedTables.Count == 0)
					throw new InvalidOperationException("Cyclic dependency detected");
				pendingItems.RemoveAll(returnedTables.Contains);
			}
		}
	}
}