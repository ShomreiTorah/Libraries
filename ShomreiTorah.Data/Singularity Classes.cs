using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Linq;
using ShomreiTorah.Singularity;
using ShomreiTorah.Singularity.Sql;

namespace ShomreiTorah.Data {
    ///<summary>Describes an ad in the journal.</summary>
    public partial class JournalAd : Row {
        ///<summary>Creates a new JournalAd instance.</summary>
        public JournalAd () : base(Schema) { Initialize(); }
        partial void Initialize();
        
        ///<summary>Creates a strongly-typed Ads table.</summary>
        public static TypedTable<JournalAd> CreateTable() { return new TypedTable<JournalAd>(Schema, () => new JournalAd()); }
        
        ///<summary>Gets the schema's AdId column.</summary>
        public static ValueColumn AdIdColumn { get; private set; }
        ///<summary>Gets the schema's Year column.</summary>
        public static ValueColumn YearColumn { get; private set; }
        ///<summary>Gets the schema's DateAdded column.</summary>
        public static ValueColumn DateAddedColumn { get; private set; }
        ///<summary>Gets the schema's AdType column.</summary>
        public static ValueColumn AdTypeColumn { get; private set; }
        ///<summary>Gets the schema's ExternalId column.</summary>
        public static ValueColumn ExternalIdColumn { get; private set; }
        ///<summary>Gets the schema's Comments column.</summary>
        public static ValueColumn CommentsColumn { get; private set; }
        
        ///<summary>Gets the Ads schema instance.</summary>
        public static new TypedSchema<JournalAd> Schema { get; private set; }
        ///<summary>Gets the SchemaMapping that maps this schema to the SQL Server Ads table.</summary>
        public static SchemaMapping SchemaMapping { get; private set; }
        
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        static JournalAd() {
            #region Create Schema
            Schema = new TypedSchema<JournalAd>("Ads");
            
            Schema.PrimaryKey = AdIdColumn = Schema.Columns.AddValueColumn("AdId", typeof(Guid), null);
            AdIdColumn.Unique = true;
            AdIdColumn.AllowNulls = false;
            
            YearColumn = Schema.Columns.AddValueColumn("Year", typeof(Int32), null);
            YearColumn.AllowNulls = false;
            
            DateAddedColumn = Schema.Columns.AddValueColumn("DateAdded", typeof(DateTime), null);
            DateAddedColumn.AllowNulls = false;
            
            AdTypeColumn = Schema.Columns.AddValueColumn("AdType", typeof(String), null);
            AdTypeColumn.AllowNulls = false;
            
            ExternalIdColumn = Schema.Columns.AddValueColumn("ExternalId", typeof(Int32), null);
            ExternalIdColumn.AllowNulls = false;
            
            CommentsColumn = Schema.Columns.AddValueColumn("Comments", typeof(String), null);
            CommentsColumn.AllowNulls = true;
            #endregion
            
            #region Create SchemaMapping
            SchemaMapping = new SchemaMapping(Schema, false);
            SchemaMapping.SqlName = "Ads";
            SchemaMapping.SqlSchemaName = "MelaveMalka";
            
            SchemaMapping.Columns.AddMapping(AdIdColumn, "AdId");
            SchemaMapping.Columns.AddMapping(YearColumn, "Year");
            SchemaMapping.Columns.AddMapping(DateAddedColumn, "DateAdded");
            SchemaMapping.Columns.AddMapping(AdTypeColumn, "AdType");
            SchemaMapping.Columns.AddMapping(ExternalIdColumn, "ExternalId");
            SchemaMapping.Columns.AddMapping(CommentsColumn, "Comments");
            #endregion
            SchemaMapping.SetPrimaryMapping(SchemaMapping);
        }
        
        ///<summary>Gets the typed table that contains this row, if any.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public new TypedTable<JournalAd> Table { get { return (TypedTable<JournalAd>)base.Table; } }
        #region Value Properties
        ///<summary>Gets or sets the ad id of the ad.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Guid AdId {
            get { return base.Field<Guid>(AdIdColumn); }
            set { base[AdIdColumn] = value; }
        }
        ///<summary>Gets or sets the year of the journal containing the ad.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Int32 Year {
            get { return base.Field<Int32>(YearColumn); }
            set { base[YearColumn] = value; }
        }
        ///<summary>Gets or sets the date that the ad was entered.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public DateTime DateAdded {
            get { return base.Field<DateTime>(DateAddedColumn); }
            set { base[DateAddedColumn] = value; }
        }
        ///<summary>Gets or sets the ad type.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String AdType {
            get { return base.Field<String>(AdTypeColumn); }
            set { base[AdTypeColumn] = value; }
        }
        ///<summary>Gets or sets an external identifier for the ad.  This field links ads to pledges and payments</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Int32 ExternalId {
            get { return base.Field<Int32>(ExternalIdColumn); }
            set { base[ExternalIdColumn] = value; }
        }
        ///<summary>Gets or sets comments regarding the ad.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Comments {
            get { return base.Field<String>(CommentsColumn); }
            set { base[CommentsColumn] = value; }
        }
        #endregion
        
        #region Partial Methods
        partial void OnColumnChanged(Column column, object oldValue, object newValue);
        
        partial void ValidateAdId(Guid newValue, Action<string> error);
        partial void OnAdIdChanged(Guid? oldValue, Guid? newValue);
        
        partial void ValidateYear(Int32 newValue, Action<string> error);
        partial void OnYearChanged(Int32? oldValue, Int32? newValue);
        
        partial void ValidateDateAdded(DateTime newValue, Action<string> error);
        partial void OnDateAddedChanged(DateTime? oldValue, DateTime? newValue);
        
        partial void ValidateAdType(String newValue, Action<string> error);
        partial void OnAdTypeChanged(String oldValue, String newValue);
        
        partial void ValidateExternalId(Int32 newValue, Action<string> error);
        partial void OnExternalIdChanged(Int32? oldValue, Int32? newValue);
        
        partial void ValidateComments(String newValue, Action<string> error);
        partial void OnCommentsChanged(String oldValue, String newValue);
        #endregion
        
        #region Column Callbacks
        ///<summary>Checks whether a value would be valid for a given column in an attached row.</summary>
        ///<param name="column">The column containing the value.</param>
        ///<param name="newValue">The value to validate.</param>
        ///<returns>An error message, or null if the value is valid.</returns>
        ///<remarks>This method is overridden by typed rows to perform custom validation logic.</remarks>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public override string ValidateValue(Column column, object newValue) {
            string error = base.ValidateValue(column, newValue);
            if (!String.IsNullOrEmpty(error)) return error;
            Action<string> reporter = s => error = s;
            
            if (column == AdIdColumn) {
                ValidateAdId((Guid)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == YearColumn) {
                ValidateYear((Int32)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == DateAddedColumn) {
                ValidateDateAdded((DateTime)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == AdTypeColumn) {
                ValidateAdType((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == ExternalIdColumn) {
                ValidateExternalId((Int32)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == CommentsColumn) {
                ValidateComments((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            }
            return null;
        }
        ///<summary>Processes an explicit change of a column value.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        protected override void OnValueChanged(Column column, object oldValue, object newValue) {
            base.OnValueChanged(column, oldValue, newValue);
            OnColumnChanged(column, oldValue, newValue);
            if (column == AdIdColumn)
            	OnAdIdChanged((Guid?)oldValue, (Guid?)newValue);
            else if (column == YearColumn)
            	OnYearChanged((Int32?)oldValue, (Int32?)newValue);
            else if (column == DateAddedColumn)
            	OnDateAddedChanged((DateTime?)oldValue, (DateTime?)newValue);
            else if (column == AdTypeColumn)
            	OnAdTypeChanged((String)oldValue, (String)newValue);
            else if (column == ExternalIdColumn)
            	OnExternalIdChanged((Int32?)oldValue, (Int32?)newValue);
            else if (column == CommentsColumn)
            	OnCommentsChanged((String)oldValue, (String)newValue);
        }
        #endregion
    }
    
    ///<summary>Describes a person who will make phone calls asking for ads.</summary>
    public partial class Caller : Row {
        ///<summary>Creates a new Caller instance.</summary>
        public Caller () : base(Schema) { Initialize(); }
        partial void Initialize();
        
        ///<summary>Creates a strongly-typed Callers table.</summary>
        public static TypedTable<Caller> CreateTable() { return new TypedTable<Caller>(Schema, () => new Caller()); }
        
        ///<summary>Gets the schema's RowId column.</summary>
        public static ValueColumn RowIdColumn { get; private set; }
        ///<summary>Gets the schema's Person column.</summary>
        public static ForeignKeyColumn PersonColumn { get; private set; }
        ///<summary>Gets the schema's DateAdded column.</summary>
        public static ValueColumn DateAddedColumn { get; private set; }
        ///<summary>Gets the schema's Year column.</summary>
        public static ValueColumn YearColumn { get; private set; }
        ///<summary>Gets the schema's EmailAddresses column.</summary>
        public static CalculatedColumn EmailAddressesColumn { get; private set; }
        ///<summary>Gets the schema's CalleeCount column.</summary>
        public static CalculatedColumn CalleeCountColumn { get; private set; }
        
        ///<summary>Gets the Callers schema instance.</summary>
        public static new TypedSchema<Caller> Schema { get; private set; }
        ///<summary>Gets the SchemaMapping that maps this schema to the SQL Server Callers table.</summary>
        public static SchemaMapping SchemaMapping { get; private set; }
        
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        static Caller() {
            #region Create Schema
            Schema = new TypedSchema<Caller>("Callers");
            
            Schema.PrimaryKey = RowIdColumn = Schema.Columns.AddValueColumn("RowId", typeof(Guid), null);
            RowIdColumn.Unique = true;
            RowIdColumn.AllowNulls = false;
            
            PersonColumn = Schema.Columns.AddForeignKey("Person", ShomreiTorah.Data.Person.Schema, "Callers");
            PersonColumn.AllowNulls = false;
            
            DateAddedColumn = Schema.Columns.AddValueColumn("DateAdded", typeof(DateTime), null);
            DateAddedColumn.AllowNulls = false;
            
            YearColumn = Schema.Columns.AddValueColumn("Year", typeof(Int32), null);
            YearColumn.AllowNulls = false;
            
            EmailAddressesColumn = Schema.Columns.AddCalculatedColumn<Caller, String>("EmailAddresses", row => String.Join(", ", row.Person.EmailAddresses.Select(e => e.Email)));
            
            CalleeCountColumn = Schema.Columns.AddCalculatedColumn<Caller, Int32>("CalleeCount", row => row.Callees.Count);
            #endregion
            
            #region Create SchemaMapping
            SchemaMapping = new SchemaMapping(Schema, false);
            SchemaMapping.SqlName = "Callers";
            SchemaMapping.SqlSchemaName = "MelaveMalka";
            
            SchemaMapping.Columns.AddMapping(RowIdColumn, "RowId");
            SchemaMapping.Columns.AddMapping(PersonColumn, "PersonId");
            SchemaMapping.Columns.AddMapping(DateAddedColumn, "DateAdded");
            SchemaMapping.Columns.AddMapping(YearColumn, "Year");
            #endregion
            SchemaMapping.SetPrimaryMapping(SchemaMapping);
        }
        
        ///<summary>Gets the typed table that contains this row, if any.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public new TypedTable<Caller> Table { get { return (TypedTable<Caller>)base.Table; } }
        #region Value Properties
        ///<summary>Gets or sets the row id of the caller.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Guid RowId {
            get { return base.Field<Guid>(RowIdColumn); }
            set { base[RowIdColumn] = value; }
        }
        ///<summary>Gets or sets the person.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Person Person {
            get { return base.Field<Person>(PersonColumn); }
            set { base[PersonColumn] = value; }
        }
        ///<summary>Gets or sets the date added of the caller.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public DateTime DateAdded {
            get { return base.Field<DateTime>(DateAddedColumn); }
            set { base[DateAddedColumn] = value; }
        }
        ///<summary>Gets or sets the year of the caller.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Int32 Year {
            get { return base.Field<Int32>(YearColumn); }
            set { base[YearColumn] = value; }
        }
        ///<summary>Gets or sets the email addresses of the caller.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String EmailAddresses {
            get { return base.Field<String>(EmailAddressesColumn); }
        }
        ///<summary>Gets or sets the number of people assigned to the caller.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Int32 CalleeCount {
            get { return base.Field<Int32>(CalleeCountColumn); }
        }
        #endregion
        
        #region ChildRows Properties
        ///<summary>Gets the people that the caller should call.</summary>
        public IChildRowCollection<MelaveMalkaInvitation> Callees { get { return TypedChildRows<MelaveMalkaInvitation>(MelaveMalkaInvitation.CallerColumn); } }
        #endregion
        
        #region Partial Methods
        partial void OnColumnChanged(Column column, object oldValue, object newValue);
        
        partial void ValidateRowId(Guid newValue, Action<string> error);
        partial void OnRowIdChanged(Guid? oldValue, Guid? newValue);
        
        partial void ValidatePerson(Person newValue, Action<string> error);
        partial void OnPersonChanged(Person oldValue, Person newValue);
        
        partial void ValidateDateAdded(DateTime newValue, Action<string> error);
        partial void OnDateAddedChanged(DateTime? oldValue, DateTime? newValue);
        
        partial void ValidateYear(Int32 newValue, Action<string> error);
        partial void OnYearChanged(Int32? oldValue, Int32? newValue);
        #endregion
        
        #region Column Callbacks
        ///<summary>Checks whether a value would be valid for a given column in an attached row.</summary>
        ///<param name="column">The column containing the value.</param>
        ///<param name="newValue">The value to validate.</param>
        ///<returns>An error message, or null if the value is valid.</returns>
        ///<remarks>This method is overridden by typed rows to perform custom validation logic.</remarks>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public override string ValidateValue(Column column, object newValue) {
            string error = base.ValidateValue(column, newValue);
            if (!String.IsNullOrEmpty(error)) return error;
            Action<string> reporter = s => error = s;
            
            if (column == RowIdColumn) {
                ValidateRowId((Guid)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == PersonColumn) {
                ValidatePerson((Person)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == DateAddedColumn) {
                ValidateDateAdded((DateTime)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == YearColumn) {
                ValidateYear((Int32)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            }
            return null;
        }
        ///<summary>Processes an explicit change of a column value.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        protected override void OnValueChanged(Column column, object oldValue, object newValue) {
            base.OnValueChanged(column, oldValue, newValue);
            OnColumnChanged(column, oldValue, newValue);
            if (column == RowIdColumn)
            	OnRowIdChanged((Guid?)oldValue, (Guid?)newValue);
            else if (column == PersonColumn)
            	OnPersonChanged((Person)oldValue, (Person)newValue);
            else if (column == DateAddedColumn)
            	OnDateAddedChanged((DateTime?)oldValue, (DateTime?)newValue);
            else if (column == YearColumn)
            	OnYearChanged((Int32?)oldValue, (Int32?)newValue);
        }
        #endregion
    }
    
    ///<summary>Describes a deposit.</summary>
    public partial class Deposit : Row {
        ///<summary>Creates a new Deposit instance.</summary>
        public Deposit () : base(Schema) { Initialize(); }
        partial void Initialize();
        
        ///<summary>Creates a strongly-typed Deposits table.</summary>
        public static TypedTable<Deposit> CreateTable() { return new TypedTable<Deposit>(Schema, () => new Deposit()); }
        
        ///<summary>Gets the schema's DepositId column.</summary>
        public static ValueColumn DepositIdColumn { get; private set; }
        ///<summary>Gets the schema's Date column.</summary>
        public static ValueColumn DateColumn { get; private set; }
        ///<summary>Gets the schema's Number column.</summary>
        public static ValueColumn NumberColumn { get; private set; }
        ///<summary>Gets the schema's Account column.</summary>
        public static ValueColumn AccountColumn { get; private set; }
        ///<summary>Gets the schema's Amount column.</summary>
        public static CalculatedColumn AmountColumn { get; private set; }
        ///<summary>Gets the schema's Count column.</summary>
        public static CalculatedColumn CountColumn { get; private set; }
        
        ///<summary>Gets the Deposits schema instance.</summary>
        public static new TypedSchema<Deposit> Schema { get; private set; }
        ///<summary>Gets the SchemaMapping that maps this schema to the SQL Server Deposits table.</summary>
        public static SchemaMapping SchemaMapping { get; private set; }
        
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        static Deposit() {
            #region Create Schema
            Schema = new TypedSchema<Deposit>("Deposits");
            
            Schema.PrimaryKey = DepositIdColumn = Schema.Columns.AddValueColumn("DepositId", typeof(Guid), null);
            DepositIdColumn.Unique = true;
            DepositIdColumn.AllowNulls = false;
            
            DateColumn = Schema.Columns.AddValueColumn("Date", typeof(DateTime), null);
            DateColumn.AllowNulls = false;
            
            NumberColumn = Schema.Columns.AddValueColumn("Number", typeof(Int32), null);
            NumberColumn.AllowNulls = false;
            
            AccountColumn = Schema.Columns.AddValueColumn("Account", typeof(String), null);
            AccountColumn.AllowNulls = false;
            
            AmountColumn = Schema.Columns.AddCalculatedColumn<Deposit, Decimal>("Amount", row => row.Payments.Sum(p => p.Amount));
            
            CountColumn = Schema.Columns.AddCalculatedColumn<Deposit, Int32>("Count", row => row.Payments.Count);
            #endregion
            
            #region Create SchemaMapping
            SchemaMapping = new SchemaMapping(Schema, false);
            SchemaMapping.SqlName = "Deposits";
            SchemaMapping.SqlSchemaName = "Billing";
            
            SchemaMapping.Columns.AddMapping(DepositIdColumn, "DepositId");
            SchemaMapping.Columns.AddMapping(DateColumn, "Date");
            SchemaMapping.Columns.AddMapping(NumberColumn, "Number");
            SchemaMapping.Columns.AddMapping(AccountColumn, "Account");
            #endregion
            SchemaMapping.SetPrimaryMapping(SchemaMapping);
        }
        
        ///<summary>Gets the typed table that contains this row, if any.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public new TypedTable<Deposit> Table { get { return (TypedTable<Deposit>)base.Table; } }
        #region Value Properties
        ///<summary>Gets or sets the row's unique ID.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Guid DepositId {
            get { return base.Field<Guid>(DepositIdColumn); }
            set { base[DepositIdColumn] = value; }
        }
        ///<summary>Gets or sets the date of the deposit.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public DateTime Date {
            get { return base.Field<DateTime>(DateColumn); }
            set { base[DateColumn] = value; }
        }
        ///<summary>Gets or sets the number of the deposit.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Int32 Number {
            get { return base.Field<Int32>(NumberColumn); }
            set { base[NumberColumn] = value; }
        }
        ///<summary>Gets or sets the account of the deposit.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Account {
            get { return base.Field<String>(AccountColumn); }
            set { base[AccountColumn] = value; }
        }
        ///<summary>Gets the total value of the payments in the deposit.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Decimal Amount {
            get { return base.Field<Decimal>(AmountColumn); }
        }
        ///<summary>Gets the number of payments in the deposit.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Int32 Count {
            get { return base.Field<Int32>(CountColumn); }
        }
        #endregion
        
        #region ChildRows Properties
        ///<summary>Gets the payments in the deposit.</summary>
        public IChildRowCollection<Payment> Payments { get { return TypedChildRows<Payment>(Payment.DepositColumn); } }
        #endregion
        
        #region Partial Methods
        partial void OnColumnChanged(Column column, object oldValue, object newValue);
        
        partial void ValidateDepositId(Guid newValue, Action<string> error);
        partial void OnDepositIdChanged(Guid? oldValue, Guid? newValue);
        
        partial void ValidateDate(DateTime newValue, Action<string> error);
        partial void OnDateChanged(DateTime? oldValue, DateTime? newValue);
        
        partial void ValidateNumber(Int32 newValue, Action<string> error);
        partial void OnNumberChanged(Int32? oldValue, Int32? newValue);
        
        partial void ValidateAccount(String newValue, Action<string> error);
        partial void OnAccountChanged(String oldValue, String newValue);
        #endregion
        
        #region Column Callbacks
        ///<summary>Checks whether a value would be valid for a given column in an attached row.</summary>
        ///<param name="column">The column containing the value.</param>
        ///<param name="newValue">The value to validate.</param>
        ///<returns>An error message, or null if the value is valid.</returns>
        ///<remarks>This method is overridden by typed rows to perform custom validation logic.</remarks>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public override string ValidateValue(Column column, object newValue) {
            string error = base.ValidateValue(column, newValue);
            if (!String.IsNullOrEmpty(error)) return error;
            Action<string> reporter = s => error = s;
            
            if (column == DepositIdColumn) {
                ValidateDepositId((Guid)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == DateColumn) {
                ValidateDate((DateTime)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == NumberColumn) {
                ValidateNumber((Int32)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == AccountColumn) {
                ValidateAccount((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            }
            return null;
        }
        ///<summary>Processes an explicit change of a column value.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        protected override void OnValueChanged(Column column, object oldValue, object newValue) {
            base.OnValueChanged(column, oldValue, newValue);
            OnColumnChanged(column, oldValue, newValue);
            if (column == DepositIdColumn)
            	OnDepositIdChanged((Guid?)oldValue, (Guid?)newValue);
            else if (column == DateColumn)
            	OnDateChanged((DateTime?)oldValue, (DateTime?)newValue);
            else if (column == NumberColumn)
            	OnNumberChanged((Int32?)oldValue, (Int32?)newValue);
            else if (column == AccountColumn)
            	OnAccountChanged((String)oldValue, (String)newValue);
        }
        #endregion
    }
    
    ///<summary>Describes a subscriber to the email list.</summary>
    public partial class EmailAddress : Row {
        ///<summary>Creates a new EmailAddress instance.</summary>
        public EmailAddress () : base(Schema) { Initialize(); }
        partial void Initialize();
        
        ///<summary>Creates a strongly-typed EmailAddresses table.</summary>
        public static TypedTable<EmailAddress> CreateTable() { return new TypedTable<EmailAddress>(Schema, () => new EmailAddress()); }
        
        ///<summary>Gets the schema's Name column.</summary>
        public static ValueColumn NameColumn { get; private set; }
        ///<summary>Gets the schema's Email column.</summary>
        public static ValueColumn EmailColumn { get; private set; }
        ///<summary>Gets the schema's RandomCode column.</summary>
        public static ValueColumn RandomCodeColumn { get; private set; }
        ///<summary>Gets the schema's Active column.</summary>
        public static ValueColumn ActiveColumn { get; private set; }
        ///<summary>Gets the schema's DateAdded column.</summary>
        public static ValueColumn DateAddedColumn { get; private set; }
        ///<summary>Gets the schema's UseHtml column.</summary>
        public static ValueColumn UseHtmlColumn { get; private set; }
        ///<summary>Gets the schema's Person column.</summary>
        public static ForeignKeyColumn PersonColumn { get; private set; }
        ///<summary>Gets the schema's RowId column.</summary>
        public static ValueColumn RowIdColumn { get; private set; }
        
        ///<summary>Gets the EmailAddresses schema instance.</summary>
        public static new TypedSchema<EmailAddress> Schema { get; private set; }
        ///<summary>Gets the SchemaMapping that maps this schema to the SQL Server tblMLMembers table.</summary>
        public static SchemaMapping SchemaMapping { get; private set; }
        
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        static EmailAddress() {
            #region Create Schema
            Schema = new TypedSchema<EmailAddress>("EmailAddresses");
            
            NameColumn = Schema.Columns.AddValueColumn("Name", typeof(String), null);
            NameColumn.AllowNulls = true;
            
            EmailColumn = Schema.Columns.AddValueColumn("Email", typeof(String), null);
            EmailColumn.Unique = true;
            EmailColumn.AllowNulls = false;
            
            RandomCodeColumn = Schema.Columns.AddValueColumn("RandomCode", typeof(String), null);
            RandomCodeColumn.AllowNulls = true;
            
            ActiveColumn = Schema.Columns.AddValueColumn("Active", typeof(Boolean), true);
            ActiveColumn.AllowNulls = false;
            
            DateAddedColumn = Schema.Columns.AddValueColumn("DateAdded", typeof(DateTime), null);
            DateAddedColumn.AllowNulls = false;
            
            UseHtmlColumn = Schema.Columns.AddValueColumn("UseHtml", typeof(Boolean), true);
            UseHtmlColumn.AllowNulls = false;
            
            PersonColumn = Schema.Columns.AddForeignKey("Person", ShomreiTorah.Data.Person.Schema, "EmailAddresses");
            PersonColumn.AllowNulls = true;
            
            Schema.PrimaryKey = RowIdColumn = Schema.Columns.AddValueColumn("RowId", typeof(Guid), null);
            RowIdColumn.Unique = true;
            RowIdColumn.AllowNulls = false;
            #endregion
            
            #region Create SchemaMapping
            SchemaMapping = new SchemaMapping(Schema, false);
            SchemaMapping.SqlName = "tblMLMembers";
            SchemaMapping.SqlSchemaName = "dbo";
            
            SchemaMapping.Columns.AddMapping(NameColumn, "Name");
            SchemaMapping.Columns.AddMapping(EmailColumn, "Email");
            SchemaMapping.Columns.AddMapping(RandomCodeColumn, "ID_Code");
            SchemaMapping.Columns.AddMapping(ActiveColumn, "Active");
            SchemaMapping.Columns.AddMapping(DateAddedColumn, "Join_Date");
            SchemaMapping.Columns.AddMapping(UseHtmlColumn, "HTMLformat");
            SchemaMapping.Columns.AddMapping(PersonColumn, "PersonId");
            SchemaMapping.Columns.AddMapping(RowIdColumn, "RowId");
            #endregion
            SchemaMapping.SetPrimaryMapping(SchemaMapping);
        }
        
        ///<summary>Gets the typed table that contains this row, if any.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public new TypedTable<EmailAddress> Table { get { return (TypedTable<EmailAddress>)base.Table; } }
        #region Value Properties
        ///<summary>Gets or sets the display name associated with the email address.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Name {
            get { return base.Field<String>(NameColumn); }
            set { base[NameColumn] = value; }
        }
        ///<summary>Gets or sets the email address of the subscriber.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Email {
            get { return base.Field<String>(EmailColumn); }
            set { base[EmailColumn] = value; }
        }
        ///<summary>Gets or sets a random string unique to this address.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String RandomCode {
            get { return base.Field<String>(RandomCodeColumn); }
            set { base[RandomCodeColumn] = value; }
        }
        ///<summary>Gets or sets whether the Shul's emails will be sent to this address.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Boolean Active {
            get { return base.Field<Boolean>(ActiveColumn); }
            set { base[ActiveColumn] = value; }
        }
        ///<summary>Gets or sets the date that the subscriber joined the list.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public DateTime DateAdded {
            get { return base.Field<DateTime>(DateAddedColumn); }
            set { base[DateAddedColumn] = value; }
        }
        ///<summary>Gets or sets the whether the subscriber should receive HTML emails.  Not currently used.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Boolean UseHtml {
            get { return base.Field<Boolean>(UseHtmlColumn); }
            set { base[UseHtmlColumn] = value; }
        }
        ///<summary>Gets or sets the person that uses the email address.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Person Person {
            get { return base.Field<Person>(PersonColumn); }
            set { base[PersonColumn] = value; }
        }
        ///<summary>Gets or sets the row's unique ID.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Guid RowId {
            get { return base.Field<Guid>(RowIdColumn); }
            set { base[RowIdColumn] = value; }
        }
        #endregion
        
        #region Partial Methods
        partial void OnColumnChanged(Column column, object oldValue, object newValue);
        
        partial void ValidateName(String newValue, Action<string> error);
        partial void OnNameChanged(String oldValue, String newValue);
        
        partial void ValidateEmail(String newValue, Action<string> error);
        partial void OnEmailChanged(String oldValue, String newValue);
        
        partial void ValidateRandomCode(String newValue, Action<string> error);
        partial void OnRandomCodeChanged(String oldValue, String newValue);
        
        partial void ValidateActive(Boolean newValue, Action<string> error);
        partial void OnActiveChanged(Boolean? oldValue, Boolean? newValue);
        
        partial void ValidateDateAdded(DateTime newValue, Action<string> error);
        partial void OnDateAddedChanged(DateTime? oldValue, DateTime? newValue);
        
        partial void ValidateUseHtml(Boolean newValue, Action<string> error);
        partial void OnUseHtmlChanged(Boolean? oldValue, Boolean? newValue);
        
        partial void ValidatePerson(Person newValue, Action<string> error);
        partial void OnPersonChanged(Person oldValue, Person newValue);
        
        partial void ValidateRowId(Guid newValue, Action<string> error);
        partial void OnRowIdChanged(Guid? oldValue, Guid? newValue);
        #endregion
        
        #region Column Callbacks
        ///<summary>Checks whether a value would be valid for a given column in an attached row.</summary>
        ///<param name="column">The column containing the value.</param>
        ///<param name="newValue">The value to validate.</param>
        ///<returns>An error message, or null if the value is valid.</returns>
        ///<remarks>This method is overridden by typed rows to perform custom validation logic.</remarks>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public override string ValidateValue(Column column, object newValue) {
            string error = base.ValidateValue(column, newValue);
            if (!String.IsNullOrEmpty(error)) return error;
            Action<string> reporter = s => error = s;
            
            if (column == NameColumn) {
                ValidateName((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == EmailColumn) {
                ValidateEmail((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == RandomCodeColumn) {
                ValidateRandomCode((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == ActiveColumn) {
                ValidateActive((Boolean)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == DateAddedColumn) {
                ValidateDateAdded((DateTime)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == UseHtmlColumn) {
                ValidateUseHtml((Boolean)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == PersonColumn) {
                ValidatePerson((Person)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == RowIdColumn) {
                ValidateRowId((Guid)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            }
            return null;
        }
        ///<summary>Processes an explicit change of a column value.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        protected override void OnValueChanged(Column column, object oldValue, object newValue) {
            base.OnValueChanged(column, oldValue, newValue);
            OnColumnChanged(column, oldValue, newValue);
            if (column == NameColumn)
            	OnNameChanged((String)oldValue, (String)newValue);
            else if (column == EmailColumn)
            	OnEmailChanged((String)oldValue, (String)newValue);
            else if (column == RandomCodeColumn)
            	OnRandomCodeChanged((String)oldValue, (String)newValue);
            else if (column == ActiveColumn)
            	OnActiveChanged((Boolean?)oldValue, (Boolean?)newValue);
            else if (column == DateAddedColumn)
            	OnDateAddedChanged((DateTime?)oldValue, (DateTime?)newValue);
            else if (column == UseHtmlColumn)
            	OnUseHtmlChanged((Boolean?)oldValue, (Boolean?)newValue);
            else if (column == PersonColumn)
            	OnPersonChanged((Person)oldValue, (Person)newValue);
            else if (column == RowIdColumn)
            	OnRowIdChanged((Guid?)oldValue, (Guid?)newValue);
        }
        #endregion
    }
    
    ///<summary>Links a payment imported from an external source.</summary>
    public partial class ImportedPayment : Row {
        ///<summary>Creates a new ImportedPayment instance.</summary>
        public ImportedPayment () : base(Schema) { Initialize(); }
        partial void Initialize();
        
        ///<summary>Creates a strongly-typed ImportedPayments table.</summary>
        public static TypedTable<ImportedPayment> CreateTable() { return new TypedTable<ImportedPayment>(Schema, () => new ImportedPayment()); }
        
        ///<summary>Gets the schema's ImportedPaymentId column.</summary>
        public static ValueColumn ImportedPaymentIdColumn { get; private set; }
        ///<summary>Gets the schema's Payment column.</summary>
        public static ForeignKeyColumn PaymentColumn { get; private set; }
        ///<summary>Gets the schema's Source column.</summary>
        public static ValueColumn SourceColumn { get; private set; }
        ///<summary>Gets the schema's ExternalId column.</summary>
        public static ValueColumn ExternalIdColumn { get; private set; }
        ///<summary>Gets the schema's DateImported column.</summary>
        public static ValueColumn DateImportedColumn { get; private set; }
        ///<summary>Gets the schema's ImportingUser column.</summary>
        public static ValueColumn ImportingUserColumn { get; private set; }
        
        ///<summary>Gets the ImportedPayments schema instance.</summary>
        public static new TypedSchema<ImportedPayment> Schema { get; private set; }
        ///<summary>Gets the SchemaMapping that maps this schema to the SQL Server ImportedPayments table.</summary>
        public static SchemaMapping SchemaMapping { get; private set; }
        
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        static ImportedPayment() {
            #region Create Schema
            Schema = new TypedSchema<ImportedPayment>("ImportedPayments");
            
            Schema.PrimaryKey = ImportedPaymentIdColumn = Schema.Columns.AddValueColumn("ImportedPaymentId", typeof(Guid), null);
            ImportedPaymentIdColumn.Unique = true;
            ImportedPaymentIdColumn.AllowNulls = false;
            
            PaymentColumn = Schema.Columns.AddForeignKey("Payment", ShomreiTorah.Data.Payment.Schema, "ImportedPayments");
            PaymentColumn.Unique = true;
            PaymentColumn.AllowNulls = false;
            
            SourceColumn = Schema.Columns.AddValueColumn("Source", typeof(String), null);
            SourceColumn.AllowNulls = false;
            
            ExternalIdColumn = Schema.Columns.AddValueColumn("ExternalId", typeof(String), null);
            ExternalIdColumn.AllowNulls = false;
            
            DateImportedColumn = Schema.Columns.AddValueColumn("DateImported", typeof(DateTime), null);
            DateImportedColumn.AllowNulls = false;
            
            ImportingUserColumn = Schema.Columns.AddValueColumn("ImportingUser", typeof(String), null);
            ImportingUserColumn.AllowNulls = false;
            #endregion
            
            #region Create SchemaMapping
            SchemaMapping = new SchemaMapping(Schema, false);
            SchemaMapping.SqlName = "ImportedPayments";
            SchemaMapping.SqlSchemaName = "Billing";
            
            SchemaMapping.Columns.AddMapping(ImportedPaymentIdColumn, "ImportedPaymentId");
            SchemaMapping.Columns.AddMapping(PaymentColumn, "PaymentId");
            SchemaMapping.Columns.AddMapping(SourceColumn, "Source");
            SchemaMapping.Columns.AddMapping(ExternalIdColumn, "ExternalId");
            SchemaMapping.Columns.AddMapping(DateImportedColumn, "DateImported");
            SchemaMapping.Columns.AddMapping(ImportingUserColumn, "ImportingUser");
            #endregion
            SchemaMapping.SetPrimaryMapping(SchemaMapping);
        }
        
        ///<summary>Gets the typed table that contains this row, if any.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public new TypedTable<ImportedPayment> Table { get { return (TypedTable<ImportedPayment>)base.Table; } }
        #region Value Properties
        ///<summary>Gets or sets a unique ID for this row.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Guid ImportedPaymentId {
            get { return base.Field<Guid>(ImportedPaymentIdColumn); }
            set { base[ImportedPaymentIdColumn] = value; }
        }
        ///<summary>Gets or sets the payment that was imported.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Payment Payment {
            get { return base.Field<Payment>(PaymentColumn); }
            set { base[PaymentColumn] = value; }
        }
        ///<summary>Gets or sets the source that the payment was imported from.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Source {
            get { return base.Field<String>(SourceColumn); }
            set { base[SourceColumn] = value; }
        }
        ///<summary>Gets or sets the payment's ID within the external source.  This is an opaque value, and must be unique within the source.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String ExternalId {
            get { return base.Field<String>(ExternalIdColumn); }
            set { base[ExternalIdColumn] = value; }
        }
        ///<summary>Gets or sets the date that the payment was imported on.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public DateTime DateImported {
            get { return base.Field<DateTime>(DateImportedColumn); }
            set { base[DateImportedColumn] = value; }
        }
        ///<summary>Gets or sets the user that imported the payment.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String ImportingUser {
            get { return base.Field<String>(ImportingUserColumn); }
            set { base[ImportingUserColumn] = value; }
        }
        #endregion
        
        #region Partial Methods
        partial void OnColumnChanged(Column column, object oldValue, object newValue);
        
        partial void ValidateImportedPaymentId(Guid newValue, Action<string> error);
        partial void OnImportedPaymentIdChanged(Guid? oldValue, Guid? newValue);
        
        partial void ValidatePayment(Payment newValue, Action<string> error);
        partial void OnPaymentChanged(Payment oldValue, Payment newValue);
        
        partial void ValidateSource(String newValue, Action<string> error);
        partial void OnSourceChanged(String oldValue, String newValue);
        
        partial void ValidateExternalId(String newValue, Action<string> error);
        partial void OnExternalIdChanged(String oldValue, String newValue);
        
        partial void ValidateDateImported(DateTime newValue, Action<string> error);
        partial void OnDateImportedChanged(DateTime? oldValue, DateTime? newValue);
        
        partial void ValidateImportingUser(String newValue, Action<string> error);
        partial void OnImportingUserChanged(String oldValue, String newValue);
        #endregion
        
        #region Column Callbacks
        ///<summary>Checks whether a value would be valid for a given column in an attached row.</summary>
        ///<param name="column">The column containing the value.</param>
        ///<param name="newValue">The value to validate.</param>
        ///<returns>An error message, or null if the value is valid.</returns>
        ///<remarks>This method is overridden by typed rows to perform custom validation logic.</remarks>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public override string ValidateValue(Column column, object newValue) {
            string error = base.ValidateValue(column, newValue);
            if (!String.IsNullOrEmpty(error)) return error;
            Action<string> reporter = s => error = s;
            
            if (column == ImportedPaymentIdColumn) {
                ValidateImportedPaymentId((Guid)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == PaymentColumn) {
                ValidatePayment((Payment)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == SourceColumn) {
                ValidateSource((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == ExternalIdColumn) {
                ValidateExternalId((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == DateImportedColumn) {
                ValidateDateImported((DateTime)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == ImportingUserColumn) {
                ValidateImportingUser((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            }
            return null;
        }
        ///<summary>Processes an explicit change of a column value.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        protected override void OnValueChanged(Column column, object oldValue, object newValue) {
            base.OnValueChanged(column, oldValue, newValue);
            OnColumnChanged(column, oldValue, newValue);
            if (column == ImportedPaymentIdColumn)
            	OnImportedPaymentIdChanged((Guid?)oldValue, (Guid?)newValue);
            else if (column == PaymentColumn)
            	OnPaymentChanged((Payment)oldValue, (Payment)newValue);
            else if (column == SourceColumn)
            	OnSourceChanged((String)oldValue, (String)newValue);
            else if (column == ExternalIdColumn)
            	OnExternalIdChanged((String)oldValue, (String)newValue);
            else if (column == DateImportedColumn)
            	OnDateImportedChanged((DateTime?)oldValue, (DateTime?)newValue);
            else if (column == ImportingUserColumn)
            	OnImportingUserChanged((String)oldValue, (String)newValue);
        }
        #endregion
    }
    
    ///<summary>Describes a person invited to the Melave Malka.</summary>
    public partial class MelaveMalkaInvitation : Row {
        ///<summary>Creates a new MelaveMalkaInvitation instance.</summary>
        public MelaveMalkaInvitation () : base(Schema) { Initialize(); }
        partial void Initialize();
        
        ///<summary>Creates a strongly-typed Invitees table.</summary>
        public static TypedTable<MelaveMalkaInvitation> CreateTable() { return new TypedTable<MelaveMalkaInvitation>(Schema, () => new MelaveMalkaInvitation()); }
        
        ///<summary>Gets the schema's RowId column.</summary>
        public static ValueColumn RowIdColumn { get; private set; }
        ///<summary>Gets the schema's Person column.</summary>
        public static ForeignKeyColumn PersonColumn { get; private set; }
        ///<summary>Gets the schema's Year column.</summary>
        public static ValueColumn YearColumn { get; private set; }
        ///<summary>Gets the schema's Source column.</summary>
        public static ValueColumn SourceColumn { get; private set; }
        ///<summary>Gets the schema's DateAdded column.</summary>
        public static ValueColumn DateAddedColumn { get; private set; }
        ///<summary>Gets the schema's ShouldCall column.</summary>
        public static ValueColumn ShouldCallColumn { get; private set; }
        ///<summary>Gets the schema's Caller column.</summary>
        public static ForeignKeyColumn CallerColumn { get; private set; }
        ///<summary>Gets the schema's CallerNote column.</summary>
        public static ValueColumn CallerNoteColumn { get; private set; }
        ///<summary>Gets the schema's AdAmount column.</summary>
        public static CalculatedColumn AdAmountColumn { get; private set; }
        ///<summary>Gets the schema's ShouldEmail column.</summary>
        public static ValueColumn ShouldEmailColumn { get; private set; }
        ///<summary>Gets the schema's EmailSubject column.</summary>
        public static ValueColumn EmailSubjectColumn { get; private set; }
        ///<summary>Gets the schema's EmailSource column.</summary>
        public static ValueColumn EmailSourceColumn { get; private set; }
        ///<summary>Gets the schema's EmailAddresses column.</summary>
        public static CalculatedColumn EmailAddressesColumn { get; private set; }
        
        ///<summary>Gets the Invitees schema instance.</summary>
        public static new TypedSchema<MelaveMalkaInvitation> Schema { get; private set; }
        ///<summary>Gets the SchemaMapping that maps this schema to the SQL Server Invitees table.</summary>
        public static SchemaMapping SchemaMapping { get; private set; }
        
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        static MelaveMalkaInvitation() {
            #region Create Schema
            Schema = new TypedSchema<MelaveMalkaInvitation>("Invitees");
            
            Schema.PrimaryKey = RowIdColumn = Schema.Columns.AddValueColumn("RowId", typeof(Guid), null);
            RowIdColumn.Unique = true;
            RowIdColumn.AllowNulls = false;
            
            PersonColumn = Schema.Columns.AddForeignKey("Person", ShomreiTorah.Data.Person.Schema, "Invitees");
            PersonColumn.AllowNulls = false;
            
            YearColumn = Schema.Columns.AddValueColumn("Year", typeof(Int32), null);
            YearColumn.AllowNulls = false;
            
            SourceColumn = Schema.Columns.AddValueColumn("Source", typeof(String), null);
            SourceColumn.AllowNulls = false;
            
            DateAddedColumn = Schema.Columns.AddValueColumn("DateAdded", typeof(DateTime), null);
            DateAddedColumn.AllowNulls = false;
            
            ShouldCallColumn = Schema.Columns.AddValueColumn("ShouldCall", typeof(Boolean), false);
            ShouldCallColumn.AllowNulls = false;
            
            CallerColumn = Schema.Columns.AddForeignKey("Caller", ShomreiTorah.Data.Caller.Schema, "Callees");
            CallerColumn.AllowNulls = true;
            
            CallerNoteColumn = Schema.Columns.AddValueColumn("CallerNote", typeof(String), null);
            CallerNoteColumn.AllowNulls = true;
            
            AdAmountColumn = Schema.Columns.AddCalculatedColumn<MelaveMalkaInvitation, Decimal>("AdAmount", row => row.Person.Pledges.Where(p => p.ExternalSource == "Journal " + row.Year).Sum(p => p.Amount));
            
            ShouldEmailColumn = Schema.Columns.AddValueColumn("ShouldEmail", typeof(Boolean), false);
            ShouldEmailColumn.AllowNulls = false;
            
            EmailSubjectColumn = Schema.Columns.AddValueColumn("EmailSubject", typeof(String), null);
            EmailSubjectColumn.AllowNulls = true;
            
            EmailSourceColumn = Schema.Columns.AddValueColumn("EmailSource", typeof(String), null);
            EmailSourceColumn.AllowNulls = true;
            
            EmailAddressesColumn = Schema.Columns.AddCalculatedColumn<MelaveMalkaInvitation, String>("EmailAddresses", row => String.Join(", ", row.Person.EmailAddresses.Select(e => e.Email)));
            #endregion
            
            #region Create SchemaMapping
            SchemaMapping = new SchemaMapping(Schema, false);
            SchemaMapping.SqlName = "Invitees";
            SchemaMapping.SqlSchemaName = "MelaveMalka";
            
            SchemaMapping.Columns.AddMapping(RowIdColumn, "RowId");
            SchemaMapping.Columns.AddMapping(PersonColumn, "PersonId");
            SchemaMapping.Columns.AddMapping(YearColumn, "Year");
            SchemaMapping.Columns.AddMapping(SourceColumn, "Source");
            SchemaMapping.Columns.AddMapping(DateAddedColumn, "DateAdded");
            SchemaMapping.Columns.AddMapping(ShouldCallColumn, "ShouldCall");
            SchemaMapping.Columns.AddMapping(CallerColumn, "Caller");
            SchemaMapping.Columns.AddMapping(CallerNoteColumn, "CallerNote");
            SchemaMapping.Columns.AddMapping(ShouldEmailColumn, "ShouldEmail");
            SchemaMapping.Columns.AddMapping(EmailSubjectColumn, "EmailSubject");
            SchemaMapping.Columns.AddMapping(EmailSourceColumn, "EmailSource");
            #endregion
            SchemaMapping.SetPrimaryMapping(SchemaMapping);
        }
        
        ///<summary>Gets the typed table that contains this row, if any.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public new TypedTable<MelaveMalkaInvitation> Table { get { return (TypedTable<MelaveMalkaInvitation>)base.Table; } }
        #region Value Properties
        ///<summary>Gets or sets the row id of the invitee.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Guid RowId {
            get { return base.Field<Guid>(RowIdColumn); }
            set { base[RowIdColumn] = value; }
        }
        ///<summary>Gets or sets the person invited.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Person Person {
            get { return base.Field<Person>(PersonColumn); }
            set { base[PersonColumn] = value; }
        }
        ///<summary>Gets or sets the year that the invitation applies to.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Int32 Year {
            get { return base.Field<Int32>(YearColumn); }
            set { base[YearColumn] = value; }
        }
        ///<summary>Gets or sets the source of the invitation.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Source {
            get { return base.Field<String>(SourceColumn); }
            set { base[SourceColumn] = value; }
        }
        ///<summary>Gets or sets the date that the invitation was entered.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public DateTime DateAdded {
            get { return base.Field<DateTime>(DateAddedColumn); }
            set { base[DateAddedColumn] = value; }
        }
        ///<summary>Gets or sets whether the person should be called to ask for an ad</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Boolean ShouldCall {
            get { return base.Field<Boolean>(ShouldCallColumn); }
            set { base[ShouldCallColumn] = value; }
        }
        ///<summary>Gets or sets the caller assigned to this person.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Caller Caller {
            get { return base.Field<Caller>(CallerColumn); }
            set { base[CallerColumn] = value; }
        }
        ///<summary>Gets or sets a note from the person's caller.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String CallerNote {
            get { return base.Field<String>(CallerNoteColumn); }
            set { base[CallerNoteColumn] = value; }
        }
        ///<summary>Gets or sets the total pledge amount of the person's ads, if any.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Decimal AdAmount {
            get { return base.Field<Decimal>(AdAmountColumn); }
        }
        ///<summary>Gets or sets whether the person should be emailed to ask for an ad.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Boolean ShouldEmail {
            get { return base.Field<Boolean>(ShouldEmailColumn); }
            set { base[ShouldEmailColumn] = value; }
        }
        ///<summary>Gets or sets the subject of the email to send to the person to ask for an ad.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String EmailSubject {
            get { return base.Field<String>(EmailSubjectColumn); }
            set { base[EmailSubjectColumn] = value; }
        }
        ///<summary>Gets or sets the HTML source of the email to send to the person to ask for an ad.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String EmailSource {
            get { return base.Field<String>(EmailSourceColumn); }
            set { base[EmailSourceColumn] = value; }
        }
        ///<summary>Gets or sets the person's email addresses.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String EmailAddresses {
            get { return base.Field<String>(EmailAddressesColumn); }
        }
        #endregion
        
        #region ChildRows Properties
        ///<summary>Gets the emails sent to the person to ask for an ad.</summary>
        public IChildRowCollection<AdReminderEmail> ReminderEmails { get { return TypedChildRows<AdReminderEmail>(AdReminderEmail.RecipientColumn); } }
        #endregion
        
        #region Partial Methods
        partial void OnColumnChanged(Column column, object oldValue, object newValue);
        
        partial void ValidateRowId(Guid newValue, Action<string> error);
        partial void OnRowIdChanged(Guid? oldValue, Guid? newValue);
        
        partial void ValidatePerson(Person newValue, Action<string> error);
        partial void OnPersonChanged(Person oldValue, Person newValue);
        
        partial void ValidateYear(Int32 newValue, Action<string> error);
        partial void OnYearChanged(Int32? oldValue, Int32? newValue);
        
        partial void ValidateSource(String newValue, Action<string> error);
        partial void OnSourceChanged(String oldValue, String newValue);
        
        partial void ValidateDateAdded(DateTime newValue, Action<string> error);
        partial void OnDateAddedChanged(DateTime? oldValue, DateTime? newValue);
        
        partial void ValidateShouldCall(Boolean newValue, Action<string> error);
        partial void OnShouldCallChanged(Boolean? oldValue, Boolean? newValue);
        
        partial void ValidateCaller(Caller newValue, Action<string> error);
        partial void OnCallerChanged(Caller oldValue, Caller newValue);
        
        partial void ValidateCallerNote(String newValue, Action<string> error);
        partial void OnCallerNoteChanged(String oldValue, String newValue);
        
        partial void ValidateShouldEmail(Boolean newValue, Action<string> error);
        partial void OnShouldEmailChanged(Boolean? oldValue, Boolean? newValue);
        
        partial void ValidateEmailSubject(String newValue, Action<string> error);
        partial void OnEmailSubjectChanged(String oldValue, String newValue);
        
        partial void ValidateEmailSource(String newValue, Action<string> error);
        partial void OnEmailSourceChanged(String oldValue, String newValue);
        #endregion
        
        #region Column Callbacks
        ///<summary>Checks whether a value would be valid for a given column in an attached row.</summary>
        ///<param name="column">The column containing the value.</param>
        ///<param name="newValue">The value to validate.</param>
        ///<returns>An error message, or null if the value is valid.</returns>
        ///<remarks>This method is overridden by typed rows to perform custom validation logic.</remarks>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public override string ValidateValue(Column column, object newValue) {
            string error = base.ValidateValue(column, newValue);
            if (!String.IsNullOrEmpty(error)) return error;
            Action<string> reporter = s => error = s;
            
            if (column == RowIdColumn) {
                ValidateRowId((Guid)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == PersonColumn) {
                ValidatePerson((Person)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == YearColumn) {
                ValidateYear((Int32)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == SourceColumn) {
                ValidateSource((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == DateAddedColumn) {
                ValidateDateAdded((DateTime)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == ShouldCallColumn) {
                ValidateShouldCall((Boolean)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == CallerColumn) {
                ValidateCaller((Caller)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == CallerNoteColumn) {
                ValidateCallerNote((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == ShouldEmailColumn) {
                ValidateShouldEmail((Boolean)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == EmailSubjectColumn) {
                ValidateEmailSubject((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == EmailSourceColumn) {
                ValidateEmailSource((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            }
            return null;
        }
        ///<summary>Processes an explicit change of a column value.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        protected override void OnValueChanged(Column column, object oldValue, object newValue) {
            base.OnValueChanged(column, oldValue, newValue);
            OnColumnChanged(column, oldValue, newValue);
            if (column == RowIdColumn)
            	OnRowIdChanged((Guid?)oldValue, (Guid?)newValue);
            else if (column == PersonColumn)
            	OnPersonChanged((Person)oldValue, (Person)newValue);
            else if (column == YearColumn)
            	OnYearChanged((Int32?)oldValue, (Int32?)newValue);
            else if (column == SourceColumn)
            	OnSourceChanged((String)oldValue, (String)newValue);
            else if (column == DateAddedColumn)
            	OnDateAddedChanged((DateTime?)oldValue, (DateTime?)newValue);
            else if (column == ShouldCallColumn)
            	OnShouldCallChanged((Boolean?)oldValue, (Boolean?)newValue);
            else if (column == CallerColumn)
            	OnCallerChanged((Caller)oldValue, (Caller)newValue);
            else if (column == CallerNoteColumn)
            	OnCallerNoteChanged((String)oldValue, (String)newValue);
            else if (column == ShouldEmailColumn)
            	OnShouldEmailChanged((Boolean?)oldValue, (Boolean?)newValue);
            else if (column == EmailSubjectColumn)
            	OnEmailSubjectChanged((String)oldValue, (String)newValue);
            else if (column == EmailSourceColumn)
            	OnEmailSourceChanged((String)oldValue, (String)newValue);
        }
        #endregion
    }
    
    ///<summary>Describes a person.</summary>
    public partial class Person : Row {
        ///<summary>Creates a new Person instance.</summary>
        public Person () : base(Schema) { Initialize(); }
        partial void Initialize();
        
        ///<summary>Creates a strongly-typed MasterDirectory table.</summary>
        public static TypedTable<Person> CreateTable() { return new TypedTable<Person>(Schema, () => new Person()); }
        
        ///<summary>Gets the schema's Id column.</summary>
        public static ValueColumn IdColumn { get; private set; }
        ///<summary>Gets the schema's YKID column.</summary>
        public static ValueColumn YKIDColumn { get; private set; }
        ///<summary>Gets the schema's LastName column.</summary>
        public static ValueColumn LastNameColumn { get; private set; }
        ///<summary>Gets the schema's HisName column.</summary>
        public static ValueColumn HisNameColumn { get; private set; }
        ///<summary>Gets the schema's HerName column.</summary>
        public static ValueColumn HerNameColumn { get; private set; }
        ///<summary>Gets the schema's FullName column.</summary>
        public static ValueColumn FullNameColumn { get; private set; }
        ///<summary>Gets the schema's Address column.</summary>
        public static ValueColumn AddressColumn { get; private set; }
        ///<summary>Gets the schema's City column.</summary>
        public static ValueColumn CityColumn { get; private set; }
        ///<summary>Gets the schema's State column.</summary>
        public static ValueColumn StateColumn { get; private set; }
        ///<summary>Gets the schema's Zip column.</summary>
        public static ValueColumn ZipColumn { get; private set; }
        ///<summary>Gets the schema's Phone column.</summary>
        public static ValueColumn PhoneColumn { get; private set; }
        ///<summary>Gets the schema's Source column.</summary>
        public static ValueColumn SourceColumn { get; private set; }
        ///<summary>Gets the schema's Salutation column.</summary>
        public static ValueColumn SalutationColumn { get; private set; }
        
        ///<summary>Gets the MasterDirectory schema instance.</summary>
        public static new TypedSchema<Person> Schema { get; private set; }
        ///<summary>Gets the SchemaMapping that maps this schema to the SQL Server MasterDirectory table.</summary>
        public static SchemaMapping SchemaMapping { get; private set; }
        
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        static Person() {
            #region Create Schema
            Schema = new TypedSchema<Person>("MasterDirectory");
            
            Schema.PrimaryKey = IdColumn = Schema.Columns.AddValueColumn("Id", typeof(Guid), null);
            IdColumn.Unique = true;
            IdColumn.AllowNulls = false;
            
            YKIDColumn = Schema.Columns.AddValueColumn("YKID", typeof(Int32), null);
            YKIDColumn.AllowNulls = true;
            
            LastNameColumn = Schema.Columns.AddValueColumn("LastName", typeof(String), null);
            LastNameColumn.AllowNulls = false;
            
            HisNameColumn = Schema.Columns.AddValueColumn("HisName", typeof(String), null);
            HisNameColumn.AllowNulls = true;
            
            HerNameColumn = Schema.Columns.AddValueColumn("HerName", typeof(String), null);
            HerNameColumn.AllowNulls = true;
            
            FullNameColumn = Schema.Columns.AddValueColumn("FullName", typeof(String), null);
            FullNameColumn.AllowNulls = true;
            
            AddressColumn = Schema.Columns.AddValueColumn("Address", typeof(String), null);
            AddressColumn.AllowNulls = true;
            
            CityColumn = Schema.Columns.AddValueColumn("City", typeof(String), null);
            CityColumn.AllowNulls = true;
            
            StateColumn = Schema.Columns.AddValueColumn("State", typeof(String), null);
            StateColumn.AllowNulls = true;
            
            ZipColumn = Schema.Columns.AddValueColumn("Zip", typeof(String), null);
            ZipColumn.AllowNulls = true;
            
            PhoneColumn = Schema.Columns.AddValueColumn("Phone", typeof(String), null);
            PhoneColumn.AllowNulls = false;
            
            SourceColumn = Schema.Columns.AddValueColumn("Source", typeof(String), null);
            SourceColumn.AllowNulls = false;
            
            SalutationColumn = Schema.Columns.AddValueColumn("Salutation", typeof(String), null);
            SalutationColumn.AllowNulls = false;
            #endregion
            
            #region Create SchemaMapping
            SchemaMapping = new SchemaMapping(Schema, false);
            SchemaMapping.SqlName = "MasterDirectory";
            SchemaMapping.SqlSchemaName = "Data";
            
            SchemaMapping.Columns.AddMapping(IdColumn, "Id");
            SchemaMapping.Columns.AddMapping(YKIDColumn, "YKID");
            SchemaMapping.Columns.AddMapping(LastNameColumn, "LastName");
            SchemaMapping.Columns.AddMapping(HisNameColumn, "HisName");
            SchemaMapping.Columns.AddMapping(HerNameColumn, "HerName");
            SchemaMapping.Columns.AddMapping(FullNameColumn, "FullName");
            SchemaMapping.Columns.AddMapping(AddressColumn, "Address");
            SchemaMapping.Columns.AddMapping(CityColumn, "City");
            SchemaMapping.Columns.AddMapping(StateColumn, "State");
            SchemaMapping.Columns.AddMapping(ZipColumn, "Zip");
            SchemaMapping.Columns.AddMapping(PhoneColumn, "Phone");
            SchemaMapping.Columns.AddMapping(SourceColumn, "Source");
            SchemaMapping.Columns.AddMapping(SalutationColumn, "Salutation");
            #endregion
            SchemaMapping.SetPrimaryMapping(SchemaMapping);
        }
        
        ///<summary>Gets the typed table that contains this row, if any.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public new TypedTable<Person> Table { get { return (TypedTable<Person>)base.Table; } }
        #region Value Properties
        ///<summary>Gets or sets the row's unique ID.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Guid Id {
            get { return base.Field<Guid>(IdColumn); }
            set { base[IdColumn] = value; }
        }
        ///<summary>Gets or sets the YKID of the master directory.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Int32? YKID {
            get { return base.Field<Int32?>(YKIDColumn); }
            set { base[YKIDColumn] = value; }
        }
        ///<summary>Gets or sets the last name of the master directory.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String LastName {
            get { return base.Field<String>(LastNameColumn); }
            set { base[LastNameColumn] = value; }
        }
        ///<summary>Gets or sets the his name of the master directory.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String HisName {
            get { return base.Field<String>(HisNameColumn); }
            set { base[HisNameColumn] = value; }
        }
        ///<summary>Gets or sets the her name of the master directory.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String HerName {
            get { return base.Field<String>(HerNameColumn); }
            set { base[HerNameColumn] = value; }
        }
        ///<summary>Gets or sets the full name of the master directory.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String FullName {
            get { return base.Field<String>(FullNameColumn); }
            set { base[FullNameColumn] = value; }
        }
        ///<summary>Gets or sets the address of the master directory.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Address {
            get { return base.Field<String>(AddressColumn); }
            set { base[AddressColumn] = value; }
        }
        ///<summary>Gets or sets the city of the master directory.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String City {
            get { return base.Field<String>(CityColumn); }
            set { base[CityColumn] = value; }
        }
        ///<summary>Gets or sets the state of the master directory.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String State {
            get { return base.Field<String>(StateColumn); }
            set { base[StateColumn] = value; }
        }
        ///<summary>Gets or sets the zip of the master directory.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Zip {
            get { return base.Field<String>(ZipColumn); }
            set { base[ZipColumn] = value; }
        }
        ///<summary>Gets or sets the phone of the master directory.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Phone {
            get { return base.Field<String>(PhoneColumn); }
            set { base[PhoneColumn] = value; }
        }
        ///<summary>Gets or sets the source of the master directory.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Source {
            get { return base.Field<String>(SourceColumn); }
            set { base[SourceColumn] = value; }
        }
        ///<summary>Gets or sets a salutation to use for the person.   This is the full name without any first names.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Salutation {
            get { return base.Field<String>(SalutationColumn); }
            set { base[SalutationColumn] = value; }
        }
        #endregion
        
        #region ChildRows Properties
        ///<summary>Gets the person's pledges.</summary>
        public IChildRowCollection<Pledge> Pledges { get { return TypedChildRows<Pledge>(Pledge.PersonColumn); } }
        ///<summary>Gets the person's payments.</summary>
        public IChildRowCollection<Payment> Payments { get { return TypedChildRows<Payment>(Payment.PersonColumn); } }
        ///<summary>Gets the person's email addresses.</summary>
        public IChildRowCollection<EmailAddress> EmailAddresses { get { return TypedChildRows<EmailAddress>(EmailAddress.PersonColumn); } }
        ///<summary>Gets the statements sent to the person.</summary>
        public IChildRowCollection<LoggedStatement> LoggedStatements { get { return TypedChildRows<LoggedStatement>(LoggedStatement.PersonColumn); } }
        ///<summary>Gets the person's Melave Malka seats.</summary>
        public IChildRowCollection<MelaveMalkaSeat> MelaveMalkaSeats { get { return TypedChildRows<MelaveMalkaSeat>(MelaveMalkaSeat.PersonColumn); } }
        ///<summary>Gets the Melave Malkas that the person has been honored by.</summary>
        public IChildRowCollection<MelaveMalkaInfo> Honorees { get { return TypedChildRows<MelaveMalkaInfo>(MelaveMalkaInfo.HonoreeColumn); } }
        ///<summary>Gets the Melave Malkas that the person has been secondarily honored by.</summary>
        public IChildRowCollection<MelaveMalkaInfo> MelaveMalkaInfoes { get { return TypedChildRows<MelaveMalkaInfo>(MelaveMalkaInfo.Honoree2Column); } }
        ///<summary>Gets the person's caller rows.</summary>
        public IChildRowCollection<Caller> Callers { get { return TypedChildRows<Caller>(Caller.PersonColumn); } }
        ///<summary>Gets the person's Melave Malka invitations.</summary>
        public IChildRowCollection<MelaveMalkaInvitation> Invitees { get { return TypedChildRows<MelaveMalkaInvitation>(MelaveMalkaInvitation.PersonColumn); } }
        ///<summary>Gets the raffle tickets that the person purchased.</summary>
        public IChildRowCollection<RaffleTicket> RaffleTickets { get { return TypedChildRows<RaffleTicket>(RaffleTicket.PersonColumn); } }
        ///<summary>Gets this member's known relatives.</summary>
        public IChildRowCollection<RelativeLink> ForeignRelatives { get { return TypedChildRows<RelativeLink>(RelativeLink.MemberColumn); } }
        ///<summary>Gets the member(s) that this person is related to.</summary>
        public IChildRowCollection<RelativeLink> RelatedMembers { get { return TypedChildRows<RelativeLink>(RelativeLink.RelativeColumn); } }
        #endregion
        
        #region Partial Methods
        partial void OnColumnChanged(Column column, object oldValue, object newValue);
        
        partial void ValidateId(Guid newValue, Action<string> error);
        partial void OnIdChanged(Guid? oldValue, Guid? newValue);
        
        partial void ValidateYKID(Int32? newValue, Action<string> error);
        partial void OnYKIDChanged(Int32? oldValue, Int32? newValue);
        
        partial void ValidateLastName(String newValue, Action<string> error);
        partial void OnLastNameChanged(String oldValue, String newValue);
        
        partial void ValidateHisName(String newValue, Action<string> error);
        partial void OnHisNameChanged(String oldValue, String newValue);
        
        partial void ValidateHerName(String newValue, Action<string> error);
        partial void OnHerNameChanged(String oldValue, String newValue);
        
        partial void ValidateFullName(String newValue, Action<string> error);
        partial void OnFullNameChanged(String oldValue, String newValue);
        
        partial void ValidateAddress(String newValue, Action<string> error);
        partial void OnAddressChanged(String oldValue, String newValue);
        
        partial void ValidateCity(String newValue, Action<string> error);
        partial void OnCityChanged(String oldValue, String newValue);
        
        partial void ValidateState(String newValue, Action<string> error);
        partial void OnStateChanged(String oldValue, String newValue);
        
        partial void ValidateZip(String newValue, Action<string> error);
        partial void OnZipChanged(String oldValue, String newValue);
        
        partial void ValidatePhone(String newValue, Action<string> error);
        partial void OnPhoneChanged(String oldValue, String newValue);
        
        partial void ValidateSource(String newValue, Action<string> error);
        partial void OnSourceChanged(String oldValue, String newValue);
        
        partial void ValidateSalutation(String newValue, Action<string> error);
        partial void OnSalutationChanged(String oldValue, String newValue);
        #endregion
        
        #region Column Callbacks
        ///<summary>Checks whether a value would be valid for a given column in an attached row.</summary>
        ///<param name="column">The column containing the value.</param>
        ///<param name="newValue">The value to validate.</param>
        ///<returns>An error message, or null if the value is valid.</returns>
        ///<remarks>This method is overridden by typed rows to perform custom validation logic.</remarks>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public override string ValidateValue(Column column, object newValue) {
            string error = base.ValidateValue(column, newValue);
            if (!String.IsNullOrEmpty(error)) return error;
            Action<string> reporter = s => error = s;
            
            if (column == IdColumn) {
                ValidateId((Guid)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == YKIDColumn) {
                ValidateYKID((Int32?)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == LastNameColumn) {
                ValidateLastName((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == HisNameColumn) {
                ValidateHisName((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == HerNameColumn) {
                ValidateHerName((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == FullNameColumn) {
                ValidateFullName((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == AddressColumn) {
                ValidateAddress((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == CityColumn) {
                ValidateCity((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == StateColumn) {
                ValidateState((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == ZipColumn) {
                ValidateZip((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == PhoneColumn) {
                ValidatePhone((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == SourceColumn) {
                ValidateSource((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == SalutationColumn) {
                ValidateSalutation((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            }
            return null;
        }
        ///<summary>Processes an explicit change of a column value.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        protected override void OnValueChanged(Column column, object oldValue, object newValue) {
            base.OnValueChanged(column, oldValue, newValue);
            OnColumnChanged(column, oldValue, newValue);
            if (column == IdColumn)
            	OnIdChanged((Guid?)oldValue, (Guid?)newValue);
            else if (column == YKIDColumn)
            	OnYKIDChanged((Int32?)oldValue, (Int32?)newValue);
            else if (column == LastNameColumn)
            	OnLastNameChanged((String)oldValue, (String)newValue);
            else if (column == HisNameColumn)
            	OnHisNameChanged((String)oldValue, (String)newValue);
            else if (column == HerNameColumn)
            	OnHerNameChanged((String)oldValue, (String)newValue);
            else if (column == FullNameColumn)
            	OnFullNameChanged((String)oldValue, (String)newValue);
            else if (column == AddressColumn)
            	OnAddressChanged((String)oldValue, (String)newValue);
            else if (column == CityColumn)
            	OnCityChanged((String)oldValue, (String)newValue);
            else if (column == StateColumn)
            	OnStateChanged((String)oldValue, (String)newValue);
            else if (column == ZipColumn)
            	OnZipChanged((String)oldValue, (String)newValue);
            else if (column == PhoneColumn)
            	OnPhoneChanged((String)oldValue, (String)newValue);
            else if (column == SourceColumn)
            	OnSourceChanged((String)oldValue, (String)newValue);
            else if (column == SalutationColumn)
            	OnSalutationChanged((String)oldValue, (String)newValue);
        }
        #endregion
    }
    
    ///<summary>Describes a single Melave Malka.</summary>
    public partial class MelaveMalkaInfo : Row {
        ///<summary>Creates a new MelaveMalkaInfo instance.</summary>
        public MelaveMalkaInfo () : base(Schema) { Initialize(); }
        partial void Initialize();
        
        ///<summary>Creates a strongly-typed MelaveMalkaInfo table.</summary>
        public static TypedTable<MelaveMalkaInfo> CreateTable() { return new TypedTable<MelaveMalkaInfo>(Schema, () => new MelaveMalkaInfo()); }
        
        ///<summary>Gets the schema's RowId column.</summary>
        public static ValueColumn RowIdColumn { get; private set; }
        ///<summary>Gets the schema's Year column.</summary>
        public static ValueColumn YearColumn { get; private set; }
        ///<summary>Gets the schema's AdDeadline column.</summary>
        public static ValueColumn AdDeadlineColumn { get; private set; }
        ///<summary>Gets the schema's MelaveMalkaDate column.</summary>
        public static ValueColumn MelaveMalkaDateColumn { get; private set; }
        ///<summary>Gets the schema's Honoree column.</summary>
        public static ForeignKeyColumn HonoreeColumn { get; private set; }
        ///<summary>Gets the schema's Speaker column.</summary>
        public static ValueColumn SpeakerColumn { get; private set; }
        ///<summary>Gets the schema's HonoreeTitle column.</summary>
        public static ValueColumn HonoreeTitleColumn { get; private set; }
        ///<summary>Gets the schema's Honoree2 column.</summary>
        public static ForeignKeyColumn Honoree2Column { get; private set; }
        ///<summary>Gets the schema's Honoree2Title column.</summary>
        public static ValueColumn Honoree2TitleColumn { get; private set; }
        
        ///<summary>Gets the MelaveMalkaInfo schema instance.</summary>
        public static new TypedSchema<MelaveMalkaInfo> Schema { get; private set; }
        ///<summary>Gets the SchemaMapping that maps this schema to the SQL Server MelaveMalkaInfo table.</summary>
        public static SchemaMapping SchemaMapping { get; private set; }
        
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        static MelaveMalkaInfo() {
            #region Create Schema
            Schema = new TypedSchema<MelaveMalkaInfo>("MelaveMalkaInfo");
            
            Schema.PrimaryKey = RowIdColumn = Schema.Columns.AddValueColumn("RowId", typeof(Guid), null);
            RowIdColumn.Unique = true;
            RowIdColumn.AllowNulls = false;
            
            YearColumn = Schema.Columns.AddValueColumn("Year", typeof(Int32), null);
            YearColumn.Unique = true;
            YearColumn.AllowNulls = false;
            
            AdDeadlineColumn = Schema.Columns.AddValueColumn("AdDeadline", typeof(DateTime), null);
            AdDeadlineColumn.AllowNulls = false;
            
            MelaveMalkaDateColumn = Schema.Columns.AddValueColumn("MelaveMalkaDate", typeof(DateTime), null);
            MelaveMalkaDateColumn.AllowNulls = false;
            
            HonoreeColumn = Schema.Columns.AddForeignKey("Honoree", ShomreiTorah.Data.Person.Schema, "Honorees");
            HonoreeColumn.AllowNulls = false;
            
            SpeakerColumn = Schema.Columns.AddValueColumn("Speaker", typeof(String), null);
            SpeakerColumn.AllowNulls = false;
            
            HonoreeTitleColumn = Schema.Columns.AddValueColumn("HonoreeTitle", typeof(String), null);
            HonoreeTitleColumn.AllowNulls = false;
            
            Honoree2Column = Schema.Columns.AddForeignKey("Honoree2", ShomreiTorah.Data.Person.Schema, "MelaveMalkaInfoes");
            Honoree2Column.AllowNulls = true;
            
            Honoree2TitleColumn = Schema.Columns.AddValueColumn("Honoree2Title", typeof(String), null);
            Honoree2TitleColumn.AllowNulls = true;
            #endregion
            
            #region Create SchemaMapping
            SchemaMapping = new SchemaMapping(Schema, false);
            SchemaMapping.SqlName = "MelaveMalkaInfo";
            SchemaMapping.SqlSchemaName = "MelaveMalka";
            
            SchemaMapping.Columns.AddMapping(RowIdColumn, "RowId");
            SchemaMapping.Columns.AddMapping(YearColumn, "Year");
            SchemaMapping.Columns.AddMapping(AdDeadlineColumn, "AdDeadline");
            SchemaMapping.Columns.AddMapping(MelaveMalkaDateColumn, "MelaveMalkaDate");
            SchemaMapping.Columns.AddMapping(HonoreeColumn, "Honoree");
            SchemaMapping.Columns.AddMapping(SpeakerColumn, "Speaker");
            SchemaMapping.Columns.AddMapping(HonoreeTitleColumn, "HonoreeTitle");
            SchemaMapping.Columns.AddMapping(Honoree2Column, "Honoree2");
            SchemaMapping.Columns.AddMapping(Honoree2TitleColumn, "Honoree2Title");
            #endregion
            SchemaMapping.SetPrimaryMapping(SchemaMapping);
        }
        
        ///<summary>Gets the typed table that contains this row, if any.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public new TypedTable<MelaveMalkaInfo> Table { get { return (TypedTable<MelaveMalkaInfo>)base.Table; } }
        #region Value Properties
        ///<summary>Gets or sets the row id of the melave malka info.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Guid RowId {
            get { return base.Field<Guid>(RowIdColumn); }
            set { base[RowIdColumn] = value; }
        }
        ///<summary>Gets or sets the year of the Melave Malka.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Int32 Year {
            get { return base.Field<Int32>(YearColumn); }
            set { base[YearColumn] = value; }
        }
        ///<summary>Gets or sets the stated deadline for receiving ads.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public DateTime AdDeadline {
            get { return base.Field<DateTime>(AdDeadlineColumn); }
            set { base[AdDeadlineColumn] = value; }
        }
        ///<summary>Gets or sets the date that the Melave Malka takes place.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public DateTime MelaveMalkaDate {
            get { return base.Field<DateTime>(MelaveMalkaDateColumn); }
            set { base[MelaveMalkaDateColumn] = value; }
        }
        ///<summary>Gets or sets the guest of honor.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Person Honoree {
            get { return base.Field<Person>(HonoreeColumn); }
            set { base[HonoreeColumn] = value; }
        }
        ///<summary>Gets or sets the speaker of the melave malka info.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Speaker {
            get { return base.Field<String>(SpeakerColumn); }
            set { base[SpeakerColumn] = value; }
        }
        ///<summary>Gets or sets the title of the primary honoree.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String HonoreeTitle {
            get { return base.Field<String>(HonoreeTitleColumn); }
            set { base[HonoreeTitleColumn] = value; }
        }
        ///<summary>Gets or sets the second honoree.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Person Honoree2 {
            get { return base.Field<Person>(Honoree2Column); }
            set { base[Honoree2Column] = value; }
        }
        ///<summary>Gets or sets the title of the secondary honoree.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Honoree2Title {
            get { return base.Field<String>(Honoree2TitleColumn); }
            set { base[Honoree2TitleColumn] = value; }
        }
        #endregion
        
        #region Partial Methods
        partial void OnColumnChanged(Column column, object oldValue, object newValue);
        
        partial void ValidateRowId(Guid newValue, Action<string> error);
        partial void OnRowIdChanged(Guid? oldValue, Guid? newValue);
        
        partial void ValidateYear(Int32 newValue, Action<string> error);
        partial void OnYearChanged(Int32? oldValue, Int32? newValue);
        
        partial void ValidateAdDeadline(DateTime newValue, Action<string> error);
        partial void OnAdDeadlineChanged(DateTime? oldValue, DateTime? newValue);
        
        partial void ValidateMelaveMalkaDate(DateTime newValue, Action<string> error);
        partial void OnMelaveMalkaDateChanged(DateTime? oldValue, DateTime? newValue);
        
        partial void ValidateHonoree(Person newValue, Action<string> error);
        partial void OnHonoreeChanged(Person oldValue, Person newValue);
        
        partial void ValidateSpeaker(String newValue, Action<string> error);
        partial void OnSpeakerChanged(String oldValue, String newValue);
        
        partial void ValidateHonoreeTitle(String newValue, Action<string> error);
        partial void OnHonoreeTitleChanged(String oldValue, String newValue);
        
        partial void ValidateHonoree2(Person newValue, Action<string> error);
        partial void OnHonoree2Changed(Person oldValue, Person newValue);
        
        partial void ValidateHonoree2Title(String newValue, Action<string> error);
        partial void OnHonoree2TitleChanged(String oldValue, String newValue);
        #endregion
        
        #region Column Callbacks
        ///<summary>Checks whether a value would be valid for a given column in an attached row.</summary>
        ///<param name="column">The column containing the value.</param>
        ///<param name="newValue">The value to validate.</param>
        ///<returns>An error message, or null if the value is valid.</returns>
        ///<remarks>This method is overridden by typed rows to perform custom validation logic.</remarks>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public override string ValidateValue(Column column, object newValue) {
            string error = base.ValidateValue(column, newValue);
            if (!String.IsNullOrEmpty(error)) return error;
            Action<string> reporter = s => error = s;
            
            if (column == RowIdColumn) {
                ValidateRowId((Guid)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == YearColumn) {
                ValidateYear((Int32)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == AdDeadlineColumn) {
                ValidateAdDeadline((DateTime)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == MelaveMalkaDateColumn) {
                ValidateMelaveMalkaDate((DateTime)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == HonoreeColumn) {
                ValidateHonoree((Person)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == SpeakerColumn) {
                ValidateSpeaker((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == HonoreeTitleColumn) {
                ValidateHonoreeTitle((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == Honoree2Column) {
                ValidateHonoree2((Person)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == Honoree2TitleColumn) {
                ValidateHonoree2Title((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            }
            return null;
        }
        ///<summary>Processes an explicit change of a column value.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        protected override void OnValueChanged(Column column, object oldValue, object newValue) {
            base.OnValueChanged(column, oldValue, newValue);
            OnColumnChanged(column, oldValue, newValue);
            if (column == RowIdColumn)
            	OnRowIdChanged((Guid?)oldValue, (Guid?)newValue);
            else if (column == YearColumn)
            	OnYearChanged((Int32?)oldValue, (Int32?)newValue);
            else if (column == AdDeadlineColumn)
            	OnAdDeadlineChanged((DateTime?)oldValue, (DateTime?)newValue);
            else if (column == MelaveMalkaDateColumn)
            	OnMelaveMalkaDateChanged((DateTime?)oldValue, (DateTime?)newValue);
            else if (column == HonoreeColumn)
            	OnHonoreeChanged((Person)oldValue, (Person)newValue);
            else if (column == SpeakerColumn)
            	OnSpeakerChanged((String)oldValue, (String)newValue);
            else if (column == HonoreeTitleColumn)
            	OnHonoreeTitleChanged((String)oldValue, (String)newValue);
            else if (column == Honoree2Column)
            	OnHonoree2Changed((Person)oldValue, (Person)newValue);
            else if (column == Honoree2TitleColumn)
            	OnHonoree2TitleChanged((String)oldValue, (String)newValue);
        }
        #endregion
    }
    
    ///<summary>Describes a payment.</summary>
    public partial class Payment : Row {
        ///<summary>Creates a new Payment instance.</summary>
        public Payment () : base(Schema) { Initialize(); }
        partial void Initialize();
        
        ///<summary>Creates a strongly-typed Payments table.</summary>
        public static TypedTable<Payment> CreateTable() { return new TypedTable<Payment>(Schema, () => new Payment()); }
        
        ///<summary>Gets the schema's PaymentId column.</summary>
        public static ValueColumn PaymentIdColumn { get; private set; }
        ///<summary>Gets the schema's Person column.</summary>
        public static ForeignKeyColumn PersonColumn { get; private set; }
        ///<summary>Gets the schema's Date column.</summary>
        public static ValueColumn DateColumn { get; private set; }
        ///<summary>Gets the schema's Method column.</summary>
        public static ValueColumn MethodColumn { get; private set; }
        ///<summary>Gets the schema's CheckNumber column.</summary>
        public static ValueColumn CheckNumberColumn { get; private set; }
        ///<summary>Gets the schema's Account column.</summary>
        public static ValueColumn AccountColumn { get; private set; }
        ///<summary>Gets the schema's Amount column.</summary>
        public static ValueColumn AmountColumn { get; private set; }
        ///<summary>Gets the schema's Deposit column.</summary>
        public static ForeignKeyColumn DepositColumn { get; private set; }
        ///<summary>Gets the schema's Comments column.</summary>
        public static ValueColumn CommentsColumn { get; private set; }
        ///<summary>Gets the schema's Modified column.</summary>
        public static ValueColumn ModifiedColumn { get; private set; }
        ///<summary>Gets the schema's Modifier column.</summary>
        public static ValueColumn ModifierColumn { get; private set; }
        ///<summary>Gets the schema's ExternalSource column.</summary>
        public static ValueColumn ExternalSourceColumn { get; private set; }
        ///<summary>Gets the schema's ExternalId column.</summary>
        public static ValueColumn ExternalIdColumn { get; private set; }
        
        ///<summary>Gets the Payments schema instance.</summary>
        public static new TypedSchema<Payment> Schema { get; private set; }
        ///<summary>Gets the SchemaMapping that maps this schema to the SQL Server Payments table.</summary>
        public static SchemaMapping SchemaMapping { get; private set; }
        
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        static Payment() {
            #region Create Schema
            Schema = new TypedSchema<Payment>("Payments");
            
            Schema.PrimaryKey = PaymentIdColumn = Schema.Columns.AddValueColumn("PaymentId", typeof(Guid), null);
            PaymentIdColumn.Unique = true;
            PaymentIdColumn.AllowNulls = false;
            
            PersonColumn = Schema.Columns.AddForeignKey("Person", ShomreiTorah.Data.Person.Schema, "Payments");
            PersonColumn.AllowNulls = false;
            
            DateColumn = Schema.Columns.AddValueColumn("Date", typeof(DateTime), null);
            DateColumn.AllowNulls = false;
            
            MethodColumn = Schema.Columns.AddValueColumn("Method", typeof(String), null);
            MethodColumn.AllowNulls = false;
            
            CheckNumberColumn = Schema.Columns.AddValueColumn("CheckNumber", typeof(String), null);
            CheckNumberColumn.AllowNulls = true;
            
            AccountColumn = Schema.Columns.AddValueColumn("Account", typeof(String), null);
            AccountColumn.AllowNulls = false;
            
            AmountColumn = Schema.Columns.AddValueColumn("Amount", typeof(Decimal), null);
            AmountColumn.AllowNulls = false;
            
            DepositColumn = Schema.Columns.AddForeignKey("Deposit", ShomreiTorah.Data.Deposit.Schema, "Payments");
            DepositColumn.AllowNulls = true;
            
            CommentsColumn = Schema.Columns.AddValueColumn("Comments", typeof(String), null);
            CommentsColumn.AllowNulls = true;
            
            ModifiedColumn = Schema.Columns.AddValueColumn("Modified", typeof(DateTime), null);
            ModifiedColumn.AllowNulls = false;
            
            ModifierColumn = Schema.Columns.AddValueColumn("Modifier", typeof(String), null);
            ModifierColumn.AllowNulls = false;
            
            ExternalSourceColumn = Schema.Columns.AddValueColumn("ExternalSource", typeof(String), null);
            ExternalSourceColumn.AllowNulls = true;
            
            ExternalIdColumn = Schema.Columns.AddValueColumn("ExternalId", typeof(Int32), null);
            ExternalIdColumn.AllowNulls = true;
            #endregion
            
            #region Create SchemaMapping
            SchemaMapping = new SchemaMapping(Schema, false);
            SchemaMapping.SqlName = "Payments";
            SchemaMapping.SqlSchemaName = "Billing";
            
            SchemaMapping.Columns.AddMapping(PaymentIdColumn, "PaymentId");
            SchemaMapping.Columns.AddMapping(PersonColumn, "PersonId");
            SchemaMapping.Columns.AddMapping(DateColumn, "Date");
            SchemaMapping.Columns.AddMapping(MethodColumn, "Method");
            SchemaMapping.Columns.AddMapping(CheckNumberColumn, "CheckNumber");
            SchemaMapping.Columns.AddMapping(AccountColumn, "Account");
            SchemaMapping.Columns.AddMapping(AmountColumn, "Amount");
            SchemaMapping.Columns.AddMapping(DepositColumn, "DepositId");
            SchemaMapping.Columns.AddMapping(CommentsColumn, "Comments");
            SchemaMapping.Columns.AddMapping(ModifiedColumn, "Modified");
            SchemaMapping.Columns.AddMapping(ModifierColumn, "Modifier");
            SchemaMapping.Columns.AddMapping(ExternalSourceColumn, "ExternalSource");
            SchemaMapping.Columns.AddMapping(ExternalIdColumn, "ExternalID");
            #endregion
            SchemaMapping.SetPrimaryMapping(SchemaMapping);
        }
        
        ///<summary>Gets the typed table that contains this row, if any.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public new TypedTable<Payment> Table { get { return (TypedTable<Payment>)base.Table; } }
        #region Value Properties
        ///<summary>Gets or sets the row's unique ID.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Guid PaymentId {
            get { return base.Field<Guid>(PaymentIdColumn); }
            set { base[PaymentIdColumn] = value; }
        }
        ///<summary>Gets or sets the person who made the payment.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Person Person {
            get { return base.Field<Person>(PersonColumn); }
            set { base[PersonColumn] = value; }
        }
        ///<summary>Gets or sets the date of the payment.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public DateTime Date {
            get { return base.Field<DateTime>(DateColumn); }
            set { base[DateColumn] = value; }
        }
        ///<summary>Gets or sets the method of the payment.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Method {
            get { return base.Field<String>(MethodColumn); }
            set { base[MethodColumn] = value; }
        }
        ///<summary>Gets or sets the check number of the payment.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String CheckNumber {
            get { return base.Field<String>(CheckNumberColumn); }
            set { base[CheckNumberColumn] = value; }
        }
        ///<summary>Gets or sets the account of the payment.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Account {
            get { return base.Field<String>(AccountColumn); }
            set { base[AccountColumn] = value; }
        }
        ///<summary>Gets or sets the amount of the payment.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Decimal Amount {
            get { return base.Field<Decimal>(AmountColumn); }
            set { base[AmountColumn] = value; }
        }
        ///<summary>Gets or sets the deposit of the payment.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Deposit Deposit {
            get { return base.Field<Deposit>(DepositColumn); }
            set { base[DepositColumn] = value; }
        }
        ///<summary>Gets or sets the comments of the payment.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Comments {
            get { return base.Field<String>(CommentsColumn); }
            set { base[CommentsColumn] = value; }
        }
        ///<summary>Gets or sets the modified of the payment.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public DateTime Modified {
            get { return base.Field<DateTime>(ModifiedColumn); }
            set { base[ModifiedColumn] = value; }
        }
        ///<summary>Gets or sets the modifier of the payment.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Modifier {
            get { return base.Field<String>(ModifierColumn); }
            set { base[ModifierColumn] = value; }
        }
        ///<summary>Gets or sets the external source of the payment.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String ExternalSource {
            get { return base.Field<String>(ExternalSourceColumn); }
            set { base[ExternalSourceColumn] = value; }
        }
        ///<summary>Gets or sets the external id of the payment.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Int32? ExternalId {
            get { return base.Field<Int32?>(ExternalIdColumn); }
            set { base[ExternalIdColumn] = value; }
        }
        #endregion
        
        #region ChildRows Properties
        ///<summary>Gets the pledges that this payment is paying for.</summary>
        public IChildRowCollection<PledgeLink> LinkedPledges { get { return TypedChildRows<PledgeLink>(PledgeLink.PaymentColumn); } }
        ///<summary>Gets the source that this payment was imported from, if any.</summary>
        public IChildRowCollection<ImportedPayment> ImportedPayments { get { return TypedChildRows<ImportedPayment>(ImportedPayment.PaymentColumn); } }
        #endregion
        
        #region Partial Methods
        partial void OnColumnChanged(Column column, object oldValue, object newValue);
        
        partial void ValidatePaymentId(Guid newValue, Action<string> error);
        partial void OnPaymentIdChanged(Guid? oldValue, Guid? newValue);
        
        partial void ValidatePerson(Person newValue, Action<string> error);
        partial void OnPersonChanged(Person oldValue, Person newValue);
        
        partial void ValidateDate(DateTime newValue, Action<string> error);
        partial void OnDateChanged(DateTime? oldValue, DateTime? newValue);
        
        partial void ValidateMethod(String newValue, Action<string> error);
        partial void OnMethodChanged(String oldValue, String newValue);
        
        partial void ValidateCheckNumber(String newValue, Action<string> error);
        partial void OnCheckNumberChanged(String oldValue, String newValue);
        
        partial void ValidateAccount(String newValue, Action<string> error);
        partial void OnAccountChanged(String oldValue, String newValue);
        
        partial void ValidateAmount(Decimal newValue, Action<string> error);
        partial void OnAmountChanged(Decimal? oldValue, Decimal? newValue);
        
        partial void ValidateDeposit(Deposit newValue, Action<string> error);
        partial void OnDepositChanged(Deposit oldValue, Deposit newValue);
        
        partial void ValidateComments(String newValue, Action<string> error);
        partial void OnCommentsChanged(String oldValue, String newValue);
        
        partial void ValidateModified(DateTime newValue, Action<string> error);
        partial void OnModifiedChanged(DateTime? oldValue, DateTime? newValue);
        
        partial void ValidateModifier(String newValue, Action<string> error);
        partial void OnModifierChanged(String oldValue, String newValue);
        
        partial void ValidateExternalSource(String newValue, Action<string> error);
        partial void OnExternalSourceChanged(String oldValue, String newValue);
        
        partial void ValidateExternalId(Int32? newValue, Action<string> error);
        partial void OnExternalIdChanged(Int32? oldValue, Int32? newValue);
        #endregion
        
        #region Column Callbacks
        ///<summary>Checks whether a value would be valid for a given column in an attached row.</summary>
        ///<param name="column">The column containing the value.</param>
        ///<param name="newValue">The value to validate.</param>
        ///<returns>An error message, or null if the value is valid.</returns>
        ///<remarks>This method is overridden by typed rows to perform custom validation logic.</remarks>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public override string ValidateValue(Column column, object newValue) {
            string error = base.ValidateValue(column, newValue);
            if (!String.IsNullOrEmpty(error)) return error;
            Action<string> reporter = s => error = s;
            
            if (column == PaymentIdColumn) {
                ValidatePaymentId((Guid)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == PersonColumn) {
                ValidatePerson((Person)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == DateColumn) {
                ValidateDate((DateTime)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == MethodColumn) {
                ValidateMethod((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == CheckNumberColumn) {
                ValidateCheckNumber((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == AccountColumn) {
                ValidateAccount((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == AmountColumn) {
                ValidateAmount((Decimal)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == DepositColumn) {
                ValidateDeposit((Deposit)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == CommentsColumn) {
                ValidateComments((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == ModifiedColumn) {
                ValidateModified((DateTime)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == ModifierColumn) {
                ValidateModifier((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == ExternalSourceColumn) {
                ValidateExternalSource((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == ExternalIdColumn) {
                ValidateExternalId((Int32?)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            }
            return null;
        }
        ///<summary>Processes an explicit change of a column value.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        protected override void OnValueChanged(Column column, object oldValue, object newValue) {
            base.OnValueChanged(column, oldValue, newValue);
            OnColumnChanged(column, oldValue, newValue);
            if (column == PaymentIdColumn)
            	OnPaymentIdChanged((Guid?)oldValue, (Guid?)newValue);
            else if (column == PersonColumn)
            	OnPersonChanged((Person)oldValue, (Person)newValue);
            else if (column == DateColumn)
            	OnDateChanged((DateTime?)oldValue, (DateTime?)newValue);
            else if (column == MethodColumn)
            	OnMethodChanged((String)oldValue, (String)newValue);
            else if (column == CheckNumberColumn)
            	OnCheckNumberChanged((String)oldValue, (String)newValue);
            else if (column == AccountColumn)
            	OnAccountChanged((String)oldValue, (String)newValue);
            else if (column == AmountColumn)
            	OnAmountChanged((Decimal?)oldValue, (Decimal?)newValue);
            else if (column == DepositColumn)
            	OnDepositChanged((Deposit)oldValue, (Deposit)newValue);
            else if (column == CommentsColumn)
            	OnCommentsChanged((String)oldValue, (String)newValue);
            else if (column == ModifiedColumn)
            	OnModifiedChanged((DateTime?)oldValue, (DateTime?)newValue);
            else if (column == ModifierColumn)
            	OnModifierChanged((String)oldValue, (String)newValue);
            else if (column == ExternalSourceColumn)
            	OnExternalSourceChanged((String)oldValue, (String)newValue);
            else if (column == ExternalIdColumn)
            	OnExternalIdChanged((Int32?)oldValue, (Int32?)newValue);
        }
        #endregion
    }
    
    ///<summary>Indicates that a payment was intended to pay for a specific pledge.</summary>
    public partial class PledgeLink : Row {
        ///<summary>Creates a new PledgeLink instance.</summary>
        public PledgeLink () : base(Schema) { Initialize(); }
        partial void Initialize();
        
        ///<summary>Creates a strongly-typed PledgeLinks table.</summary>
        public static TypedTable<PledgeLink> CreateTable() { return new TypedTable<PledgeLink>(Schema, () => new PledgeLink()); }
        
        ///<summary>Gets the schema's LinkId column.</summary>
        public static ValueColumn LinkIdColumn { get; private set; }
        ///<summary>Gets the schema's Pledge column.</summary>
        public static ForeignKeyColumn PledgeColumn { get; private set; }
        ///<summary>Gets the schema's Payment column.</summary>
        public static ForeignKeyColumn PaymentColumn { get; private set; }
        ///<summary>Gets the schema's Amount column.</summary>
        public static ValueColumn AmountColumn { get; private set; }
        
        ///<summary>Gets the PledgeLinks schema instance.</summary>
        public static new TypedSchema<PledgeLink> Schema { get; private set; }
        ///<summary>Gets the SchemaMapping that maps this schema to the SQL Server PledgeLinks table.</summary>
        public static SchemaMapping SchemaMapping { get; private set; }
        
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        static PledgeLink() {
            #region Create Schema
            Schema = new TypedSchema<PledgeLink>("PledgeLinks");
            
            Schema.PrimaryKey = LinkIdColumn = Schema.Columns.AddValueColumn("LinkId", typeof(Guid), null);
            LinkIdColumn.Unique = true;
            LinkIdColumn.AllowNulls = false;
            
            PledgeColumn = Schema.Columns.AddForeignKey("Pledge", ShomreiTorah.Data.Pledge.Schema, "LinkedPayments");
            PledgeColumn.AllowNulls = false;
            
            PaymentColumn = Schema.Columns.AddForeignKey("Payment", ShomreiTorah.Data.Payment.Schema, "LinkedPledges");
            PaymentColumn.AllowNulls = false;
            
            AmountColumn = Schema.Columns.AddValueColumn("Amount", typeof(Decimal), null);
            AmountColumn.AllowNulls = false;
            #endregion
            
            #region Create SchemaMapping
            SchemaMapping = new SchemaMapping(Schema, false);
            SchemaMapping.SqlName = "PledgeLinks";
            SchemaMapping.SqlSchemaName = "Billing";
            
            SchemaMapping.Columns.AddMapping(LinkIdColumn, "LinkId");
            SchemaMapping.Columns.AddMapping(PledgeColumn, "PledgeId");
            SchemaMapping.Columns.AddMapping(PaymentColumn, "PaymentId");
            SchemaMapping.Columns.AddMapping(AmountColumn, "Amount");
            #endregion
            SchemaMapping.SetPrimaryMapping(SchemaMapping);
        }
        
        ///<summary>Gets the typed table that contains this row, if any.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public new TypedTable<PledgeLink> Table { get { return (TypedTable<PledgeLink>)base.Table; } }
        #region Value Properties
        ///<summary>Gets or sets the link id of the pledge link.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Guid LinkId {
            get { return base.Field<Guid>(LinkIdColumn); }
            set { base[LinkIdColumn] = value; }
        }
        ///<summary>Gets or sets the pledge that is being paid off.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Pledge Pledge {
            get { return base.Field<Pledge>(PledgeColumn); }
            set { base[PledgeColumn] = value; }
        }
        ///<summary>Gets or sets the payment that this instance describes.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Payment Payment {
            get { return base.Field<Payment>(PaymentColumn); }
            set { base[PaymentColumn] = value; }
        }
        ///<summary>Gets or sets the amount of the payment that covers this pledge.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Decimal Amount {
            get { return base.Field<Decimal>(AmountColumn); }
            set { base[AmountColumn] = value; }
        }
        #endregion
        
        #region Partial Methods
        partial void OnColumnChanged(Column column, object oldValue, object newValue);
        
        partial void ValidateLinkId(Guid newValue, Action<string> error);
        partial void OnLinkIdChanged(Guid? oldValue, Guid? newValue);
        
        partial void ValidatePledge(Pledge newValue, Action<string> error);
        partial void OnPledgeChanged(Pledge oldValue, Pledge newValue);
        
        partial void ValidatePayment(Payment newValue, Action<string> error);
        partial void OnPaymentChanged(Payment oldValue, Payment newValue);
        
        partial void ValidateAmount(Decimal newValue, Action<string> error);
        partial void OnAmountChanged(Decimal? oldValue, Decimal? newValue);
        #endregion
        
        #region Column Callbacks
        ///<summary>Checks whether a value would be valid for a given column in an attached row.</summary>
        ///<param name="column">The column containing the value.</param>
        ///<param name="newValue">The value to validate.</param>
        ///<returns>An error message, or null if the value is valid.</returns>
        ///<remarks>This method is overridden by typed rows to perform custom validation logic.</remarks>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public override string ValidateValue(Column column, object newValue) {
            string error = base.ValidateValue(column, newValue);
            if (!String.IsNullOrEmpty(error)) return error;
            Action<string> reporter = s => error = s;
            
            if (column == LinkIdColumn) {
                ValidateLinkId((Guid)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == PledgeColumn) {
                ValidatePledge((Pledge)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == PaymentColumn) {
                ValidatePayment((Payment)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == AmountColumn) {
                ValidateAmount((Decimal)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            }
            return null;
        }
        ///<summary>Processes an explicit change of a column value.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        protected override void OnValueChanged(Column column, object oldValue, object newValue) {
            base.OnValueChanged(column, oldValue, newValue);
            OnColumnChanged(column, oldValue, newValue);
            if (column == LinkIdColumn)
            	OnLinkIdChanged((Guid?)oldValue, (Guid?)newValue);
            else if (column == PledgeColumn)
            	OnPledgeChanged((Pledge)oldValue, (Pledge)newValue);
            else if (column == PaymentColumn)
            	OnPaymentChanged((Payment)oldValue, (Payment)newValue);
            else if (column == AmountColumn)
            	OnAmountChanged((Decimal?)oldValue, (Decimal?)newValue);
        }
        #endregion
    }
    
    ///<summary>Describes a pledge.</summary>
    public partial class Pledge : Row {
        ///<summary>Creates a new Pledge instance.</summary>
        public Pledge () : base(Schema) { Initialize(); }
        partial void Initialize();
        
        ///<summary>Creates a strongly-typed Pledges table.</summary>
        public static TypedTable<Pledge> CreateTable() { return new TypedTable<Pledge>(Schema, () => new Pledge()); }
        
        ///<summary>Gets the schema's PledgeId column.</summary>
        public static ValueColumn PledgeIdColumn { get; private set; }
        ///<summary>Gets the schema's Person column.</summary>
        public static ForeignKeyColumn PersonColumn { get; private set; }
        ///<summary>Gets the schema's Date column.</summary>
        public static ValueColumn DateColumn { get; private set; }
        ///<summary>Gets the schema's Type column.</summary>
        public static ValueColumn TypeColumn { get; private set; }
        ///<summary>Gets the schema's SubType column.</summary>
        public static ValueColumn SubTypeColumn { get; private set; }
        ///<summary>Gets the schema's Account column.</summary>
        public static ValueColumn AccountColumn { get; private set; }
        ///<summary>Gets the schema's Amount column.</summary>
        public static ValueColumn AmountColumn { get; private set; }
        ///<summary>Gets the schema's Note column.</summary>
        public static ValueColumn NoteColumn { get; private set; }
        ///<summary>Gets the schema's Comments column.</summary>
        public static ValueColumn CommentsColumn { get; private set; }
        ///<summary>Gets the schema's Modified column.</summary>
        public static ValueColumn ModifiedColumn { get; private set; }
        ///<summary>Gets the schema's Modifier column.</summary>
        public static ValueColumn ModifierColumn { get; private set; }
        ///<summary>Gets the schema's ExternalSource column.</summary>
        public static ValueColumn ExternalSourceColumn { get; private set; }
        ///<summary>Gets the schema's ExternalId column.</summary>
        public static ValueColumn ExternalIdColumn { get; private set; }
        
        ///<summary>Gets the Pledges schema instance.</summary>
        public static new TypedSchema<Pledge> Schema { get; private set; }
        ///<summary>Gets the SchemaMapping that maps this schema to the SQL Server Pledges table.</summary>
        public static SchemaMapping SchemaMapping { get; private set; }
        
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        static Pledge() {
            #region Create Schema
            Schema = new TypedSchema<Pledge>("Pledges");
            
            Schema.PrimaryKey = PledgeIdColumn = Schema.Columns.AddValueColumn("PledgeId", typeof(Guid), null);
            PledgeIdColumn.Unique = true;
            PledgeIdColumn.AllowNulls = false;
            
            PersonColumn = Schema.Columns.AddForeignKey("Person", ShomreiTorah.Data.Person.Schema, "Pledges");
            PersonColumn.AllowNulls = false;
            
            DateColumn = Schema.Columns.AddValueColumn("Date", typeof(DateTime), null);
            DateColumn.AllowNulls = false;
            
            TypeColumn = Schema.Columns.AddValueColumn("Type", typeof(String), null);
            TypeColumn.AllowNulls = false;
            
            SubTypeColumn = Schema.Columns.AddValueColumn("SubType", typeof(String), "");
            SubTypeColumn.AllowNulls = false;
            
            AccountColumn = Schema.Columns.AddValueColumn("Account", typeof(String), null);
            AccountColumn.AllowNulls = false;
            
            AmountColumn = Schema.Columns.AddValueColumn("Amount", typeof(Decimal), null);
            AmountColumn.AllowNulls = false;
            
            NoteColumn = Schema.Columns.AddValueColumn("Note", typeof(String), null);
            NoteColumn.AllowNulls = true;
            
            CommentsColumn = Schema.Columns.AddValueColumn("Comments", typeof(String), null);
            CommentsColumn.AllowNulls = true;
            
            ModifiedColumn = Schema.Columns.AddValueColumn("Modified", typeof(DateTime), null);
            ModifiedColumn.AllowNulls = false;
            
            ModifierColumn = Schema.Columns.AddValueColumn("Modifier", typeof(String), null);
            ModifierColumn.AllowNulls = false;
            
            ExternalSourceColumn = Schema.Columns.AddValueColumn("ExternalSource", typeof(String), null);
            ExternalSourceColumn.AllowNulls = true;
            
            ExternalIdColumn = Schema.Columns.AddValueColumn("ExternalId", typeof(Int32), null);
            ExternalIdColumn.AllowNulls = true;
            #endregion
            
            #region Create SchemaMapping
            SchemaMapping = new SchemaMapping(Schema, false);
            SchemaMapping.SqlName = "Pledges";
            SchemaMapping.SqlSchemaName = "Billing";
            
            SchemaMapping.Columns.AddMapping(PledgeIdColumn, "PledgeId");
            SchemaMapping.Columns.AddMapping(PersonColumn, "PersonId");
            SchemaMapping.Columns.AddMapping(DateColumn, "Date");
            SchemaMapping.Columns.AddMapping(TypeColumn, "Type");
            SchemaMapping.Columns.AddMapping(SubTypeColumn, "SubType");
            SchemaMapping.Columns.AddMapping(AccountColumn, "Account");
            SchemaMapping.Columns.AddMapping(AmountColumn, "Amount");
            SchemaMapping.Columns.AddMapping(NoteColumn, "Note");
            SchemaMapping.Columns.AddMapping(CommentsColumn, "Comments");
            SchemaMapping.Columns.AddMapping(ModifiedColumn, "Modified");
            SchemaMapping.Columns.AddMapping(ModifierColumn, "Modifier");
            SchemaMapping.Columns.AddMapping(ExternalSourceColumn, "ExternalSource");
            SchemaMapping.Columns.AddMapping(ExternalIdColumn, "ExternalID");
            #endregion
            SchemaMapping.SetPrimaryMapping(SchemaMapping);
        }
        
        ///<summary>Gets the typed table that contains this row, if any.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public new TypedTable<Pledge> Table { get { return (TypedTable<Pledge>)base.Table; } }
        #region Value Properties
        ///<summary>Gets or sets the row's unique ID.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Guid PledgeId {
            get { return base.Field<Guid>(PledgeIdColumn); }
            set { base[PledgeIdColumn] = value; }
        }
        ///<summary>Gets or sets the person who made the pledge.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Person Person {
            get { return base.Field<Person>(PersonColumn); }
            set { base[PersonColumn] = value; }
        }
        ///<summary>Gets or sets the date of the pledge.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public DateTime Date {
            get { return base.Field<DateTime>(DateColumn); }
            set { base[DateColumn] = value; }
        }
        ///<summary>Gets or sets the type of the pledge.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Type {
            get { return base.Field<String>(TypeColumn); }
            set { base[TypeColumn] = value; }
        }
        ///<summary>Gets or sets the sub type of the pledge.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String SubType {
            get { return base.Field<String>(SubTypeColumn); }
            set { base[SubTypeColumn] = value; }
        }
        ///<summary>Gets or sets the account of the pledge.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Account {
            get { return base.Field<String>(AccountColumn); }
            set { base[AccountColumn] = value; }
        }
        ///<summary>Gets or sets the amount of the pledge.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Decimal Amount {
            get { return base.Field<Decimal>(AmountColumn); }
            set { base[AmountColumn] = value; }
        }
        ///<summary>Gets or sets the note of the pledge.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Note {
            get { return base.Field<String>(NoteColumn); }
            set { base[NoteColumn] = value; }
        }
        ///<summary>Gets or sets the comments of the pledge.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Comments {
            get { return base.Field<String>(CommentsColumn); }
            set { base[CommentsColumn] = value; }
        }
        ///<summary>Gets or sets the modified of the pledge.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public DateTime Modified {
            get { return base.Field<DateTime>(ModifiedColumn); }
            set { base[ModifiedColumn] = value; }
        }
        ///<summary>Gets or sets the modifier of the pledge.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Modifier {
            get { return base.Field<String>(ModifierColumn); }
            set { base[ModifierColumn] = value; }
        }
        ///<summary>Gets or sets the external source of the pledge.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String ExternalSource {
            get { return base.Field<String>(ExternalSourceColumn); }
            set { base[ExternalSourceColumn] = value; }
        }
        ///<summary>Gets or sets the external id of the pledge.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Int32? ExternalId {
            get { return base.Field<Int32?>(ExternalIdColumn); }
            set { base[ExternalIdColumn] = value; }
        }
        #endregion
        
        #region ChildRows Properties
        ///<summary>Gets the seating reservations associated with the pledge.</summary>
        public IChildRowCollection<SeatingReservation> SeatingReservations { get { return TypedChildRows<SeatingReservation>(SeatingReservation.PledgeColumn); } }
        ///<summary>Gets the payments that pay for this pledge.</summary>
        public IChildRowCollection<PledgeLink> LinkedPayments { get { return TypedChildRows<PledgeLink>(PledgeLink.PledgeColumn); } }
        #endregion
        
        #region Partial Methods
        partial void OnColumnChanged(Column column, object oldValue, object newValue);
        
        partial void ValidatePledgeId(Guid newValue, Action<string> error);
        partial void OnPledgeIdChanged(Guid? oldValue, Guid? newValue);
        
        partial void ValidatePerson(Person newValue, Action<string> error);
        partial void OnPersonChanged(Person oldValue, Person newValue);
        
        partial void ValidateDate(DateTime newValue, Action<string> error);
        partial void OnDateChanged(DateTime? oldValue, DateTime? newValue);
        
        partial void ValidateType(String newValue, Action<string> error);
        partial void OnTypeChanged(String oldValue, String newValue);
        
        partial void ValidateSubType(String newValue, Action<string> error);
        partial void OnSubTypeChanged(String oldValue, String newValue);
        
        partial void ValidateAccount(String newValue, Action<string> error);
        partial void OnAccountChanged(String oldValue, String newValue);
        
        partial void ValidateAmount(Decimal newValue, Action<string> error);
        partial void OnAmountChanged(Decimal? oldValue, Decimal? newValue);
        
        partial void ValidateNote(String newValue, Action<string> error);
        partial void OnNoteChanged(String oldValue, String newValue);
        
        partial void ValidateComments(String newValue, Action<string> error);
        partial void OnCommentsChanged(String oldValue, String newValue);
        
        partial void ValidateModified(DateTime newValue, Action<string> error);
        partial void OnModifiedChanged(DateTime? oldValue, DateTime? newValue);
        
        partial void ValidateModifier(String newValue, Action<string> error);
        partial void OnModifierChanged(String oldValue, String newValue);
        
        partial void ValidateExternalSource(String newValue, Action<string> error);
        partial void OnExternalSourceChanged(String oldValue, String newValue);
        
        partial void ValidateExternalId(Int32? newValue, Action<string> error);
        partial void OnExternalIdChanged(Int32? oldValue, Int32? newValue);
        #endregion
        
        #region Column Callbacks
        ///<summary>Checks whether a value would be valid for a given column in an attached row.</summary>
        ///<param name="column">The column containing the value.</param>
        ///<param name="newValue">The value to validate.</param>
        ///<returns>An error message, or null if the value is valid.</returns>
        ///<remarks>This method is overridden by typed rows to perform custom validation logic.</remarks>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public override string ValidateValue(Column column, object newValue) {
            string error = base.ValidateValue(column, newValue);
            if (!String.IsNullOrEmpty(error)) return error;
            Action<string> reporter = s => error = s;
            
            if (column == PledgeIdColumn) {
                ValidatePledgeId((Guid)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == PersonColumn) {
                ValidatePerson((Person)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == DateColumn) {
                ValidateDate((DateTime)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == TypeColumn) {
                ValidateType((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == SubTypeColumn) {
                ValidateSubType((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == AccountColumn) {
                ValidateAccount((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == AmountColumn) {
                ValidateAmount((Decimal)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == NoteColumn) {
                ValidateNote((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == CommentsColumn) {
                ValidateComments((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == ModifiedColumn) {
                ValidateModified((DateTime)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == ModifierColumn) {
                ValidateModifier((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == ExternalSourceColumn) {
                ValidateExternalSource((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == ExternalIdColumn) {
                ValidateExternalId((Int32?)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            }
            return null;
        }
        ///<summary>Processes an explicit change of a column value.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        protected override void OnValueChanged(Column column, object oldValue, object newValue) {
            base.OnValueChanged(column, oldValue, newValue);
            OnColumnChanged(column, oldValue, newValue);
            if (column == PledgeIdColumn)
            	OnPledgeIdChanged((Guid?)oldValue, (Guid?)newValue);
            else if (column == PersonColumn)
            	OnPersonChanged((Person)oldValue, (Person)newValue);
            else if (column == DateColumn)
            	OnDateChanged((DateTime?)oldValue, (DateTime?)newValue);
            else if (column == TypeColumn)
            	OnTypeChanged((String)oldValue, (String)newValue);
            else if (column == SubTypeColumn)
            	OnSubTypeChanged((String)oldValue, (String)newValue);
            else if (column == AccountColumn)
            	OnAccountChanged((String)oldValue, (String)newValue);
            else if (column == AmountColumn)
            	OnAmountChanged((Decimal?)oldValue, (Decimal?)newValue);
            else if (column == NoteColumn)
            	OnNoteChanged((String)oldValue, (String)newValue);
            else if (column == CommentsColumn)
            	OnCommentsChanged((String)oldValue, (String)newValue);
            else if (column == ModifiedColumn)
            	OnModifiedChanged((DateTime?)oldValue, (DateTime?)newValue);
            else if (column == ModifierColumn)
            	OnModifierChanged((String)oldValue, (String)newValue);
            else if (column == ExternalSourceColumn)
            	OnExternalSourceChanged((String)oldValue, (String)newValue);
            else if (column == ExternalIdColumn)
            	OnExternalIdChanged((Int32?)oldValue, (Int32?)newValue);
        }
        #endregion
    }
    
    ///<summary>Describes a raffle ticket.</summary>
    public partial class RaffleTicket : Row {
        ///<summary>Creates a new RaffleTicket instance.</summary>
        public RaffleTicket () : base(Schema) { Initialize(); }
        partial void Initialize();
        
        ///<summary>Creates a strongly-typed RaffleTickets table.</summary>
        public static TypedTable<RaffleTicket> CreateTable() { return new TypedTable<RaffleTicket>(Schema, () => new RaffleTicket()); }
        
        ///<summary>Gets the schema's RowId column.</summary>
        public static ValueColumn RowIdColumn { get; private set; }
        ///<summary>Gets the schema's Person column.</summary>
        public static ForeignKeyColumn PersonColumn { get; private set; }
        ///<summary>Gets the schema's DateAdded column.</summary>
        public static ValueColumn DateAddedColumn { get; private set; }
        ///<summary>Gets the schema's Year column.</summary>
        public static ValueColumn YearColumn { get; private set; }
        ///<summary>Gets the schema's TicketId column.</summary>
        public static ValueColumn TicketIdColumn { get; private set; }
        ///<summary>Gets the schema's Paid column.</summary>
        public static ValueColumn PaidColumn { get; private set; }
        ///<summary>Gets the schema's Comments column.</summary>
        public static ValueColumn CommentsColumn { get; private set; }
        
        ///<summary>Gets the RaffleTickets schema instance.</summary>
        public static new TypedSchema<RaffleTicket> Schema { get; private set; }
        ///<summary>Gets the SchemaMapping that maps this schema to the SQL Server RaffleTickets table.</summary>
        public static SchemaMapping SchemaMapping { get; private set; }
        
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        static RaffleTicket() {
            #region Create Schema
            Schema = new TypedSchema<RaffleTicket>("RaffleTickets");
            
            Schema.PrimaryKey = RowIdColumn = Schema.Columns.AddValueColumn("RowId", typeof(Guid), null);
            RowIdColumn.Unique = true;
            RowIdColumn.AllowNulls = false;
            
            PersonColumn = Schema.Columns.AddForeignKey("Person", ShomreiTorah.Data.Person.Schema, "RaffleTickets");
            PersonColumn.AllowNulls = false;
            
            DateAddedColumn = Schema.Columns.AddValueColumn("DateAdded", typeof(DateTime), null);
            DateAddedColumn.AllowNulls = false;
            
            YearColumn = Schema.Columns.AddValueColumn("Year", typeof(Int32), null);
            YearColumn.AllowNulls = false;
            
            TicketIdColumn = Schema.Columns.AddValueColumn("TicketId", typeof(Int32), null);
            TicketIdColumn.AllowNulls = false;
            
            PaidColumn = Schema.Columns.AddValueColumn("Paid", typeof(Boolean), null);
            PaidColumn.AllowNulls = false;
            
            CommentsColumn = Schema.Columns.AddValueColumn("Comments", typeof(String), null);
            CommentsColumn.AllowNulls = true;
            #endregion
            
            #region Create SchemaMapping
            SchemaMapping = new SchemaMapping(Schema, false);
            SchemaMapping.SqlName = "RaffleTickets";
            SchemaMapping.SqlSchemaName = "MelaveMalka";
            
            SchemaMapping.Columns.AddMapping(RowIdColumn, "RowId");
            SchemaMapping.Columns.AddMapping(PersonColumn, "PersonId");
            SchemaMapping.Columns.AddMapping(DateAddedColumn, "DateAdded");
            SchemaMapping.Columns.AddMapping(YearColumn, "Year");
            SchemaMapping.Columns.AddMapping(TicketIdColumn, "TicketId");
            SchemaMapping.Columns.AddMapping(PaidColumn, "Paid");
            SchemaMapping.Columns.AddMapping(CommentsColumn, "Comments");
            #endregion
            SchemaMapping.SetPrimaryMapping(SchemaMapping);
        }
        
        ///<summary>Gets the typed table that contains this row, if any.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public new TypedTable<RaffleTicket> Table { get { return (TypedTable<RaffleTicket>)base.Table; } }
        #region Value Properties
        ///<summary>Gets or sets the row id of the raffle ticket.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Guid RowId {
            get { return base.Field<Guid>(RowIdColumn); }
            set { base[RowIdColumn] = value; }
        }
        ///<summary>Gets or sets the person that bought the ticket.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Person Person {
            get { return base.Field<Person>(PersonColumn); }
            set { base[PersonColumn] = value; }
        }
        ///<summary>Gets or sets the date added of the raffle ticket.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public DateTime DateAdded {
            get { return base.Field<DateTime>(DateAddedColumn); }
            set { base[DateAddedColumn] = value; }
        }
        ///<summary>Gets or sets the year of the raffle.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Int32 Year {
            get { return base.Field<Int32>(YearColumn); }
            set { base[YearColumn] = value; }
        }
        ///<summary>Gets or sets the ticket number.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Int32 TicketId {
            get { return base.Field<Int32>(TicketIdColumn); }
            set { base[TicketIdColumn] = value; }
        }
        ///<summary>Gets or sets whether ticket has been paid for.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Boolean Paid {
            get { return base.Field<Boolean>(PaidColumn); }
            set { base[PaidColumn] = value; }
        }
        ///<summary>Gets or sets comments regarding the ticket.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Comments {
            get { return base.Field<String>(CommentsColumn); }
            set { base[CommentsColumn] = value; }
        }
        #endregion
        
        #region Partial Methods
        partial void OnColumnChanged(Column column, object oldValue, object newValue);
        
        partial void ValidateRowId(Guid newValue, Action<string> error);
        partial void OnRowIdChanged(Guid? oldValue, Guid? newValue);
        
        partial void ValidatePerson(Person newValue, Action<string> error);
        partial void OnPersonChanged(Person oldValue, Person newValue);
        
        partial void ValidateDateAdded(DateTime newValue, Action<string> error);
        partial void OnDateAddedChanged(DateTime? oldValue, DateTime? newValue);
        
        partial void ValidateYear(Int32 newValue, Action<string> error);
        partial void OnYearChanged(Int32? oldValue, Int32? newValue);
        
        partial void ValidateTicketId(Int32 newValue, Action<string> error);
        partial void OnTicketIdChanged(Int32? oldValue, Int32? newValue);
        
        partial void ValidatePaid(Boolean newValue, Action<string> error);
        partial void OnPaidChanged(Boolean? oldValue, Boolean? newValue);
        
        partial void ValidateComments(String newValue, Action<string> error);
        partial void OnCommentsChanged(String oldValue, String newValue);
        #endregion
        
        #region Column Callbacks
        ///<summary>Checks whether a value would be valid for a given column in an attached row.</summary>
        ///<param name="column">The column containing the value.</param>
        ///<param name="newValue">The value to validate.</param>
        ///<returns>An error message, or null if the value is valid.</returns>
        ///<remarks>This method is overridden by typed rows to perform custom validation logic.</remarks>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public override string ValidateValue(Column column, object newValue) {
            string error = base.ValidateValue(column, newValue);
            if (!String.IsNullOrEmpty(error)) return error;
            Action<string> reporter = s => error = s;
            
            if (column == RowIdColumn) {
                ValidateRowId((Guid)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == PersonColumn) {
                ValidatePerson((Person)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == DateAddedColumn) {
                ValidateDateAdded((DateTime)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == YearColumn) {
                ValidateYear((Int32)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == TicketIdColumn) {
                ValidateTicketId((Int32)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == PaidColumn) {
                ValidatePaid((Boolean)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == CommentsColumn) {
                ValidateComments((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            }
            return null;
        }
        ///<summary>Processes an explicit change of a column value.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        protected override void OnValueChanged(Column column, object oldValue, object newValue) {
            base.OnValueChanged(column, oldValue, newValue);
            OnColumnChanged(column, oldValue, newValue);
            if (column == RowIdColumn)
            	OnRowIdChanged((Guid?)oldValue, (Guid?)newValue);
            else if (column == PersonColumn)
            	OnPersonChanged((Person)oldValue, (Person)newValue);
            else if (column == DateAddedColumn)
            	OnDateAddedChanged((DateTime?)oldValue, (DateTime?)newValue);
            else if (column == YearColumn)
            	OnYearChanged((Int32?)oldValue, (Int32?)newValue);
            else if (column == TicketIdColumn)
            	OnTicketIdChanged((Int32?)oldValue, (Int32?)newValue);
            else if (column == PaidColumn)
            	OnPaidChanged((Boolean?)oldValue, (Boolean?)newValue);
            else if (column == CommentsColumn)
            	OnCommentsChanged((String)oldValue, (String)newValue);
        }
        #endregion
    }
    
    ///<summary>Describes a relation between a member and someone else.</summary>
    public partial class RelativeLink : Row {
        ///<summary>Creates a new RelativeLink instance.</summary>
        public RelativeLink () : base(Schema) { Initialize(); }
        partial void Initialize();
        
        ///<summary>Creates a strongly-typed Relatives table.</summary>
        public static TypedTable<RelativeLink> CreateTable() { return new TypedTable<RelativeLink>(Schema, () => new RelativeLink()); }
        
        ///<summary>Gets the schema's RowId column.</summary>
        public static ValueColumn RowIdColumn { get; private set; }
        ///<summary>Gets the schema's Member column.</summary>
        public static ForeignKeyColumn MemberColumn { get; private set; }
        ///<summary>Gets the schema's Relative column.</summary>
        public static ForeignKeyColumn RelativeColumn { get; private set; }
        ///<summary>Gets the schema's Relation column.</summary>
        public static ValueColumn RelationTypeColumn { get; private set; }
        
        ///<summary>Gets the Relatives schema instance.</summary>
        public static new TypedSchema<RelativeLink> Schema { get; private set; }
        ///<summary>Gets the SchemaMapping that maps this schema to the SQL Server Relatives table.</summary>
        public static SchemaMapping SchemaMapping { get; private set; }
        
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        static RelativeLink() {
            #region Create Schema
            Schema = new TypedSchema<RelativeLink>("Relatives");
            
            Schema.PrimaryKey = RowIdColumn = Schema.Columns.AddValueColumn("RowId", typeof(Guid), null);
            RowIdColumn.Unique = true;
            RowIdColumn.AllowNulls = false;
            
            MemberColumn = Schema.Columns.AddForeignKey("Member", ShomreiTorah.Data.Person.Schema, "ForeignRelatives");
            MemberColumn.AllowNulls = false;
            
            RelativeColumn = Schema.Columns.AddForeignKey("Relative", ShomreiTorah.Data.Person.Schema, "Relatives");
            RelativeColumn.AllowNulls = false;
            
            RelationTypeColumn = Schema.Columns.AddValueColumn("Relation", typeof(String), null);
            RelationTypeColumn.AllowNulls = false;
            #endregion
            
            #region Create SchemaMapping
            SchemaMapping = new SchemaMapping(Schema, false);
            SchemaMapping.SqlName = "Relatives";
            SchemaMapping.SqlSchemaName = "Data";
            
            SchemaMapping.Columns.AddMapping(RowIdColumn, "RowId");
            SchemaMapping.Columns.AddMapping(MemberColumn, "MemberId");
            SchemaMapping.Columns.AddMapping(RelativeColumn, "RelativeId");
            SchemaMapping.Columns.AddMapping(RelationTypeColumn, "Relation");
            #endregion
            SchemaMapping.SetPrimaryMapping(SchemaMapping);
        }
        
        ///<summary>Gets the typed table that contains this row, if any.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public new TypedTable<RelativeLink> Table { get { return (TypedTable<RelativeLink>)base.Table; } }
        #region Value Properties
        ///<summary>Gets or sets the row id of the relative.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Guid RowId {
            get { return base.Field<Guid>(RowIdColumn); }
            set { base[RowIdColumn] = value; }
        }
        ///<summary>Gets or sets the member of the relative.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Person Member {
            get { return base.Field<Person>(MemberColumn); }
            set { base[MemberColumn] = value; }
        }
        ///<summary>Gets or sets the relative of the relative.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Person Relative {
            get { return base.Field<Person>(RelativeColumn); }
            set { base[RelativeColumn] = value; }
        }
        ///<summary>Gets or sets the type of relation.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String RelationType {
            get { return base.Field<String>(RelationTypeColumn); }
            set { base[RelationTypeColumn] = value; }
        }
        #endregion
        
        #region Partial Methods
        partial void OnColumnChanged(Column column, object oldValue, object newValue);
        
        partial void ValidateRowId(Guid newValue, Action<string> error);
        partial void OnRowIdChanged(Guid? oldValue, Guid? newValue);
        
        partial void ValidateMember(Person newValue, Action<string> error);
        partial void OnMemberChanged(Person oldValue, Person newValue);
        
        partial void ValidateRelative(Person newValue, Action<string> error);
        partial void OnRelativeChanged(Person oldValue, Person newValue);
        
        partial void ValidateRelationType(String newValue, Action<string> error);
        partial void OnRelationTypeChanged(String oldValue, String newValue);
        #endregion
        
        #region Column Callbacks
        ///<summary>Checks whether a value would be valid for a given column in an attached row.</summary>
        ///<param name="column">The column containing the value.</param>
        ///<param name="newValue">The value to validate.</param>
        ///<returns>An error message, or null if the value is valid.</returns>
        ///<remarks>This method is overridden by typed rows to perform custom validation logic.</remarks>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public override string ValidateValue(Column column, object newValue) {
            string error = base.ValidateValue(column, newValue);
            if (!String.IsNullOrEmpty(error)) return error;
            Action<string> reporter = s => error = s;
            
            if (column == RowIdColumn) {
                ValidateRowId((Guid)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == MemberColumn) {
                ValidateMember((Person)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == RelativeColumn) {
                ValidateRelative((Person)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == RelationTypeColumn) {
                ValidateRelationType((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            }
            return null;
        }
        ///<summary>Processes an explicit change of a column value.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        protected override void OnValueChanged(Column column, object oldValue, object newValue) {
            base.OnValueChanged(column, oldValue, newValue);
            OnColumnChanged(column, oldValue, newValue);
            if (column == RowIdColumn)
            	OnRowIdChanged((Guid?)oldValue, (Guid?)newValue);
            else if (column == MemberColumn)
            	OnMemberChanged((Person)oldValue, (Person)newValue);
            else if (column == RelativeColumn)
            	OnRelativeChanged((Person)oldValue, (Person)newValue);
            else if (column == RelationTypeColumn)
            	OnRelationTypeChanged((String)oldValue, (String)newValue);
        }
        #endregion
    }
    
    ///<summary>Describes an email sent to an invitee to ask for an ad.</summary>
    public partial class AdReminderEmail : Row {
        ///<summary>Creates a new AdReminderEmail instance.</summary>
        public AdReminderEmail () : base(Schema) { Initialize(); }
        partial void Initialize();
        
        ///<summary>Creates a strongly-typed ReminderEmailLog table.</summary>
        public static TypedTable<AdReminderEmail> CreateTable() { return new TypedTable<AdReminderEmail>(Schema, () => new AdReminderEmail()); }
        
        ///<summary>Gets the schema's RowId column.</summary>
        public static ValueColumn RowIdColumn { get; private set; }
        ///<summary>Gets the schema's Recipient column.</summary>
        public static ForeignKeyColumn RecipientColumn { get; private set; }
        ///<summary>Gets the schema's Date column.</summary>
        public static ValueColumn DateColumn { get; private set; }
        ///<summary>Gets the schema's EmailSubject column.</summary>
        public static ValueColumn EmailSubjectColumn { get; private set; }
        ///<summary>Gets the schema's EmailSource column.</summary>
        public static ValueColumn EmailSourceColumn { get; private set; }
        
        ///<summary>Gets the ReminderEmailLog schema instance.</summary>
        public static new TypedSchema<AdReminderEmail> Schema { get; private set; }
        ///<summary>Gets the SchemaMapping that maps this schema to the SQL Server ReminderEmailLog table.</summary>
        public static SchemaMapping SchemaMapping { get; private set; }
        
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        static AdReminderEmail() {
            #region Create Schema
            Schema = new TypedSchema<AdReminderEmail>("ReminderEmailLog");
            
            Schema.PrimaryKey = RowIdColumn = Schema.Columns.AddValueColumn("RowId", typeof(Guid), null);
            RowIdColumn.Unique = true;
            RowIdColumn.AllowNulls = false;
            
            RecipientColumn = Schema.Columns.AddForeignKey("Recipient", ShomreiTorah.Data.MelaveMalkaInvitation.Schema, "ReminderEmailLogs");
            RecipientColumn.AllowNulls = false;
            
            DateColumn = Schema.Columns.AddValueColumn("Date", typeof(DateTime), null);
            DateColumn.AllowNulls = false;
            
            EmailSubjectColumn = Schema.Columns.AddValueColumn("EmailSubject", typeof(String), null);
            EmailSubjectColumn.AllowNulls = false;
            
            EmailSourceColumn = Schema.Columns.AddValueColumn("EmailSource", typeof(String), null);
            EmailSourceColumn.AllowNulls = false;
            #endregion
            
            #region Create SchemaMapping
            SchemaMapping = new SchemaMapping(Schema, false);
            SchemaMapping.SqlName = "ReminderEmailLog";
            SchemaMapping.SqlSchemaName = "MelaveMalka";
            
            SchemaMapping.Columns.AddMapping(RowIdColumn, "RowId");
            SchemaMapping.Columns.AddMapping(RecipientColumn, "InviteId");
            SchemaMapping.Columns.AddMapping(DateColumn, "Date");
            SchemaMapping.Columns.AddMapping(EmailSubjectColumn, "EmailSubject");
            SchemaMapping.Columns.AddMapping(EmailSourceColumn, "EmailSource");
            #endregion
            SchemaMapping.SetPrimaryMapping(SchemaMapping);
        }
        
        ///<summary>Gets the typed table that contains this row, if any.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public new TypedTable<AdReminderEmail> Table { get { return (TypedTable<AdReminderEmail>)base.Table; } }
        #region Value Properties
        ///<summary>Gets or sets the row id of the reminder email log.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Guid RowId {
            get { return base.Field<Guid>(RowIdColumn); }
            set { base[RowIdColumn] = value; }
        }
        ///<summary>Gets or sets the person who received the email.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public MelaveMalkaInvitation Recipient {
            get { return base.Field<MelaveMalkaInvitation>(RecipientColumn); }
            set { base[RecipientColumn] = value; }
        }
        ///<summary>Gets or sets the date that the email was sent.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public DateTime Date {
            get { return base.Field<DateTime>(DateColumn); }
            set { base[DateColumn] = value; }
        }
        ///<summary>Gets or sets the subject of the email.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String EmailSubject {
            get { return base.Field<String>(EmailSubjectColumn); }
            set { base[EmailSubjectColumn] = value; }
        }
        ///<summary>Gets or sets the email's HTML source.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String EmailSource {
            get { return base.Field<String>(EmailSourceColumn); }
            set { base[EmailSourceColumn] = value; }
        }
        #endregion
        
        #region Partial Methods
        partial void OnColumnChanged(Column column, object oldValue, object newValue);
        
        partial void ValidateRowId(Guid newValue, Action<string> error);
        partial void OnRowIdChanged(Guid? oldValue, Guid? newValue);
        
        partial void ValidateRecipient(MelaveMalkaInvitation newValue, Action<string> error);
        partial void OnRecipientChanged(MelaveMalkaInvitation oldValue, MelaveMalkaInvitation newValue);
        
        partial void ValidateDate(DateTime newValue, Action<string> error);
        partial void OnDateChanged(DateTime? oldValue, DateTime? newValue);
        
        partial void ValidateEmailSubject(String newValue, Action<string> error);
        partial void OnEmailSubjectChanged(String oldValue, String newValue);
        
        partial void ValidateEmailSource(String newValue, Action<string> error);
        partial void OnEmailSourceChanged(String oldValue, String newValue);
        #endregion
        
        #region Column Callbacks
        ///<summary>Checks whether a value would be valid for a given column in an attached row.</summary>
        ///<param name="column">The column containing the value.</param>
        ///<param name="newValue">The value to validate.</param>
        ///<returns>An error message, or null if the value is valid.</returns>
        ///<remarks>This method is overridden by typed rows to perform custom validation logic.</remarks>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public override string ValidateValue(Column column, object newValue) {
            string error = base.ValidateValue(column, newValue);
            if (!String.IsNullOrEmpty(error)) return error;
            Action<string> reporter = s => error = s;
            
            if (column == RowIdColumn) {
                ValidateRowId((Guid)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == RecipientColumn) {
                ValidateRecipient((MelaveMalkaInvitation)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == DateColumn) {
                ValidateDate((DateTime)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == EmailSubjectColumn) {
                ValidateEmailSubject((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == EmailSourceColumn) {
                ValidateEmailSource((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            }
            return null;
        }
        ///<summary>Processes an explicit change of a column value.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        protected override void OnValueChanged(Column column, object oldValue, object newValue) {
            base.OnValueChanged(column, oldValue, newValue);
            OnColumnChanged(column, oldValue, newValue);
            if (column == RowIdColumn)
            	OnRowIdChanged((Guid?)oldValue, (Guid?)newValue);
            else if (column == RecipientColumn)
            	OnRecipientChanged((MelaveMalkaInvitation)oldValue, (MelaveMalkaInvitation)newValue);
            else if (column == DateColumn)
            	OnDateChanged((DateTime?)oldValue, (DateTime?)newValue);
            else if (column == EmailSubjectColumn)
            	OnEmailSubjectChanged((String)oldValue, (String)newValue);
            else if (column == EmailSourceColumn)
            	OnEmailSourceChanged((String)oldValue, (String)newValue);
        }
        #endregion
    }
    
    ///<summary>Describes a ימים נוראים seating reservation.</summary>
    public partial class SeatingReservation : Row {
        ///<summary>Creates a new SeatingReservation instance.</summary>
        public SeatingReservation () : base(Schema) { Initialize(); }
        partial void Initialize();
        
        ///<summary>Creates a strongly-typed SeatingReservations table.</summary>
        public static TypedTable<SeatingReservation> CreateTable() { return new TypedTable<SeatingReservation>(Schema, () => new SeatingReservation()); }
        
        ///<summary>Gets the schema's Id column.</summary>
        public static ValueColumn IdColumn { get; private set; }
        ///<summary>Gets the schema's Pledge column.</summary>
        public static ForeignKeyColumn PledgeColumn { get; private set; }
        ///<summary>Gets the schema's MensSeats column.</summary>
        public static ValueColumn MensSeatsColumn { get; private set; }
        ///<summary>Gets the schema's WomensSeats column.</summary>
        public static ValueColumn WomensSeatsColumn { get; private set; }
        ///<summary>Gets the schema's BoysSeats column.</summary>
        public static ValueColumn BoysSeatsColumn { get; private set; }
        ///<summary>Gets the schema's GirlsSeats column.</summary>
        public static ValueColumn GirlsSeatsColumn { get; private set; }
        ///<summary>Gets the schema's Notes column.</summary>
        public static ValueColumn NotesColumn { get; private set; }
        
        ///<summary>Gets the SeatingReservations schema instance.</summary>
        public static new TypedSchema<SeatingReservation> Schema { get; private set; }
        ///<summary>Gets the SchemaMapping that maps this schema to the SQL Server SeatingReservations table.</summary>
        public static SchemaMapping SchemaMapping { get; private set; }
        
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        static SeatingReservation() {
            #region Create Schema
            Schema = new TypedSchema<SeatingReservation>("SeatingReservations");
            
            Schema.PrimaryKey = IdColumn = Schema.Columns.AddValueColumn("Id", typeof(Guid), null);
            IdColumn.Unique = true;
            IdColumn.AllowNulls = false;
            
            PledgeColumn = Schema.Columns.AddForeignKey("Pledge", ShomreiTorah.Data.Pledge.Schema, "SeatingReservations");
            PledgeColumn.Unique = true;
            PledgeColumn.AllowNulls = false;
            
            MensSeatsColumn = Schema.Columns.AddValueColumn("MensSeats", typeof(Int32), 0);
            MensSeatsColumn.AllowNulls = false;
            
            WomensSeatsColumn = Schema.Columns.AddValueColumn("WomensSeats", typeof(Int32), 0);
            WomensSeatsColumn.AllowNulls = false;
            
            BoysSeatsColumn = Schema.Columns.AddValueColumn("BoysSeats", typeof(Int32), 0);
            BoysSeatsColumn.AllowNulls = false;
            
            GirlsSeatsColumn = Schema.Columns.AddValueColumn("GirlsSeats", typeof(Int32), 0);
            GirlsSeatsColumn.AllowNulls = false;
            
            NotesColumn = Schema.Columns.AddValueColumn("Notes", typeof(String), "");
            NotesColumn.AllowNulls = false;
            #endregion
            
            #region Create SchemaMapping
            SchemaMapping = new SchemaMapping(Schema, false);
            SchemaMapping.SqlName = "SeatingReservations";
            SchemaMapping.SqlSchemaName = "Seating";
            
            SchemaMapping.Columns.AddMapping(IdColumn, "Id");
            SchemaMapping.Columns.AddMapping(PledgeColumn, "PledgeId");
            SchemaMapping.Columns.AddMapping(MensSeatsColumn, "MensSeats");
            SchemaMapping.Columns.AddMapping(WomensSeatsColumn, "WomensSeats");
            SchemaMapping.Columns.AddMapping(BoysSeatsColumn, "BoysSeats");
            SchemaMapping.Columns.AddMapping(GirlsSeatsColumn, "GirlsSeats");
            SchemaMapping.Columns.AddMapping(NotesColumn, "Notes");
            #endregion
            SchemaMapping.SetPrimaryMapping(SchemaMapping);
        }
        
        ///<summary>Gets the typed table that contains this row, if any.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public new TypedTable<SeatingReservation> Table { get { return (TypedTable<SeatingReservation>)base.Table; } }
        #region Value Properties
        ///<summary>Gets or sets the row's unique ID.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Guid Id {
            get { return base.Field<Guid>(IdColumn); }
            set { base[IdColumn] = value; }
        }
        ///<summary>Gets or sets the pledge associated with this reservation.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Pledge Pledge {
            get { return base.Field<Pledge>(PledgeColumn); }
            set { base[PledgeColumn] = value; }
        }
        ///<summary>Gets or sets the mens seats of the seating reservation.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Int32 MensSeats {
            get { return base.Field<Int32>(MensSeatsColumn); }
            set { base[MensSeatsColumn] = value; }
        }
        ///<summary>Gets or sets the womens seats of the seating reservation.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Int32 WomensSeats {
            get { return base.Field<Int32>(WomensSeatsColumn); }
            set { base[WomensSeatsColumn] = value; }
        }
        ///<summary>Gets or sets the boys seats of the seating reservation.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Int32 BoysSeats {
            get { return base.Field<Int32>(BoysSeatsColumn); }
            set { base[BoysSeatsColumn] = value; }
        }
        ///<summary>Gets or sets the girls seats of the seating reservation.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Int32 GirlsSeats {
            get { return base.Field<Int32>(GirlsSeatsColumn); }
            set { base[GirlsSeatsColumn] = value; }
        }
        ///<summary>Gets or sets the notes of the seating reservation.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Notes {
            get { return base.Field<String>(NotesColumn); }
            set { base[NotesColumn] = value; }
        }
        #endregion
        
        #region Partial Methods
        partial void OnColumnChanged(Column column, object oldValue, object newValue);
        
        partial void ValidateId(Guid newValue, Action<string> error);
        partial void OnIdChanged(Guid? oldValue, Guid? newValue);
        
        partial void ValidatePledge(Pledge newValue, Action<string> error);
        partial void OnPledgeChanged(Pledge oldValue, Pledge newValue);
        
        partial void ValidateMensSeats(Int32 newValue, Action<string> error);
        partial void OnMensSeatsChanged(Int32? oldValue, Int32? newValue);
        
        partial void ValidateWomensSeats(Int32 newValue, Action<string> error);
        partial void OnWomensSeatsChanged(Int32? oldValue, Int32? newValue);
        
        partial void ValidateBoysSeats(Int32 newValue, Action<string> error);
        partial void OnBoysSeatsChanged(Int32? oldValue, Int32? newValue);
        
        partial void ValidateGirlsSeats(Int32 newValue, Action<string> error);
        partial void OnGirlsSeatsChanged(Int32? oldValue, Int32? newValue);
        
        partial void ValidateNotes(String newValue, Action<string> error);
        partial void OnNotesChanged(String oldValue, String newValue);
        #endregion
        
        #region Column Callbacks
        ///<summary>Checks whether a value would be valid for a given column in an attached row.</summary>
        ///<param name="column">The column containing the value.</param>
        ///<param name="newValue">The value to validate.</param>
        ///<returns>An error message, or null if the value is valid.</returns>
        ///<remarks>This method is overridden by typed rows to perform custom validation logic.</remarks>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public override string ValidateValue(Column column, object newValue) {
            string error = base.ValidateValue(column, newValue);
            if (!String.IsNullOrEmpty(error)) return error;
            Action<string> reporter = s => error = s;
            
            if (column == IdColumn) {
                ValidateId((Guid)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == PledgeColumn) {
                ValidatePledge((Pledge)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == MensSeatsColumn) {
                ValidateMensSeats((Int32)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == WomensSeatsColumn) {
                ValidateWomensSeats((Int32)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == BoysSeatsColumn) {
                ValidateBoysSeats((Int32)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == GirlsSeatsColumn) {
                ValidateGirlsSeats((Int32)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == NotesColumn) {
                ValidateNotes((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            }
            return null;
        }
        ///<summary>Processes an explicit change of a column value.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        protected override void OnValueChanged(Column column, object oldValue, object newValue) {
            base.OnValueChanged(column, oldValue, newValue);
            OnColumnChanged(column, oldValue, newValue);
            if (column == IdColumn)
            	OnIdChanged((Guid?)oldValue, (Guid?)newValue);
            else if (column == PledgeColumn)
            	OnPledgeChanged((Pledge)oldValue, (Pledge)newValue);
            else if (column == MensSeatsColumn)
            	OnMensSeatsChanged((Int32?)oldValue, (Int32?)newValue);
            else if (column == WomensSeatsColumn)
            	OnWomensSeatsChanged((Int32?)oldValue, (Int32?)newValue);
            else if (column == BoysSeatsColumn)
            	OnBoysSeatsChanged((Int32?)oldValue, (Int32?)newValue);
            else if (column == GirlsSeatsColumn)
            	OnGirlsSeatsChanged((Int32?)oldValue, (Int32?)newValue);
            else if (column == NotesColumn)
            	OnNotesChanged((String)oldValue, (String)newValue);
        }
        #endregion
    }
    
    ///<summary>Describes a seating reservation for the Melave Malka.</summary>
    public partial class MelaveMalkaSeat : Row {
        ///<summary>Creates a new MelaveMalkaSeat instance.</summary>
        public MelaveMalkaSeat () : base(Schema) { Initialize(); }
        partial void Initialize();
        
        ///<summary>Creates a strongly-typed SeatReservations table.</summary>
        public static TypedTable<MelaveMalkaSeat> CreateTable() { return new TypedTable<MelaveMalkaSeat>(Schema, () => new MelaveMalkaSeat()); }
        
        ///<summary>Gets the schema's RowId column.</summary>
        public static ValueColumn RowIdColumn { get; private set; }
        ///<summary>Gets the schema's Person column.</summary>
        public static ForeignKeyColumn PersonColumn { get; private set; }
        ///<summary>Gets the schema's DateAdded column.</summary>
        public static ValueColumn DateAddedColumn { get; private set; }
        ///<summary>Gets the schema's Year column.</summary>
        public static ValueColumn YearColumn { get; private set; }
        ///<summary>Gets the schema's MensSeats column.</summary>
        public static ValueColumn MensSeatsColumn { get; private set; }
        ///<summary>Gets the schema's WomensSeats column.</summary>
        public static ValueColumn WomensSeatsColumn { get; private set; }
        
        ///<summary>Gets the SeatReservations schema instance.</summary>
        public static new TypedSchema<MelaveMalkaSeat> Schema { get; private set; }
        ///<summary>Gets the SchemaMapping that maps this schema to the SQL Server SeatReservations table.</summary>
        public static SchemaMapping SchemaMapping { get; private set; }
        
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        static MelaveMalkaSeat() {
            #region Create Schema
            Schema = new TypedSchema<MelaveMalkaSeat>("SeatReservations");
            
            Schema.PrimaryKey = RowIdColumn = Schema.Columns.AddValueColumn("RowId", typeof(Guid), null);
            RowIdColumn.Unique = true;
            RowIdColumn.AllowNulls = false;
            
            PersonColumn = Schema.Columns.AddForeignKey("Person", ShomreiTorah.Data.Person.Schema, "SeatReservations");
            PersonColumn.AllowNulls = false;
            
            DateAddedColumn = Schema.Columns.AddValueColumn("DateAdded", typeof(DateTime), null);
            DateAddedColumn.AllowNulls = false;
            
            YearColumn = Schema.Columns.AddValueColumn("Year", typeof(Int32), null);
            YearColumn.AllowNulls = false;
            
            MensSeatsColumn = Schema.Columns.AddValueColumn("MensSeats", typeof(Int32), 0);
            MensSeatsColumn.AllowNulls = true;
            
            WomensSeatsColumn = Schema.Columns.AddValueColumn("WomensSeats", typeof(Int32), 0);
            WomensSeatsColumn.AllowNulls = true;
            #endregion
            
            #region Create SchemaMapping
            SchemaMapping = new SchemaMapping(Schema, false);
            SchemaMapping.SqlName = "SeatReservations";
            SchemaMapping.SqlSchemaName = "MelaveMalka";
            
            SchemaMapping.Columns.AddMapping(RowIdColumn, "RowId");
            SchemaMapping.Columns.AddMapping(PersonColumn, "PersonId");
            SchemaMapping.Columns.AddMapping(DateAddedColumn, "DateAdded");
            SchemaMapping.Columns.AddMapping(YearColumn, "Year");
            SchemaMapping.Columns.AddMapping(MensSeatsColumn, "MensSeats");
            SchemaMapping.Columns.AddMapping(WomensSeatsColumn, "WomensSeats");
            #endregion
            SchemaMapping.SetPrimaryMapping(SchemaMapping);
        }
        
        ///<summary>Gets the typed table that contains this row, if any.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public new TypedTable<MelaveMalkaSeat> Table { get { return (TypedTable<MelaveMalkaSeat>)base.Table; } }
        #region Value Properties
        ///<summary>Gets or sets the row id of the seat reservation.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Guid RowId {
            get { return base.Field<Guid>(RowIdColumn); }
            set { base[RowIdColumn] = value; }
        }
        ///<summary>Gets or sets the person that reserved the seat.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Person Person {
            get { return base.Field<Person>(PersonColumn); }
            set { base[PersonColumn] = value; }
        }
        ///<summary>Gets or sets the date added of the seat reservation.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public DateTime DateAdded {
            get { return base.Field<DateTime>(DateAddedColumn); }
            set { base[DateAddedColumn] = value; }
        }
        ///<summary>Gets or sets the year of the seat reservation.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Int32 Year {
            get { return base.Field<Int32>(YearColumn); }
            set { base[YearColumn] = value; }
        }
        ///<summary>Gets or sets the mens seats of the seat reservation.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Int32? MensSeats {
            get { return base.Field<Int32?>(MensSeatsColumn); }
            set { base[MensSeatsColumn] = value; }
        }
        ///<summary>Gets or sets the womens seats of the seat reservation.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Int32? WomensSeats {
            get { return base.Field<Int32?>(WomensSeatsColumn); }
            set { base[WomensSeatsColumn] = value; }
        }
        #endregion
        
        #region Partial Methods
        partial void OnColumnChanged(Column column, object oldValue, object newValue);
        
        partial void ValidateRowId(Guid newValue, Action<string> error);
        partial void OnRowIdChanged(Guid? oldValue, Guid? newValue);
        
        partial void ValidatePerson(Person newValue, Action<string> error);
        partial void OnPersonChanged(Person oldValue, Person newValue);
        
        partial void ValidateDateAdded(DateTime newValue, Action<string> error);
        partial void OnDateAddedChanged(DateTime? oldValue, DateTime? newValue);
        
        partial void ValidateYear(Int32 newValue, Action<string> error);
        partial void OnYearChanged(Int32? oldValue, Int32? newValue);
        
        partial void ValidateMensSeats(Int32? newValue, Action<string> error);
        partial void OnMensSeatsChanged(Int32? oldValue, Int32? newValue);
        
        partial void ValidateWomensSeats(Int32? newValue, Action<string> error);
        partial void OnWomensSeatsChanged(Int32? oldValue, Int32? newValue);
        #endregion
        
        #region Column Callbacks
        ///<summary>Checks whether a value would be valid for a given column in an attached row.</summary>
        ///<param name="column">The column containing the value.</param>
        ///<param name="newValue">The value to validate.</param>
        ///<returns>An error message, or null if the value is valid.</returns>
        ///<remarks>This method is overridden by typed rows to perform custom validation logic.</remarks>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public override string ValidateValue(Column column, object newValue) {
            string error = base.ValidateValue(column, newValue);
            if (!String.IsNullOrEmpty(error)) return error;
            Action<string> reporter = s => error = s;
            
            if (column == RowIdColumn) {
                ValidateRowId((Guid)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == PersonColumn) {
                ValidatePerson((Person)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == DateAddedColumn) {
                ValidateDateAdded((DateTime)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == YearColumn) {
                ValidateYear((Int32)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == MensSeatsColumn) {
                ValidateMensSeats((Int32?)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == WomensSeatsColumn) {
                ValidateWomensSeats((Int32?)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            }
            return null;
        }
        ///<summary>Processes an explicit change of a column value.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        protected override void OnValueChanged(Column column, object oldValue, object newValue) {
            base.OnValueChanged(column, oldValue, newValue);
            OnColumnChanged(column, oldValue, newValue);
            if (column == RowIdColumn)
            	OnRowIdChanged((Guid?)oldValue, (Guid?)newValue);
            else if (column == PersonColumn)
            	OnPersonChanged((Person)oldValue, (Person)newValue);
            else if (column == DateAddedColumn)
            	OnDateAddedChanged((DateTime?)oldValue, (DateTime?)newValue);
            else if (column == YearColumn)
            	OnYearChanged((Int32?)oldValue, (Int32?)newValue);
            else if (column == MensSeatsColumn)
            	OnMensSeatsChanged((Int32?)oldValue, (Int32?)newValue);
            else if (column == WomensSeatsColumn)
            	OnWomensSeatsChanged((Int32?)oldValue, (Int32?)newValue);
        }
        #endregion
    }
    
    ///<summary>Describes a statement sent to a person.</summary>
    public partial class LoggedStatement : Row {
        ///<summary>Creates a new LoggedStatement instance.</summary>
        public LoggedStatement () : base(Schema) { Initialize(); }
        partial void Initialize();
        
        ///<summary>Creates a strongly-typed StatementLog table.</summary>
        public static TypedTable<LoggedStatement> CreateTable() { return new TypedTable<LoggedStatement>(Schema, () => new LoggedStatement()); }
        
        ///<summary>Gets the schema's Id column.</summary>
        public static ValueColumn IdColumn { get; private set; }
        ///<summary>Gets the schema's Person column.</summary>
        public static ForeignKeyColumn PersonColumn { get; private set; }
        ///<summary>Gets the schema's DateGenerated column.</summary>
        public static ValueColumn DateGeneratedColumn { get; private set; }
        ///<summary>Gets the schema's Media column.</summary>
        public static ValueColumn MediaColumn { get; private set; }
        ///<summary>Gets the schema's StatementKind column.</summary>
        public static ValueColumn StatementKindColumn { get; private set; }
        ///<summary>Gets the schema's StartDate column.</summary>
        public static ValueColumn StartDateColumn { get; private set; }
        ///<summary>Gets the schema's EndDate column.</summary>
        public static ValueColumn EndDateColumn { get; private set; }
        ///<summary>Gets the schema's UserName column.</summary>
        public static ValueColumn UserNameColumn { get; private set; }
        
        ///<summary>Gets the StatementLog schema instance.</summary>
        public static new TypedSchema<LoggedStatement> Schema { get; private set; }
        ///<summary>Gets the SchemaMapping that maps this schema to the SQL Server StatementLog table.</summary>
        public static SchemaMapping SchemaMapping { get; private set; }
        
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        static LoggedStatement() {
            #region Create Schema
            Schema = new TypedSchema<LoggedStatement>("StatementLog");
            
            Schema.PrimaryKey = IdColumn = Schema.Columns.AddValueColumn("Id", typeof(Guid), null);
            IdColumn.Unique = true;
            IdColumn.AllowNulls = false;
            
            PersonColumn = Schema.Columns.AddForeignKey("Person", ShomreiTorah.Data.Person.Schema, "LoggedStatements");
            PersonColumn.AllowNulls = false;
            
            DateGeneratedColumn = Schema.Columns.AddValueColumn("DateGenerated", typeof(DateTime), null);
            DateGeneratedColumn.AllowNulls = false;
            
            MediaColumn = Schema.Columns.AddValueColumn("Media", typeof(String), null);
            MediaColumn.AllowNulls = false;
            
            StatementKindColumn = Schema.Columns.AddValueColumn("StatementKind", typeof(String), null);
            StatementKindColumn.AllowNulls = false;
            
            StartDateColumn = Schema.Columns.AddValueColumn("StartDate", typeof(DateTime), null);
            StartDateColumn.AllowNulls = false;
            
            EndDateColumn = Schema.Columns.AddValueColumn("EndDate", typeof(DateTime), null);
            EndDateColumn.AllowNulls = false;
            
            UserNameColumn = Schema.Columns.AddValueColumn("UserName", typeof(String), null);
            UserNameColumn.AllowNulls = false;
            #endregion
            
            #region Create SchemaMapping
            SchemaMapping = new SchemaMapping(Schema, false);
            SchemaMapping.SqlName = "StatementLog";
            SchemaMapping.SqlSchemaName = "Billing";
            
            SchemaMapping.Columns.AddMapping(IdColumn, "Id");
            SchemaMapping.Columns.AddMapping(PersonColumn, "PersonId");
            SchemaMapping.Columns.AddMapping(DateGeneratedColumn, "DateGenerated");
            SchemaMapping.Columns.AddMapping(MediaColumn, "Media");
            SchemaMapping.Columns.AddMapping(StatementKindColumn, "StatementKind");
            SchemaMapping.Columns.AddMapping(StartDateColumn, "StartDate");
            SchemaMapping.Columns.AddMapping(EndDateColumn, "EndDate");
            SchemaMapping.Columns.AddMapping(UserNameColumn, "UserName");
            #endregion
            SchemaMapping.SetPrimaryMapping(SchemaMapping);
        }
        
        ///<summary>Gets the typed table that contains this row, if any.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public new TypedTable<LoggedStatement> Table { get { return (TypedTable<LoggedStatement>)base.Table; } }
        #region Value Properties
        ///<summary>Gets or sets the row's unique ID.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Guid Id {
            get { return base.Field<Guid>(IdColumn); }
            set { base[IdColumn] = value; }
        }
        ///<summary>Gets or sets the person who received the statement.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Person Person {
            get { return base.Field<Person>(PersonColumn); }
            set { base[PersonColumn] = value; }
        }
        ///<summary>Gets or sets the date generated of the statement log.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public DateTime DateGenerated {
            get { return base.Field<DateTime>(DateGeneratedColumn); }
            set { base[DateGeneratedColumn] = value; }
        }
        ///<summary>Gets or sets the media of the statement log.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Media {
            get { return base.Field<String>(MediaColumn); }
            set { base[MediaColumn] = value; }
        }
        ///<summary>Gets or sets the statement kind of the statement log.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String StatementKind {
            get { return base.Field<String>(StatementKindColumn); }
            set { base[StatementKindColumn] = value; }
        }
        ///<summary>Gets or sets the start date of the statement log.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public DateTime StartDate {
            get { return base.Field<DateTime>(StartDateColumn); }
            set { base[StartDateColumn] = value; }
        }
        ///<summary>Gets or sets the end date of the statement log.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public DateTime EndDate {
            get { return base.Field<DateTime>(EndDateColumn); }
            set { base[EndDateColumn] = value; }
        }
        ///<summary>Gets or sets the user name of the statement log.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String UserName {
            get { return base.Field<String>(UserNameColumn); }
            set { base[UserNameColumn] = value; }
        }
        #endregion
        
        #region Partial Methods
        partial void OnColumnChanged(Column column, object oldValue, object newValue);
        
        partial void ValidateId(Guid newValue, Action<string> error);
        partial void OnIdChanged(Guid? oldValue, Guid? newValue);
        
        partial void ValidatePerson(Person newValue, Action<string> error);
        partial void OnPersonChanged(Person oldValue, Person newValue);
        
        partial void ValidateDateGenerated(DateTime newValue, Action<string> error);
        partial void OnDateGeneratedChanged(DateTime? oldValue, DateTime? newValue);
        
        partial void ValidateMedia(String newValue, Action<string> error);
        partial void OnMediaChanged(String oldValue, String newValue);
        
        partial void ValidateStatementKind(String newValue, Action<string> error);
        partial void OnStatementKindChanged(String oldValue, String newValue);
        
        partial void ValidateStartDate(DateTime newValue, Action<string> error);
        partial void OnStartDateChanged(DateTime? oldValue, DateTime? newValue);
        
        partial void ValidateEndDate(DateTime newValue, Action<string> error);
        partial void OnEndDateChanged(DateTime? oldValue, DateTime? newValue);
        
        partial void ValidateUserName(String newValue, Action<string> error);
        partial void OnUserNameChanged(String oldValue, String newValue);
        #endregion
        
        #region Column Callbacks
        ///<summary>Checks whether a value would be valid for a given column in an attached row.</summary>
        ///<param name="column">The column containing the value.</param>
        ///<param name="newValue">The value to validate.</param>
        ///<returns>An error message, or null if the value is valid.</returns>
        ///<remarks>This method is overridden by typed rows to perform custom validation logic.</remarks>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public override string ValidateValue(Column column, object newValue) {
            string error = base.ValidateValue(column, newValue);
            if (!String.IsNullOrEmpty(error)) return error;
            Action<string> reporter = s => error = s;
            
            if (column == IdColumn) {
                ValidateId((Guid)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == PersonColumn) {
                ValidatePerson((Person)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == DateGeneratedColumn) {
                ValidateDateGenerated((DateTime)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == MediaColumn) {
                ValidateMedia((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == StatementKindColumn) {
                ValidateStatementKind((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == StartDateColumn) {
                ValidateStartDate((DateTime)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == EndDateColumn) {
                ValidateEndDate((DateTime)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == UserNameColumn) {
                ValidateUserName((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            }
            return null;
        }
        ///<summary>Processes an explicit change of a column value.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        protected override void OnValueChanged(Column column, object oldValue, object newValue) {
            base.OnValueChanged(column, oldValue, newValue);
            OnColumnChanged(column, oldValue, newValue);
            if (column == IdColumn)
            	OnIdChanged((Guid?)oldValue, (Guid?)newValue);
            else if (column == PersonColumn)
            	OnPersonChanged((Person)oldValue, (Person)newValue);
            else if (column == DateGeneratedColumn)
            	OnDateGeneratedChanged((DateTime?)oldValue, (DateTime?)newValue);
            else if (column == MediaColumn)
            	OnMediaChanged((String)oldValue, (String)newValue);
            else if (column == StatementKindColumn)
            	OnStatementKindChanged((String)oldValue, (String)newValue);
            else if (column == StartDateColumn)
            	OnStartDateChanged((DateTime?)oldValue, (DateTime?)newValue);
            else if (column == EndDateColumn)
            	OnEndDateChanged((DateTime?)oldValue, (DateTime?)newValue);
            else if (column == UserNameColumn)
            	OnUserNameChanged((String)oldValue, (String)newValue);
        }
        #endregion
    }
}
