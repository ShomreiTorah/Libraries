using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using ShomreiTorah.Common;

namespace ShomreiTorah.Singularity.Sql {
	///<summary>Synchronizes a Singularity table with a table in an SQL database.</summary>
	public class TableSynchronizer {
		///<summary>Creates a TableSynchronizer for the specified table.</summary>
		public TableSynchronizer(Table table, SchemaMapping mapping, ISqlProvider sqlProvider) {
			if (table == null) throw new ArgumentNullException("table");
			if (mapping == null) throw new ArgumentNullException("mapping");
			if (sqlProvider == null) throw new ArgumentNullException("sqlProvider");

			if (table.Schema != mapping.Schema) throw new ArgumentException("Table and mapping must have the same schema", "mapping");

			Table = table;
			Mapping = mapping;
			SqlProvider = sqlProvider;

			Table.RowAdded += Table_RowAdded;
			Table.ValueChanged += Table_ValueChanged;
			Table.RowRemoved += Table_RowRemoved;

			Changes = new ReadOnlyCollection<RowChange>(changes);
		}


		///<summary>Gets the table that this TableSynchronizer synchronizes.</summary>
		public Table Table { get; private set; }
		///<summary>Gets the SchemaMapping that maps the Singularity table to the SQL table.</summary>
		public SchemaMapping Mapping { get; private set; }
		///<summary>Gets an ISqlProvider implementation used to create SQL commands for the database.</summary>
		public ISqlProvider SqlProvider { get; private set; }

		#region Read Database
		///<summary>Populates this instance's table from the database.</summary>
		public void ReadData() {
			using (var connection = SqlProvider.OpenConnection())
				ReadData(connection);
		}
		///<summary>Populates this instance's table from the database.</summary>
		public void ReadData(DbConnection connection) { ReadData(connection, null); }
		///<summary>Populates this instance's table from the database.</summary>
		///<param name="connection">A connection to the database.</param>
		///<param name="threadContext">An optional SynchronizationContext for the thread to raise the LoadCompleted event on.</param>
		public void ReadData(DbConnection connection, SynchronizationContext threadContext) {
			using (Table.BeginLoadData())
			using (var command = SqlProvider.CreateSelectCommand(connection, Mapping)) {
				try {
					isReadingDB = true;

					using (var reader = command.ExecuteReader()) {
						new DataReaderTablePopulator(Table, Mapping, reader).FillTable();
					}
					changes.Clear();
				} finally { isReadingDB = false; }
			}
			if (threadContext == null)
				Table.OnLoadCompleted();
			else
				threadContext.Post(delegate { Table.OnLoadCompleted(); }, null);
		}
		sealed class DataReaderTablePopulator : TablePopulator<IDataRecord> {
			readonly DbDataReader reader;
			readonly SchemaMapping mapping;

			readonly int[] columnIndices;
			readonly int primaryKeyIndex;
			readonly int rowVersionIndex;
			public DataReaderTablePopulator(Table table, SchemaMapping mapping, DbDataReader reader)
				: base(table) {
				this.reader = reader;
				this.mapping = mapping;
				columnIndices = mapping.Columns.Select(c => reader.GetOrdinal(c.SqlName)).ToArray();
				if (table.Schema.PrimaryKey != null)
					primaryKeyIndex = reader.GetOrdinal(mapping.Columns[table.Schema.PrimaryKey].SqlName);
				rowVersionIndex = reader.GetOrdinal("RowVersion");
			}

			protected override IEnumerable<Column> Columns { get { return mapping.Columns.Select(cm => cm.Column); } }

			protected override IEnumerable<IDataRecord> GetRows() { return reader.Cast<IDataRecord>(); }

			protected override IEnumerable<KeyValuePair<Column, object>> GetValues(IDataRecord values) {
				return columnIndices.Select((readerIndex, tableIndex) => new KeyValuePair<Column, object>(mapping.Columns[tableIndex].Column, values[readerIndex]));
			}
			protected override void OnRowPopulated(Row row, IDataRecord values) {
				base.OnRowPopulated(row, values);
				row.RowVersion = values.GetValue(rowVersionIndex);
			}

			[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
			protected override object GetPrimaryKey(IDataRecord values) { return values[primaryKeyIndex]; }
		}
		#endregion
		#region Write Database
		///<summary>If true, all change events will be ignored.</summary>
		bool isReadingDB;

		readonly List<RowChange> changes = new List<RowChange>();
		///<summary>Gets the changed rows that have not been persisted to the database.</summary>
		public ReadOnlyCollection<RowChange> Changes { get; private set; }

		void Table_RowAdded(object sender, RowListEventArgs e) {
			if (isReadingDB) return;
			var changeIndex = GetChangeIndex(e.Row);
			if (changeIndex >= 0) {
				Debug.Assert(changes[changeIndex].ChangeType == RowChangeType.Removed, "Detached row was " + changes[changeIndex].ChangeType);
				changes.RemoveAt(changeIndex);
				changes.Add(new RowChange(e.Row, RowChangeType.Changed));	//Assume that the row was changed while it was detached
			} else
				changes.Add(new RowChange(e.Row, RowChangeType.Added));
		}

		void Table_ValueChanged(object sender, ValueChangedEventArgs e) {
			if (isReadingDB) return;
			var changeIndex = GetChangeIndex(e.Row);

			if (changeIndex >= 0)
				Debug.Assert(changes[changeIndex].ChangeType != RowChangeType.Removed);	//If it wasn't added to the DB yet, or if it's already been changed, we don't need to track value changes
			else
				changes.Add(new RowChange(e.Row, RowChangeType.Changed));
		}

		void Table_RowRemoved(object sender, RowListEventArgs e) {
			if (isReadingDB) return;
			var changeIndex = GetChangeIndex(e.Row);
			if (changeIndex >= 0) {
				var type = changes[changeIndex].ChangeType;

				Debug.Assert(type != RowChangeType.Removed, "Row was removed twice");

				//If a row was added, then removed, we shouldn't 
				//record any change.  If a row was changed, then 
				//removed, we should remove the Change change and
				//add a Remove change.

				changes.RemoveAt(changeIndex);

				if (type != RowChangeType.Added)
					changes.Add(new RowChange(e.Row, RowChangeType.Removed));

			} else
				changes.Add(new RowChange(e.Row, RowChangeType.Removed));
		}

		int GetChangeIndex(Row row) {
			return changes.FindIndex(rc => rc.Row == row);
		}

		///<summary>Saves changes in this instance's table to the database.</summary>
		public void WriteData() { WriteData(progress: null); }
		///<summary>Saves changes in this instance's table to the database.</summary>
		public void WriteData(IProgressReporter progress) {
			using (var connection = SqlProvider.OpenConnection())
				WriteData(connection, progress);
		}
		///<summary>Saves changes in this instance's table to the database.</summary>
		public void WriteData(DbConnection connection) { WriteData(connection, null); }
		///<summary>Saves changes in this instance's table to the database.</summary>
		public void WriteData(DbConnection connection, IProgressReporter progress) {
			if (connection == null) throw new ArgumentNullException("connection");

			using (var transactionContext = new TransactionContext(connection)) {
				WriteChanges(transactionContext, null, progress);
				transactionContext.Commit();
			}
			//If we didn't get an exception, clear the changes.
			//If we did, don't clear, since we didn't commit it
			ClearChanges();
		}

		internal void WriteChanges(TransactionContext context, RowChangeType? changeType, IProgressReporter progress) {
			IEnumerable<RowChange> relevantChanges;
			progress = progress ?? new EmptyProgressReporter();
			progress.Caption = "Saving " + Table.Schema.Name;

			if (changeType == null)
				relevantChanges = changes;
			else
				relevantChanges = changes.Where(rc => rc.ChangeType == changeType);

			progress.Maximum = relevantChanges.Count();
			int i = 0;
			foreach (var change in relevantChanges) {
				progress.Progress = i++;
				try {
					switch (change.ChangeType) {
						case RowChangeType.Added:
							SqlProvider.ApplyInsert(context, Mapping, change.Row);
							break;
						case RowChangeType.Changed:
							SqlProvider.ApplyUpdate(context, Mapping, change.Row);
							break;
						case RowChangeType.Removed:
							SqlProvider.ApplyDelete(context, Mapping, change.Row);
							break;
						default:
							throw new InvalidOperationException("Unknown change type: " + change.ChangeType);
					}
				} catch (RowDeletedException) {
					if (change.ChangeType == RowChangeType.Removed)
						continue;	//If two users delete the same row, don't complain

					//If the row was previously deleted, we
					//need to convert the change to an Add 
					//change (which will itself be removed 
					//if our user deletes the row).
					changes.Remove(change);
					changes.Add(new RowChange(change.Row, RowChangeType.Added));
					throw;
				} catch (RowException) {
					throw;
				} catch (Exception ex) {
					throw new RowException(change.Row, ex);
				}
			}
		}

		internal void ClearChanges() { changes.Clear(); }
		#endregion
	}

	//None of these exceptions can be serialized correctly,
	//since it is not possible to serialize a Row instance.
	//(Rows require TableSchemas and foreign keys)
	//However, the Exceptions must be serializable so that 
	//they can cross AppDomains.

	///<summary>Thrown when an an error occurs while saving a row to the database.</summary>
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "Specialized Exception")]
	[Serializable]
	public class RowException : Exception {
		///<summary>Creates a new RowException instance.</summary>
		public RowException(Row row, string message)
			: base(message) {
			if (row == null) throw new ArgumentNullException("row");
			Row = row;
		}
		///<summary>Creates a new RowException instance.</summary>
		public RowException(Row row, Exception inner)
			: base((inner ?? new InvalidOperationException()).Message, inner) {
			if (row == null) throw new ArgumentNullException("row");
			if (inner == null) throw new ArgumentNullException("inner");

			Row = row;
		}

		///<summary>Serialization constructor</summary>
		protected RowException(SerializationInfo info, StreamingContext context) : base(info, context) { }

		///<summary>Gets the row that was modified in the database.</summary>
		public Row Row { get; private set; }
	}

	///<summary>Thrown when an existing row was modified in the database.</summary>
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "Specialized Exception")]
	[Serializable]
	public class RowModifiedException : RowException {
		///<summary>Creates a new RowModifiedException instance.</summary>
		public RowModifiedException(Row row, IDictionary<Column, object> dbValues)
			: base(row, "The row was modified in the database") {
			if (dbValues == null) throw new ArgumentNullException("dbValues");
			DatabaseValues = new ReadOnlyDictionary<Column, object>(dbValues);
		}

		///<summary>Serialization constructor</summary>
		protected RowModifiedException(SerializationInfo info, StreamingContext context) : base(info, context) { }

		///<summary>Gets the current values of the row in the database.</summary>
		public ReadOnlyDictionary<Column, object> DatabaseValues { get; private set; }
	}
	///<summary>Thrown when an existing row was deleted in the database.</summary>
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "Specialized Exception")]
	[Serializable]
	public class RowDeletedException : RowException {
		///<summary>Creates a new RowModifiedException instance.</summary>
		public RowDeletedException(Row row) : base(row, "The row was deleted in the database") { }

		///<summary>Serialization constructor</summary>
		protected RowDeletedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	///<summary>Represents a changed row that will be synchronized to an SQL database.</summary>
	public sealed class RowChange {
		internal RowChange(Row row, RowChangeType type) { Row = row; ChangeType = type; }

		///<summary>Gets the row that was changed.</summary>
		public Row Row { get; private set; }

		///<summary>Gets the way that the row was changed.</summary>
		public RowChangeType ChangeType { get; private set; }

		///<summary>Returns a string representation of this change.</summary>
		public override string ToString() {
			return ChangeType + ": " + Row;
		}
	}
	///<summary>A type of change in a tracked row.</summary>
	public enum RowChangeType {
		///<summary>The row was added to a Singularity table.</summary>
		Added,
		///<summary>The row's values were changed in its Singularity table.</summary>
		Changed,
		///<summary>The row was removed from a Singularity table.</summary>
		Removed
	}
}
