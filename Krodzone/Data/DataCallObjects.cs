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
        /// <summary>
        /// Generates the code for an EmbeddedSettings class
        /// </summary>
        /// <param name="writer">System.IO.StringWriter used to write the output code to</param>
        /// <param name="classModifier">Access modifier for the EmbeddedSettings class (public | internal | protected | private)</param>
        void CreateEmbeddedSettingClass(StringWriter writer, string classModifier);
        /// <summary>
        /// Generates the code for an EmbeddedSettings class
        /// </summary>
        /// <param name="writer">System.IO.StringWriter used to write the output code to</param>
        /// <param name="classModifier">Access modifier for the EmbeddedSettings class (public | internal | protected | private)</param>
        /// <param name="classNamespace">The namespace the EmbeddedSettings class should be placed</param>
        void CreateEmbeddedSettingClass(StringWriter writer, string classModifier, string classNamespace);
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
        SqlCredential Credential { get; }
        IDataSetting DataSetting { get; set; }
        DataTable[] Tables { get; }
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
        IDataReader GetReader();
        IDataReader GetReader(object[] paramValues);
        #endregion

    }

    /// <summary>
    /// Used to create objects containing connection and procedure information when connecting to the database
    /// </summary>
    public class DataSetting : IDataSetting
    {
        
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
            ConnectionString = connectionString;
            ProcedureName = procedureName;
            InputParamNames = inputParamNames;
            InputParamTypes = inputParamTypes;
            OutputParamNames = outputParamNames;
            OutputParamTypes = outputParamTypes;
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

            if ((InputParamNames == null) || (InputParamTypes == null) || (paramValues == null))
            {
                throw new InvalidOperationException("Input Parameter Names, Types, and Values must be valid arrays with equal length!");
            }

            if ((InputParamNames.Length != InputParamTypes.Length) || (InputParamNames.Length != paramValues.Length))
            {
                throw new InvalidOperationException("Input Parameter Names, Types, and Values must be valid arrays with equal length!");
            }

            for (int i = 0; i < InputParamNames.Length; i++)
            {
                SqlParameter param = new SqlParameter()
                {
                    ParameterName = InputParamNames[i],
                    SqlDbType = InputParamTypes[i],
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

            if ((OutputParamNames == null) || (OutputParamTypes == null))
            {
                throw new InvalidOperationException("Output Parameter Names and Types must be valid arrays with equal length!");
            }

            if ((OutputParamNames.Length != OutputParamTypes.Length))
            {
                throw new InvalidOperationException("Output Parameter Names and Types must be valid arrays with equal length!");
            }

            for (int i = 0; i < OutputParamNames.Length; i++)
            {
                SqlParameter param = new SqlParameter()
                {
                    ParameterName = OutputParamNames[i],
                    SqlDbType = OutputParamTypes[i],
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
        protected readonly ISqlObjectCommand<List<DataSettingCreationParameter>> Command;

        private List<DataSettingCreationParameter> _parameterData;
        protected readonly string ConfigSettingName;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of DataSettingFactory
        /// </summary>
        /// <param name="connectionString">The connection string to use for retrieving the stored procedure data</param>
        /// <param name="configSettingName">The Config Setting name that references your connection string</param>
        public DataSettingFactory(string connectionString, string configSettingName)
        {
            CommandString = new DataSettingCreationString();
            Command = new DataSettingCreationCommand(CommandString, connectionString);
            _parameterData = new List<DataSettingCreationParameter>();
            ConfigSettingName = configSettingName;
        }

        ~DataSettingFactory()
        {
            Dispose(false);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the ICommandString to be executed
        /// </summary>
        public ICommandString CommandString { get; private set; }

        #endregion

        #region Public Methods
        /// <summary>
        /// Generates the code for an EmbeddedSettings class
        /// </summary>
        /// <param name="writer">System.IO.StringWriter used to write the output code to</param>
        public virtual void CreateEmbeddedSettingClass(StringWriter writer)
        {
            _parameterData = Command.ExecuteCommand();

            if ((writer != null) && (_parameterData.Count > 0))
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
                writer.WriteLine("\t\t\treturn ConfigurationManager.ConnectionStrings[\"{0}\"].ToString();", ConfigSettingName);
                writer.WriteLine("\t\t}");
                writer.WriteLine("\t}");
                writer.WriteLine("\t#endregion");

                writer.WriteLine("");

                //  Create DataSetting Objects
                writer.WriteLine("\t#region DataSetting Objects");

                List<string> schemas = (from obj in _parameterData select obj.SchemaName).Distinct().ToList();

                foreach (string schema in schemas)
                {
                    writer.WriteLine("\r\n\t#region DataSettings for Schema {0}", schema);
                    writer.WriteLine("\tpublic static class {0}", schema);
                    writer.WriteLine("\t{");
                    writer.WriteLine("\r\n\t\t#region Properties");

                    List<int> objectIDs = (from obj in _parameterData where obj.SchemaName == schema select obj.ObjectID).Distinct().ToList();

                    for (int i = 0; i < objectIDs.Count; i++)
                    {
                        int objId = objectIDs[i];
                        List<DataSettingCreationParameter> parms = (from obj in _parameterData where obj.ObjectID == objId select obj).ToList();

                        writer.WriteLine("\t\tpublic static IDataSetting {0}", parms[0].ProcedureName);
                        writer.WriteLine("\t\t{");
                        writer.WriteLine("\t\t\tget");
                        writer.WriteLine("\t\t\t{");

                        if (string.IsNullOrEmpty(parms[0].ParameterName))   //  There are no parameters
                        {
                            writer.WriteLine("\t\t\t\treturn new DataSetting(EmbeddedSettings.ConnectionString, \"{0}.{1}\", new string[] {2}, new SqlDbType[] {2}, new string[] {2}, new SqlDbType[] {2});", parms[0].SchemaName, parms[0].ProcedureName, "{ }");
                        }
                        else
                        {
                            StringBuilder sbInNames = new StringBuilder("string[] inputParmNames = new string[] { ");
                            StringBuilder sbInTypes = new StringBuilder("SqlDbType[] inputParmTypes = new SqlDbType[] { ");
                            StringBuilder sbOutNames = new StringBuilder("string[] outputParmNames = new string[] { ");
                            StringBuilder sbOutTypes = new StringBuilder("SqlDbType[] outputParmTypes = new SqlDbType[] { ");

                            List<DataSettingCreationParameter> inputParms = (from obj in parms where obj.IsOutParam == false select obj).ToList();
                            List<DataSettingCreationParameter> outputParms = (from obj in parms where obj.IsOutParam select obj).ToList();

                            for (int j = 0; j < inputParms.Count; j++)
                            {
                                sbInNames.Append($"{(j == 0 ? "" : ", ")}\"{inputParms[j].ParameterName}\"");
                                sbInTypes.Append($"{(j == 0 ? "" : ", ")}(SqlDbType){inputParms[j].CSTypeValue}");
                            }

                            for (int j = 0; j < outputParms.Count; j++)
                            {
                                sbOutNames.Append($"{(j == 0 ? "" : ", ")}\"{outputParms[j].ParameterName}\"");
                                sbOutTypes.Append($"{(j == 0 ? "" : ", ")}(SqlDbType){outputParms[j].CSTypeValue}");
                            }

                            sbInNames.Append(" };");
                            sbInTypes.Append(" };");
                            sbOutNames.Append(" };");
                            sbOutTypes.Append(" };");

                            writer.WriteLine("\t\t\t\t{0}", sbInNames);
                            writer.WriteLine("\t\t\t\t{0}", sbInTypes);
                            writer.WriteLine("\t\t\t\t{0}", sbOutNames);
                            writer.WriteLine("\t\t\t\t{0}", sbOutTypes);
                            writer.WriteLine("");
                            writer.WriteLine("\t\t\t\treturn new DataSetting(EmbeddedSettings.ConnectionString, \"{0}.{1}\", inputParmNames, inputParmTypes, outputParmNames, outputParmTypes);", parms[0].SchemaName, parms[0].ProcedureName);

                        }

                        writer.WriteLine("\t\t\t}");
                        writer.WriteLine("\t\t}");
                        writer.Write("{0}", (i < (objectIDs.Count - 1) ? "\r\n" : ""));

                    }


                    writer.WriteLine("\t\t#endregion\r\n");
                    writer.WriteLine("\t}");
                    writer.WriteLine("\t#endregion");

                }


                writer.WriteLine("\r\n\t#endregion");

                writer.WriteLine("");
                writer.WriteLine("}");

            }

        }

        /// <summary>
        /// Generates the code for an EmbeddedSettings class
        /// </summary>
        /// <param name="writer">System.IO.StringWriter used to write the output code to</param>
        /// <param name="classModifier">Access modifier for the EmbeddedSettings class (public | internal | protected | private)</param>
        public virtual void CreateEmbeddedSettingClass(StringWriter writer, string classModifier)
        {
            _parameterData = Command.ExecuteCommand();

            if ((writer != null) && (_parameterData.Count > 0))
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
                writer.WriteLine("{0} class EmbeddedSettings", classModifier);
                writer.WriteLine("{");
                writer.WriteLine("");

                //  Connection String Property
                writer.WriteLine("\t#region Connection String");
                writer.WriteLine("\tpublic static string ConnectionString");
                writer.WriteLine("\t{");
                writer.WriteLine("\t\tget");
                writer.WriteLine("\t\t{");
                writer.WriteLine("\t\t\treturn ConfigurationManager.ConnectionStrings[\"{0}\"].ToString();", ConfigSettingName);
                writer.WriteLine("\t\t}");
                writer.WriteLine("\t}");
                writer.WriteLine("\t#endregion");

                writer.WriteLine("");

                //  Create DataSetting Objects
                writer.WriteLine("\t#region DataSetting Objects");

                List<string> schemas = (from obj in _parameterData select obj.SchemaName).Distinct().ToList();

                foreach (string schema in schemas)
                {
                    writer.WriteLine("\r\n\t#region DataSettings for Schema {0}", schema);
                    writer.WriteLine("\tpublic static class {0}", schema);
                    writer.WriteLine("\t{");
                    writer.WriteLine("\r\n\t\t#region Properties");

                    List<int> objectIDs = (from obj in _parameterData where obj.SchemaName == schema select obj.ObjectID).Distinct().ToList();

                    for (int i = 0; i < objectIDs.Count; i++)
                    {
                        int objId = objectIDs[i];
                        List<DataSettingCreationParameter> parms = (from obj in _parameterData where obj.ObjectID == objId select obj).ToList();

                        writer.WriteLine("\t\tpublic static IDataSetting {0}", parms[0].ProcedureName);
                        writer.WriteLine("\t\t{");
                        writer.WriteLine("\t\t\tget");
                        writer.WriteLine("\t\t\t{");

                        if (string.IsNullOrEmpty(parms[0].ParameterName))   //  There are no parameters
                        {
                            writer.WriteLine("\t\t\t\treturn new DataSetting(EmbeddedSettings.ConnectionString, \"{0}.{1}\", new string[] {2}, new SqlDbType[] {2}, new string[] {2}, new SqlDbType[] {2});", parms[0].SchemaName, parms[0].ProcedureName, "{ }");
                        }
                        else
                        {
                            StringBuilder sbInNames = new StringBuilder("string[] inputParmNames = new string[] { ");
                            StringBuilder sbInTypes = new StringBuilder("SqlDbType[] inputParmTypes = new SqlDbType[] { ");
                            StringBuilder sbOutNames = new StringBuilder("string[] outputParmNames = new string[] { ");
                            StringBuilder sbOutTypes = new StringBuilder("SqlDbType[] outputParmTypes = new SqlDbType[] { ");

                            List<DataSettingCreationParameter> inputParms = (from obj in parms where obj.IsOutParam == false select obj).ToList();
                            List<DataSettingCreationParameter> outputParms = (from obj in parms where obj.IsOutParam select obj).ToList();

                            for (int j = 0; j < inputParms.Count; j++)
                            {
                                sbInNames.Append($"{(j == 0 ? "" : ", ")}\"{inputParms[j].ParameterName}\"");
                                sbInTypes.Append($"{(j == 0 ? "" : ", ")}(SqlDbType){inputParms[j].CSTypeValue}");
                            }

                            for (int j = 0; j < outputParms.Count; j++)
                            {
                                sbOutNames.Append($"{(j == 0 ? "" : ", ")}\"{outputParms[j].ParameterName}\"");
                                sbOutTypes.Append($"{(j == 0 ? "" : ", ")}(SqlDbType){outputParms[j].CSTypeValue}");
                            }

                            sbInNames.Append(" };");
                            sbInTypes.Append(" };");
                            sbOutNames.Append(" };");
                            sbOutTypes.Append(" };");

                            writer.WriteLine("\t\t\t\t{0}", sbInNames);
                            writer.WriteLine("\t\t\t\t{0}", sbInTypes);
                            writer.WriteLine("\t\t\t\t{0}", sbOutNames);
                            writer.WriteLine("\t\t\t\t{0}", sbOutTypes);
                            writer.WriteLine("");
                            writer.WriteLine("\t\t\t\treturn new DataSetting(EmbeddedSettings.ConnectionString, \"{0}.{1}\", inputParmNames, inputParmTypes, outputParmNames, outputParmTypes);", parms[0].SchemaName, parms[0].ProcedureName);

                        }

                        writer.WriteLine("\t\t\t}");
                        writer.WriteLine("\t\t}");
                        writer.Write("{0}", (i < (objectIDs.Count - 1) ? "\r\n" : ""));

                    }


                    writer.WriteLine("\t\t#endregion\r\n");
                    writer.WriteLine("\t}");
                    writer.WriteLine("\t#endregion");

                }


                writer.WriteLine("\r\n\t#endregion");

                writer.WriteLine("");
                writer.WriteLine("}");

            }

        }

        /// <summary>
        /// Generates the code for an EmbeddedSettings class
        /// </summary>
        /// <param name="writer">System.IO.StringWriter used to write the output code to</param>
        /// <param name="classModifier">Access modifier for the EmbeddedSettings class (public | internal | protected | private)</param>
        /// <param name="classNamespace">The namespace the EmbeddedSettings class should be placed</param>
        public virtual void CreateEmbeddedSettingClass(StringWriter writer, string classModifier, string classNamespace)
        {
            _parameterData = Command.ExecuteCommand();

            if ((writer != null) && (_parameterData.Count > 0))
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

                //  Namespace Declaration
                writer.WriteLine("namespace {0}", classNamespace);
                writer.WriteLine("{");
                writer.WriteLine("");

                //  Class Declaration
                writer.WriteLine("\t[Serializable]");
                writer.WriteLine("\t{0} class EmbeddedSettings", classModifier);
                writer.WriteLine("\t{");
                writer.WriteLine("");

                //  Connection String Property
                writer.WriteLine("\t\t#region Connection String");
                writer.WriteLine("\t\tpublic static string ConnectionString");
                writer.WriteLine("\t\t{");
                writer.WriteLine("\t\t\tget");
                writer.WriteLine("\t\t\t{");
                writer.WriteLine("\t\t\t\treturn ConfigurationManager.ConnectionStrings[\"{0}\"].ToString();", ConfigSettingName);
                writer.WriteLine("\t\t\t}");
                writer.WriteLine("\t\t}");
                writer.WriteLine("\t\t#endregion");

                writer.WriteLine("");

                //  Create DataSetting Objects
                writer.WriteLine("\t\t#region DataSetting Objects");

                List<string> schemas = (from obj in _parameterData select obj.SchemaName).Distinct().ToList();

                foreach (string schema in schemas)
                {
                    writer.WriteLine("\r\n\t\t#region DataSettings for Schema {0}", schema);
                    writer.WriteLine("\t\tpublic static class {0}", schema);
                    writer.WriteLine("\t\t{");
                    writer.WriteLine("\r\n\t\t\t#region Properties");

                    List<int> objectIDs = (from obj in _parameterData where obj.SchemaName == schema select obj.ObjectID).Distinct().ToList();

                    for (int i = 0; i < objectIDs.Count; i++)
                    {
                        int objId = objectIDs[i];
                        List<DataSettingCreationParameter> parms = (from obj in _parameterData where obj.ObjectID == objId select obj).ToList();

                        writer.WriteLine("\t\t\tpublic static IDataSetting {0}", parms[0].ProcedureName);
                        writer.WriteLine("\t\t\t{");
                        writer.WriteLine("\t\t\t\tget");
                        writer.WriteLine("\t\t\t\t{");

                        if (string.IsNullOrEmpty(parms[0].ParameterName))   //  There are no parameters
                        {
                            writer.WriteLine("\t\t\t\t\treturn new DataSetting(EmbeddedSettings.ConnectionString, \"{0}.{1}\", new string[] {2}, new SqlDbType[] {2}, new string[] {2}, new SqlDbType[] {2});", parms[0].SchemaName, parms[0].ProcedureName, "{ }");
                        }
                        else
                        {
                            StringBuilder sbInNames = new StringBuilder("string[] inputParmNames = new string[] { ");
                            StringBuilder sbInTypes = new StringBuilder("SqlDbType[] inputParmTypes = new SqlDbType[] { ");
                            StringBuilder sbOutNames = new StringBuilder("string[] outputParmNames = new string[] { ");
                            StringBuilder sbOutTypes = new StringBuilder("SqlDbType[] outputParmTypes = new SqlDbType[] { ");

                            List<DataSettingCreationParameter> inputParms = (from obj in parms where obj.IsOutParam == false select obj).ToList();
                            List<DataSettingCreationParameter> outputParms = (from obj in parms where obj.IsOutParam select obj).ToList();

                            for (int j = 0; j < inputParms.Count; j++)
                            {
                                sbInNames.Append($"{(j == 0 ? "" : ", ")}\"{inputParms[j].ParameterName}\"");
                                sbInTypes.Append($"{(j == 0 ? "" : ", ")}(SqlDbType){inputParms[j].CSTypeValue}");
                            }

                            for (int j = 0; j < outputParms.Count; j++)
                            {
                                sbOutNames.Append($"{(j == 0 ? "" : ", ")}\"{outputParms[j].ParameterName}\"");
                                sbOutTypes.Append($"{(j == 0 ? "" : ", ")}(SqlDbType){outputParms[j].CSTypeValue}");
                            }

                            sbInNames.Append(" };");
                            sbInTypes.Append(" };");
                            sbOutNames.Append(" };");
                            sbOutTypes.Append(" };");

                            writer.WriteLine("\t\t\t\t\t{0}", sbInNames);
                            writer.WriteLine("\t\t\t\t\t{0}", sbInTypes);
                            writer.WriteLine("\t\t\t\t\t{0}", sbOutNames);
                            writer.WriteLine("\t\t\t\t\t{0}", sbOutTypes);
                            writer.WriteLine("");
                            writer.WriteLine("\t\t\t\t\treturn new DataSetting(EmbeddedSettings.ConnectionString, \"{0}.{1}\", inputParmNames, inputParmTypes, outputParmNames, outputParmTypes);", parms[0].SchemaName, parms[0].ProcedureName);

                        }

                        writer.WriteLine("\t\t\t\t}");
                        writer.WriteLine("\t\t\t}");
                        writer.Write("{0}", (i < (objectIDs.Count - 1) ? "\r\n" : ""));

                    }


                    writer.WriteLine("\t\t\t#endregion\r\n");
                    writer.WriteLine("\t\t}");
                    writer.WriteLine("\t\t#endregion");

                }


                writer.WriteLine("\r\n\t\t#endregion");

                writer.WriteLine("");
                writer.WriteLine("\t}");

                writer.WriteLine("");
                writer.WriteLine("}");

            }

        }

        /// <summary>
        /// Disposes of the object
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
                CommandString = null;
                Command.Dispose();
                _parameterData = null;
            }

        }
        #endregion

        #region Nested Classes
        /// <summary>
        /// Used to hold the stored procedure data necessary for creating DataSetting objects
        /// </summary>
        protected class DataSettingCreationParameter : ICommonObject
        {
            
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
                return new object[] { ObjectID, SchemaName, ProcedureName, ParameterName, CSTypeValue, IsOutParam };
            }
            #endregion

        }

        /// <summary>
        /// Implements ICommandString for the purpose of creating a command to return the stored procedure information used in creating DataSetting objects
        /// </summary>
        protected class DataSettingCreationString : ICommandString
        {
            
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
                sb.Append("\r\n\t,\tCSTypeName varchar(100)");
                sb.Append("\r\n\t,\tCSTypeValue int");
                sb.Append(")\r\n");

                sb.Append("\r\nINSERT INTO @SqlTypes");
                sb.Append("\r\nSELECT DISTINCT");
                sb.Append("\r\n\t\tsystem_type_id AS SystemTypeID");
                sb.Append("\r\n\t,\tCASE");
                sb.Append("\r\n\t\t\tWHEN system_type_id = 243 THEN 'UserType'");
                sb.Append("\r\n\t\t\tELSE name");
                sb.Append("\r\n\t\tEND AS SystemTypeName");
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
                sb.Append("\r\n\t\t\tELSE 'SqlDbType.Structured'");
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
                sb.Append("\r\n\t\t\tELSE 30");
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
            protected SqlConnection sqlConnection;

            private string _connectionString;
            #endregion

            #region Constructor
            /// <summary>
            /// Creates a new instance of ISQlObjectCommand
            /// </summary>
            /// <param name="commandString"></param>
            /// <param name="connectionString"></param>
            public DataSettingCreationCommand(ICommandString commandString, string connectionString)
            {
                CommandString = commandString;
                _connectionString = connectionString;
                sqlConnection = new SqlConnection(_connectionString);
            }

            /// <summary>
            /// Destroys the instance of the SqlObjectCommandBase class
            /// </summary>
            ~DataSettingCreationCommand()
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
                get { return _connectionString; }
                set
                {
                    _connectionString = value;
                    sqlConnection = new SqlConnection(_connectionString);
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
            public SqlCredential Credential => sqlConnection.Credential;

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

                    if (sqlConnection != null)
                    {
                        sqlConnection.Open();

                        SqlCommand cmd = new SqlCommand()
                        {
                            Connection = sqlConnection,
                            CommandText = CommandString.GetCommandString(),
                            CommandType = CommandType.Text,
                            CommandTimeout = ConnectionTimeout
                        };

                        DataTable dt = new DataTable();
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                        adapter.Fill(dt);

                        results.AddRange(from DataRow row in dt.Rows
                            select new DataSettingCreationParameter
                            {
                                ObjectID = (int) row["ObjectID"], SchemaName = (string) row["SchemaName"], ProcedureName = (string) row["ProcedureName"], ParameterName = (string) row["ParameterName"], CSTypeValue = (int) row["CSTypeValue"], IsOutParam = (bool) row["IsOutParam"]
                            });
                    }

                }
                catch
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
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            #endregion

            #region Protected Methods
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "sqlConnection")]
            protected virtual void Dispose(bool disposing)
            {
                if (!disposing) return;
                sqlConnection?.Dispose();
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
            _Exception = exception;
            _RollbackAllowed = rollbackAllowed;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the Exception that was thrown
        /// </summary>
        public Exception Exception => _Exception;

        /// <summary>
        /// Gets a value indicating if a transaction was started prior to beginning execution
        /// </summary>
        public bool RollbackAllowed => _RollbackAllowed;

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
            _Items = new Dictionary<string, object>();
        }
        #endregion

        #region Properties
        public object this[string key]
        {
            get
            {
                if (!_Items.ContainsKey(key))
                {
                    return null;
                }
                else
                {
                    return _Items[key];
                }
            }
        }
        #endregion

        #region Public Methods
        public void Add(string key, object value)
        {
            if (_Items.ContainsKey(key))
            {
                _Items[key] = value;
            }
            else
            {
                _Items.Add(key, value);
            }
        }

        public object[] ToArray()
        {
            return _Items.Values.ToArray();
        }
        #endregion

    }


    public class SqlConnectionObject : ISqlConnectionObject
    {

        #region Local Variables
        private SqlConnection _Connection;
        private SqlTransaction _Transaction;
        private SqlCommand _Command;

        protected readonly string _ConnectionString;

        private bool transactionInitialized;
        #endregion

        #region Constructor
        public SqlConnectionObject(IDataSetting dataSetting)
        {
            DataSetting = dataSetting;
            _ConnectionString = dataSetting.ConnectionString;
        }

        ~SqlConnectionObject()
        {
            Dispose(false);
        }
        #endregion

        #region Events
        public event SqlExceptionEventHandler ExecutionError;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the connection string to use when connecting to the database
        /// </summary>
        public string ConnectionString => _ConnectionString;

        /// <summary>
        /// Gets the server used in the connection
        /// </summary>
        public string Server => _Connection.DataSource;

        /// <summary>
        /// Gets the database used in the connection
        /// </summary>
        public string Database => _Connection.Database;

        /// <summary>
        /// Gets the connection timeout used in the connection
        /// </summary>
        public int ConnectionTimeout => _Connection.ConnectionTimeout;

        /// <summary>
        /// Gets the System.Data.SqlClient.SqlCredential used in the connection
        /// </summary>
        public SqlCredential Credential => _Connection.Credential;

        public IDataSetting DataSetting { get; set; }

        public DataTable[] Tables { get; private set; }

        public bool ConnectionOpen { get; private set; }

        public IValueCollection ReturnValues { get; private set; }

        #endregion

        #region Public Methods
        public virtual void Open()
        {

            try
            {
                if (ConnectionOpen) { Close(); }

                if (DataSetting == null)
                {
                    throw new InvalidOperationException("The DataSetting object is not valid for the current operation.");
                }

                _Connection = new SqlConnection(DataSetting.ConnectionString);
                _Connection.Open();
                ConnectionOpen = true;

            }
            catch (Exception ex)
            {
                OnExecutionError(new SqlAbortArgs(ex, false));
            }

        }

        public virtual void Close()
        {

            try
            {
                if (_Connection != null)
                {
                    _Connection.Dispose();
                    ConnectionOpen = false;
                }
            }
            catch (Exception ex)
            {
                ConnectionOpen = false;
                _Connection = null;
                OnExecutionError(new SqlAbortArgs(ex, false));
            }

        }

        public virtual void BeginTransaction()
        {

            try
            {

                if ((transactionInitialized) || (_Transaction != null))
                {
                    throw new InvalidOperationException("Cannot begin a new transaction without first completing the previous transaction.");
                }

                if (DataSetting == null)
                {
                    throw new InvalidOperationException("DataSetting cannot be null when starting a new transaction.");
                }

                if (_Connection.State != ConnectionState.Open) { Open(); }

                _Transaction = _Connection.BeginTransaction();
                transactionInitialized = true;

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

                if ((transactionInitialized) && (_Transaction != null))
                {
                    if (commitChanges)
                    {
                        _Transaction.Commit();
                        transactionInitialized = false;
                    }
                    else
                    {
                        _Transaction.Rollback();
                        transactionInitialized = false;
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
            bool result;

            try
            {

                if (DataSetting == null)
                {
                    throw new InvalidOperationException("DataSetting cannot be null when executing a query.");
                }

                if (!ConnectionOpen) { Open(); }

                _Command = new SqlCommand()
                {
                    CommandText = DataSetting.ProcedureName,
                    CommandType = CommandType.StoredProcedure,
                    Connection = _Connection,
                    CommandTimeout = (_Connection.ConnectionTimeout < 1000 ? 1000 : _Connection.ConnectionTimeout)
                };

                if (DataSetting.OutputParamNames.Length > 0) { _Command.Parameters.AddRange(DataSetting.ToOutputParameterArray()); }

                ReturnValues = new ReturnValueCollection();

                DataSet ds = new DataSet();
                SqlDataAdapter adapter = new SqlDataAdapter(_Command);

                adapter.Fill(ds);

                Tables = new DataTable[ds.Tables.Count];
                ds.Tables.CopyTo(Tables, 0);

                foreach (string parmName in DataSetting.OutputParamNames)
                {
                    ReturnValues.Add(parmName, _Command.Parameters[parmName].Value);
                }

                result = true;

            }
            catch (SqlException ex)
            {
                OnExecutionError(new SqlAbortArgs(ex, transactionInitialized));
                result = false;
            }
            catch (Exception ex)
            {
                OnExecutionError(new SqlAbortArgs(ex, transactionInitialized));
                result = false;
            }

            return result;

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public virtual bool ExecuteQuery(object[] paramValues)
        {
            bool result;

            try
            {

                if (DataSetting == null)
                {
                    throw new InvalidOperationException("DataSetting cannot be null when executing a query.");
                }

                if (!ConnectionOpen) { Open(); }

                _Command = new SqlCommand()
                {
                    CommandText = DataSetting.ProcedureName,
                    CommandType = CommandType.StoredProcedure,
                    Connection = _Connection,
                    CommandTimeout = (_Connection.ConnectionTimeout < 1000 ? 1000 : _Connection.ConnectionTimeout)
                };

                if ((paramValues != null) && (paramValues.Length > 0))
                {

                    if ((DataSetting.InputParamNames.Length != DataSetting.InputParamTypes.Length) || (DataSetting.InputParamNames.Length != paramValues.Length))
                    {
                        throw new ArgumentOutOfRangeException("The value passed for paramValues is outside the range for the value of DataSetting.InputParamNames.");
                    }

                    _Command.Parameters.AddRange(DataSetting.ToInputParameterArray(paramValues));

                }

                if (DataSetting.OutputParamNames.Length > 0) { _Command.Parameters.AddRange(DataSetting.ToOutputParameterArray()); }

                ReturnValues = new ReturnValueCollection();

                DataSet ds = new DataSet();
                SqlDataAdapter adapter = new SqlDataAdapter(_Command);

                adapter.Fill(ds);

                Tables = new DataTable[ds.Tables.Count];
                ds.Tables.CopyTo(Tables, 0);

                foreach (string parmName in DataSetting.OutputParamNames)
                {
                    ReturnValues.Add(parmName, _Command.Parameters[parmName].Value);
                }

                result = true;

            }
            catch (SqlException ex)
            {
                OnExecutionError(new SqlAbortArgs(ex, transactionInitialized));
                result = false;
            }
            catch (Exception ex)
            {
                OnExecutionError(new SqlAbortArgs(ex, transactionInitialized));
                result = false;
            }

            return result;

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public virtual bool ExecuteNonQuery()
        {
            bool result;

            try
            {

                if (DataSetting == null)
                {
                    throw new InvalidOperationException("DataSetting cannot be null when executing a query.");
                }

                if (!ConnectionOpen) { Open(); }

                _Command = new SqlCommand()
                {
                    CommandText = DataSetting.ProcedureName,
                    CommandType = CommandType.StoredProcedure,
                    Connection = _Connection,
                    CommandTimeout = (_Connection.ConnectionTimeout < 1000 ? 1000 : _Connection.ConnectionTimeout),
                    Transaction = _Transaction
                };

                if (DataSetting.OutputParamNames.Length > 0) { _Command.Parameters.AddRange(DataSetting.ToOutputParameterArray()); }

                ReturnValues = new ReturnValueCollection();

                _Command.ExecuteNonQuery();

                foreach (string parmName in DataSetting.OutputParamNames)
                {
                    ReturnValues.Add(parmName, _Command.Parameters[parmName].Value);
                }

                result = true;

            }
            catch (SqlException ex)
            {
                OnExecutionError(new SqlAbortArgs(ex, transactionInitialized));
                result = false;
            }
            catch (Exception ex)
            {
                OnExecutionError(new SqlAbortArgs(ex, transactionInitialized));
                result = false;
            }

            return result;

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public virtual bool ExecuteNonQuery(object[] paramValues)
        {
            bool result;

            try
            {

                if (DataSetting == null)
                {
                    throw new InvalidOperationException("DataSetting cannot be null when executing a query.");
                }

                if (!ConnectionOpen) { Open(); }

                _Command = new SqlCommand()
                {
                    CommandText = DataSetting.ProcedureName,
                    CommandType = CommandType.StoredProcedure,
                    Connection = _Connection,
                    CommandTimeout = (_Connection.ConnectionTimeout < 1000 ? 1000 : _Connection.ConnectionTimeout),
                    Transaction = _Transaction
                };

                if ((paramValues != null) && (paramValues.Length > 0))
                {

                    if ((DataSetting.InputParamNames.Length != DataSetting.InputParamTypes.Length) || (DataSetting.InputParamNames.Length != paramValues.Length))
                    {
                        throw new ArgumentOutOfRangeException("The value passed for paramValues is outside the range for the value of DataSetting.InputParamNames.");
                    }

                    _Command.Parameters.AddRange(DataSetting.ToInputParameterArray(paramValues));

                }

                if (DataSetting.OutputParamNames.Length > 0) { _Command.Parameters.AddRange(DataSetting.ToOutputParameterArray()); }

                ReturnValues = new ReturnValueCollection();

                _Command.ExecuteNonQuery();

                foreach (string parmName in DataSetting.OutputParamNames)
                {
                    ReturnValues.Add(parmName, _Command.Parameters[parmName].Value);
                }

                result = true;

            }
            catch (SqlException ex)
            {
                OnExecutionError(new SqlAbortArgs(ex, transactionInitialized));
                result = false;
            }
            catch (Exception ex)
            {
                OnExecutionError(new SqlAbortArgs(ex, transactionInitialized));
                result = false;
            }

            return result;

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public virtual IDataReader GetReader()
        {

            try
            {

                if (DataSetting == null)
                {
                    throw new InvalidOperationException("DataSetting cannot be null when creating an instance of IDataReader.");
                }

                if (!ConnectionOpen) { Open(); }

                _Command = new SqlCommand()
                {
                    CommandText = DataSetting.ProcedureName,
                    CommandType = CommandType.StoredProcedure,
                    Connection = _Connection,
                    CommandTimeout = (_Connection.ConnectionTimeout < 1000 ? 1000 : _Connection.ConnectionTimeout)
                };

                return _Command.ExecuteReader();

            }
            catch (SqlException ex)
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
        public virtual IDataReader GetReader(object[] paramValues)
        {

            try
            {

                if (DataSetting == null)
                {
                    throw new InvalidOperationException("DataSetting cannot be null when creating an instance of IDataReader.");
                }

                if (!ConnectionOpen) { Open(); }

                _Command = new SqlCommand()
                {
                    CommandText = DataSetting.ProcedureName,
                    CommandType = CommandType.StoredProcedure,
                    Connection = _Connection,
                    CommandTimeout = (_Connection.ConnectionTimeout < 1000 ? 1000 : _Connection.ConnectionTimeout)
                };

                if ((paramValues != null) && (paramValues.Length > 0))
                {

                    if ((DataSetting.InputParamNames.Length != DataSetting.InputParamTypes.Length) || (DataSetting.InputParamNames.Length != paramValues.Length))
                    {
                        throw new ArgumentOutOfRangeException("The value passed for paramValues is outside the range for the value of DataSetting.InputParamNames.");
                    }

                    _Command.Parameters.AddRange(DataSetting.ToInputParameterArray(paramValues));

                }

                return _Command.ExecuteReader();

            }
            catch (SqlException ex)
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
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Protected Methods
        protected virtual void OnExecutionError(SqlAbortArgs e)
        {
            ExecutionError?.Invoke(this, e);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_Connection")]
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            _Transaction?.Dispose();
            _Command?.Dispose();
            _Connection?.Dispose();
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
            _Setting = setting;
            _HasParameters = hasParameters;
            _ParamValues = paramValues;
        }
        #endregion

        #region Events
        public event SqlExceptionEventHandler ExecutionError;
        #endregion

        #region Properties
        /// <summary>
        /// Not implemented: Throws an exception of type NotImplementedException
        /// </summary>
        public ICommandString CommandString { get; set; }

        /// <summary>
        /// Gets the IDataSetting object
        /// </summary>
        public IDataSetting Setting => _Setting;

        /// <summary>
        /// Gets a value indicating whether the ParamValues is initialized and the length is greater than zero
        /// </summary>
        public bool HasParameters => _HasParameters;

        /// <summary>
        /// Gets an array of object containing the input parameter values
        /// </summary>
        public object[] ParamValues => _ParamValues;

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
            return
                $"{GetType().Name} (Has Params: {(_HasParameters ? "Yes" : "No")}, Param Count: {_ParamValues?.Length ?? 0})";
        }
        #endregion

        #region Protected Methods
        protected virtual void OnExecutionError(SqlAbortArgs e)
        {
            ExecutionError?.Invoke(this, e);
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
            _Items = new List<T>();
        }

        public SqlExecutionCommandResult(SqlCommandResultArgs result, string message)
        {
            Result = result;
            Message = message;
            _Items = new List<T>();
        }
        #endregion

        #region Properties
        public SqlCommandResultArgs Result { get; set; }
        public string Message { get; set; }
        public Exception ExecutionException { get; set; }
        public List<T> Items => _Items;

        #endregion

        #region Public Methods
        public object[] ToArray()
        {
            return new object[] { (int)Result, Message };
        }
        #endregion

    }

    public class SqlPutCommand : SqlExecutionCommandBase<SqlExecutionCommandResult<object>>
    {

        #region Local Variables
        protected readonly SqlExecutionCommandResult<object> ExecutionResult;
        #endregion

        #region Constructor
        public SqlPutCommand(IDataSetting setting, object[] paramValues)
            : base(setting, true, paramValues)
        {
            ExecutionResult = new SqlExecutionCommandResult<object>();
        }
        #endregion

        #region Public Methods
        public override SqlExecutionCommandResult<object> ExecuteCommand()
        {
            SqlConnectionObject conn = new SqlConnectionObject(Setting);

            conn.ExecutionError += SqlException;

            try
            {
                bool result;

                using (conn)
                {
                    conn.Open();
                    conn.BeginTransaction();
                    result = conn.ExecuteNonQuery(ParamValues);
                    conn.EndTransaction(result);
                }

                if (result)
                {

                    if (Setting.OutputParamNames.Length > 0)
                    {
                        foreach (string key in Setting.OutputParamNames)
                        {
                            ExecutionResult.Items.Add(conn.ReturnValues[key]);
                        }
                    }

                    ExecutionResult.Result = SqlCommandResultArgs.Successful;
                    ExecutionResult.Message = "Successfully Saved";
                }
                else
                {
                    ExecutionResult.Result = SqlCommandResultArgs.Failed;
                    ExecutionResult.Message = "Unable to Save";

                }

            }
            catch (Exception e)
            {
                ExecutionResult.Result = SqlCommandResultArgs.Error;
                ExecutionResult.Message = e.Message;
                ExecutionResult.ExecutionException = e;
            }

            return ExecutionResult;

        }
        #endregion

        #region Protected Methods
        protected virtual void SqlException(object sender, ISqlAbortArgs e)
        {
            ExecutionResult.Result = SqlCommandResultArgs.Error;
            ExecutionResult.Message = e.Exception.Message;
            ExecutionResult.ExecutionException = e.Exception;

            e.RollbackChanges = e.RollbackAllowed;

            OnExecutionError((SqlAbortArgs)e);

        }
        #endregion

    }

    public class SqlGetCommand<T> : SqlExecutionCommandBase<SqlExecutionCommandResult<T>> where T : ICommonObject, new()
    {

        #region Local Variables
        protected readonly SqlExecutionCommandResult<T> result;
        #endregion

        #region Constructor
        public SqlGetCommand(IDataSetting setting, bool hasParameters, object[] paramValues)
            : base(setting, hasParameters, paramValues)
        {
            result = new SqlExecutionCommandResult<T>();
        }
        #endregion

        #region Public Methods
        public override SqlExecutionCommandResult<T> ExecuteCommand()
        {
            SqlConnectionObject conn = new SqlConnectionObject(Setting);

            conn.ExecutionError += SqlException;

            try
            {
                
                using (conn)
                {
                    DataTable dt = null;

                    conn.Open();

                    if (HasParameters)
                    {
                        if (conn.ExecuteQuery(ParamValues))
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
                        foreach (DataRow row in dt.Rows)
                        {
                            T obj = Krodzone.CreateInstance<T>(row);

                            //  For Backwards Compatibility
                            obj = (obj == null ? Krodzone.CreateObject<T>(row) : obj);

                            if (obj != null)
                            {
                                result.Items.Add(obj);
                            }
                            
                        }
                    }

                }
            }
            catch (Exception e)
            {
                result.Result = SqlCommandResultArgs.Error;
                result.Message = e.Message;
                result.ExecutionException = e;
            }

            return result;

        }
        #endregion

        #region Protected Methods
        protected virtual void SqlException(object sender, ISqlAbortArgs e)
        {
            result.Result = SqlCommandResultArgs.Error;
            result.Message = e.Exception.Message;
            result.ExecutionException = e.Exception;

            e.RollbackChanges = e.RollbackAllowed;

            OnExecutionError((SqlAbortArgs)e);

        }
        #endregion

    }

    public class DatabaseConnector
    {

        #region Local Variables
        private static DatabaseConnector _Instance;
        #endregion

        #region Constructor
        private DatabaseConnector() { }
        #endregion

        #region Events
        public event SqlExceptionEventHandler ExecutionError;
        #endregion

        #region Properties
        public static DatabaseConnector Instance => _Instance ?? (_Instance = new DatabaseConnector());

        #endregion

        #region Public Static Methods
        public SqlExecutionCommandResult<object> SaveData(IDataSetting setting, object[] values)
        {
            KrodzoneDataProvider provider = new KrodzoneDataProvider();
            ISqlExecutionCommand<SqlExecutionCommandResult<object>> cmd = new SqlPutCommand(setting, values);

            return provider.ExecuteCommand<ISqlExecutionCommand<SqlExecutionCommandResult<object>>, SqlExecutionCommandResult<object>>(cmd);

        }

        public Task<SqlExecutionCommandResult<object>> SaveDataAsync(IDataSetting setting, object[] values)
        {
            KrodzoneDataProvider provider = new KrodzoneDataProvider();
            ISqlExecutionCommand<SqlExecutionCommandResult<object>> cmd = new SqlPutCommand(setting, values);

            SqlExecutionCommandResult<object> result = provider.ExecuteCommand<ISqlExecutionCommand<SqlExecutionCommandResult<object>>, SqlExecutionCommandResult<object>>(cmd);

            return Task.FromResult(result);

        }

        public List<T> GetData<T>(ISqlExecutionCommand<SqlExecutionCommandResult<T>> cmd)
            where T : ICommonObject, new()
        {
            //  This should be put in a single method that takes an array of object as it's only param
            KrodzoneDataProvider provider = new KrodzoneDataProvider();
            SqlExecutionCommandResult<T> result = provider.ExecuteCommand<ISqlExecutionCommand<SqlExecutionCommandResult<T>>, SqlExecutionCommandResult<T>>(cmd);

            if (result.Result == SqlCommandResultArgs.Error)
            {
                ExecutionError?.Invoke(result, new SqlAbortArgs(result.ExecutionException, false));
            }

            return result.Items;

        }

        public Task<List<T>> GetDataAsync<T>(ISqlExecutionCommand<SqlExecutionCommandResult<T>> cmd)
            where T : ICommonObject, new()
        {
            //  This should be put in a single method that takes an array of object as it's only param
            KrodzoneDataProvider provider = new KrodzoneDataProvider();
            SqlExecutionCommandResult<T> result = provider.ExecuteCommand<ISqlExecutionCommand<SqlExecutionCommandResult<T>>, SqlExecutionCommandResult<T>>(cmd);

            if (result.Result == SqlCommandResultArgs.Error)
            {
                ExecutionError?.Invoke(result, new SqlAbortArgs(result.ExecutionException, false));
            }

            return Task.FromResult(result.Items);

        }
        #endregion

    }

    public class KrodzoneDataProvider : ICommandExecuter
    {
        
        #region Public Methods
        public W ExecuteCommand<T, W>(T obj)
            where T : ICommand<W>
            where W : new()
        {
            W value = default(W);

            try
            {
                MethodInfo method = obj.GetType().GetMethod("ExecuteCommand");

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
