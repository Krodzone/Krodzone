/*
 * The Krodzone assembly provides a number of classes for making database object creation
 * easier, and for making database connections easier. It also classes for handling binary 
 * and XML serialization.
 * 
 * Copyright (C) 2005  Krodzone Technologies, LLC.
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Configuration;
using Krodzone;

namespace Krodzone.Attributes
{

    /// <summary>
    /// Base interface used for dynamically creating SQL Server tables
    /// </summary>
    public interface ISqlTableAttribute
    {

        #region Properties
        /// <summary>
        /// Gets the name of the schema the table belongs to
        /// </summary>
        string Schema { get; }
        /// <summary>
        /// Gets the database table name
        /// </summary>
        string Tablename { get; }
        /// <summary>
        /// Get the setting name for retrieving the connection string to use in creating the table
        /// </summary>
        string ConnectionStringSetting { get; }
        /// <summary>
        /// Get the connection string to use when creating the table. Throws a NullReferenceException if the connection string isn't found in the config file.
        /// </summary>
        string ConnectionString { get; }
        /// <summary>
        /// Gets a list of ISqlTableColumnAttribute to use when creating the table
        /// </summary>
        List<ISqlTableColumnAttribute> Columns { get; }
        /// <summary>
        /// Gets a Dictionary containing lists of AbstractSqlIndexAttribute to be used by an instance of SqlTableCommandString when creating the table indexes
        /// </summary>
        Dictionary<string, List<AbstractSqlIndexAttribute>> TableIndexes { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Returns [schema].[tablename]
        /// </summary>
        /// <returns></returns>
        string GetFormattedTableName();
        #endregion

    }

    /// <summary>
    /// Base interface used for dynamically creating columns for SQL Server tables. This should be used with Attribute SqlTableAttribute
    /// </summary>
    public interface ISqlTableColumnAttribute
    {

        #region Properties
        /// <summary>
        /// Gets the name of the table column
        /// </summary>
        string ColumnName { get; }
        /// <summary>
        /// Gets the column data type
        /// </summary>
        System.Data.SqlDbType DataType { get; }
        /// <summary>
        /// Gets a value indicating if null values are allowed in the column
        /// </summary>
        bool AllowNulls { get; }
        /// <summary>
        /// Gets or Sets the value used to identify columns that should have the identity specifier
        /// </summary>
        bool Identity { get; set; }
        /// <summary>
        /// Gets or Sets the starting point for columns that have the identity specifier. Defaults to 1.
        /// </summary>
        int Seed { get; set; }
        /// <summary>
        /// Gets or Sets the increment value for columns that have the identity specifier. Defaults to 1.
        /// </summary>
        int Increment { get; set; }
        /// <summary>
        /// Gets or Sets the length value for string and binary types, and the precision value for float and date/time types. Use -1 to signify that the maximum value should be used for each.
        /// </summary>
        int Length { get; set; }
        /// <summary>
        /// Gets or Sets the precision value for decimal and numeric data types
        /// </summary>
        int Precision { get; set; }
        /// <summary>
        /// Gets or Sets the scale value for decimal and numeric data types
        /// </summary>
        int Scale { get; set; }
        /// <summary>
        /// Gets or Sets PrimaryKey value used when creating the tables clustered index
        /// </summary>
        bool PrimaryKey { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Returns the column name as it should appear during table creation
        /// </summary>
        /// <returns>System.String</returns>
        string GetFormattedColumnName();
        /// <summary>
        /// Returns a parameter string as it should appear in a stored procedure
        /// </summary>
        /// <returns>System.String</returns>
        string GetStoredProcedureParameter();
        #endregion

    }

    /// <summary>
    /// Used for dynamically creating indexes for SQL Server tables. This should be used with Attributes SqlTableAttribute and SqlTableColumnAttribute.
    /// </summary>
    public interface ISqlIndexAttribute
    {

        #region Properties
        /// <summary>
        /// Gets the name of the table index
        /// </summary>
        string IndexName { get; }
        /// <summary>
        /// Gets the name of the table column
        /// </summary>
        string ColumnName { get; }
        /// <summary>
        /// Gets the order the column should appear in the index
        /// </summary>
        int OrderOfPrecedence { get; }
        /// <summary>
        /// Gets the sort order that should be applied to the column index
        /// </summary>
        SQL.SqlSortOrderArgs SortOrder { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Returns the column name as it should appear in the index
        /// </summary>
        /// <returns></returns>
        string GetFormattedIndex();
        #endregion

    }

    /// <summary>
    /// Used for dynamically creating ISqlTableAttribute objects from classes that declare the SqlTableAttribute, and thos that do not.
    /// </summary>
    public interface ISqlTableAttributeFactory
    {

        #region Methods
        /// <summary>
        /// Returns a SqlTableAttribute object using the object passed in the constructor
        /// </summary>
        /// <returns></returns>
        ISqlTableAttribute GetTableAttribute();
        #endregion

    }

    /// <summary>
    /// Used for dynamically creating SQL Server tables
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SqlTableAttribute : Attribute, ISqlTableAttribute
    {

        #region Local Variables
        /// <summary>
        /// Readonly member instance used for returning the schema
        /// </summary>
        protected readonly string _Schema;
        /// <summary>
        /// Readonly member instance used for returning the table name
        /// </summary>
        protected readonly string _Tablename;
        /// <summary>
        /// Readonly member instance used for returning the connection string config setting name
        /// </summary>
        protected readonly string _ConnectionStringSetting;

        private List<ISqlTableColumnAttribute> _Columns;
        private Dictionary<string, List<AbstractSqlIndexAttribute>> _TableIndexes;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of the SqlTableAttribute class
        /// </summary>
        /// <param name="schema">The name of the schema the table should belong to</param>
        /// <param name="tablename">The name of the database table</param>
        /// <param name="connStringSetting">The setting name for retrieving the connection string to use in creating the table</param>
        public SqlTableAttribute(string schema, string tablename, string connStringSetting)
        {
            this._Schema = schema;
            this._Tablename = tablename;
            this._ConnectionStringSetting = connStringSetting;
            this._Columns = new List<ISqlTableColumnAttribute>();
            this._TableIndexes = new Dictionary<string, List<AbstractSqlIndexAttribute>>();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the name of the schema the table belongs to
        /// </summary>
        public string Schema
        {
            get { return this._Schema; }
        }

        /// <summary>
        /// Gets the database table name
        /// </summary>
        public string Tablename
        {
            get { return this._Tablename; }
        }

        /// <summary>
        /// Get the setting name for retrieving the connection string to use in creating the table
        /// </summary>
        public string ConnectionStringSetting
        {
            get { return this._ConnectionStringSetting; }
        }

        /// <summary>
        /// Get the connection string to use when creating the table.  Throws a NullReferenceException if the connection string isn't found in the config file.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                if (ConfigurationManager.ConnectionStrings[this.ConnectionStringSetting] != null)
                {
                    return ConfigurationManager.ConnectionStrings[this.ConnectionStringSetting].ConnectionString;
                }
                else
                {
                    throw new InvalidOperationException(string.Format("A connection string named {0} could not be found!", this.ConnectionStringSetting));
                }
            }
        }

        /// <summary>
        /// Gets a list of ISqlTableColumnAttribute to use when creating the table
        /// </summary>
        public List<ISqlTableColumnAttribute> Columns
        {
            get { return this._Columns; }
        }

        /// <summary>
        /// Gets a HashSet containing lists of AbstractSqlIndexAttribute to be used by an instance of SqlTableCommandString when creating the table indexes
        /// </summary>
        public Dictionary<string, List<AbstractSqlIndexAttribute>> TableIndexes
        {
            get { return this._TableIndexes; }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns [schema].[tablename]
        /// </summary>
        /// <returns></returns>
        public string GetFormattedTableName()
        {
            return string.Format("{0}.{1}", this.Schema, this.Tablename);
        }

        /// <summary>
        /// Returns a string that represents the current object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} ({1}.{2})", this.GetType().Name, this.Schema, this.Tablename);
        }
        #endregion

    }

    /// <summary>
    /// Used for dynamically creating columns for SQL Server tables. This should be used with Attribute SqlTableAttribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SqlTableColumnAttribute : Attribute, ISqlTableColumnAttribute
    {

        #region Local Variables
        /// <summary>
        /// Read only member instance used for returning the value of property ColumnName
        /// </summary>
        protected readonly string _ColumnName;
        /// <summary>
        /// Read only member instance used for returning the value of property DataType
        /// </summary>
        protected readonly SqlDbType _DataType;
        /// <summary>
        /// Read only member instance used for returning the value of property AllowNulls
        /// </summary>
        protected readonly bool _AllowNulls;

        private bool _Identity;
        private int _Seed;
        private int _Increment;
        private int _Length;
        private int _Precision;
        private int _Scale;
        private bool _PrimaryKey;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of the SqlTableColumnAttribute class
        /// </summary>
        /// <param name="columnName">The name of the table column</param>
        /// <param name="dataType">The column data type</param>
        /// <param name="allowNulls">Indicates if nulls should be allowed</param>
        public SqlTableColumnAttribute(string columnName, SqlDbType dataType, bool allowNulls)
        {
            this._ColumnName = columnName;
            this._DataType = dataType;
            this._AllowNulls = allowNulls;
            this._Identity = false;
            this._Seed = 1;
            this._Increment = 1;
            this._Length = -1;
            this._Precision = 18;
            this._Scale = 0;
            this._PrimaryKey = false;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the name of the table column
        /// </summary>
        public string ColumnName
        {
            get { return this._ColumnName; }
        }

        /// <summary>
        /// Gets the column data type
        /// </summary>
        public SqlDbType DataType
        {
            get { return this._DataType; }
        }

        /// <summary>
        /// Gets a value indicating if null values are allowed in the column
        /// </summary>
        public bool AllowNulls
        {
            get { return this._AllowNulls; }
        }

        /// <summary>
        /// Gets or Sets the value used to identify columns that should have the identity specifier
        /// </summary>
        public bool Identity
        {
            get { return this._Identity; }
            set { this._Identity = value; }
        }

        /// <summary>
        /// Gets or Sets the starting point for columns that have the identity specifier. Defaults to 1.
        /// </summary>
        public int Seed
        {
            get { return this._Seed; }
            set { this._Seed = value; }
        }

        /// <summary>
        /// Gets or Sets the increment value for columns that have the identity specifier. Defaults to 1.
        /// </summary>
        public int Increment
        {
            get { return this._Increment; }
            set { this._Increment = value; }
        }
        /// <summary>
        /// Gets or Sets the length value for string and binary types, and the precision value for float and date/time types. Use -1 to signify that the maximum value should be used for each.
        /// </summary>
        public int Length
        {
            get { return this._Length; }
            set { this._Length = value; }
        }

        /// <summary>
        /// Gets or Sets the precision value for decimal and numeric data types
        /// </summary>
        public int Precision
        {
            get { return this._Precision; }
            set { this._Precision = value; }
        }

        /// <summary>
        /// Gets or Sets the scale value for decimal and numeric data types
        /// </summary>
        public int Scale
        {
            get { return this._Scale; }
            set { this._Scale = value; }
        }

        /// <summary>
        /// Gets or Sets PrimaryKey value used when creating the tables clustered index
        /// </summary>
        public bool PrimaryKey
        {
            get { return this._PrimaryKey; }
            set { this._PrimaryKey = value; }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the column name as it should appear during table creation
        /// </summary>
        /// <returns>System.String</returns>
        public string GetFormattedColumnName()
        {
            string formattedColumn = string.Format("{0} {1}{2}", this.ColumnName, this.ColumnDataTypeName(), (this.DataType == SqlDbType.Int && this.Identity == true ? string.Format(" identity({0},{1})", this.Seed, this.Increment) : ""));
            return string.Format("{0}{1}", formattedColumn, (this.AllowNulls == false ? " not null" : ""));
        }

        /// <summary>
        /// Returns a parameter string as it should appear in a stored procedure
        /// </summary>
        /// <returns>System.String</returns>
        public string GetStoredProcedureParameter()
        {
            return string.Format("@{0} {1}", this.ColumnName, this.ColumnDataTypeName());
        }

        /// <summary>
        /// Returns a string that represents the current object
        /// </summary>
        /// <returns>System.String</returns>
        public override string ToString()
        {
            return string.Format("{0} ({1}{2})", this.GetType().Name, this.GetFormattedColumnName(), (this.PrimaryKey == true ? ": Primary Key Column" : ""));
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Formats the data type
        /// </summary>
        /// <returns>System.String</returns>
        protected string ColumnDataTypeName()
        {
            string name = "";

            switch (this._DataType)
            {

                //Binary data types
                case SqlDbType.Binary:
                    // The maximum length of a binary data type is 8,000
                    name = (this.Length <= 0 || this.Length > 8000 ? "binary(8000)" : string.Format("binary({0})", this.Length));
                    break;
                case SqlDbType.VarBinary:
                    // The maximum specified length of a varbinary data type is 8,000. Anything over should use max.
                    name = (this.Length <= 0 || this.Length > 8000 ? "varbinary(max)" : string.Format("varbinary({0})", this.Length));
                    break;

                //Boolean data types
                case SqlDbType.Bit:
                    name = "bit";
                    break;

                //Date & Time data types
                case SqlDbType.Date:
                    name = "date";
                    break;
                case SqlDbType.DateTime:
                    name = "datetime";
                    break;
                case SqlDbType.DateTime2:
                    // The maximum precision length for data type datetime2 is 7
                    name = (this.Length < 0 || this.Length >= 7 ? "datetime2(7)" : this.Length == 0 ? "datetime2(0)" : string.Format("datetime2({0})", this.Length));
                    break;
                case SqlDbType.DateTimeOffset:
                    // The maximum precision length for data type datetimeoffset is 7
                    name = (this.Length < 0 || this.Length >= 7 ? "datetimeoffset(7)" : this.Length == 0 ? "datetimeoffset(0)" : string.Format("datetimeoffset({0})", this.Length));
                    break;
                case SqlDbType.SmallDateTime:
                    name = "smalldatetime";
                    break;
                case SqlDbType.Time:
                    // The maximum precision length for data type time is 7
                    name = (this.Length < 0 || this.Length >= 7 ? "time(7)" : this.Length == 0 ? "time(0)" : string.Format("time({0})", this.Length));
                    break;

                //Misc data types
                case SqlDbType.UniqueIdentifier:
                    name = "uniqueidentifier";
                    break;
                case SqlDbType.Variant:
                    name = "sql_variant";
                    break;

                //Numeric data types
                case SqlDbType.BigInt:
                    name = "bigint";
                    break;
                case SqlDbType.Decimal:
                    name = string.Format("decimal({0},{1})", this.Precision, this.Scale);
                    break;
                case SqlDbType.Float:
                    // The maximum precision length for data type float is 53
                    name = (this.Length < 0 || this.Length >= 53 ? "float(53)" : this.Length == 0 ? "float" : string.Format("float({0})", this.Length));
                    break;
                case SqlDbType.Int:
                    name = "int";
                    break;
                case SqlDbType.Money:
                    name = "money";
                    break;
                case SqlDbType.Real:
                    name = "real";
                    break;
                case SqlDbType.SmallInt:
                    name = "smallint";
                    break;
                case SqlDbType.SmallMoney:
                    name = "smallmoney";
                    break;
                case SqlDbType.TinyInt:
                    name = "tinyint";
                    break;

                //String data types
                case SqlDbType.Char:
                    // The maximum length of data type char is 8,000
                    name = (this.Length <= 0 || this.Length > 8000 ? "char(8000)" : string.Format("char({0})", this.Length));
                    break;
                case SqlDbType.VarChar:
                    // The maximum specified length of data type varchar is 8,000. Anything over should use max
                    name = (this.Length <= 0 || this.Length > 8000 ? "varchar(max)" : string.Format("varchar({0})", this.Length));
                    break;
                case SqlDbType.NChar:
                    // The maximum length of data type nchar is 4,000
                    name = (this.Length <= 0 || this.Length > 4000 ? "nchar(4000)" : string.Format("nchar({0})", this.Length));
                    break;
                case SqlDbType.NVarChar:
                    // The maximum specified length of data type nvarchar is 4,000. Anything over should use max
                    name = (this.Length <= 0 || this.Length > 4000 ? "nvarchar(max)" : string.Format("nvarchar({0})", this.Length));
                    break;

                //Large string data types
                case SqlDbType.NText:
                    name = "ntext";
                    break;
                case SqlDbType.Text:
                    name = "text";
                    break;
                case SqlDbType.Image:
                    name = "image";
                    break;

                default:
                    name = "varchar(max)";
                    break;
            }

            return name;

        }
        #endregion

    }

    /// <summary>
    /// Used for dynamically creating indexes for SQL Server tables. This should be used with Attributes SqlTableAttribute and SqlTableColumnAttribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class AbstractSqlIndexAttribute : Attribute, ISqlIndexAttribute
    {

        #region Local Variables
        /// <summary>
        /// Read only member instance used for returning the value of property IndexName
        /// </summary>
        protected readonly string _IndexName;
        /// <summary>
        /// Read only member instance used for returning the value of property ColumnName
        /// </summary>
        protected readonly string _ColumnName;
        /// <summary>
        /// Read only member instance used for returning the value of property OrderOfPrecedence
        /// </summary>
        protected readonly int _OrderOfPrecedence;
        /// <summary>
        /// Read only member instance used for returning the value of property SortOrder
        /// </summary>
        protected readonly SQL.SqlSortOrderArgs _SortOrder;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the class inheriting AbstractSqlIndexAttribute class
        /// </summary>
        /// <param name="indexName">The name of the table index</param>
        /// <param name="columnName">The name of the table column</param>
        /// <param name="orderOfPrecedence">The order the column should appear in the index</param>
        /// <param name="sortOrder">The sort order that should be applied to the column index</param>
        public AbstractSqlIndexAttribute(string indexName, string columnName, int orderOfPrecedence, SQL.SqlSortOrderArgs sortOrder)
        {
            if (string.IsNullOrEmpty(indexName))
            {
                throw new ArgumentOutOfRangeException("The index name cannot be empty, and cannot contain spaces or special characters.");
            }

            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentOutOfRangeException("The column name cannot be empty, and cannot contain spaces or special characters.");
            }

            if (orderOfPrecedence <= 0)
            {
                throw new ArgumentOutOfRangeException("Order of Precedence must be a value greater than zero");
            }

            if ((sortOrder != SQL.SqlSortOrderArgs.Ascending) && (sortOrder != SQL.SqlSortOrderArgs.Descending))
            {
                throw new ArgumentOutOfRangeException("Invalid value for sort order");
            }

            this._IndexName = indexName;
            this._ColumnName = columnName;
            this._OrderOfPrecedence = orderOfPrecedence;
            this._SortOrder = sortOrder;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the name of the table index
        /// </summary>
        public string IndexName
        {
            get { return this._IndexName; }
        }

        /// <summary>
        /// Gets the name of the table column
        /// </summary>
        public string ColumnName
        {
            get { return this._ColumnName; }
        }

        /// <summary>
        /// Gets the order the column should appear in the index
        /// </summary>
        public int OrderOfPrecedence
        {
            get { return this._OrderOfPrecedence; }
        }

        /// <summary>
        /// Gets the sort order that should be applied to the column index
        /// </summary>
        public SQL.SqlSortOrderArgs SortOrder
        {
            get { return this._SortOrder; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns the column name as it should appear in the index
        /// </summary>
        /// <returns></returns>
        public string GetFormattedIndex()
        {
            return string.Format("{0} {1}", this.ColumnName, (this.SortOrder == SQL.SqlSortOrderArgs.Ascending ? "ASC" : "DESC"));
        }
        #endregion

    }

    public class CustomIndexOneAttribute : AbstractSqlIndexAttribute
    {
        
        #region Constructor
        /// <summary>
        /// Creates a new instance of the class inheriting AbstractSqlIndexAttribute class
        /// </summary>
        /// <param name="indexName">The name of the table index</param>
        /// <param name="columnName">The name of the table column</param>
        /// <param name="orderOfPrecedence">The order the column should appear in the index</param>
        /// <param name="sortOrder">The sort order that should be applied to the column index</param>
        public CustomIndexOneAttribute(string indexName, string columnName, int orderOfPrecedence, SQL.SqlSortOrderArgs sortOrder)
            : base(indexName, columnName, orderOfPrecedence, sortOrder)
        {

        }
        #endregion

    }

    public class CustomIndexTwoAttribute : AbstractSqlIndexAttribute
    {

        #region Constructor
        /// <summary>
        /// Creates a new instance of the class inheriting AbstractSqlIndexAttribute class
        /// </summary>
        /// <param name="indexName">The name of the table index</param>
        /// <param name="columnName">The name of the table column</param>
        /// <param name="orderOfPrecedence">The order the column should appear in the index</param>
        /// <param name="sortOrder">The sort order that should be applied to the column index</param>
        public CustomIndexTwoAttribute(string indexName, string columnName, int orderOfPrecedence, SQL.SqlSortOrderArgs sortOrder)
            : base(indexName, columnName, orderOfPrecedence, sortOrder)
        {

        }
        #endregion

    }

    public class CustomIndexThreeAttribute : AbstractSqlIndexAttribute
    {

        #region Constructor
        /// <summary>
        /// Creates a new instance of the class inheriting AbstractSqlIndexAttribute class
        /// </summary>
        /// <param name="indexName">The name of the table index</param>
        /// <param name="columnName">The name of the table column</param>
        /// <param name="orderOfPrecedence">The order the column should appear in the index</param>
        /// <param name="sortOrder">The sort order that should be applied to the column index</param>
        public CustomIndexThreeAttribute(string indexName, string columnName, int orderOfPrecedence, SQL.SqlSortOrderArgs sortOrder)
            : base(indexName, columnName, orderOfPrecedence, sortOrder)
        {

        }
        #endregion

    }

    public class CustomIndexFourAttribute : AbstractSqlIndexAttribute
    {

        #region Constructor
        /// <summary>
        /// Creates a new instance of the class inheriting AbstractSqlIndexAttribute class
        /// </summary>
        /// <param name="indexName">The name of the table index</param>
        /// <param name="columnName">The name of the table column</param>
        /// <param name="orderOfPrecedence">The order the column should appear in the index</param>
        /// <param name="sortOrder">The sort order that should be applied to the column index</param>
        public CustomIndexFourAttribute(string indexName, string columnName, int orderOfPrecedence, SQL.SqlSortOrderArgs sortOrder)
            : base(indexName, columnName, orderOfPrecedence, sortOrder)
        {

        }
        #endregion

    }

    public class CustomIndexFiveAttribute : AbstractSqlIndexAttribute
    {

        #region Constructor
        /// <summary>
        /// Creates a new instance of the class inheriting AbstractSqlIndexAttribute class
        /// </summary>
        /// <param name="indexName">The name of the table index</param>
        /// <param name="columnName">The name of the table column</param>
        /// <param name="orderOfPrecedence">The order the column should appear in the index</param>
        /// <param name="sortOrder">The sort order that should be applied to the column index</param>
        public CustomIndexFiveAttribute(string indexName, string columnName, int orderOfPrecedence, SQL.SqlSortOrderArgs sortOrder)
            : base(indexName, columnName, orderOfPrecedence, sortOrder)
        {

        }
        #endregion

    }

    /// <summary>
    /// Used for extracting or creating an ISqlTableAttribute instance from the object pass in the constructor
    /// </summary>
    /// <typeparam name="T">The object type from which to extract or create the ISqlTableAttribute instance</typeparam>
    public class SqlTableAttributeFactory<T> : ISqlTableAttributeFactory
    {

        #region Local Variables
        /// <summary>
        ///  Read only member instance used for returning the value of property ClassObject
        /// </summary>
        protected readonly T _ClassObject;
        /// <summary>
        ///  Read only member instance used when creating the SQL Object(s)
        /// </summary>
        protected readonly string _ConnectionString;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of the SqlTableAttributeFactory class
        /// </summary>
        /// <param name="classObject">
        /// The object from which to create the ISqlTableAttribute instance.
        /// Use this constructor only when the class object implements the ISqlTableAttribute attribute.
        /// </param>
        public SqlTableAttributeFactory(T classObject)
        {
            this._ClassObject = classObject;
        }

        /// <summary>
        /// Creates a new instance of the SqlTableAttributeFactory class
        /// </summary>
        /// <param name="classObject">
        /// The object from which to create the ISqlTableAttribute instance.
        /// Use this constructor only when the class object does not implement the ISqlTableAttribute attribute.
        /// </param>
        /// <param name="connectionString">
        /// The connection to use when creating the SQL Object(s)
        /// </param>
        public SqlTableAttributeFactory(T classObject, string connectionString)
        {
            this._ClassObject = classObject;
            this._ConnectionString = connectionString;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the class object passed in the constructor
        /// </summary>
        public virtual T ClassObject
        {
            get { return this._ClassObject; }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Creates an ISqlTableAttribute instance from the class object passed in the constructor even when the object doesn't implement the ISqlTableAttribute attribute
        /// </summary>
        /// <returns></returns>
        public ISqlTableAttribute GetTableAttribute()
        {
            ISqlTableAttribute sqlAttr = null;

            if (this._ClassObject != null)
            {
                sqlAttr = this.ExtractSqlAttribute(this._ClassObject);

                if (sqlAttr == null)
                {
                    sqlAttr = this.CreateSqlAttribute(this._ClassObject);
                }
                else if (sqlAttr.Columns.Count == 0)
                {
                    sqlAttr.Columns.AddRange(this.CreateColumns(this._ClassObject));
                }

            }

            return sqlAttr;

        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Extracts the ISqlTableAttribute instance from the object passed in the constructor
        /// </summary>
        /// <param name="obj">The object from which to extract the attribute instance</param>
        /// <returns></returns>
        protected virtual ISqlTableAttribute ExtractSqlAttribute(T obj)
        {
            ISqlTableAttribute sqlAttr = null;

            // Get all of the attributes assigned to the class
            object[] classAttrs = obj.GetType().GetCustomAttributes(true);

            //Loop through the attributes looking for our SqlTableAttribute
            foreach (object clsAttr in classAttrs)
            {

                if (clsAttr.GetType() == typeof(SqlTableAttribute))
                {
                    // SqlTableAttribute was found.....assign it the our return variable
                    sqlAttr = (SqlTableAttribute)clsAttr;

                    // Now look for the SqlTableColumnAttribute on the class properties
                    foreach (System.Reflection.PropertyInfo pi in obj.GetType().GetProperties())
                    {

                        foreach (object propAttr in pi.GetCustomAttributes(true))
                        {
                            if (propAttr.GetType() == typeof(SqlTableColumnAttribute))
                            {
                                sqlAttr.Columns.Add((SqlTableColumnAttribute)propAttr);
                            }
                            else if (propAttr is AbstractSqlIndexAttribute)
                            {
                                string indexName = ((AbstractSqlIndexAttribute)propAttr).IndexName;

                                if (sqlAttr.TableIndexes.ContainsKey(indexName))
                                {
                                    sqlAttr.TableIndexes[indexName].Add((AbstractSqlIndexAttribute)propAttr);
                                }
                                else
                                {
                                    List<AbstractSqlIndexAttribute> items = new List<AbstractSqlIndexAttribute>();
                                    items.Add((AbstractSqlIndexAttribute)propAttr);
                                    sqlAttr.TableIndexes.Add(indexName, items);
                                }

                            }
                        }
                    }

                }

            }

            return sqlAttr;

        }

        /// <summary>
        /// Creates an ISqlTableAttribute object from the class object passed in the constructor if the class doesn't implement the attribute
        /// </summary>
        /// <param name="obj">The object from which the ISqlTableAttribute object will be created</param>
        /// <returns></returns>
        protected virtual ISqlTableAttribute CreateSqlAttribute(T obj)
        {
            ISqlTableAttribute sqlAttr = new SqlTableAttribute("dbo", obj.GetType().Name, this._ConnectionString);

            sqlAttr.Columns.AddRange(this.CreateColumns(obj));

            return sqlAttr;

        }

        /// <summary>
        /// Creates a List of ISqlTableColumnAttribute
        /// </summary>
        /// <param name="obj">The object from which to create the list</param>
        /// <returns></returns>
        protected virtual IEnumerable<ISqlTableColumnAttribute> CreateColumns(T obj)
        {
            IEnumerable<ISqlTableColumnAttribute> columns = new List<ISqlTableColumnAttribute>();
            bool primaryKey = true;

            // Now look for the SqlTableColumnAttribute on the class properties
            foreach (System.Reflection.PropertyInfo pi in obj.GetType().GetProperties())
            {
                ISqlTableColumnAttribute column = this.CreateColumn(pi, primaryKey);

                primaryKey = false;

                if (column != null)
                {
                    ((List<ISqlTableColumnAttribute>)columns).Add(column);
                }

            }

            return columns;

        }

        /// <summary>
        /// Creates an ISqlTableColumnAttribute from the PropertyInfo
        /// </summary>
        /// <param name="property">The PropertyInfo to use when creating the object</param>
        /// <param name="primaryKey">Causes the method to create the object as a Primary Key column</param>
        /// <returns></returns>
        protected virtual ISqlTableColumnAttribute CreateColumn(System.Reflection.PropertyInfo property, bool primaryKey)
        {
            ISqlTableColumnAttribute col = null;

            switch (property.PropertyType.Name)
            {
                case "System.Int64":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.BigInt, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    if (primaryKey)
                    {
                        col.Identity = true;
                        col.Seed = 1;
                        col.Increment = 1;
                    }
                    break;
                case "Int64":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.BigInt, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    if (primaryKey)
                    {
                        col.Identity = true;
                        col.Seed = 1;
                        col.Increment = 1;
                    }
                    break;
                case "System.Int32":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.Int, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    if (primaryKey)
                    {
                        col.Identity = true;
                        col.Seed = 1;
                        col.Increment = 1;
                    }
                    break;
                case "Int32":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.Int, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    if (primaryKey)
                    {
                        col.Identity = true;
                        col.Seed = 1;
                        col.Increment = 1;
                    }
                    break;
                case "System.Int16":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.SmallInt, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    if (primaryKey)
                    {
                        col.Identity = true;
                        col.Seed = 1;
                        col.Increment = 1;
                    }
                    break;
                case "System.Byte":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.TinyInt, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    if (primaryKey)
                    {
                        col.Identity = true;
                        col.Seed = 1;
                        col.Increment = 1;
                    }
                    break;
                case "Byte":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.TinyInt, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    break;
                case "System.Byte[]":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.Binary, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    break;
                case "Byte[]":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.Binary, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    break;
                case "System.Decimal":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.Decimal, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    col.Precision = 18;
                    col.Scale = 8;
                    break;
                case "Decimal":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.Decimal, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    col.Precision = 18;
                    col.Scale = 8;
                    break;
                case "System.Double":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.Float, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    break;
                case "Double":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.Float, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    break;
                case "System.Single":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.Float, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    break;
                case "Single":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.Float, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    break;
                case "System.DateTime":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.DateTime, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    col.Length = 0;
                    break;
                case "DateTime":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.DateTime, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    col.Length = 0;
                    break;
                case "System.DateTimeOffset":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.DateTimeOffset, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    col.Length = 0;
                    break;
                case "DateTimeOffset":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.DateTimeOffset, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    col.Length = 0;
                    break;
                case "System.TimeSpan":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.Time, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    col.Length = 0;
                    break;
                case "TimeSpan":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.Time, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    col.Length = 0;
                    break;
                case "System.Boolean":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.Bit, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    break;
                case "Boolean":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.Bit, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    break;
                case "System.String":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.VarChar, !primaryKey);
                    col.PrimaryKey = primaryKey;

                    if ((property.Name.ToUpper() == "COMMENTS") || (property.Name.ToUpper() == "DESCRIPTION"))
                    {
                        col.Length = -1;
                    }
                    else
                    {
                        col.Length = 200;
                    }

                    break;

                case "String":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.VarChar, !primaryKey);
                    col.PrimaryKey = primaryKey;

                    if ((property.Name.ToUpper() == "COMMENTS") || (property.Name.ToUpper() == "DESCRIPTION"))
                    {
                        col.Length = -1;
                    }
                    else
                    {
                        col.Length = 200;
                    }

                    break;

                case "System.Char[]":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.VarChar, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    col.Length = -1;
                    break;
                case "Char[]":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.VarChar, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    col.Length = -1;
                    break;
                case "System.Guid":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.UniqueIdentifier, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    break;
                case "Guid":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.UniqueIdentifier, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    break;
                case "System.Xml":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.Xml, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    break;
                case "Xml":
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.Xml, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    break;
                default:
                    col = new SqlTableColumnAttribute(property.Name, SqlDbType.VarChar, !primaryKey);
                    col.PrimaryKey = primaryKey;
                    col.Length = -1;
                    break;
            }

            return col;

        }
        #endregion

    }
}
