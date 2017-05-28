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
using System.Data.SqlClient;
using System.IO;
using Krodzone.SQL;
using System.Reflection;

namespace Krodzone.Data
{

    /// <summary>
    /// EventHandler for Exceptions thrown in instances of ISqlConnectionObject
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void SqlExceptionEventHandler(object sender, SqlAbortArgs e);

    /// <summary>
    /// Used for determining the success of SqlPutCommand and SqlGetCommand
    /// </summary>
    public enum SqlCommandResultArgs
    {
        /// <summary>
        /// Indicates the execution was successful
        /// </summary>
        Successful,
        /// <summary>
        /// Indicates that the execution failed
        /// </summary>
        Failed,
        /// <summary>
        /// Indicates that error occurred during execution
        /// </summary>
        Error
    }

    /// <summary>
    /// The base interface for implementing a DataSetting class
    /// </summary>
    public interface IDataSetting
    {

        #region Properties
        /// <summary>
        /// Gets or Sets the connection string
        /// </summary>
        string ConnectionString { get; set; }
        /// <summary>
        /// Gets or Sets the procedure name to be executed
        /// </summary>
        string ProcedureName { get; set; }
        /// <summary>
        /// Gets or Sets the procedures input parameter names as an array of string
        /// </summary>
        string[] InputParamNames { get; set; }
        /// <summary>
        /// Gets or Sets the procedures input parameter types as an array of System.Data.SqlDbType
        /// </summary>
        SqlDbType[] InputParamTypes { get; set; }
        /// <summary>
        /// Gets or Sets the procedures output parameter names as an array of string
        /// </summary>
        string[] OutputParamNames { get; set; }
        /// <summary>
        /// Gets or Sets the procedures output parameter types as an array of System.Data.SqlDbType
        /// </summary>
        SqlDbType[] OutputParamTypes { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Creates an array of System.Data.SqlClient.SqlParameter for the input parameters
        /// </summary>
        /// <param name="paramValues">The values to be assigned to the parameters</param>
        /// <returns>Array of System.Data.SqlClient.SqlParameter</returns>
        SqlParameter[] ToInputParameterArray(object[] paramValues);
        /// <summary>
        /// Creates an array of System.Data.SqlClient.SqlParameter for the output parameters
        /// </summary>
        /// <returns>Array of System.Data.SqlClient.SqlParameter</returns>
        SqlParameter[] ToOutputParameterArray();
        #endregion

    }

    /// <summary>
    /// Base interface for all DataSettingFactory objects
    /// </summary>
    public interface IDataSettingFactory : IDisposable
    {

        #region Properties
        /// <summary>
        /// Gets the ICommandString to be executed
        /// </summary>
        ICommandString CommandString { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Generates the code for an EmbeddedSettings class
        /// </summary>
        /// <param name="writer">System.IO.StringWriter used to write the output code to</param>
        void CreateEmbeddedSettingClass(StringWriter writer);
        #endregion

    }

    /// <summary>
    /// Base interface for SQL Execution Commands 
    /// </summary>
    /// <typeparam name="T">Type returned by the ExecuteCommand method defined in interface ICommand</typeparam>
    public interface ISqlExecutionCommand<T> : ICommand<T>
    {

        #region Events
        event SqlExceptionEventHandler ExecutionError;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the IDataSetting object
        /// </summary>
        IDataSetting Setting { get; }
        /// <summary>
        /// Gets a value indicating whether the ParamValues is initialized and the length is greater than zero
        /// </summary>
        bool HasParameters { get; }
        /// <summary>
        /// Gets an array of object containing the input parameter values
        /// </summary>
        object[] ParamValues { get; }
        #endregion

    }

    /// <summary>
    /// Base interface for all objects passed as the second parameter to SqlExceptionEventHandler
    /// </summary>
    public interface ISqlAbortArgs
    {

        #region Properties
        /// <summary>
        /// Gets the Exception that was thrown
        /// </summary>
        Exception Exception { get; }
        /// <summary>
        /// Gets a value indicating if a transaction was started prior to beginning execution
        /// </summary>
        bool RollbackAllowed { get; }
        /// <summary>
        /// Gets or Sets the value used for determining if the transaction should be rolled back
        /// </summary>
        bool RollbackChanges { get; set; }
        #endregion

    }

    public interface ISqlConnectionObject : IDisposable
    {

        #region Events
        event SqlExceptionEventHandler ExecutionError;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the connection string to use when connecting to the database
        /// </summary>
        string ConnectionString { get; }
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
        IDataSetting DataSetting { get; set; }
        System.Data.DataTable[] Tables { get; }
        bool ConnectionOpen { get; }
        IValueCollection ReturnValues { get; }
        #endregion

        #region Methods
        void Open();
        void Close();
        void BeginTransaction();
        void EndTransaction(bool commitChanges);
        bool ExecuteQuery();
        bool ExecuteQuery(object[] paramValues);
        bool ExecuteNonQuery();
        bool ExecuteNonQuery(object[] paramValues);
        System.Data.IDataReader GetReader();
        System.Data.IDataReader GetReader(object[] paramValues);
        #endregion

    }

    /// <summary>
    /// Used to create objects containing connection and procedure information when connecting to the database
    /// </summary>
    public class DataSetting : IDataSetting
    {

        #region Local Variables

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of the DataSetting class
        /// </summary>
        public DataSetting() { }

        /// <summary>
        /// Creates a new instance of the DataSetting class
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        /// <param name="procedureName">The procedure to be executed</param>
        /// <param name="inputParamNames">The input parameter names</param>
        /// <param name="inputParamTypes">The input parameter types</param>
        /// <param name="outputParamNames">The output parameter names</param>
        /// <param name="outputParamTypes">The output parameter types</param>
        public DataSetting(string connectionString, string procedureName, string[] inputParamNames, SqlDbType[] inputParamTypes, string[] outputParamNames, SqlDbType[] outputParamTypes)
        {
            this.ConnectionString = connectionString;
            this.ProcedureName = procedureName;
            this.InputParamNames = inputParamNames;
            this.InputParamTypes = inputParamTypes;
            this.OutputParamNames = outputParamNames;
            this.OutputParamTypes = outputParamTypes;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or Sets the connection string
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// Gets or Sets the procedure name to be executed
        /// </summary>
        public string ProcedureName { get; set; }
        /// <summary>
        /// Gets or Sets the procedures input parameter names as an array of string
        /// </summary>
        public string[] InputParamNames { get; set; }
        /// <summary>
        /// Gets or Sets the procedures input parameter types as an array of System.Data.SqlDbType
        /// </summary>
        public SqlDbType[] InputParamTypes { get; set; }
        /// <summary>
        /// Gets or Sets the procedures output parameter names as an array of string
        /// </summary>
        public string[] OutputParamNames { get; set; }
        /// <summary>
        /// Gets or Sets the procedures output parameter types as an array of System.Data.SqlDbType
        /// </summary>
        public SqlDbType[] OutputParamTypes { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Creates an array of System.Data.SqlClient.SqlParameter for the input parameters
        /// </summary>
        /// <param name="paramValues">The values to be assigned to the parameters</param>
        /// <returns>Array of System.Data.SqlClient.SqlParameter</returns>
        public SqlParameter[] ToInputParameterArray(object[] paramValues)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();

            if ((this.InputParamNames == null) || (this.InputParamTypes == null) || (paramValues == null))
            {
                throw new InvalidOperationException("Input Parameter Names, Types, and Values must be valid arrays with equal length!");
            }

            if ((this.InputParamNames.Length != this.InputParamTypes.Length) || (this.InputParamNames.Length != paramValues.Length))
            {
                throw new InvalidOperationException("Input Parameter Names, Types, and Values must be valid arrays with equal length!");
            }

            for (int i = 0; i < this.InputParamNames.Length; i++)
            {
                SqlParameter param = new SqlParameter()
                {
                    ParameterName = this.InputParamNames[i],
                    SqlDbType = this.InputParamTypes[i],
                    Direction = ParameterDirection.Input,
                    Value = paramValues[i]
                };

                parameters.Add(param);

            }

            return parameters.ToArray();

        }

        /// <summary>
        /// Creates an array of System.Data.SqlClient.SqlParameter for the output parameters
        /// </summary>
        /// <returns>Array of System.Data.SqlClient.SqlParameter</returns>
        public SqlParameter[] ToOutputParameterArray()
        {
            List<SqlParameter> parameters = new List<SqlParameter>();

            if ((this.OutputParamNames == null) || (this.OutputParamTypes == null))
            {
                throw new InvalidOperationException("Output Parameter Names and Types must be valid arrays with equal length!");
            }

            if ((this.OutputParamNames.Length != this.OutputParamTypes.Length))
            {
                throw new InvalidOperationException("Output Parameter Names and Types must be valid arrays with equal length!");
            }

            for (int i = 0; i < this.OutputParamNames.Length; i++)
            {
                SqlParameter param = new SqlParameter()
                {
                    ParameterName = this.OutputParamNames[i],
                    SqlDbType = this.OutputParamTypes[i],
                    Direction = ParameterDirection.Output
                };

                parameters.Add(param);

            }

            return parameters.ToArray();

        }
        #endregion

    }

    /// <summary>
    /// Used for generating EmbeddedSetting classes from the stored procedure in your transaction database
    /// </summary>
    public class DataSettingFactory : IDataSettingFactory
    {

        #region Local Variables
        /// <summary>
        /// Read only member instance for retrieving the Stored Procedure data used when creating the EmbeddingSettings class
        /// </summary>
        protected readonly ISqlObjectCommand<List<DataSettingCreationParameter>> _Command;

        private ICommandString _CommandString;
        private List<DataSettingCreationParameter> parameterData;
        private string _ConfigSettingName;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of DataSettingFactory
        /// </summary>
        /// <param name="connectionString">The connection string to use for retrieving the stored procedure data</param>
        /// <param name="configSettingName">The Config Setting name that references your connection string</param>
        public DataSettingFactory(string connectionString, string configSettingName)
        {
            this._CommandString = new DataSettingCreationString();
            this._Command = new DataSettingCreationCommand(this._CommandString, connectionString);
            this.parameterData = new List<DataSettingCreationParameter>();
            this._ConfigSettingName = configSettingName;
        }

        ~DataSettingFactory()
        {
            this.Dispose(false);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the ICommandString to be executed
        /// </summary>
        public ICommandString CommandString
        {
            get { return this._CommandString; }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Generates the code for an EmbeddedSettings class
        /// </summary>
        /// <param name="writer">System.IO.StringWriter used to write the output code to</param>
        public virtual void CreateEmbeddedSettingClass(StringWriter writer)
        {
            this.parameterData = this._Command.ExecuteCommand();

            if ((writer != null) && (this.parameterData.Count > 0))
            {
                //  Write Using Statements
                writer.WriteLine("using System;");
                writer.WriteLine("using System.Data;");
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine("using System.Configuration;");
                writer.WriteLine("using System.Linq;");
                writer.WriteLine("using System.Text;");
                writer.WriteLine("using System.Threading.Tasks;");
                writer.WriteLine("using Krodzone.Data;");
                writer.WriteLine("");

                //  Class Declaration
                writer.WriteLine("[Serializable]");
                writer.WriteLine("public class EmbeddedSettings");
                writer.WriteLine("{");
                writer.WriteLine("");

                //  Connection String Property
                writer.WriteLine("\t#region Connection String");
                writer.WriteLine("\tpublic static string ConnectionString");
                writer.WriteLine("\t{");
                writer.WriteLine("\t\tget");
                writer.WriteLine("\t\t{");
                writer.WriteLine(string.Format("\t\t\treturn ConfigurationManager.ConnectionStrings[\"{0}\"].ToString();", this._ConfigSettingName));
                writer.WriteLine("\t\t}");
                writer.WriteLine("\t}");
                writer.WriteLine("\t#endregion");

                writer.WriteLine("");

                //  Create DataSetting Objects
                writer.WriteLine("\t#region DataSetting Objects");

                List<string> schemas = (from obj in this.parameterData select obj.SchemaName).Distinct<string>().ToList();

                foreach (string schema in schemas)
                {
                    writer.WriteLine("\r\n\t#region DataSettings for Schema {0}", schema);
                    writer.WriteLine("\tpublic static class {0}", schema);
                    writer.WriteLine("\t{");
                    writer.WriteLine("\r\n\t\t#region Properties");

                    List<int> objectIDs = (from obj in this.parameterData where obj.SchemaName == schema select obj.ObjectID).Distinct<int>().ToList();

                    foreach (int objID in objectIDs)
                    {
                        List<DataSettingCreationParameter> parms = (from obj in this.parameterData where obj.ObjectID == objID select obj).ToList();
                        string procedureName = parms[0].ProcedureName;

                        writer.WriteLine(string.Format("\t\tpublic static IDataSetting {0}", parms[0].ProcedureName));
                        writer.WriteLine("\t\t{");
                        writer.WriteLine("\t\t\tget");
                        writer.WriteLine("\t\t\t{");

                        if (string.IsNullOrEmpty(parms[0].ParameterName))   //  There are no parameters
                        {
                            writer.WriteLine(string.Format("\t\t\t\treturn new DataSetting(EmbeddedSettings.ConnectionString, \"{0}.{1}\", new string[] {2}, new SqlDbType[] {2}, new string[] {2}, new SqlDbType[] {2});", parms[0].SchemaName, parms[0].ProcedureName, "{ }"));
                        }
                        else
                        {
                            StringBuilder sbInNames = new StringBuilder("string[] inputParmNames = new string[] { ");
                            StringBuilder sbInTypes = new StringBuilder("SqlDbType[] inputParmTypes = new SqlDbType[] { ");
                            StringBuilder sbOutNames = new StringBuilder("string[] outputParmNames = new string[] { ");
                            StringBuilder sbOutTypes = new StringBuilder("SqlDbType[] outputParmTypes = new SqlDbType[] { ");

                            IDataSetting setting = new DataSetting("", "", new string[] { }, new SqlDbType[] { }, new string[] { }, new SqlDbType[] { });
                            List<DataSettingCreationParameter> inputParms = (from obj in parms where obj.IsOutParam == false select obj).ToList();
                            List<DataSettingCreationParameter> outputParms = (from obj in parms where obj.IsOutParam == true select obj).ToList();

                            for (int i = 0; i < inputParms.Count; i++)
                            {
                                sbInNames.Append(string.Format("{0}\"{1}\"", (i == 0 ? "" : ", "), inputParms[i].ParameterName));
                                sbInTypes.Append(string.Format("{0}(SqlDbType){1}", (i == 0 ? "" : ", "), inputParms[i].CSTypeValue));
                            }

                            for (int i = 0; i < outputParms.Count; i++)
                            {
                                sbOutNames.Append(string.Format("{0}\"{1}\"", (i == 0 ? "" : ", "), outputParms[i].ParameterName));
                                sbOutTypes.Append(string.Format("{0}(SqlDbType){1}", (i == 0 ? "" : ", "), outputParms[i].CSTypeValue));
                            }

                            sbInNames.Append(" };");
                            sbInTypes.Append(" };");
                            sbOutNames.Append(" };");
                            sbOutTypes.Append(" };");

                            writer.WriteLine(string.Format("\t\t\t\t{0}", sbInNames.ToString()));
                            writer.WriteLine(string.Format("\t\t\t\t{0}", sbInTypes.ToString()));
                            writer.WriteLine(string.Format("\t\t\t\t{0}", sbOutNames.ToString()));
                            writer.WriteLine(string.Format("\t\t\t\t{0}", sbOutTypes.ToString()));
                            writer.WriteLine("");
                            writer.WriteLine(string.Format("\t\t\t\treturn new DataSetting(EmbeddedSettings.ConnectionString, \"{0}.{1}\", inputParmNames, inputParmTypes, outputParmNames, outputParmTypes);", parms[0].SchemaName, parms[0].ProcedureName));

                        }

                        writer.WriteLine("\t\t\t}");
                        writer.WriteLine("\t\t}");
                        writer.WriteLine("");

                    }


                    writer.WriteLine("\t\t#endregion");
                    writer.WriteLine("\t\t}");
                    writer.WriteLine("\t#endregion");

                }
                

                writer.WriteLine("\r\n\t#endregion");

                writer.WriteLine("");
                writer.WriteLine("}");

            }

        }

        /// <summary>
        /// Disposes of the object
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Protected Methods
        protected virtual void Dispose(bool disposing)
        {

            if (disposing)
            {
                this._CommandString = null;
                this._Command.Dispose();
                this.parameterData = null;
            }

        }
        #endregion

        #region Nested Classes
        /// <summary>
        /// Used to hold the stored procedure data necessary for creating DataSetting objects
        /// </summary>
        protected class DataSettingCreationParameter : ICommonObject
        {

            #region Local Variables

            #endregion

            #region Constructors
            /// <summary>
            /// Creates a new instance of the DataSettingCreationParameter class
            /// </summary>
            public DataSettingCreationParameter() { }
            #endregion

            #region Properties
            /// <summary>
            /// Gets the database Object_ID
            /// </summary>
            public int ObjectID { get; set; }
            /// <summary>
            /// Gets the stored procedure schema name
            /// </summary>
            public string SchemaName { get; set; }
            /// <summary>
            /// Gets the stored procedure name
            /// </summary>
            public string ProcedureName { get; set; }
            /// <summary>
            /// Gets the stored procedure parameter name
            /// </summary>
            public string ParameterName { get; set; }
            /// <summary>
            /// Gets the integer value that corresponds to the System.Data.SqlDbType equivalent
            /// </summary>
            public int CSTypeValue { get; set; }
            /// <summary>
            /// Gets a value indicating if the parameter is an output parameter
            /// </summary>
            public bool IsOutParam { get; set; }
            #endregion

            #region Public Methods
            /// <summary>
            /// Creates an array of object from the class properties
            /// </summary>
            /// <returns>Array of Object</returns>
            public object[] ToArray()
            {
                return new object[] { this.ObjectID, this.SchemaName, this.ProcedureName, this.ParameterName, this.CSTypeValue, this.IsOutParam };
            }
            #endregion

        }

        /// <summary>
        /// Implements ICommandString for the purpose of creating a command to return the stored procedure information used in creating DataSetting objects
        /// </summary>
        protected class DataSettingCreationString : ICommandString
        {

            #region Local Variables

            #endregion

            #region Constructors
            /// <summary>
            /// Creates a new instance of DataSettingCreationString
            /// </summary>
            public DataSettingCreationString() { }
            #endregion

            #region Properties

            #endregion

            #region Public Methods
            /// <summary>
            /// Creates a command as a string to be executed
            /// </summary>
            /// <returns>System.String</returns>
            public virtual string GetCommandString()
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("DECLARE @SqlTypes TABLE (");
                sb.Append("\r\n\t\tSystemTypeID int not null");
                sb.Append("\r\n\t,\tSystemTypeName varchar(100) not null");
                sb.Append("\r\n\t,\tCSTypeName varchar(100) not null");
                sb.Append("\r\n\t,\tCSTypeValue int not null");
                sb.Append(")\r\n");

                sb.Append("\r\nINSERT INTO @SqlTypes");
                sb.Append("\r\nSELECT	system_type_id AS SystemTypeID");
                sb.Append("\r\n\t,\tname AS SystemTypeName");
                sb.Append("\r\n\t,\tCASE");
                sb.Append("\r\n\t\t\tWHEN name = 'image' THEN 'SqlDbType.Binary'");
                sb.Append("\r\n\t\t\tWHEN name = 'text' THEN 'SqlDbType.Text'");
                sb.Append("\r\n\t\t\tWHEN name = 'uniqueidentifier' THEN 'SqlDbType.UniqueIdentifier'");
                sb.Append("\r\n\t\t\tWHEN name = 'date' THEN 'SqlDbType.Date'");
                sb.Append("\r\n\t\t\tWHEN name = 'time' THEN 'SqlDbType.Time'");
                sb.Append("\r\n\t\t\tWHEN name = 'datetime2' THEN 'SqlDbType.DateTime'");
                sb.Append("\r\n\t\t\tWHEN name = 'datetimeoffset' THEN 'SqlDbType.DateTimeOffset'");
                sb.Append("\r\n\t\t\tWHEN name = 'tinyint' THEN 'SqlDbType.TinyInt'");
                sb.Append("\r\n\t\t\tWHEN name = 'smallint' THEN 'SqlDbType.SmallInt'");
                sb.Append("\r\n\t\t\tWHEN name = 'int' THEN 'SqlDbType.Int'");
                sb.Append("\r\n\t\t\tWHEN name = 'smalldatetime' THEN 'SqlDbType.SmallDateTime'");
                sb.Append("\r\n\t\t\tWHEN name = 'real' THEN 'SqlDbType.Real'");
                sb.Append("\r\n\t\t\tWHEN name = 'money' THEN 'SqlDbType.Money'");
                sb.Append("\r\n\t\t\tWHEN name = 'datetime' THEN 'SqlDbType.DateTime'");
                sb.Append("\r\n\t\t\tWHEN name = 'float' THEN 'SqlDbType.Float'");
                sb.Append("\r\n\t\t\tWHEN name = 'sql_variant' THEN 'SqlDbType.Variant'");
                sb.Append("\r\n\t\t\tWHEN name = 'ntext' THEN 'SqlDbType.NText'");
                sb.Append("\r\n\t\t\tWHEN name = 'bit' THEN 'SqlDbType.Bit'");
                sb.Append("\r\n\t\t\tWHEN name = 'decimal' THEN 'SqlDbType.Decimal'");
                sb.Append("\r\n\t\t\tWHEN name = 'numeric' THEN 'SqlDbType.Decimal'");
                sb.Append("\r\n\t\t\tWHEN name = 'smallmoney' THEN 'SqlDbType.SmallMoney'");
                sb.Append("\r\n\t\t\tWHEN name = 'bigint' THEN 'SqlDbType.BigInt'");
                sb.Append("\r\n\t\t\tWHEN name = 'varbinary' THEN 'SqlDbType.VarBinary'");
                sb.Append("\r\n\t\t\tWHEN name = 'varchar' THEN 'SqlDbType.VarChar'");
                sb.Append("\r\n\t\t\tWHEN name = 'binary' THEN 'SqlDbType.Binary'");
                sb.Append("\r\n\t\t\tWHEN name = 'char' THEN 'SqlDbType.Char'");
                sb.Append("\r\n\t\t\tWHEN name = 'timestamp' THEN 'SqlDbType.Timestamp'");
                sb.Append("\r\n\t\t\tWHEN name = 'nvarchar' THEN 'SqlDbType.NVarChar'");
                sb.Append("\r\n\t\t\tWHEN name = 'nchar' THEN 'SqlDbType.NChar'");
                sb.Append("\r\n\t\t\tWHEN name = 'xml' THEN 'SqlDbType.Xml'");
                sb.Append("\r\n\t\tEND AS CSTypeName");
                sb.Append("\r\n\t,\tCASE");
                sb.Append("\r\n\t\t\tWHEN name = 'image' THEN 1");
                sb.Append("\r\n\t\t\tWHEN name = 'text' THEN 18");
                sb.Append("\r\n\t\t\tWHEN name = 'uniqueidentifier' THEN 14");
                sb.Append("\r\n\t\t\tWHEN name = 'date' THEN 31");
                sb.Append("\r\n\t\t\tWHEN name = 'time' THEN 32");
                sb.Append("\r\n\t\t\tWHEN name = 'datetime2' THEN 4");
                sb.Append("\r\n\t\t\tWHEN name = 'datetimeoffset' THEN 34");
                sb.Append("\r\n\t\t\tWHEN name = 'tinyint' THEN 20");
                sb.Append("\r\n\t\t\tWHEN name = 'smallint' THEN 16");
                sb.Append("\r\n\t\t\tWHEN name = 'int' THEN 8");
                sb.Append("\r\n\t\t\tWHEN name = 'smalldatetime' THEN 15");
                sb.Append("\r\n\t\t\tWHEN name = 'real' THEN 13");
                sb.Append("\r\n\t\t\tWHEN name = 'money' THEN 9");
                sb.Append("\r\n\t\t\tWHEN name = 'datetime' THEN 4");
                sb.Append("\r\n\t\t\tWHEN name = 'float' THEN 6");
                sb.Append("\r\n\t\t\tWHEN name = 'sql_variant' THEN 23");
                sb.Append("\r\n\t\t\tWHEN name = 'ntext' THEN 11");
                sb.Append("\r\n\t\t\tWHEN name = 'bit' THEN 2");
                sb.Append("\r\n\t\t\tWHEN name = 'decimal' THEN 5");
                sb.Append("\r\n\t\t\tWHEN name = 'numeric' THEN 5");
                sb.Append("\r\n\t\t\tWHEN name = 'smallmoney' THEN 17");
                sb.Append("\r\n\t\t\tWHEN name = 'bigint' THEN 0");
                sb.Append("\r\n\t\t\tWHEN name = 'varbinary' THEN 21");
                sb.Append("\r\n\t\t\tWHEN name = 'varchar' THEN 22");
                sb.Append("\r\n\t\t\tWHEN name = 'binary' THEN 1");
                sb.Append("\r\n\t\t\tWHEN name = 'char' THEN 3");
                sb.Append("\r\n\t\t\tWHEN name = 'timestamp' THEN 19");
                sb.Append("\r\n\t\t\tWHEN name = 'nvarchar' THEN 12");
                sb.Append("\r\n\t\t\tWHEN name = 'nchar' THEN 10");
                sb.Append("\r\n\t\t\tWHEN name = 'xml' THEN 25");
                sb.Append("\r\n\t\tEND AS CSTypeValue");
                sb.Append("\r\nFROM sys.types");
                sb.Append("\r\nWHERE system_type_id NOT IN (231,240)");
                sb.Append("\r\nSELECT	o.object_id AS ObjectID");
                sb.Append("\r\n\t,\ts.name AS SchemaName");
                sb.Append("\r\n\t,\to.name AS ProcedureName");
                sb.Append("\r\n\t,\tISNULL(c.name, '') AS ParameterName");
                sb.Append("\r\n\t,\tISNULL(typs.CSTypeName, '') AS CSTypeName");
                sb.Append("\r\n\t,\tISNULL(typs.CSTypeValue, '') AS CSTypeValue");
                sb.Append("\r\n\t,\tCASE WHEN ISNULL(c.isoutparam, 0) = 1 THEN CAST(1 as bit) ELSE CAST(0 as bit) END AS IsOutParam");
                sb.Append("\r\nFROM sys.objects o");
                sb.Append("\r\n\tINNER JOIN sys.schemas s");
                sb.Append("\r\n\t\tON o.schema_id = s.schema_id");
                sb.Append("\r\n\tLEFT JOIN sys.syscolumns c");
                sb.Append("\r\n\t\t\tINNER JOIN @SqlTypes typs");
                sb.Append("\r\n\t\t\t\tON c.xtype = typs.SystemTypeID");
                sb.Append("\r\n\t\tON o.object_id = c.id");
                sb.Append("\r\nWHERE o.type IN ('P','PC')");
                sb.Append("\r\n\tAND o.name NOT LIKE '%sp_%diagram%'");
                sb.Append("\r\nORDER BY o.object_id, c.colorder");

                return sb.ToString();

            }
            #endregion

        }

        /// <summary>
        /// The command used to retrieve the stored procedure information used in creating the DataSetting objects
        /// </summary>
        protected class DataSettingCreationCommand : ISqlObjectCommand<List<DataSettingCreationParameter>>
        {
            
        #region Local Variables
        /// <summary>
        /// The connection string used when creating the SQL object
        /// </summary>
        protected System.Data.SqlClient.SqlConnection sqlConnection;

        private ICommandString _CommandString;
        private string _ConnectionString;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of ISQlObjectCommand
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="connectionString"></param>
        public DataSettingCreationCommand(ICommandString commandString, string connectionString)
        {
            this._CommandString = commandString;
            this._ConnectionString = connectionString;
            this.sqlConnection = new System.Data.SqlClient.SqlConnection(this._ConnectionString);
        }

        /// <summary>
        /// Destroys the instance of the SqlObjectCommandBase class
        /// </summary>
        ~DataSettingCreationCommand()
        {
            this.Dispose(false);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the ICommandString to execute
        /// </summary>
        public ICommandString CommandString
        {
            get { return this._CommandString; }
            set { this._CommandString = value; }
        }

        /// <summary>
        /// Gets the connection string used when creating the SQL Object
        /// </summary>
        public virtual string ConnectionString
        {
            get { return this._ConnectionString; }
            set
            {
                this._ConnectionString = value;
                this.sqlConnection = new System.Data.SqlClient.SqlConnection(this._ConnectionString);
            }
        }

        /// <summary>
        /// Gets the server name
        /// </summary>
        public string Server
        {
            get { return this.sqlConnection.DataSource; }
        }

        /// <summary>
        /// Gets the database name
        /// </summary>
        public string Database
        {
            get { return this.sqlConnection.Database; }
        }

        /// <summary>
        /// Gets the connection timeout
        /// </summary>
        public int ConnectionTimeout
        {
            get { return this.sqlConnection.ConnectionTimeout; }
        }

        /// <summary>
        /// Gets the System.Data.SqlClient.SqlCredential used to make the connection
        /// </summary>
        public System.Data.SqlClient.SqlCredential Credential
        {
            get { return this.sqlConnection.Credential; }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Executes the command that creates the SQL Object(s)
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public List<DataSettingCreationParameter> ExecuteCommand()
        {
            List<DataSettingCreationParameter> results = new List<DataSettingCreationParameter>();

            try
            {

                if (this.sqlConnection != null)
                {
                    this.sqlConnection.Open();

                    System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand()
                    {
                        Connection = this.sqlConnection,
                        CommandText = this.CommandString.GetCommandString(),
                        CommandType = System.Data.CommandType.Text,
                        CommandTimeout = this.ConnectionTimeout
                    };

                    DataTable dt = new DataTable();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                    adapter.Fill(dt);

                    foreach (DataRow row in dt.Rows)
                    {
                        DataSettingCreationParameter param = new DataSettingCreationParameter();

                        param.ObjectID = (int)row["ObjectID"];
                        param.SchemaName = (string)row["SchemaName"];
                        param.ProcedureName = (string)row["ProcedureName"];
                        param.ParameterName = (string)row["ParameterName"];
                        param.CSTypeValue = (int)row["CSTypeValue"];
                        param.IsOutParam = (bool)row["IsOutParam"];
                        

                        results.Add(param);

                    }

                }

            }
            catch (Exception e)
            {
                results = new List<DataSettingCreationParameter>();
            }

            return results;

        }

        /// <summary>
        /// Disposes of the SqlConnection object
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Protected Methods
        protected virtual void Dispose(bool disposing)
        {

            if (disposing)
            {
                if (this.sqlConnection != null) { this.sqlConnection.Dispose(); }
            }

        }
        #endregion

        }
        #endregion

    }

    /// <summary>
    /// Used for passing Exception and SQL Transaction information to the ISqlConnectionObject instance
    /// </summary>
    public class SqlAbortArgs : EventArgs, ISqlAbortArgs
    {

        #region Local Variables
        /// <summary>
        /// Read only member instance used for returning the value of property Exception
        /// </summary>
        protected readonly Exception _Exception;
        /// <summary>
        /// Read only member instance used for returning the value of property RollbackAllowed
        /// </summary>
        protected readonly bool _RollbackAllowed;
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception">The Exception object returned by property Exception</param>
        /// <param name="rollbackAllowed">The boolean value returned by property RollbackAllowed</param>
        public SqlAbortArgs(Exception exception, bool rollbackAllowed)
        {
            this._Exception = exception;
            this._RollbackAllowed = rollbackAllowed;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the Exception that was thrown
        /// </summary>
        public Exception Exception
        {
            get { return this._Exception; }
        }

        /// <summary>
        /// Gets a value indicating if a transaction was started prior to beginning execution
        /// </summary>
        public bool RollbackAllowed
        {
            get { return this._RollbackAllowed; }
        }

        /// <summary>
        /// Gets or Sets the value used for determining if the transaction should be rolled back
        /// </summary>
        public bool RollbackChanges { get; set; }
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public class ReturnValueCollection : IValueCollection
    {

        #region Local Variables
        protected readonly Dictionary<string, object> _Items;
        #endregion

        #region Constructor
        public ReturnValueCollection()
        {
            this._Items = new Dictionary<string, object>();
        }
        #endregion

        #region Properties
        public object this[string key]
        {
            get
            {
                if (!this._Items.ContainsKey(key))
                {
                    return null;
                }
                else
                {
                    return this._Items[key];
                }
            }
        }
        #endregion

        #region Public Methods
        public void Add(string key, object value)
        {
            if (this._Items.ContainsKey(key))
            {
                this._Items[key] = value;
            }
            else
            {
                this._Items.Add(key, value);
            }
        }

        public object[] ToArray()
        {
            return this._Items.Values.ToArray<object>();
        }
        #endregion

    }


    public class SqlConnectionObject : ISqlConnectionObject
    {

        #region Local Variables
        private System.Data.SqlClient.SqlConnection _Connection;
        private System.Data.SqlClient.SqlTransaction _Transaction;
        private System.Data.SqlClient.SqlCommand _Command;
        private System.Data.DataTable[] _Tables;

        private string _ConnectionString;
        private bool _ConnectionOpen;
        private IValueCollection _ReturnValues;

        private bool transactionInitialized;
        #endregion

        #region Constructor
        public SqlConnectionObject(IDataSetting dataSetting)
        {
            this.DataSetting = dataSetting;
            this._ConnectionString = dataSetting.ConnectionString;
        }

        ~SqlConnectionObject()
        {
            this.Dispose(false);
        }
        #endregion

        #region Events
        public event SqlExceptionEventHandler ExecutionError;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the connection string to use when connecting to the database
        /// </summary>
        public string ConnectionString
        {
            get { return this._ConnectionString; }
        }
        /// <summary>
        /// Gets the server used in the connection
        /// </summary>
        public string Server
        {
            get { return this._Connection.DataSource; }
        }
        /// <summary>
        /// Gets the database used in the connection
        /// </summary>
        public string Database
        {
            get { return this._Connection.Database; }
        }
        /// <summary>
        /// Gets the connection timeout used in the connection
        /// </summary>
        public int ConnectionTimeout
        {
            get { return this._Connection.ConnectionTimeout; }
        }
        /// <summary>
        /// Gets the System.Data.SqlClient.SqlCredential used in the connection
        /// </summary>
        public System.Data.SqlClient.SqlCredential Credential
        {
            get { return this._Connection.Credential; }
        }

        public IDataSetting DataSetting { get; set; }

        public System.Data.DataTable[] Tables
        {
            get { return this._Tables; }
        }

        public bool ConnectionOpen
        {
            get { return this._ConnectionOpen; }
        }

        public IValueCollection ReturnValues
        {
            get { return this._ReturnValues; }
        }
        #endregion

        #region Public Methods
        public virtual void Open()
        {

            try
            {
                if (this._ConnectionOpen) { this.Close(); }

                if (this.DataSetting == null)
                {
                    throw new InvalidOperationException("The DataSetting object is not valid for the current operation.");
                }

                this._Connection = new System.Data.SqlClient.SqlConnection(this.DataSetting.ConnectionString);
                this._Connection.Open();
                this._ConnectionOpen = true;

            }
            catch (Exception ex)
            {
                this.OnExecutionError(new SqlAbortArgs(ex, false));
            }

        }

        public virtual void Close()
        {

            try
            {
                if (this._Connection != null)
                {
                    this._Connection.Dispose();
                    this._ConnectionOpen = false;
                }
            }
            catch (Exception ex)
            {
                this._ConnectionOpen = false;
                this._Connection = null;
                OnExecutionError(new SqlAbortArgs(ex, false));
            }

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public virtual void BeginTransaction()
        {

            try
            {

                if ((this.transactionInitialized) || (this._Transaction != null))
                {
                    throw new InvalidOperationException("Cannot begin a new transaction without first completing the previous transaction.");
                }

                if (this.DataSetting == null)
                {
                    throw new InvalidOperationException("DataSetting cannot be null when starting a new transaction.");
                }

                if (this._Connection.State != ConnectionState.Open) { this.Open(); }

                this._Transaction = this._Connection.BeginTransaction();
                this.transactionInitialized = true;

            }
            catch (Exception ex)
            {
                OnExecutionError(new SqlAbortArgs(ex, false));
            }

        }

        public virtual void EndTransaction(bool commitChanges)
        {

            try
            {

                if ((this.transactionInitialized) && (this._Transaction != null))
                {
                    if (commitChanges)
                    {
                        this._Transaction.Commit();
                        this.transactionInitialized = false;
                    }
                    else
                    {
                        this._Transaction.Rollback();
                        this.transactionInitialized = false;
                    }
                }
            }
            catch (Exception ex)
            {
                OnExecutionError(new SqlAbortArgs(ex, false));
            }

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public virtual bool ExecuteQuery()
        {
            bool result = false;

            try
            {

                if (this.DataSetting == null)
                {
                    throw new InvalidOperationException("DataSetting cannot be null when executing a query.");
                }

                if (!this._ConnectionOpen) { this.Open(); }

                this._Command = new System.Data.SqlClient.SqlCommand()
                {
                    CommandText = this.DataSetting.ProcedureName,
                    CommandType = System.Data.CommandType.StoredProcedure,
                    Connection = this._Connection,
                    CommandTimeout = (this._Connection.ConnectionTimeout < 1000 ? 1000 : this._Connection.ConnectionTimeout)
                };

                if (this.DataSetting.OutputParamNames.Length > 0) { this._Command.Parameters.AddRange(this.DataSetting.ToOutputParameterArray()); }

                this._ReturnValues = new ReturnValueCollection();

                System.Data.DataSet ds = new System.Data.DataSet();
                System.Data.SqlClient.SqlDataAdapter adapter = new System.Data.SqlClient.SqlDataAdapter(this._Command);

                adapter.Fill(ds);

                this._Tables = new DataTable[ds.Tables.Count];
                ds.Tables.CopyTo(this._Tables, 0);

                foreach (string parmName in this.DataSetting.OutputParamNames)
                {
                    this._ReturnValues.Add(parmName, this._Command.Parameters[parmName].Value);
                }

                result = true;

            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                OnExecutionError(new SqlAbortArgs(ex, this.transactionInitialized));
                result = false;
            }
            catch (Exception ex)
            {
                OnExecutionError(new SqlAbortArgs(ex, this.transactionInitialized));
                result = false;
            }

            return result;

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public virtual bool ExecuteQuery(object[] paramValues)
        {
            bool result = false;

            try
            {

                if (this.DataSetting == null)
                {
                    throw new InvalidOperationException("DataSetting cannot be null when executing a query.");
                }

                if (!this._ConnectionOpen) { this.Open(); }

                this._Command = new System.Data.SqlClient.SqlCommand()
                {
                    CommandText = this.DataSetting.ProcedureName,
                    CommandType = System.Data.CommandType.StoredProcedure,
                    Connection = this._Connection,
                    CommandTimeout = (this._Connection.ConnectionTimeout < 1000 ? 1000 : this._Connection.ConnectionTimeout)
                };

                if ((paramValues != null) && (paramValues.Length > 0))
                {

                    if ((this.DataSetting.InputParamNames.Length != this.DataSetting.InputParamTypes.Length) || (this.DataSetting.InputParamNames.Length != paramValues.Length))
                    {
                        throw new ArgumentOutOfRangeException("The value passed for paramValues is outside the range for the value of DataSetting.InputParamNames.");
                    }

                    this._Command.Parameters.AddRange(this.DataSetting.ToInputParameterArray(paramValues));

                }

                if (this.DataSetting.OutputParamNames.Length > 0) { this._Command.Parameters.AddRange(this.DataSetting.ToOutputParameterArray()); }

                this._ReturnValues = new ReturnValueCollection();

                System.Data.DataSet ds = new System.Data.DataSet();
                System.Data.SqlClient.SqlDataAdapter adapter = new System.Data.SqlClient.SqlDataAdapter(this._Command);

                adapter.Fill(ds);

                this._Tables = new DataTable[ds.Tables.Count];
                ds.Tables.CopyTo(this._Tables, 0);

                foreach (string parmName in this.DataSetting.OutputParamNames)
                {
                    this._ReturnValues.Add(parmName, this._Command.Parameters[parmName].Value);
                }

                result = true;

            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                OnExecutionError(new SqlAbortArgs(ex, this.transactionInitialized));
                result = false;
            }
            catch (Exception ex)
            {
                OnExecutionError(new SqlAbortArgs(ex, this.transactionInitialized));
                result = false;
            }

            return result;

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public virtual bool ExecuteNonQuery()
        {
            bool result = false;

            try
            {

                if (this.DataSetting == null)
                {
                    throw new InvalidOperationException("DataSetting cannot be null when executing a query.");
                }

                if (!this._ConnectionOpen) { this.Open(); }

                this._Command = new System.Data.SqlClient.SqlCommand()
                {
                    CommandText = this.DataSetting.ProcedureName,
                    CommandType = System.Data.CommandType.StoredProcedure,
                    Connection = this._Connection,
                    CommandTimeout = (this._Connection.ConnectionTimeout < 1000 ? 1000 : this._Connection.ConnectionTimeout),
                    Transaction = this._Transaction
                };

                if (this.DataSetting.OutputParamNames.Length > 0) { this._Command.Parameters.AddRange(this.DataSetting.ToOutputParameterArray()); }

                this._ReturnValues = new ReturnValueCollection();

                this._Command.ExecuteNonQuery();

                foreach (string parmName in this.DataSetting.OutputParamNames)
                {
                    this._ReturnValues.Add(parmName, this._Command.Parameters[parmName].Value);
                }

                result = true;

            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                OnExecutionError(new SqlAbortArgs(ex, this.transactionInitialized));
                result = false;
            }
            catch (Exception ex)
            {
                OnExecutionError(new SqlAbortArgs(ex, this.transactionInitialized));
                result = false;
            }

            return result;

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public virtual bool ExecuteNonQuery(object[] paramValues)
        {
            bool result = false;

            try
            {

                if (this.DataSetting == null)
                {
                    throw new InvalidOperationException("DataSetting cannot be null when executing a query.");
                }

                if (!this._ConnectionOpen) { this.Open(); }

                this._Command = new System.Data.SqlClient.SqlCommand()
                {
                    CommandText = this.DataSetting.ProcedureName,
                    CommandType = System.Data.CommandType.StoredProcedure,
                    Connection = this._Connection,
                    CommandTimeout = (this._Connection.ConnectionTimeout < 1000 ? 1000 : this._Connection.ConnectionTimeout),
                    Transaction = this._Transaction
                };

                if ((paramValues != null) && (paramValues.Length > 0))
                {

                    if ((this.DataSetting.InputParamNames.Length != this.DataSetting.InputParamTypes.Length) || (this.DataSetting.InputParamNames.Length != paramValues.Length))
                    {
                        throw new ArgumentOutOfRangeException("The value passed for paramValues is outside the range for the value of DataSetting.InputParamNames.");
                    }

                    this._Command.Parameters.AddRange(this.DataSetting.ToInputParameterArray(paramValues));

                }

                if (this.DataSetting.OutputParamNames.Length > 0) { this._Command.Parameters.AddRange(this.DataSetting.ToOutputParameterArray()); }

                this._ReturnValues = new ReturnValueCollection();

                this._Command.ExecuteNonQuery();

                foreach (string parmName in this.DataSetting.OutputParamNames)
                {
                    this._ReturnValues.Add(parmName, this._Command.Parameters[parmName].Value);
                }

                result = true;

            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                OnExecutionError(new SqlAbortArgs(ex, this.transactionInitialized));
                result = false;
            }
            catch (Exception ex)
            {
                OnExecutionError(new SqlAbortArgs(ex, this.transactionInitialized));
                result = false;
            }

            return result;

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public virtual System.Data.IDataReader GetReader()
        {

            try
            {

                if (this.DataSetting == null)
                {
                    throw new InvalidOperationException("DataSetting cannot be null when creating an instance of IDataReader.");
                }

                if (!this._ConnectionOpen) { this.Open(); }

                this._Command = new System.Data.SqlClient.SqlCommand()
                {
                    CommandText = this.DataSetting.ProcedureName,
                    CommandType = System.Data.CommandType.StoredProcedure,
                    Connection = this._Connection,
                    CommandTimeout = (this._Connection.ConnectionTimeout < 1000 ? 1000 : this._Connection.ConnectionTimeout)
                };

                return this._Command.ExecuteReader();

            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                OnExecutionError(new SqlAbortArgs(ex, false));
                return null;
            }
            catch (Exception ex)
            {
                OnExecutionError(new SqlAbortArgs(ex, false));
                return null;
            }

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public virtual System.Data.IDataReader GetReader(object[] paramValues)
        {

            try
            {

                if (this.DataSetting == null)
                {
                    throw new InvalidOperationException("DataSetting cannot be null when creating an instance of IDataReader.");
                }

                if (!this._ConnectionOpen) { this.Open(); }

                this._Command = new System.Data.SqlClient.SqlCommand()
                {
                    CommandText = this.DataSetting.ProcedureName,
                    CommandType = System.Data.CommandType.StoredProcedure,
                    Connection = this._Connection,
                    CommandTimeout = (this._Connection.ConnectionTimeout < 1000 ? 1000 : this._Connection.ConnectionTimeout)
                };

                if ((paramValues != null) && (paramValues.Length > 0))
                {

                    if ((this.DataSetting.InputParamNames.Length != this.DataSetting.InputParamTypes.Length) || (this.DataSetting.InputParamNames.Length != paramValues.Length))
                    {
                        throw new ArgumentOutOfRangeException("The value passed for paramValues is outside the range for the value of DataSetting.InputParamNames.");
                    }

                    this._Command.Parameters.AddRange(this.DataSetting.ToInputParameterArray(paramValues));

                }

                return this._Command.ExecuteReader();

            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                OnExecutionError(new SqlAbortArgs(ex, false));
                return null;
            }
            catch (Exception ex)
            {
                OnExecutionError(new SqlAbortArgs(ex, false));
                return null;
            }

        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Protected Methods
        protected virtual void OnExecutionError(SqlAbortArgs e)
        {
            SqlExceptionEventHandler handler = this.ExecutionError;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void Dispose(bool disposing)
        {

            if (disposing)
            {
                if (this._Transaction != null) { this._Transaction.Dispose(); }
                if (this._Command != null) { this._Command.Dispose(); }
                if (this._Connection != null) { this._Connection.Dispose(); }
            }

        }
        #endregion

    }

    /// <summary>
    /// Used as the base class for all SQL execution commands
    /// </summary>
    /// <typeparam name="T">Type returned by the ExecuteCommand method defined in interface ICommand</typeparam>
    public abstract class SqlExecutionCommandBase<T> : ISqlExecutionCommand<T>
    {

        #region Local Variables
        /// <summary>
        /// Read only member instance used for returning the value of property Setting
        /// </summary>
        protected readonly IDataSetting _Setting;
        /// <summary>
        /// Read only member instance used for returning the value of property HasParameters
        /// </summary>
        protected readonly bool _HasParameters;
        /// <summary>
        /// Read only member instance used for returning the value of property ParamValues
        /// </summary>
        protected readonly object[] _ParamValues;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of classes derived from SqlExecutionCommandBase
        /// </summary>
        /// <param name="setting">The IDataSetting object returned by property Setting</param>
        /// <param name="hasParameters">The boolean value returned by property HasParameters</param>
        /// <param name="paramValues">The array of object returned by property ParamValues</param>
        public SqlExecutionCommandBase(IDataSetting setting, bool hasParameters, object[] paramValues)
        {
            this._Setting = setting;
            this._HasParameters = hasParameters;
            this._ParamValues = paramValues;
        }
        #endregion

        #region Events
        public event SqlExceptionEventHandler ExecutionError;
        #endregion

        #region Properties
        /// <summary>
        /// Not implemented: Throws an exception of type NotImplementedException
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public ICommandString CommandString
        {
            get { throw new NotImplementedException("The property is not implemented in this class."); }
            set { throw new NotImplementedException("The property is not implemented in this class."); }
        }

        /// <summary>
        /// Gets the IDataSetting object
        /// </summary>
        public IDataSetting Setting
        {
            get { return this._Setting; }
        }

        /// <summary>
        /// Gets a value indicating whether the ParamValues is initialized and the length is greater than zero
        /// </summary>
        public bool HasParameters
        {
            get { return this._HasParameters; }
        }

        /// <summary>
        /// Gets an array of object containing the input parameter values
        /// </summary>
        public object[] ParamValues
        {
            get { return this._ParamValues; }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Abstract method for executing the command
        /// </summary>
        /// <returns>An instance of T</returns>
        public abstract T ExecuteCommand();

        /// <summary>
        /// 
        /// </summary>
        /// <returns>System.String</returns>
        public override string ToString()
        {
            return string.Format("{0} (Has Params: {1}, Param Count: {2})", this.GetType().Name, (this._HasParameters ? "Yes" : "No"), (this._ParamValues != null ? this._ParamValues.Count() : 0));
        }
        #endregion

        #region Protected Methods
        protected virtual void OnExecutionError(SqlAbortArgs e)
        {
            SqlExceptionEventHandler handler = this.ExecutionError;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion

    }

    public class SqlExecutionCommandResult<T> : ICommonObject
    {

        #region Local Variables
        protected readonly List<T> _Items;
        #endregion

        #region Constructors
        public SqlExecutionCommandResult()
        {
            this._Items = new List<T>();
        }

        public SqlExecutionCommandResult(SqlCommandResultArgs result, string message)
        {
            this.Result = result;
            this.Message = message;
            this._Items = new List<T>();
        }
        #endregion

        #region Properties
        public SqlCommandResultArgs Result { get; set; }
        public string Message { get; set; }
        public List<T> Items
        {
            get { return this._Items; }
        }
        #endregion

        #region Public Methods
        public object[] ToArray()
        {
            return new object[] { (int)this.Result, this.Message };
        }
        #endregion

    }

    public class SqlPutCommand : SqlExecutionCommandBase<SqlExecutionCommandResult<object>>
    {

        #region Local Variables
        protected readonly SqlExecutionCommandResult<object> result;
        #endregion

        #region Constructor
        public SqlPutCommand(IDataSetting setting, object[] paramValues)
            : base(setting, true, paramValues)
        {
            this.result = new SqlExecutionCommandResult<object>();
        }
        #endregion

        #region Public Methods
        public override SqlExecutionCommandResult<object> ExecuteCommand()
        {
            SqlConnectionObject conn = new SqlConnectionObject(this.Setting);

            conn.ExecutionError += this.SqlException;

            try
            {
                bool result = false;

                using (conn)
                {
                    conn.Open();
                    conn.BeginTransaction();
                    result = conn.ExecuteNonQuery(this.ParamValues);
                    conn.EndTransaction(result);
                }

                if (result)
                {

                    if (this.Setting.OutputParamNames.Length > 0)
                    {
                        foreach (string key in this.Setting.OutputParamNames)
                        {
                            this.result.Items.Add(conn.ReturnValues[key]);
                        }
                    }

                    this.result.Result = SqlCommandResultArgs.Successful;
                    this.result.Message = "Successfully Saved";
                }
                else
                {
                    this.result.Result = SqlCommandResultArgs.Failed;
                    this.result.Message = "Unable to Save";

                }

            }
            catch (Exception e)
            {
                this.result.Result = SqlCommandResultArgs.Error;
                this.result.Message = e.Message;
            }

            return this.result;

        }
        #endregion

        #region Protected Methods
        protected virtual void SqlException(object sender, ISqlAbortArgs e)
        {
            this.result.Result = SqlCommandResultArgs.Error;
            this.result.Message = e.Exception.Message;

            e.RollbackChanges = e.RollbackAllowed;

            this.OnExecutionError((SqlAbortArgs)e);

        }
        #endregion

    }

    public class SqlGetCommand<T> : SqlExecutionCommandBase<SqlExecutionCommandResult<T>> where T : ICommonObject, new()
    {

        #region Local Variables
        public readonly List<T> items;
        #endregion

        #region Constructor
        public SqlGetCommand(IDataSetting setting, bool hasParameters, object[] paramValues)
            : base(setting, hasParameters, paramValues)
        {
            this.items = new List<T>();
        }
        #endregion

        #region Public Methods
        public override SqlExecutionCommandResult<T> ExecuteCommand()
        {
            SqlExecutionCommandResult<T> items = new SqlExecutionCommandResult<T>();

            SqlConnectionObject conn = new SqlConnectionObject(this.Setting);

            conn.ExecutionError += this.SqlException;

            try
            {
                System.Data.DataSet ds = new System.Data.DataSet();

                using (conn)
                {
                    System.Data.DataTable dt = null;

                    conn.Open();

                    if (this.HasParameters)
                    {
                        if (conn.ExecuteQuery(this.ParamValues))
                        {
                            dt = conn.Tables[0];
                        }
                    }
                    else
                    {
                        if (conn.ExecuteQuery())
                        {
                            dt = conn.Tables[0];
                        }
                    }

                    if (dt != null)
                    {
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            T obj = Krodzone.CreateObject<T>(row.ItemArray);

                            items.Items.Add(obj);

                        }
                    }

                }

            }
            catch (Exception e)
            {
                items = new SqlExecutionCommandResult<T>();
            }

            return items;

        }
        #endregion

        #region Protected Methods
        protected virtual void SqlException(object sender, ISqlAbortArgs e)
        {

            e.RollbackChanges = e.RollbackAllowed;

            this.OnExecutionError((SqlAbortArgs)e);

        }
        #endregion

    }

    public static class DatabaseConnector
    {

        #region Public Static Methods
        public static SqlExecutionCommandResult<object> SaveData(IDataSetting setting, object[] values)
        {
            KrodzoneDataProvider provider = new KrodzoneDataProvider();
            ISqlExecutionCommand<SqlExecutionCommandResult<object>> cmd = new SqlPutCommand(setting, values);

            return provider.ExecuteCommand<ISqlExecutionCommand<SqlExecutionCommandResult<object>>, SqlExecutionCommandResult<object>>(cmd);

        }

        public static Task<SqlExecutionCommandResult<object>> SaveDataAsync(IDataSetting setting, object[] values)
        {
            KrodzoneDataProvider provider = new KrodzoneDataProvider();
            ISqlExecutionCommand<SqlExecutionCommandResult<object>> cmd = new SqlPutCommand(setting, values);

            SqlExecutionCommandResult<object> result = provider.ExecuteCommand<ISqlExecutionCommand<SqlExecutionCommandResult<object>>, SqlExecutionCommandResult<object>>(cmd);

            return Task.FromResult<SqlExecutionCommandResult<object>>(result);

        }

        public static List<T> GetData<T>(ISqlExecutionCommand<SqlExecutionCommandResult<T>> cmd)
            where T : ICommonObject, new()
        {
            //  This should be put in a single method that takes an array of object as it's only param
            KrodzoneDataProvider provider = new KrodzoneDataProvider();
            SqlExecutionCommandResult<T> result = provider.ExecuteCommand<ISqlExecutionCommand<SqlExecutionCommandResult<T>>, SqlExecutionCommandResult<T>>(cmd);

            return result.Items;

        }

        public static Task<List<T>> GetDataAsync<T>(ISqlExecutionCommand<SqlExecutionCommandResult<T>> cmd)
            where T : ICommonObject, new()
        {
            //  This should be put in a single method that takes an array of object as it's only param
            KrodzoneDataProvider provider = new KrodzoneDataProvider();
            SqlExecutionCommandResult<T> result = provider.ExecuteCommand<ISqlExecutionCommand<SqlExecutionCommandResult<T>>, SqlExecutionCommandResult<T>>(cmd);

            return Task.FromResult<List<T>>(result.Items);

        }
        #endregion

    }

    public class KrodzoneDataProvider : ICommandExecuter
    {

        #region Constructor
        public KrodzoneDataProvider() { }
        #endregion

        #region Public Methods
        public W ExecuteCommand<T, W>(T obj)
            where T : ICommand<W>
            where W : new()
        {
            W value = default(W);
            System.Reflection.MethodInfo[] methods = obj.GetType().GetMethods();

            try
            {
                System.Reflection.MethodInfo method = obj.GetType().GetMethod("ExecuteCommand");

                if (method != null)
                {
                    value = (W)method.Invoke(obj, null);
                }

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                value = default(W);
            }

            return value;

        }
        #endregion

    }

}
