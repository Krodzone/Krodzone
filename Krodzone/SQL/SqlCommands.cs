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
using Krodzone.Attributes;

namespace Krodzone.SQL
{

    /// <summary>
    /// Used for specifying sort order
    /// </summary>
    public enum SqlSortOrderArgs
    {
        /// <summary>
        /// Indicates that the column should be sorted Ascending
        /// </summary>
        Ascending = 0,
        /// <summary>
        /// Indicates that the column should be sorted Descending
        /// </summary>
        Descending = 1
    }

    /// <summary>
    /// Base interface used for creating SQL commands
    /// </summary>
    public interface ISqlObjectCommand<T> : ICommand<T>, IDisposable
    {

        #region Properties
        /// <summary>
        /// Gets the connection string to use when connecting to the database
        /// </summary>
        string ConnectionString { get; set; }
        /// <summary>
        /// Gets the server used in the connection
        /// </summary>
        string Server { get; }
        /// <summary>
        /// Gets the database used in the connection
        /// </summary>
        string Database { get; }
        /// <summary>
        /// Gets the connection timeout used in the connection
        /// </summary>
        int ConnectionTimeout { get; }
        /// <summary>
        /// Gets the System.Data.SqlClient.SqlCredential used in the connection
        /// </summary>
        System.Data.SqlClient.SqlCredential Credential { get; }
        #endregion

    }

    /// <summary>
    /// Used for dynamically creating SQL Server tables
    /// </summary>
    public class SqlTableCommandString : ICommandString
    {
        
        #region Local Variables
        /// <summary>
        /// Readonly ISqlTableAttribute instance member used for returning the command script
        /// </summary>
        protected readonly ISqlTableAttribute SqlTable;

        protected readonly bool CreateAuditTable;
        protected readonly bool CreateTriggers;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the SqlTableCommandString class
        /// </summary>
        /// <param name="sqlTable">The ISqlTableAttribute used for creating the command script</param>
        public SqlTableCommandString(ISqlTableAttribute sqlTable)
        {
            SqlTable = sqlTable;
        }

        /// <summary>
        /// Creates a new instance of the SqlTableCommandString class
        /// </summary>
        /// <param name="sqlTable">The ISqlTableAttribute used for creating the command script</param>
        /// <param name="createAuditTable">When true GetCommandString generates script to create an Audit schema, as well as an audit table for capturing data changes</param>
        /// <param name="createTriggers">When true GetCommandString generates script to create triggers for saving changes into the audit table. createAuditTable must also be true.</param>
        public SqlTableCommandString(ISqlTableAttribute sqlTable, bool createAuditTable, bool createTriggers)
        {
            SqlTable = sqlTable;
            CreateAuditTable = createAuditTable;
            CreateTriggers = createTriggers;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns a string containing the creation script for the schema, table, and all table indexes
        /// </summary>
        /// <returns></returns>
        public virtual string GetCommandString()
        {
            StringBuilder sb = new StringBuilder();

            if (SqlTable != null)
            {
                sb.Append($"IF NOT EXISTS(SELECT * FROM sys.schemas WHERE name = '{SqlTable.Schema}')\r\n");
                sb.Append(
                    $"BEGIN\r\n\tEXEC('CREATE SCHEMA [{SqlTable.Schema}] AUTHORIZATION [dbo]');\r\nEND\r\nGO\r\n\r\n");

                sb.Append($"CREATE TABLE {SqlTable.GetFormattedTableName()}(");

                for (int i = 0; i < SqlTable.Columns.Count; i++)
                {
                    sb.Append($"\r\n\t{(i == 0 ? "" : ",")}\t{SqlTable.Columns[i].GetFormattedColumnName()}");
                }

                List<ISqlTableColumnAttribute> pkeys = (from pk in SqlTable.Columns where pk.PrimaryKey select pk).ToList();

                if (pkeys.Count > 0)
                {
                    sb.Append($"\r\n\t,\tCONSTRAINT [PK_{SqlTable.Tablename}] PRIMARY KEY CLUSTERED");
                    sb.Append("\r\n(");

                    for (int i = 0; i < pkeys.Count; i++)
                    {
                        sb.Append($"\r\n\t{(i == 0 ? "" : ",")}\t{pkeys[i].ColumnName}");
                    }

                    sb.Append("\r\n)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]");
                    sb.Append("\r\n) ON [PRIMARY]");

                }
                else
                {
                    sb.Append("\r\n)");
                }

                sb.Append("\r\nGO\r\n\r\nSET ANSI_PADDING OFF\r\nGO");

                foreach (var pky in SqlTable.TableIndexes)
                {
                    string indexName = pky.Key;
                    List<AbstractSqlIndexAttribute> indexCols = (from idx in pky.Value orderby idx.OrderOfPrecedence select idx).ToList();

                    sb.Append($"\r\n\r\nCREATE NONCLUSTERED INDEX [{indexName}] ON {SqlTable.GetFormattedTableName()}");
                    sb.Append("\r\n(");

                    for (int i = 0; i < indexCols.Count; i++)
                    {
                        sb.Append($"\r\n\t{(i == 0 ? "" : ",")}\t{indexCols[i].GetFormattedIndex()}");
                    }

                    sb.Append("\r\n)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]");
                    sb.Append("\r\nGO");

                }

                if (CreateAuditTable)
                {
                    ISqlTableAttribute auditTable = new SqlTableAttribute("Audit", SqlTable.Tablename, SqlTable.ConnectionStringSetting);

                    ISqlTableColumnAttribute auditDate = new SqlTableColumnAttribute("AuditDate", System.Data.SqlDbType.DateTime, false);
                    ISqlTableColumnAttribute triggerType = new SqlTableColumnAttribute("TriggerType", System.Data.SqlDbType.VarChar, false);

                    auditDate.PrimaryKey = true;
                    auditDate.Length = 0;

                    triggerType.PrimaryKey = false;
                    triggerType.Length = 1;


                    auditTable.Columns.Add(auditDate);
                    auditTable.Columns.Add(triggerType);

                    foreach (ISqlTableColumnAttribute col in SqlTable.Columns)
                    {
                        ISqlTableColumnAttribute col2 = new SqlTableColumnAttribute(col.ColumnName, col.DataType, col.AllowNulls);

                        col2.Identity = false;
                        col2.Increment = col.Increment;
                        col2.Length = col.Length;
                        col2.Precision = col.Precision;
                        col2.PrimaryKey = col.PrimaryKey;
                        col2.Scale = col.Scale;
                        col2.Seed = col.Seed;

                        auditTable.Columns.Add(col2);
                    }

                    sb.Append("\r\n\r\n\r\n");
                    sb.Append(CreateAuditTableScript(auditTable));

                    //  Add trigger creation scripts
                    sb.Append(CreateAfterInsertTrigger(auditTable));
                    sb.Append(CreateAfterUpdateTrigger(auditTable));
                    sb.Append(CreateAfterDeleteTrigger(auditTable));
                    sb.Append(CreateBeforeAuditDeleteTrigger(auditTable));

                }

            }
            else
            {
                throw new InvalidOperationException("The ISqlTableAttribute parameter passed in the constructor is invalid!");
            }

            return sb.ToString();

        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Creates an audit table used for tracking data changes
        /// </summary>
        /// <param name="auditTable">The ISqlTableAttribute object used to create the audit table</param>
        /// <returns></returns>
        protected virtual string CreateAuditTableScript(ISqlTableAttribute auditTable)
        {
            StringBuilder sb = new StringBuilder();

            if (auditTable != null)
            {
                sb.Append("IF NOT EXISTS(SELECT * FROM sys.schemas WHERE name = \'Audit\')\r\n");
                sb.Append("BEGIN\r\n\tEXEC(\'CREATE SCHEMA [Audit] AUTHORIZATION [dbo]\');\r\nEND\r\nGO\r\n\r\n");

                sb.Append($"CREATE TABLE {auditTable.GetFormattedTableName()}(");

                for (int i = 0; i < auditTable.Columns.Count; i++)
                {
                    if (auditTable.Columns[i].Identity) { auditTable.Columns[i].Identity = false; }

                    sb.Append($"\r\n\t{(i == 0 ? "" : ",")}\t{auditTable.Columns[i].GetFormattedColumnName()}");
                }

                List<ISqlTableColumnAttribute> pkeys = (from pk in auditTable.Columns where pk.PrimaryKey select pk).ToList();

                if (pkeys.Count > 0)
                {
                    sb.Append($"\r\n\t,\tCONSTRAINT [PK_Audit_{auditTable.Tablename}] PRIMARY KEY CLUSTERED");
                    sb.Append("\r\n(");

                    for (int i = 0; i < pkeys.Count; i++)
                    {
                        sb.Append($"\r\n\t{(i == 0 ? "" : ",")}\t{pkeys[i].ColumnName}");
                    }

                    sb.Append("\r\n)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]");
                    sb.Append("\r\n) ON [PRIMARY]");

                }
                else
                {
                    sb.Append("\r\n)");
                }

                sb.Append("\r\nGO\r\n\r\nSET ANSI_PADDING OFF\r\nGO");

                foreach (var pky in auditTable.TableIndexes)
                {
                    string indexName = pky.Key;
                    List<AbstractSqlIndexAttribute> indexCols = (from idx in pky.Value orderby idx.OrderOfPrecedence select idx).ToList();

                    sb.Append($"\r\n\r\nCREATE NONCLUSTERED INDEX [{indexName}] ON {auditTable.GetFormattedTableName()}");
                    sb.Append("\r\n(");

                    for (int i = 0; i < indexCols.Count; i++)
                    {
                        sb.Append($"\r\n\t{(i == 0 ? "" : ",")}\t{indexCols[i].GetFormattedIndex()}");
                    }

                    sb.Append("\r\n)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]");
                    sb.Append("\r\nGO");

                }

            }

            return sb.ToString();

        }

        /// <summary>
        /// Creates an AFTER INSERT trigger on the transaction table
        /// </summary>
        /// <param name="auditTable">The ISqlTableAttribute object used to create the AFTER INSERT trigger</param>
        /// <returns></returns>
        protected virtual string CreateAfterInsertTrigger(ISqlTableAttribute auditTable)
        {
            StringBuilder sb = new StringBuilder("");

            if ((CreateAuditTable && CreateTriggers) && (auditTable != null))
            {
                sb.Append(string.Format("\r\n\r\n\r\nCREATE TRIGGER [{0}].[{1}_AfterInsertTrigger] ON [{0}].[{1}]", SqlTable.Schema, SqlTable.Tablename));
                sb.Append("\r\nAFTER INSERT");
                sb.Append("\r\n\r\nAS");
                sb.Append("\r\nDECLARE @Date datetime = GetDate();");
                sb.Append("\r\nDECLARE @TriggerType varchar(1) = 'I';");
                sb.Append("\r\n\r\nBEGIN;");
                sb.Append("\r\n\tSET NOCOUNT ON");
                sb.Append($"\r\n\r\n\tINSERT INTO {auditTable.GetFormattedTableName()}");
                sb.Append("\r\n\tSELECT\t@Date");
                sb.Append("\r\n\t\t,\t@TriggerType");
                sb.Append("\r\n\t\t,\t*");
                sb.Append("\r\n\tFROM INSERTED;");
                sb.Append("\r\n\r\n\tSET NOCOUNT OFF");
                sb.Append("\r\nEND");
                sb.Append("\r\nGO");
            }

            return sb.ToString();

        }

        /// <summary>
        /// Creates an AFTER UPDATE trigger on the transaction table
        /// </summary>
        /// <param name="auditTable">The ISqlTableAttribute object used to create the AFTER UPDATE trigger</param>
        /// <returns></returns>
        protected virtual string CreateAfterUpdateTrigger(ISqlTableAttribute auditTable)
        {
            StringBuilder sb = new StringBuilder("");

            if ((CreateAuditTable && CreateTriggers) && (auditTable != null))
            {
                sb.Append(string.Format("\r\n\r\n\r\nCREATE TRIGGER [{0}].[{1}_AfterUpdateTrigger] ON [{0}].[{1}]", SqlTable.Schema, SqlTable.Tablename));
                sb.Append("\r\nAFTER UPDATE");
                sb.Append("\r\n\r\nAS");
                sb.Append("\r\nDECLARE @Date datetime = GetDate();");
                sb.Append("\r\nDECLARE @TriggerType varchar(1) = 'U';");
                sb.Append("\r\n\r\nBEGIN");
                sb.Append("\r\n\tSET NOCOUNT ON");
                sb.Append($"\r\n\r\n\tINSERT INTO {auditTable.GetFormattedTableName()}");
                sb.Append("\r\n\tSELECT\t@Date");
                sb.Append("\r\n\t\t,\t@TriggerType");
                sb.Append("\r\n\t\t,\t*");
                sb.Append("\r\n\tFROM DELETED;");
                sb.Append("\r\n\r\n\tSET NOCOUNT OFF");
                sb.Append("\r\nEND");
                sb.Append("\r\nGO");
            }

            return sb.ToString();

        }

        /// <summary>
        /// Creates an AFTER DELETE trigger on the transaction table
        /// </summary>
        /// <param name="auditTable">The ISqlTableAttribute object used to create the AFTER DELETE trigger</param>
        /// <returns></returns>
        protected virtual string CreateAfterDeleteTrigger(ISqlTableAttribute auditTable)
        {
            StringBuilder sb = new StringBuilder("");

            if ((CreateAuditTable && CreateTriggers) && (auditTable != null))
            {
                sb.Append(string.Format("\r\n\r\n\r\nCREATE TRIGGER [{0}].[{1}_AfterDeleteTrigger] ON [{0}].[{1}]", SqlTable.Schema, SqlTable.Tablename));
                sb.Append("\r\nAFTER DELETE");
                sb.Append("\r\n\r\nAS");
                sb.Append("\r\nDECLARE @Date datetime = GetDate();");
                sb.Append("\r\nDECLARE @TriggerType varchar(1) = 'D';");
                sb.Append("\r\n\r\nBEGIN");
                sb.Append("\r\n\tSET NOCOUNT ON");
                sb.Append($"\r\n\r\n\tINSERT INTO {auditTable.GetFormattedTableName()}");
                sb.Append("\r\n\tSELECT\t@Date");
                sb.Append("\r\n\t\t,\t@TriggerType");
                sb.Append("\r\n\t\t,\t*");
                sb.Append("\r\n\tFROM DELETED;");
                sb.Append("\r\n\r\n\tSET NOCOUNT OFF");
                sb.Append("\r\nEND");
                sb.Append("\r\nGO");
            }

            return sb.ToString();

        }

        /// <summary>
        /// Creates an INSTEAD OF DELETE trigger on the audit table to ensure nothing can accidently be deleted
        /// </summary>
        /// <param name="auditTable">The ISqlTableAttribute object used to create the INSTEAD OF DELETE trigger</param>
        /// <returns></returns>
        protected virtual string CreateBeforeAuditDeleteTrigger(ISqlTableAttribute auditTable)
        {
            StringBuilder sb = new StringBuilder("");

            if ((CreateAuditTable && CreateTriggers) && (auditTable != null))
            {
                sb.Append(string.Format("\r\n\r\n\r\nCREATE TRIGGER [{0}].[{1}_BeforeDeleteTrigger] ON [{0}].[{1}]", auditTable.Schema, auditTable.Tablename));
                sb.Append("\r\nINSTEAD OF DELETE");
                sb.Append("\r\n\r\nAS");
                sb.Append("\r\n\r\nBEGIN");
                sb.Append("\r\n\tSET NOCOUNT ON");
                sb.Append("\r\n\r\n\t/*");
                sb.Append("\r\n\t\tDO NOTHING");
                sb.Append("\r\n\r\n\t*/");
                sb.Append("\r\n\r\n\tSET NOCOUNT OFF");
                sb.Append("\r\nEND");
                sb.Append("\r\nGO\r\n\r\n\r\n");
            }

            return sb.ToString();

        }
        #endregion

    }

    /// <summary>
    /// Used for dynamically creating SQL Server Delete procedures
    /// </summary>
    public class SqlDeleteProcedureCommandString : ICommandString
    {
        
        #region Local Variables
        /// <summary>
        /// Readonly ISqlTableAttribute instance member used for returning the command script
        /// </summary>
        protected readonly ISqlTableAttribute SqlTable;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the SqlDeleteProcedureCommandString class
        /// </summary>
        /// <param name="sqlTable">The ISqlTableAttribute used for creating the command script</param>
        public SqlDeleteProcedureCommandString(ISqlTableAttribute sqlTable)
        {
            SqlTable = sqlTable;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns a string containing the creation script for the deletion procedure
        /// </summary>
        /// <returns></returns>
        public virtual string GetCommandString()
        {
            StringBuilder sb = new StringBuilder();

            if (SqlTable != null)
            {
                sb.Append(
                    $"if EXISTS(SELECT * FROM sys.sysobjects WHERE id = OBJECT_ID(N'{SqlTable.Schema}.DeleteEntry_{SqlTable.Tablename}') AND xtype IN ('P','PC'))");
                sb.Append("\r\n\tBEGIN");
                sb.Append($"\r\n\t\tDROP PROCEDURE {SqlTable.Schema}.DeleteEntry_{SqlTable.Tablename};");
                sb.Append("\r\n\tEND");
                sb.Append("\r\nGO");

                sb.Append($"\r\n\r\nCREATE PROCEDURE {SqlTable.Schema}.DeleteEntry_{SqlTable.Tablename}(");
                sb.Append("\r\n");

                List<ISqlTableColumnAttribute> pkeys = (from pk in SqlTable.Columns where pk.PrimaryKey select pk).ToList();

                if (pkeys.Count > 0)
                {

                    for (int i = 0; i < pkeys.Count; i++)
                    {
                        sb.Append($"\r\n{pkeys[i].GetStoredProcedureParameter()}{(i == pkeys.Count - 1 ? "" : ",")}");
                    }

                    sb.Append("\r\n\r\n)");
                    sb.Append("\r\n\r\nAS");
                    sb.Append("\r\n\r\nBEGIN");
                    sb.Append($"\r\n\r\n\tDELETE FROM {SqlTable.GetFormattedTableName()}");

                    for (int i = 0; i < pkeys.Count; i++)
                    {
                        sb.Append(string.Format("\r\n\t{0} {1} = @{1}", (i == 0 ? "WHERE" : "\tAND"), pkeys[i].ColumnName));
                    }

                    sb.Append("\r\nEND");
                    sb.Append("\r\nGO\r\n\r\n");

                }
                else
                {
                    throw new InvalidOperationException("A primary key must be specified in order to create a delete procedure.");
                }

            }
            else
            {
                throw new InvalidOperationException("The ISqlTableAttribute parameter passed in the constructor is invalid!");
            }

            return sb.ToString();

        }
        #endregion
        
    }

    /// <summary>
    /// Used for dynamically creating SQL Server Retrieve procedures
    /// </summary>
    public class SqlGetProcedureCommandString : ICommandString
    {
        
        #region Local Variables
        /// <summary>
        /// Readonly ISqlTableAttribute instance member used for returning the command script
        /// </summary>
        protected readonly ISqlTableAttribute SqlTable;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the SqlGetProcedureCommandString class
        /// </summary>
        /// <param name="sqlTable">The ISqlTableAttribute used for creating the command script</param>
        public SqlGetProcedureCommandString(ISqlTableAttribute sqlTable)
        {
            SqlTable = sqlTable;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns a string containing the creation script for the retrieval procedure
        /// </summary>
        /// <returns></returns>
        public virtual string GetCommandString()
        {
            StringBuilder sb = new StringBuilder();

            if (SqlTable != null)
            {
                List<ISqlTableColumnAttribute> pkeys = (from pk in SqlTable.Columns where pk.PrimaryKey select pk).ToList();

                if (pkeys.Count > 0)
                {
                    sb.Append(
                        $"if EXISTS(SELECT * FROM sys.sysobjects WHERE id = OBJECT_ID(N'{SqlTable.Schema}.GetEntry_{SqlTable.Tablename}') AND xtype IN ('P','PC'))");
                    sb.Append("\r\n\tBEGIN");
                    sb.Append($"\r\n\t\tDROP PROCEDURE {SqlTable.Schema}.GetEntry_{SqlTable.Tablename};");
                    sb.Append("\r\n\tEND");
                    sb.Append("\r\nGO");

                    sb.Append($"\r\n\r\nCREATE PROCEDURE {SqlTable.Schema}.GetEntry_{SqlTable.Tablename}(");
                    sb.Append("\r\n");


                    for (int i = 0; i < pkeys.Count; i++)
                    {
                        sb.Append($"\r\n{pkeys[i].GetStoredProcedureParameter()}{(i == pkeys.Count - 1 ? "" : ",")}");
                    }

                    sb.Append("\r\n\r\n)");
                    sb.Append("\r\n\r\nAS");
                    sb.Append("\r\n\r\nBEGIN");
                    sb.Append($"\r\n\r\n\tSELECT *\r\n\tFROM {SqlTable.GetFormattedTableName()}");

                    for (int i = 0; i < pkeys.Count; i++)
                    {
                        sb.Append(string.Format("\r\n\t{0} {1} = @{1}", (i == 0 ? "WHERE" : "\tAND"), pkeys[i].ColumnName));
                    }

                    sb.Append("\r\nEND");
                    sb.Append("\r\nGO");

                }

                sb.Append(
                    $"\r\n\r\nif EXISTS(SELECT * FROM sys.sysobjects WHERE id = OBJECT_ID(N'{SqlTable.Schema}.GetAll_{SqlTable.Tablename}') AND xtype IN ('P','PC'))");
                sb.Append("\r\n\tBEGIN");
                sb.Append($"\r\n\t\tDROP PROCEDURE {SqlTable.Schema}.GetAll_{SqlTable.Tablename};");
                sb.Append("\r\n\tEND");
                sb.Append("\r\nGO");

                sb.Append($"\r\n\r\nCREATE PROCEDURE {SqlTable.Schema}.GetAll_{SqlTable.Tablename}");
                
                sb.Append("\r\n\r\nAS");
                sb.Append("\r\n\r\nBEGIN");
                sb.Append($"\r\n\r\n\tSELECT *\r\n\tFROM {SqlTable.GetFormattedTableName()}");

                sb.Append("\r\nEND");
                sb.Append("\r\nGO\r\n\r\n");


            }
            else
            {
                throw new InvalidOperationException("The ISqlTableAttribute parameter passed in the constructor is invalid!");
            }

            return sb.ToString();

        }
        #endregion
        
    }

    /// <summary>
    /// Used for dynamically creating SQL Server Create and Update procedures
    /// </summary>
    public class SqlSaveProcedureCommandString : ICommandString
    {
        
        #region Local Variables
        /// <summary>
        /// Readonly ISqlTableAttribute instance member used for returning the command script
        /// </summary>
        protected readonly ISqlTableAttribute SqlTable;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the SqlSaveProcedureCommandString class
        /// </summary>
        /// <param name="sqlTable">The ISqlTableAttribute used for creating the command script</param>
        public SqlSaveProcedureCommandString(ISqlTableAttribute sqlTable)
        {
            SqlTable = sqlTable;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns a string containing the creation script for the save procedure
        /// </summary>
        /// <returns></returns>
        public virtual string GetCommandString()
        {
            StringBuilder sb = new StringBuilder();

            if (this.SqlTable != null)
            {
                bool hasIdentityCol = false;
                string identityColumn = string.Empty;

                sb.Append(string.Format("if EXISTS(SELECT * FROM sys.sysobjects WHERE id = OBJECT_ID(N'{0}.SaveEntry_{1}') AND xtype IN ('P','PC'))", this.SqlTable.Schema, this.SqlTable.Tablename));
                sb.Append("\r\n\tBEGIN");
                sb.Append(string.Format("\r\n\t\tDROP PROCEDURE {0}.SaveEntry_{1};", this.SqlTable.Schema, this.SqlTable.Tablename));
                sb.Append("\r\n\tEND");
                sb.Append("\r\nGO");

                sb.Append(string.Format("\r\n\r\nCREATE PROCEDURE {0}.SaveEntry_{1}(", this.SqlTable.Schema, this.SqlTable.Tablename));
                sb.Append("\r\n");

                List<ISqlTableColumnAttribute> pkeys = (from pk in this.SqlTable.Columns where pk.PrimaryKey == true select pk).ToList();
                List<ISqlTableColumnAttribute> nokeys = (from nk in this.SqlTable.Columns where nk.PrimaryKey == false select nk).ToList();

                if (pkeys.Count > 0)
                {

                    for (int i = 0; i < this.SqlTable.Columns.Count; i++)
                    {

                        if (!hasIdentityCol && this.SqlTable.Columns[i].Identity)
                        {
                            hasIdentityCol = true;
                            identityColumn = string.Format("@{0}", this.SqlTable.Columns[i].ColumnName);
                        }

                        sb.Append(string.Format("\r\n{0}{1}", this.SqlTable.Columns[i].GetStoredProcedureParameter(), (i == this.SqlTable.Columns.Count - 1 ? "" : ",")));
                    }

                    if (hasIdentityCol)
                    {
                        sb.Append(",\r\n@@OutputID int OUTPUT");
                    }

                    sb.Append("\r\n\r\n)");
                    sb.Append("\r\n\r\nAS");
                    sb.Append("\r\n\r\nBEGIN");
                    sb.Append(string.Format("\r\n\r\n\tif EXISTS(SELECT * FROM {0}", this.SqlTable.GetFormattedTableName()));

                    for (int i = 0; i < pkeys.Count; i++)
                    {
                        sb.Append(string.Format("{0} {1} = @{1}", (i == 0 ? " WHERE" : " AND"), pkeys[i].ColumnName));
                    }

                    sb.Append(")");
                    sb.Append("\r\n\t\tBEGIN");
                    sb.Append(string.Format("\r\n\t\t\tUPDATE {0}", this.SqlTable.GetFormattedTableName()));
                    sb.Append("\r\n\t\t\tSET");

                    for (int i = 0; i < nokeys.Count; i++)
                    {
                        sb.Append(string.Format("\r\n\t\t\t\t{0}\t{1} = @{1}", (i == 0 ? "" : ","), nokeys[i].ColumnName));
                    }

                    for (int i = 0; i < pkeys.Count; i++)
                    {
                        sb.Append(string.Format("\r\n\t\t\t{0} {1} = @{1}", (i == 0 ? "WHERE" : "\tAND"), pkeys[i].ColumnName));
                    }

                    sb.Append(";");

                    if (hasIdentityCol)
                    {
                        sb.Append(string.Format("\r\n\r\n\t\t\tSELECT @@OutputID = {0}", identityColumn));
                    }

                    sb.Append("\r\n\t\tEND");

                    sb.Append("\r\n\telse");
                    sb.Append("\r\n\t\tBEGIN");
                    sb.Append(string.Format("\r\n\t\t\tINSERT INTO {0} (", this.SqlTable.GetFormattedTableName()));

                    int cnt = 0;

                    foreach (SqlTableColumnAttribute col in this.SqlTable.Columns)
                    {
                        if (!col.Identity)
                        {
                            sb.Append(string.Format("{0}{1}", (cnt == 0 ? "" : ", "), col.ColumnName));
                            cnt++;
                        }
                    }

                    cnt = 0;
                    sb.Append(")\r\n\t\t\tVALUES(");

                    foreach (SqlTableColumnAttribute col in this.SqlTable.Columns)
                    {
                        if (!col.Identity)
                        {
                            sb.Append(string.Format("{0}@{1}", (cnt == 0 ? "" : ", "), col.ColumnName));
                            cnt++;
                        }
                    }


                    if (hasIdentityCol)
                    {
                        sb.Append(");\r\n\r\n\t\t\tSELECT @@OutputID = SCOPE_IDENTITY()\r\n\t\tEND");
                    }
                    else
                    {
                        sb.Append(");\r\n\t\tEND");
                    }



                    sb.Append("\r\nEND");
                    sb.Append("\r\nGO\r\n\r\n");

                }
                else
                {
                    throw new InvalidOperationException("A primary key must be specified in order to create a delete procedure.");
                }

            }
            else
            {
                throw new InvalidOperationException("The ISqlTableAttribute parameter passed in the constructor is invalid!");
            }

            return sb.ToString();

        }
        #endregion

    }

    /// <summary>
    /// Base class for all SqlObjectCommand classes
    /// </summary>
    public abstract class SqlObjectCommandBase<T> : ISqlObjectCommand<T>
    {

        #region Local Variables
        /// <summary>
        /// The connection string used when creating the SQL object
        /// </summary>
        protected System.Data.SqlClient.SqlConnection sqlConnection;

        private string _ConnectionString;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of ISQlObjectCommand
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="connectionString"></param>
        public SqlObjectCommandBase(ICommandString commandString, string connectionString)
        {
            CommandString = commandString;
            _ConnectionString = connectionString;
            sqlConnection = new System.Data.SqlClient.SqlConnection(_ConnectionString);
        }

        /// <summary>
        /// Destroys the instance of the SqlObjectCommandBase class
        /// </summary>
        ~SqlObjectCommandBase()
        {
            Dispose(false);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the ICommandString to execute
        /// </summary>
        public ICommandString CommandString { get; set; }

        /// <summary>
        /// Gets the connection string used when creating the SQL Object
        /// </summary>
        public virtual string ConnectionString
        {
            get { return _ConnectionString; }
            set
            {
                _ConnectionString = value;
                sqlConnection = new System.Data.SqlClient.SqlConnection(_ConnectionString);
            }
        }

        /// <summary>
        /// Gets the server name
        /// </summary>
        public string Server => sqlConnection.DataSource;

        /// <summary>
        /// Gets the database name
        /// </summary>
        public string Database => sqlConnection.Database;

        /// <summary>
        /// Gets the connection timeout
        /// </summary>
        public int ConnectionTimeout => sqlConnection.ConnectionTimeout;

        /// <summary>
        /// Gets the System.Data.SqlClient.SqlCredential used to make the connection
        /// </summary>
        public System.Data.SqlClient.SqlCredential Credential => sqlConnection.Credential;

        #endregion

        #region Public Methods
        /// <summary>
        /// Executes the command
        /// </summary>
        public abstract T ExecuteCommand();

        /// <summary>
        /// Disposes of the SqlConnection object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Protected Methods
        protected virtual void Dispose(bool disposing)
        {

            if (disposing)
            {
                sqlConnection?.Dispose();
            }
        }
        #endregion

    }

    /// <summary>
    /// Used for creating SQL Object(s)
    /// </summary>
    public class SqlObjectCreationCommand : SqlObjectCommandBase<bool>
    {

        #region Constructor
        /// <summary>
        /// Creates a new instance of SqlObjectCreationCommand
        /// </summary>
        /// <param name="commandString">The ICommandString to execute</param>
        /// <param name="connectionString">The connection to use when creating the SQL Object(s)</param>
        public SqlObjectCreationCommand(ICommandString commandString, string connectionString)
            : base(commandString, connectionString)
        {

        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Executes the command that creates the SQL Object(s)
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public override bool ExecuteCommand()
        {
            bool result = false;
            
            try
            {
                
                if (sqlConnection != null)
                {

                    if (sqlConnection.State == System.Data.ConnectionState.Closed)
                    {
                        sqlConnection.Open();
                    }
                    
                    string[] commands = CommandString.GetCommandString().Trim('\r', '\n').Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

                    System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand()
                    {
                        Connection = sqlConnection,
                        CommandType = System.Data.CommandType.Text,
                        CommandTimeout = ConnectionTimeout
                    };

                    foreach (string command in commands)
                    {
                        cmd.CommandText = command.Trim('\r', '\n');
                        cmd.ExecuteNonQuery();
                    }
                    
                    result = true;

                }
                
            }
            catch
            {
                result = false;
            }

            return result;

        }
        #endregion

    }

}
