using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Linq;
using ShomreiTorah.Singularity;
using ShomreiTorah.Singularity.Sql;

namespace ShomreiTorah.Data {
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
        
        #region Value Properties
        ///<summary>Gets or sets the deposit id of the deposit.</summary>
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
        #endregion
        
        #region ChildRows Properties
        ///<summary>Gets the payments in the deposit.</summary>
        public IChildRowCollection<Payment> Payments { get { return TypedChildRows<Payment>(Payment.DepositColumn); } }
        #endregion
        
        #region Partial Methods
        partial void ValidateDepositId(Guid newValue, Action<string> error);
        partial void OnDepositIdChanged(Guid oldValue, Guid newValue);
        
        partial void ValidateDate(DateTime newValue, Action<string> error);
        partial void OnDateChanged(DateTime oldValue, DateTime newValue);
        
        partial void ValidateNumber(Int32 newValue, Action<string> error);
        partial void OnNumberChanged(Int32 oldValue, Int32 newValue);
        
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
            if (column == DepositIdColumn)
            	OnDepositIdChanged((Guid)oldValue, (Guid)newValue);
            else if (column == DateColumn)
            	OnDateChanged((DateTime)oldValue, (DateTime)newValue);
            else if (column == NumberColumn)
            	OnNumberChanged((Int32)oldValue, (Int32)newValue);
            else if (column == AccountColumn)
            	OnAccountChanged((String)oldValue, (String)newValue);
            
            base.OnValueChanged(column, oldValue, newValue);
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
        
        ///<summary>Gets the schema's Id column.</summary>
        public static ValueColumn IdColumn { get; private set; }
        ///<summary>Gets the schema's Name column.</summary>
        public static ValueColumn NameColumn { get; private set; }
        ///<summary>Gets the schema's Email column.</summary>
        public static ValueColumn EmailColumn { get; private set; }
        ///<summary>Gets the schema's RandomCode column.</summary>
        public static ValueColumn RandomCodeColumn { get; private set; }
        ///<summary>Gets the schema's Password column.</summary>
        public static ValueColumn PasswordColumn { get; private set; }
        ///<summary>Gets the schema's Salt column.</summary>
        public static ValueColumn SaltColumn { get; private set; }
        ///<summary>Gets the schema's Active column.</summary>
        public static ValueColumn ActiveColumn { get; private set; }
        ///<summary>Gets the schema's DateAdded column.</summary>
        public static ValueColumn DateAddedColumn { get; private set; }
        ///<summary>Gets the schema's UseHtml column.</summary>
        public static ValueColumn UseHtmlColumn { get; private set; }
        ///<summary>Gets the schema's Person column.</summary>
        public static ForeignKeyColumn PersonColumn { get; private set; }
        
        ///<summary>Gets the EmailAddresses schema instance.</summary>
        public static new TypedSchema<EmailAddress> Schema { get; private set; }
        ///<summary>Gets the SchemaMapping that maps this schema to the SQL Server tblMLMembers table.</summary>
        public static SchemaMapping SchemaMapping { get; private set; }
        
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        static EmailAddress() {
            #region Create Schema
            Schema = new TypedSchema<EmailAddress>("EmailAddresses");
            
            Schema.PrimaryKey = IdColumn = Schema.Columns.AddValueColumn("Id", typeof(Int32), null);
            IdColumn.Unique = true;
            IdColumn.AllowNulls = false;
            
            NameColumn = Schema.Columns.AddValueColumn("Name", typeof(String), null);
            NameColumn.AllowNulls = true;
            
            EmailColumn = Schema.Columns.AddValueColumn("Email", typeof(String), null);
            EmailColumn.AllowNulls = false;
            
            RandomCodeColumn = Schema.Columns.AddValueColumn("RandomCode", typeof(String), null);
            RandomCodeColumn.AllowNulls = true;
            
            PasswordColumn = Schema.Columns.AddValueColumn("Password", typeof(String), null);
            PasswordColumn.AllowNulls = true;
            
            SaltColumn = Schema.Columns.AddValueColumn("Salt", typeof(String), null);
            SaltColumn.AllowNulls = true;
            
            ActiveColumn = Schema.Columns.AddValueColumn("Active", typeof(Boolean), null);
            ActiveColumn.AllowNulls = false;
            
            DateAddedColumn = Schema.Columns.AddValueColumn("DateAdded", typeof(DateTime), null);
            DateAddedColumn.AllowNulls = false;
            
            UseHtmlColumn = Schema.Columns.AddValueColumn("UseHtml", typeof(Boolean), null);
            UseHtmlColumn.AllowNulls = false;
            
            PersonColumn = Schema.Columns.AddForeignKey("Person", ShomreiTorah.Data.Person.Schema, "EmailAddresss");
            PersonColumn.AllowNulls = true;
            #endregion
            
            #region Create SchemaMapping
            SchemaMapping = new SchemaMapping(Schema, false);
            SchemaMapping.SqlName = "tblMLMembers";
            SchemaMapping.SqlSchemaName = "dbo";
            
            SchemaMapping.Columns.AddMapping(IdColumn, "Mail_ID");
            SchemaMapping.Columns.AddMapping(NameColumn, "Name");
            SchemaMapping.Columns.AddMapping(EmailColumn, "Email");
            SchemaMapping.Columns.AddMapping(RandomCodeColumn, "ID_Code");
            SchemaMapping.Columns.AddMapping(PasswordColumn, "Password");
            SchemaMapping.Columns.AddMapping(SaltColumn, "Salt");
            SchemaMapping.Columns.AddMapping(ActiveColumn, "Active");
            SchemaMapping.Columns.AddMapping(DateAddedColumn, "Join_Date");
            SchemaMapping.Columns.AddMapping(UseHtmlColumn, "HTMLformat");
            SchemaMapping.Columns.AddMapping(PersonColumn, "PersonId");
            #endregion
            SchemaMapping.SetPrimaryMapping(SchemaMapping);
        }
        
        #region Value Properties
        ///<summary>Gets or sets the mail ID of the tbl ML member.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Int32 Id {
            get { return base.Field<Int32>(IdColumn); }
            set { base[IdColumn] = value; }
        }
        ///<summary>Gets or sets the name of the tbl ML member.</summary>
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
        ///<summary>Gets or sets the password of the tbl ML member.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Password {
            get { return base.Field<String>(PasswordColumn); }
            set { base[PasswordColumn] = value; }
        }
        ///<summary>Gets or sets the salt of the tbl ML member.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Salt {
            get { return base.Field<String>(SaltColumn); }
            set { base[SaltColumn] = value; }
        }
        ///<summary>Gets or sets the active of the tbl ML member.</summary>
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
        public Row Person {
            get { return base.Field<Row>(PersonColumn); }
            set { base[PersonColumn] = value; }
        }
        #endregion
        
        #region Partial Methods
        partial void ValidateId(Int32 newValue, Action<string> error);
        partial void OnIdChanged(Int32 oldValue, Int32 newValue);
        
        partial void ValidateName(String newValue, Action<string> error);
        partial void OnNameChanged(String oldValue, String newValue);
        
        partial void ValidateEmail(String newValue, Action<string> error);
        partial void OnEmailChanged(String oldValue, String newValue);
        
        partial void ValidateRandomCode(String newValue, Action<string> error);
        partial void OnRandomCodeChanged(String oldValue, String newValue);
        
        partial void ValidatePassword(String newValue, Action<string> error);
        partial void OnPasswordChanged(String oldValue, String newValue);
        
        partial void ValidateSalt(String newValue, Action<string> error);
        partial void OnSaltChanged(String oldValue, String newValue);
        
        partial void ValidateActive(Boolean newValue, Action<string> error);
        partial void OnActiveChanged(Boolean oldValue, Boolean newValue);
        
        partial void ValidateDateAdded(DateTime newValue, Action<string> error);
        partial void OnDateAddedChanged(DateTime oldValue, DateTime newValue);
        
        partial void ValidateUseHtml(Boolean newValue, Action<string> error);
        partial void OnUseHtmlChanged(Boolean oldValue, Boolean newValue);
        
        partial void ValidatePerson(Row newValue, Action<string> error);
        partial void OnPersonChanged(Row oldValue, Row newValue);
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
                ValidateId((Int32)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == NameColumn) {
                ValidateName((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == EmailColumn) {
                ValidateEmail((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == RandomCodeColumn) {
                ValidateRandomCode((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == PasswordColumn) {
                ValidatePassword((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == SaltColumn) {
                ValidateSalt((String)newValue, reporter);
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
                ValidatePerson((Row)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            }
            return null;
        }
        ///<summary>Processes an explicit change of a column value.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        protected override void OnValueChanged(Column column, object oldValue, object newValue) {
            if (column == IdColumn)
            	OnIdChanged((Int32)oldValue, (Int32)newValue);
            else if (column == NameColumn)
            	OnNameChanged((String)oldValue, (String)newValue);
            else if (column == EmailColumn)
            	OnEmailChanged((String)oldValue, (String)newValue);
            else if (column == RandomCodeColumn)
            	OnRandomCodeChanged((String)oldValue, (String)newValue);
            else if (column == PasswordColumn)
            	OnPasswordChanged((String)oldValue, (String)newValue);
            else if (column == SaltColumn)
            	OnSaltChanged((String)oldValue, (String)newValue);
            else if (column == ActiveColumn)
            	OnActiveChanged((Boolean)oldValue, (Boolean)newValue);
            else if (column == DateAddedColumn)
            	OnDateAddedChanged((DateTime)oldValue, (DateTime)newValue);
            else if (column == UseHtmlColumn)
            	OnUseHtmlChanged((Boolean)oldValue, (Boolean)newValue);
            else if (column == PersonColumn)
            	OnPersonChanged((Row)oldValue, (Row)newValue);
            
            base.OnValueChanged(column, oldValue, newValue);
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
            #endregion
            SchemaMapping.SetPrimaryMapping(SchemaMapping);
        }
        
        #region Value Properties
        ///<summary>Gets or sets the id of the master directory.</summary>
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
        #endregion
        
        #region Partial Methods
        partial void ValidateId(Guid newValue, Action<string> error);
        partial void OnIdChanged(Guid oldValue, Guid newValue);
        
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
            }
            return null;
        }
        ///<summary>Processes an explicit change of a column value.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        protected override void OnValueChanged(Column column, object oldValue, object newValue) {
            if (column == IdColumn)
            	OnIdChanged((Guid)oldValue, (Guid)newValue);
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
            
            base.OnValueChanged(column, oldValue, newValue);
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
        ///<summary>Gets the schema's Comments column.</summary>
        public static ValueColumn CommentsColumn { get; private set; }
        ///<summary>Gets the schema's Modified column.</summary>
        public static ValueColumn ModifiedColumn { get; private set; }
        ///<summary>Gets the schema's Modifier column.</summary>
        public static ValueColumn ModifierColumn { get; private set; }
        ///<summary>Gets the schema's Deposit column.</summary>
        public static ForeignKeyColumn DepositColumn { get; private set; }
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
            
            CommentsColumn = Schema.Columns.AddValueColumn("Comments", typeof(String), null);
            CommentsColumn.AllowNulls = true;
            
            ModifiedColumn = Schema.Columns.AddValueColumn("Modified", typeof(DateTime), null);
            ModifiedColumn.AllowNulls = false;
            
            ModifierColumn = Schema.Columns.AddValueColumn("Modifier", typeof(String), null);
            ModifierColumn.AllowNulls = false;
            
            DepositColumn = Schema.Columns.AddForeignKey("Deposit", ShomreiTorah.Data.Deposit.Schema, "Payments");
            DepositColumn.AllowNulls = true;
            
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
            SchemaMapping.Columns.AddMapping(CommentsColumn, "Comments");
            SchemaMapping.Columns.AddMapping(ModifiedColumn, "Modified");
            SchemaMapping.Columns.AddMapping(ModifierColumn, "Modifier");
            SchemaMapping.Columns.AddMapping(DepositColumn, "DepositId");
            SchemaMapping.Columns.AddMapping(ExternalSourceColumn, "ExternalSource");
            SchemaMapping.Columns.AddMapping(ExternalIdColumn, "ExternalID");
            #endregion
            SchemaMapping.SetPrimaryMapping(SchemaMapping);
        }
        
        #region Value Properties
        ///<summary>Gets or sets the payment id of the payment.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Guid PaymentId {
            get { return base.Field<Guid>(PaymentIdColumn); }
            set { base[PaymentIdColumn] = value; }
        }
        ///<summary>Gets or sets the person who made the payment.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Row Person {
            get { return base.Field<Row>(PersonColumn); }
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
        ///<summary>Gets or sets the deposit of the payment.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Row Deposit {
            get { return base.Field<Row>(DepositColumn); }
            set { base[DepositColumn] = value; }
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
        
        #region Partial Methods
        partial void ValidatePaymentId(Guid newValue, Action<string> error);
        partial void OnPaymentIdChanged(Guid oldValue, Guid newValue);
        
        partial void ValidatePerson(Row newValue, Action<string> error);
        partial void OnPersonChanged(Row oldValue, Row newValue);
        
        partial void ValidateDate(DateTime newValue, Action<string> error);
        partial void OnDateChanged(DateTime oldValue, DateTime newValue);
        
        partial void ValidateMethod(String newValue, Action<string> error);
        partial void OnMethodChanged(String oldValue, String newValue);
        
        partial void ValidateCheckNumber(String newValue, Action<string> error);
        partial void OnCheckNumberChanged(String oldValue, String newValue);
        
        partial void ValidateAccount(String newValue, Action<string> error);
        partial void OnAccountChanged(String oldValue, String newValue);
        
        partial void ValidateAmount(Decimal newValue, Action<string> error);
        partial void OnAmountChanged(Decimal oldValue, Decimal newValue);
        
        partial void ValidateComments(String newValue, Action<string> error);
        partial void OnCommentsChanged(String oldValue, String newValue);
        
        partial void ValidateModified(DateTime newValue, Action<string> error);
        partial void OnModifiedChanged(DateTime oldValue, DateTime newValue);
        
        partial void ValidateModifier(String newValue, Action<string> error);
        partial void OnModifierChanged(String oldValue, String newValue);
        
        partial void ValidateDeposit(Row newValue, Action<string> error);
        partial void OnDepositChanged(Row oldValue, Row newValue);
        
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
                ValidatePerson((Row)newValue, reporter);
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
            } else if (column == CommentsColumn) {
                ValidateComments((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == ModifiedColumn) {
                ValidateModified((DateTime)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == ModifierColumn) {
                ValidateModifier((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == DepositColumn) {
                ValidateDeposit((Row)newValue, reporter);
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
            if (column == PaymentIdColumn)
            	OnPaymentIdChanged((Guid)oldValue, (Guid)newValue);
            else if (column == PersonColumn)
            	OnPersonChanged((Row)oldValue, (Row)newValue);
            else if (column == DateColumn)
            	OnDateChanged((DateTime)oldValue, (DateTime)newValue);
            else if (column == MethodColumn)
            	OnMethodChanged((String)oldValue, (String)newValue);
            else if (column == CheckNumberColumn)
            	OnCheckNumberChanged((String)oldValue, (String)newValue);
            else if (column == AccountColumn)
            	OnAccountChanged((String)oldValue, (String)newValue);
            else if (column == AmountColumn)
            	OnAmountChanged((Decimal)oldValue, (Decimal)newValue);
            else if (column == CommentsColumn)
            	OnCommentsChanged((String)oldValue, (String)newValue);
            else if (column == ModifiedColumn)
            	OnModifiedChanged((DateTime)oldValue, (DateTime)newValue);
            else if (column == ModifierColumn)
            	OnModifierChanged((String)oldValue, (String)newValue);
            else if (column == DepositColumn)
            	OnDepositChanged((Row)oldValue, (Row)newValue);
            else if (column == ExternalSourceColumn)
            	OnExternalSourceChanged((String)oldValue, (String)newValue);
            else if (column == ExternalIdColumn)
            	OnExternalIdChanged((Int32?)oldValue, (Int32?)newValue);
            
            base.OnValueChanged(column, oldValue, newValue);
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
        ///<summary>Gets the schema's Subtype column.</summary>
        public static ValueColumn SubtypeColumn { get; private set; }
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
            
            SubtypeColumn = Schema.Columns.AddValueColumn("Subtype", typeof(String), null);
            SubtypeColumn.AllowNulls = false;
            
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
            SchemaMapping.Columns.AddMapping(SubtypeColumn, "SubType");
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
        
        #region Value Properties
        ///<summary>Gets or sets the pledge id of the pledge.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Guid PledgeId {
            get { return base.Field<Guid>(PledgeIdColumn); }
            set { base[PledgeIdColumn] = value; }
        }
        ///<summary>Gets or sets the person who made the pledge.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Row Person {
            get { return base.Field<Row>(PersonColumn); }
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
        ///<summary>Gets or sets the subtype of the pledge.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Subtype {
            get { return base.Field<String>(SubtypeColumn); }
            set { base[SubtypeColumn] = value; }
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
        #endregion
        
        #region Partial Methods
        partial void ValidatePledgeId(Guid newValue, Action<string> error);
        partial void OnPledgeIdChanged(Guid oldValue, Guid newValue);
        
        partial void ValidatePerson(Row newValue, Action<string> error);
        partial void OnPersonChanged(Row oldValue, Row newValue);
        
        partial void ValidateDate(DateTime newValue, Action<string> error);
        partial void OnDateChanged(DateTime oldValue, DateTime newValue);
        
        partial void ValidateType(String newValue, Action<string> error);
        partial void OnTypeChanged(String oldValue, String newValue);
        
        partial void ValidateSubtype(String newValue, Action<string> error);
        partial void OnSubtypeChanged(String oldValue, String newValue);
        
        partial void ValidateAccount(String newValue, Action<string> error);
        partial void OnAccountChanged(String oldValue, String newValue);
        
        partial void ValidateAmount(Decimal newValue, Action<string> error);
        partial void OnAmountChanged(Decimal oldValue, Decimal newValue);
        
        partial void ValidateNote(String newValue, Action<string> error);
        partial void OnNoteChanged(String oldValue, String newValue);
        
        partial void ValidateComments(String newValue, Action<string> error);
        partial void OnCommentsChanged(String oldValue, String newValue);
        
        partial void ValidateModified(DateTime newValue, Action<string> error);
        partial void OnModifiedChanged(DateTime oldValue, DateTime newValue);
        
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
                ValidatePerson((Row)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == DateColumn) {
                ValidateDate((DateTime)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == TypeColumn) {
                ValidateType((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            } else if (column == SubtypeColumn) {
                ValidateSubtype((String)newValue, reporter);
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
            if (column == PledgeIdColumn)
            	OnPledgeIdChanged((Guid)oldValue, (Guid)newValue);
            else if (column == PersonColumn)
            	OnPersonChanged((Row)oldValue, (Row)newValue);
            else if (column == DateColumn)
            	OnDateChanged((DateTime)oldValue, (DateTime)newValue);
            else if (column == TypeColumn)
            	OnTypeChanged((String)oldValue, (String)newValue);
            else if (column == SubtypeColumn)
            	OnSubtypeChanged((String)oldValue, (String)newValue);
            else if (column == AccountColumn)
            	OnAccountChanged((String)oldValue, (String)newValue);
            else if (column == AmountColumn)
            	OnAmountChanged((Decimal)oldValue, (Decimal)newValue);
            else if (column == NoteColumn)
            	OnNoteChanged((String)oldValue, (String)newValue);
            else if (column == CommentsColumn)
            	OnCommentsChanged((String)oldValue, (String)newValue);
            else if (column == ModifiedColumn)
            	OnModifiedChanged((DateTime)oldValue, (DateTime)newValue);
            else if (column == ModifierColumn)
            	OnModifierChanged((String)oldValue, (String)newValue);
            else if (column == ExternalSourceColumn)
            	OnExternalSourceChanged((String)oldValue, (String)newValue);
            else if (column == ExternalIdColumn)
            	OnExternalIdChanged((Int32?)oldValue, (Int32?)newValue);
            
            base.OnValueChanged(column, oldValue, newValue);
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
        ///<summary>Gets the schema's Status column.</summary>
        public static ValueColumn StatusColumn { get; private set; }
        
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
            
            MensSeatsColumn = Schema.Columns.AddValueColumn("MensSeats", typeof(Int32), null);
            MensSeatsColumn.AllowNulls = false;
            
            WomensSeatsColumn = Schema.Columns.AddValueColumn("WomensSeats", typeof(Int32), null);
            WomensSeatsColumn.AllowNulls = false;
            
            BoysSeatsColumn = Schema.Columns.AddValueColumn("BoysSeats", typeof(Int32), null);
            BoysSeatsColumn.AllowNulls = false;
            
            GirlsSeatsColumn = Schema.Columns.AddValueColumn("GirlsSeats", typeof(Int32), null);
            GirlsSeatsColumn.AllowNulls = false;
            
            NotesColumn = Schema.Columns.AddValueColumn("Notes", typeof(String), null);
            NotesColumn.AllowNulls = false;
            
            StatusColumn = Schema.Columns.AddValueColumn("Status", typeof(String), null);
            StatusColumn.AllowNulls = false;
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
            SchemaMapping.Columns.AddMapping(StatusColumn, "Status");
            #endregion
            SchemaMapping.SetPrimaryMapping(SchemaMapping);
        }
        
        #region Value Properties
        ///<summary>Gets or sets the id of the seating reservation.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Guid Id {
            get { return base.Field<Guid>(IdColumn); }
            set { base[IdColumn] = value; }
        }
        ///<summary>Gets or sets the pledge associated with this reservation.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Row Pledge {
            get { return base.Field<Row>(PledgeColumn); }
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
        ///<summary>Gets or sets the status of the seating reservation.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public String Status {
            get { return base.Field<String>(StatusColumn); }
            set { base[StatusColumn] = value; }
        }
        #endregion
        
        #region Partial Methods
        partial void ValidateId(Guid newValue, Action<string> error);
        partial void OnIdChanged(Guid oldValue, Guid newValue);
        
        partial void ValidatePledge(Row newValue, Action<string> error);
        partial void OnPledgeChanged(Row oldValue, Row newValue);
        
        partial void ValidateMensSeats(Int32 newValue, Action<string> error);
        partial void OnMensSeatsChanged(Int32 oldValue, Int32 newValue);
        
        partial void ValidateWomensSeats(Int32 newValue, Action<string> error);
        partial void OnWomensSeatsChanged(Int32 oldValue, Int32 newValue);
        
        partial void ValidateBoysSeats(Int32 newValue, Action<string> error);
        partial void OnBoysSeatsChanged(Int32 oldValue, Int32 newValue);
        
        partial void ValidateGirlsSeats(Int32 newValue, Action<string> error);
        partial void OnGirlsSeatsChanged(Int32 oldValue, Int32 newValue);
        
        partial void ValidateNotes(String newValue, Action<string> error);
        partial void OnNotesChanged(String oldValue, String newValue);
        
        partial void ValidateStatus(String newValue, Action<string> error);
        partial void OnStatusChanged(String oldValue, String newValue);
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
                ValidatePledge((Row)newValue, reporter);
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
            } else if (column == StatusColumn) {
                ValidateStatus((String)newValue, reporter);
                if (!String.IsNullOrEmpty(error)) return error;
            }
            return null;
        }
        ///<summary>Processes an explicit change of a column value.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        protected override void OnValueChanged(Column column, object oldValue, object newValue) {
            if (column == IdColumn)
            	OnIdChanged((Guid)oldValue, (Guid)newValue);
            else if (column == PledgeColumn)
            	OnPledgeChanged((Row)oldValue, (Row)newValue);
            else if (column == MensSeatsColumn)
            	OnMensSeatsChanged((Int32)oldValue, (Int32)newValue);
            else if (column == WomensSeatsColumn)
            	OnWomensSeatsChanged((Int32)oldValue, (Int32)newValue);
            else if (column == BoysSeatsColumn)
            	OnBoysSeatsChanged((Int32)oldValue, (Int32)newValue);
            else if (column == GirlsSeatsColumn)
            	OnGirlsSeatsChanged((Int32)oldValue, (Int32)newValue);
            else if (column == NotesColumn)
            	OnNotesChanged((String)oldValue, (String)newValue);
            else if (column == StatusColumn)
            	OnStatusChanged((String)oldValue, (String)newValue);
            
            base.OnValueChanged(column, oldValue, newValue);
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
        
        #region Value Properties
        ///<summary>Gets or sets the id of the statement log.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Guid Id {
            get { return base.Field<Guid>(IdColumn); }
            set { base[IdColumn] = value; }
        }
        ///<summary>Gets or sets the person who received the statement.</summary>
        [DebuggerNonUserCode]
        [GeneratedCode("ShomreiTorah.Singularity.Designer", "1.0")]
        public Row Person {
            get { return base.Field<Row>(PersonColumn); }
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
        partial void ValidateId(Guid newValue, Action<string> error);
        partial void OnIdChanged(Guid oldValue, Guid newValue);
        
        partial void ValidatePerson(Row newValue, Action<string> error);
        partial void OnPersonChanged(Row oldValue, Row newValue);
        
        partial void ValidateDateGenerated(DateTime newValue, Action<string> error);
        partial void OnDateGeneratedChanged(DateTime oldValue, DateTime newValue);
        
        partial void ValidateMedia(String newValue, Action<string> error);
        partial void OnMediaChanged(String oldValue, String newValue);
        
        partial void ValidateStatementKind(String newValue, Action<string> error);
        partial void OnStatementKindChanged(String oldValue, String newValue);
        
        partial void ValidateStartDate(DateTime newValue, Action<string> error);
        partial void OnStartDateChanged(DateTime oldValue, DateTime newValue);
        
        partial void ValidateEndDate(DateTime newValue, Action<string> error);
        partial void OnEndDateChanged(DateTime oldValue, DateTime newValue);
        
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
                ValidatePerson((Row)newValue, reporter);
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
            if (column == IdColumn)
            	OnIdChanged((Guid)oldValue, (Guid)newValue);
            else if (column == PersonColumn)
            	OnPersonChanged((Row)oldValue, (Row)newValue);
            else if (column == DateGeneratedColumn)
            	OnDateGeneratedChanged((DateTime)oldValue, (DateTime)newValue);
            else if (column == MediaColumn)
            	OnMediaChanged((String)oldValue, (String)newValue);
            else if (column == StatementKindColumn)
            	OnStatementKindChanged((String)oldValue, (String)newValue);
            else if (column == StartDateColumn)
            	OnStartDateChanged((DateTime)oldValue, (DateTime)newValue);
            else if (column == EndDateColumn)
            	OnEndDateChanged((DateTime)oldValue, (DateTime)newValue);
            else if (column == UserNameColumn)
            	OnUserNameChanged((String)oldValue, (String)newValue);
            
            base.OnValueChanged(column, oldValue, newValue);
        }
        #endregion
    }
}
