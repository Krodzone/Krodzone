using Krodzone.Attributes;
using Krodzone.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;

namespace Krodzone.Validation
{
    /// <summary>
    /// 
    /// </summary>
    public enum DataTypeArgs
    {
        /// <summary>
        /// 
        /// </summary>
        Integer = 0,
        /// <summary>
        /// 
        /// </summary>
        Decimal = 1,
        /// <summary>
        /// 
        /// </summary>
        Date = 2,
        /// <summary>
        /// 
        /// </summary>
        Time = 3,
        /// <summary>
        /// 
        /// </summary>
        DateTime = 4,
        /// <summary>
        /// 
        /// </summary>
        Boolean = 5,
        /// <summary>
        /// 
        /// </summary>
        String = 6
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ValidationResultArgs
    {
        /// <summary>
        /// 
        /// </summary>
        Successful = 0,
        /// <summary>
        /// 
        /// </summary>
        MatchNotFound = 1,
        /// <summary>
        /// 
        /// </summary>
        OutOfRange = 2,
        /// <summary>
        /// 
        /// </summary>
        NotEqual = 3,
        /// <summary>
        /// 
        /// </summary>
        IsEqual = 4,
        /// <summary>
        /// 
        /// </summary>
        Failed = 5,
        /// <summary>
        /// 
        /// </summary>
        Error = 6
    }

    /// <summary>
    /// 
    /// </summary>
    public enum BooleanComparisonTypeArgs
    {
        /// <summary>
        /// 
        /// </summary>
        None = 0,
        /// <summary>
        /// 
        /// </summary>
        AND = 1,
        /// <summary>
        /// 
        /// </summary>
        OR = 2,
        /// <summary>
        /// 
        /// </summary>
        XOR = 3
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IDataValidationConfigSetting : ICommonObject
    {

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        int DataValidationConfigSettingID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string ApplicationName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string ObjectName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string PropertyName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string LogicalComparisonGroup { get; set; }
        /// <summary>
        /// 
        /// </summary>
        BooleanComparisonTypeArgs ComparisonType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        EvaluationTypeArgs EvaluationType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        DataTypeArgs DataType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string DirectValue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string MinValue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string MaxValue { get; set; }
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    [SqlTable("dbo", "DataValidationConfigSetting", "DefaultConnection")]
    public class DataValidationConfigSetting : ObjectAuditBase<string>, IDataValidationConfigSetting
    {
        
        #region Properties
        /// <summary>
        /// 
        /// </summary>
        [SqlTableColumn("DataValidationConfigSettingID", SqlDbType.Int, false, PrimaryKey = true, Identity = true, Seed = 100, Increment = 1)]
        public int DataValidationConfigSettingID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SqlTableColumn("ApplicationName", SqlDbType.VarChar, false, Length = 200)]
        public string ApplicationName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SqlTableColumn("ObjectName", SqlDbType.VarChar, false, Length = 200)]
        public string ObjectName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SqlTableColumn("PropertyName", SqlDbType.VarChar, false, Length = 200)]
        public string PropertyName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SqlTableColumn("LogicalComparisonGroup", SqlDbType.VarChar, false, Length = 200)]
        public string LogicalComparisonGroup { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SqlTableColumn("ComparisonType", SqlDbType.Int, false)]
        public BooleanComparisonTypeArgs ComparisonType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SqlTableColumn("EvaluationType", SqlDbType.Int, false)]
        public EvaluationTypeArgs EvaluationType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SqlTableColumn("DataType", SqlDbType.Int, false)]
        public DataTypeArgs DataType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SqlTableColumn("DirectValue", SqlDbType.VarChar, false, Length = 500)]
        public string DirectValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SqlTableColumn("MinValue", SqlDbType.VarChar, false, Length = 100)]
        public string MinValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SqlTableColumn("MaxValue", SqlDbType.VarChar, false, Length = 100)]
        public string MaxValue { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object[] ToArray()
        {
            return new object[] { DataValidationConfigSettingID, ApplicationName, ObjectName, PropertyName, LogicalComparisonGroup, (int)ComparisonType, EvaluationType, DataType, DirectValue, MinValue, MaxValue, IsActive, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate };
        }
        #endregion

        #region Public Static Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public static IDataValidationConfigSetting CreateItem(DataRow row)
        {
            IDataValidationConfigSetting obj = null;

            if (row != null)
            {
                obj = new DataValidationConfigSetting();

                foreach (DataColumn col in row.Table.Columns)
                {
                    System.Reflection.PropertyInfo property = obj.GetType().GetProperty(col.ColumnName);

                    if (property != null)
                    {
                        property.SetValue(obj, row[col.ColumnName]);
                    }
                    else
                    {
                        obj = null;
                        break;
                    }

                }

            }

            return obj;

        }
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public interface IDataValidationSettingCollection
    {

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        IDataValidationConfigSetting this[string name] { get; }
        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="setting"></param>
        void Add(IDataValidationConfigSetting setting);
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public class DataValidationSettingCollection : IDataValidationSettingCollection
    {

        #region Local Variables
        /// <summary>
        /// 
        /// </summary>
        protected readonly IDictionary<string, IDataValidationConfigSetting> Settings;
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        public DataValidationSettingCollection()
        {
            Settings = new Dictionary<string, IDataValidationConfigSetting>();
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IDataValidationConfigSetting this[string name]
        {
            get
            {
                if (Settings.ContainsKey(name))
                {
                    return Settings[name];
                }
                return null;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="setting"></param>
        public void Add(IDataValidationConfigSetting setting)
        {

            try
            {
                if (setting != null)
                {
                    if (!Settings.ContainsKey(setting.PropertyName))
                    {
                        Settings.Add(setting.PropertyName, setting);
                    }
                }
            }
            catch
            {
                //  Suppress Error
            }

        }
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public class DataValidationConfigManager
    {

        #region Local Variables
        private static DataValidationConfigManager _Settings;
        #endregion

        #region Constructor
        private DataValidationConfigManager() { }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="objectName"></param>
        /// <param name="connStringKey"></param>
        /// <returns></returns>
        public IDataValidationSettingCollection this[string applicationName, string objectName, string connStringKey] => GetApplicationValidationSettings(applicationName, objectName, connStringKey);

        /// <summary>
        /// 
        /// </summary>
        public static DataValidationConfigManager Settings => _Settings ?? (_Settings = new DataValidationConfigManager());

        #endregion

        #region Protected Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="objectName"></param>
        /// <param name="connStringKey"></param>
        /// <returns></returns>
        protected virtual IDataValidationSettingCollection GetApplicationValidationSettings(string applicationName, string objectName, string connStringKey)
        {
            IDataSetting setting = new DataSetting(ConfigurationManager.ConnectionStrings[connStringKey].ToString(), "GetAll_DataValidationConfigSetting",
                                                    new[] { "@ApplicationName", "@ObjectName" }, new[] { SqlDbType.VarChar, SqlDbType.VarChar },
                                                    new string[] { }, new SqlDbType[] { });

            ISqlExecutionCommand<SqlExecutionCommandResult<DataValidationConfigSetting>> cmd = new SqlGetCommand<DataValidationConfigSetting>(setting, true, new object[] { applicationName, objectName });
            IList<DataValidationConfigSetting> items = DatabaseConnector.Instance.GetData(cmd);

            IDataValidationSettingCollection settings = null;

            if (items != null && items.Count > 0)
            {
                settings = new DataValidationSettingCollection();

                foreach (DataValidationConfigSetting validationSetting in items)
                {
                    settings.Add(validationSetting);
                }

            }

            return settings;

        }
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public interface IValidationValue
    {

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        EvaluationTypeArgs EvaluationType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        DataTypeArgs DataType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        DataValidationConfigSetting Setting { get; set; }
        /// <summary>
        /// 
        /// </summary>
        string ValidationError { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ValidationResultArgs Validate();
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IValidationValue<T> : IValidationValue
    {

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        T CurrentValue { get; set; }
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public interface IValidationGroup : IValidationValue
    {

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        IValidationValue this[int index] { get; }
        /// <summary>
        /// 
        /// </summary>
        int Length { get; }
        /// <summary>
        /// 
        /// </summary>
        BooleanComparisonTypeArgs ComparisonType { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        void Add(IValidationValue value);
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public interface IValidator
    {

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        IValidationValue this[int index] { get; }
        /// <summary>
        /// 
        /// </summary>
        int Length { get; }
        /// <summary>
        /// 
        /// </summary>
        string ValidationError { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        void Add(IValidationValue value);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool IsValid();
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public interface IValidatorFactory
    {

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Context"></param>
        /// <param name="appName"></param>
        /// <param name="connStringKey"></param>
        /// <returns></returns>
        IValidator CreateObjectValidator(ICommonObject Context, string appName, string connStringKey);
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ValidationValue<T> : IValidationValue<T>
    {
        
        #region Properties
        /// <summary>
        /// 
        /// </summary>
        public T CurrentValue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public EvaluationTypeArgs EvaluationType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DataTypeArgs DataType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DataValidationConfigSetting Setting { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ValidationError { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ValidationResultArgs Validate()
        {
            ValidationResultArgs result = ValidationResultArgs.Failed;

            try
            {
                switch (DataType)
                {
                    case DataTypeArgs.Boolean:
                        result = ValidateBoolean();
                        break;
                    case DataTypeArgs.Date:
                        result = ValidateDate();
                        break;
                    case DataTypeArgs.DateTime:
                        result = ValidateDateTime();
                        break;
                    case DataTypeArgs.Decimal:
                        result = ValidateDecimal();
                        break;
                    case DataTypeArgs.Integer:
                        result = ValidateInteger();
                        break;
                    case DataTypeArgs.String:
                        result = ValidateString();
                        break;
                    case DataTypeArgs.Time:
                        result = ValidateTime();
                        break;
                }
            }
            catch (Exception ex)
            {
                result = ValidationResultArgs.Error;
                ValidationError = ex.Message;
            }

            if (result != ValidationResultArgs.Successful)
            {
                ValidationError = "Invalid " + Setting.PropertyName;
            }

            return result;

        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual ValidationResultArgs ValidateInteger()
        {
            ValidationResultArgs result = ValidationResultArgs.OutOfRange;

            try
            {
                if (Setting != null)
                {
                    int currentValue; int directValue = -1; int minValue = -1; int maxValue = -1;
                    bool parseSucceeded;

                    switch (EvaluationType)
                    {
                        case EvaluationTypeArgs.Exact:
                            parseSucceeded = (int.TryParse(CurrentValue.ToString(), out currentValue) && int.TryParse(Setting.DirectValue, out directValue));

                            if (parseSucceeded)
                            {
                                result = currentValue == directValue ? ValidationResultArgs.Successful : ValidationResultArgs.NotEqual;
                            }
                            else
                            {
                                result = ValidationResultArgs.Failed;
                            }

                            break;

                        case EvaluationTypeArgs.Negation:
                            parseSucceeded = (int.TryParse(CurrentValue.ToString(), out currentValue) && int.TryParse(Setting.DirectValue, out directValue));

                            if (parseSucceeded)
                            {
                                result = currentValue != directValue ? ValidationResultArgs.Successful : ValidationResultArgs.IsEqual;
                            }
                            else
                            {
                                result = ValidationResultArgs.Failed;
                            }

                            break;

                        case EvaluationTypeArgs.Range:
                            parseSucceeded = (int.TryParse(CurrentValue.ToString(), out currentValue) && int.TryParse(Setting.MinValue, out minValue) && int.TryParse(Setting.MaxValue, out maxValue));

                            if (parseSucceeded)
                            {
                                result = ValidationResultArgs.Failed;

                                if (minValue == -1 && maxValue > minValue)
                                {
                                    result = (currentValue <= maxValue ? ValidationResultArgs.Successful : ValidationResultArgs.OutOfRange);
                                }

                                if (minValue != -1 && maxValue == -1)
                                {
                                    result = (currentValue >= minValue ? ValidationResultArgs.Successful : ValidationResultArgs.OutOfRange);
                                }

                                if (minValue != -1 && maxValue != -1)
                                {
                                    result = (currentValue >= minValue && currentValue <= maxValue ? ValidationResultArgs.Successful : ValidationResultArgs.OutOfRange);
                                }

                            }
                            else
                            {
                                result = ValidationResultArgs.Failed;
                            }

                            break;

                    }

                }
            }
            catch (Exception ex)
            {
                result = ValidationResultArgs.Error;
                ValidationError = ex.Message;
            }

            return result;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual ValidationResultArgs ValidateDecimal()
        {
            ValidationResultArgs result = ValidationResultArgs.OutOfRange;

            try
            {
                if (Setting != null)
                {
                    decimal currentValue; decimal directValue = -1; decimal minValue = -1; decimal maxValue = -1;
                    bool parseSucceeded;

                    switch (EvaluationType)
                    {
                        case EvaluationTypeArgs.Exact:
                            parseSucceeded = (decimal.TryParse(CurrentValue.ToString(), out currentValue) && decimal.TryParse(Setting.DirectValue, out directValue));

                            if (parseSucceeded)
                            {
                                result = currentValue == directValue ? ValidationResultArgs.Successful : ValidationResultArgs.NotEqual;
                            }
                            else
                            {
                                result = ValidationResultArgs.Failed;
                            }

                            break;

                        case EvaluationTypeArgs.Negation:
                            parseSucceeded = (decimal.TryParse(CurrentValue.ToString(), out currentValue) && decimal.TryParse(Setting.DirectValue, out directValue));

                            if (parseSucceeded)
                            {
                                result = currentValue != directValue ? ValidationResultArgs.Successful : ValidationResultArgs.IsEqual;
                            }
                            else
                            {
                                result = ValidationResultArgs.Failed;
                            }

                            break;

                        case EvaluationTypeArgs.Range:
                            parseSucceeded = (decimal.TryParse(CurrentValue.ToString(), out currentValue) && decimal.TryParse(Setting.MinValue, out minValue) && decimal.TryParse(Setting.MaxValue, out maxValue));

                            if (parseSucceeded)
                            {
                                result = ValidationResultArgs.Failed;

                                if (minValue == -1 && maxValue > minValue)
                                {
                                    result = (currentValue <= maxValue ? ValidationResultArgs.Successful : ValidationResultArgs.OutOfRange);
                                }

                                if (minValue != -1 && maxValue == -1)
                                {
                                    result = (currentValue >= minValue ? ValidationResultArgs.Successful : ValidationResultArgs.OutOfRange);
                                }

                                if (minValue != -1 && maxValue != -1)
                                {
                                    result = (currentValue >= minValue && currentValue <= maxValue ? ValidationResultArgs.Successful : ValidationResultArgs.OutOfRange);
                                }

                            }
                            else
                            {
                                result = ValidationResultArgs.Failed;
                            }

                            break;

                    }

                }
            }
            catch (Exception ex)
            {
                result = ValidationResultArgs.Error;
                ValidationError = ex.Message;
            }

            return result;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual ValidationResultArgs ValidateDate()
        {
            ValidationResultArgs result = ValidationResultArgs.OutOfRange;

            try
            {
                if (Setting != null)
                {
                    DateTime currentValue; DateTime directValue = new DateTime(1900, 1, 1); DateTime minValue = new DateTime(1900, 1, 1); DateTime maxValue = new DateTime(1900, 1, 1);
                    bool parseSucceeded;

                    switch (EvaluationType)
                    {
                        case EvaluationTypeArgs.Exact:
                            parseSucceeded = (DateTime.TryParse(CurrentValue.ToString(), out currentValue) && DateTime.TryParse(Setting.DirectValue, out directValue));

                            if (parseSucceeded)
                            {
                                if (currentValue.Year == directValue.Year && currentValue.Month == directValue.Month && currentValue.Day == directValue.Day)
                                {
                                    result = ValidationResultArgs.Successful;
                                }
                                else
                                {
                                    result = ValidationResultArgs.NotEqual;
                                }
                            }
                            else
                            {
                                result = ValidationResultArgs.Failed;
                            }

                            break;

                        case EvaluationTypeArgs.Negation:
                            parseSucceeded = (DateTime.TryParse(CurrentValue.ToString(), out currentValue) && DateTime.TryParse(Setting.DirectValue, out directValue));

                            if (parseSucceeded)
                            {
                                result = currentValue != directValue ? ValidationResultArgs.Successful : ValidationResultArgs.IsEqual;
                            }
                            else
                            {
                                result = ValidationResultArgs.Failed;
                            }

                            break;

                        case EvaluationTypeArgs.Range:
                            parseSucceeded = (DateTime.TryParse(CurrentValue.ToString(), out currentValue) && DateTime.TryParse(Setting.MinValue, out minValue) && DateTime.TryParse(Setting.MaxValue, out maxValue));

                            if (parseSucceeded)
                            {
                                result = ValidationResultArgs.Failed;

                                if (minValue.Year == 1900 && maxValue > minValue)
                                {
                                    result = (currentValue <= maxValue ? ValidationResultArgs.Successful : ValidationResultArgs.OutOfRange);
                                }

                                if (minValue.Year > 1900 && maxValue.Year == 1900)
                                {
                                    result = (currentValue >= minValue ? ValidationResultArgs.Successful : ValidationResultArgs.OutOfRange);
                                }

                                if (minValue.Year > 1900 && maxValue.Year > 1900)
                                {
                                    result = (currentValue >= minValue && currentValue <= maxValue ? ValidationResultArgs.Successful : ValidationResultArgs.OutOfRange);
                                }

                            }
                            else
                            {
                                result = ValidationResultArgs.Failed;
                            }

                            break;

                    }

                }
            }
            catch (Exception ex)
            {
                result = ValidationResultArgs.Error;
                ValidationError = ex.Message;
            }

            return result;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual ValidationResultArgs ValidateTime()
        {
            ValidationResultArgs result = ValidationResultArgs.OutOfRange;

            try
            {
                if (Setting != null)
                {
                    TimeSpan currentValue; TimeSpan directValue = new TimeSpan(0, 0, 0); TimeSpan minValue = new TimeSpan(0, 0, 0); TimeSpan maxValue = new TimeSpan(0, 0, 0);
                    bool parseSucceeded;

                    switch (EvaluationType)
                    {
                        case EvaluationTypeArgs.Exact:
                            parseSucceeded = (TimeSpan.TryParse(CurrentValue.ToString(), out currentValue) && TimeSpan.TryParse(Setting.DirectValue, out directValue));

                            if (parseSucceeded)
                            {
                                result = currentValue == directValue ? ValidationResultArgs.Successful : ValidationResultArgs.NotEqual;
                            }
                            else
                            {
                                result = ValidationResultArgs.Failed;
                            }

                            break;

                        case EvaluationTypeArgs.Negation:
                            parseSucceeded = (TimeSpan.TryParse(CurrentValue.ToString(), out currentValue) && TimeSpan.TryParse(Setting.DirectValue, out directValue));

                            if (parseSucceeded)
                            {
                                result = currentValue != directValue ? ValidationResultArgs.Successful : ValidationResultArgs.IsEqual;
                            }
                            else
                            {
                                result = ValidationResultArgs.Failed;
                            }

                            break;

                        case EvaluationTypeArgs.Range:
                            parseSucceeded = (TimeSpan.TryParse(CurrentValue.ToString(), out currentValue) && TimeSpan.TryParse(Setting.MinValue, out minValue) && TimeSpan.TryParse(Setting.MaxValue, out maxValue));

                            if (parseSucceeded)
                            {
                                result = (currentValue >= minValue && currentValue <= maxValue ? ValidationResultArgs.Successful : ValidationResultArgs.OutOfRange);
                            }
                            else
                            {
                                result = ValidationResultArgs.Failed;
                            }

                            break;

                    }

                }
            }
            catch (Exception ex)
            {
                result = ValidationResultArgs.Error;
                ValidationError = ex.Message;
            }

            return result;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual ValidationResultArgs ValidateDateTime()
        {
            ValidationResultArgs result = ValidationResultArgs.OutOfRange;

            try
            {
                if (Setting != null)
                {
                    DateTime currentValue; DateTime directValue = new DateTime(1900, 1, 1); DateTime minValue = new DateTime(1900, 1, 1); DateTime maxValue = new DateTime(1900, 1, 1);
                    bool parseSucceeded;

                    switch (EvaluationType)
                    {
                        case EvaluationTypeArgs.Exact:
                            parseSucceeded = (DateTime.TryParse(CurrentValue.ToString(), out currentValue) && DateTime.TryParse(Setting.DirectValue, out directValue));

                            if (parseSucceeded)
                            {
                                if (currentValue.Year == directValue.Year && currentValue.Month == directValue.Month && currentValue.Day == directValue.Day)
                                {
                                    result = ValidationResultArgs.Successful;
                                }
                                else
                                {
                                    result = ValidationResultArgs.NotEqual;
                                }
                            }
                            else
                            {
                                result = ValidationResultArgs.Failed;
                            }

                            break;

                        case EvaluationTypeArgs.Negation:
                            parseSucceeded = (DateTime.TryParse(CurrentValue.ToString(), out currentValue) && DateTime.TryParse(Setting.DirectValue, out directValue));

                            if (parseSucceeded)
                            {
                                result = currentValue != directValue ? ValidationResultArgs.Successful : ValidationResultArgs.IsEqual;
                            }
                            else
                            {
                                result = ValidationResultArgs.Failed;
                            }

                            break;

                        case EvaluationTypeArgs.Range:
                            parseSucceeded = (DateTime.TryParse(CurrentValue.ToString(), out currentValue) && DateTime.TryParse(Setting.MinValue, out minValue) && DateTime.TryParse(Setting.MaxValue, out maxValue));

                            if (parseSucceeded)
                            {
                                result = ValidationResultArgs.Failed;

                                if (minValue.Year == 1900 && maxValue > minValue)
                                {
                                    result = (currentValue <= maxValue ? ValidationResultArgs.Successful : ValidationResultArgs.OutOfRange);
                                }

                                if (minValue.Year > 1900 && maxValue.Year == 1900)
                                {
                                    result = (currentValue >= minValue ? ValidationResultArgs.Successful : ValidationResultArgs.OutOfRange);
                                }

                                if (minValue.Year > 1900 && maxValue.Year > 1900)
                                {
                                    result = (currentValue >= minValue && currentValue <= maxValue ? ValidationResultArgs.Successful : ValidationResultArgs.OutOfRange);
                                }

                            }
                            else
                            {
                                result = ValidationResultArgs.Failed;
                            }

                            break;

                    }

                }
            }
            catch (Exception ex)
            {
                result = ValidationResultArgs.Error;
                ValidationError = ex.Message;
            }

            return result;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual ValidationResultArgs ValidateBoolean()
        {
            ValidationResultArgs result = ValidationResultArgs.Failed;

            try
            {
                if (Setting != null)
                {
                    bool currentValue;
                    bool directValue = false;
                    bool parseSucceeded;

                    switch (EvaluationType)
                    {
                        case EvaluationTypeArgs.Exact:
                            parseSucceeded = (bool.TryParse(CurrentValue.ToString(), out currentValue) && bool.TryParse(Setting.DirectValue, out directValue));

                            if (parseSucceeded)
                            {
                                result = currentValue == directValue ? ValidationResultArgs.Successful : ValidationResultArgs.NotEqual;
                            }
                            else
                            {
                                result = ValidationResultArgs.Failed;
                            }

                            break;

                        case EvaluationTypeArgs.Negation:
                            parseSucceeded = (bool.TryParse(CurrentValue.ToString(), out currentValue) && bool.TryParse(Setting.DirectValue, out directValue));

                            if (parseSucceeded)
                            {
                                result = currentValue != directValue ? ValidationResultArgs.Successful : ValidationResultArgs.IsEqual;
                            }
                            else
                            {
                                result = ValidationResultArgs.Failed;
                            }

                            break;

                    }

                }
            }
            catch (Exception ex)
            {
                result = ValidationResultArgs.Error;
                ValidationError = ex.Message;
            }

            return result;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual ValidationResultArgs ValidateString()
        {
            ValidationResultArgs result = ValidationResultArgs.Failed;

            try
            {
                if (Setting != null)
                {
                    string currentValue = CurrentValue.ToString(); string directValue = Setting.DirectValue;

                    switch (EvaluationType)
                    {
                        case EvaluationTypeArgs.Exact:

                            result = currentValue.Equals(directValue, StringComparison.InvariantCultureIgnoreCase) ? ValidationResultArgs.Successful : ValidationResultArgs.NotEqual;

                            break;

                        case EvaluationTypeArgs.Negation:

                            result = !currentValue.Equals(directValue, StringComparison.InvariantCultureIgnoreCase) ? ValidationResultArgs.Successful : ValidationResultArgs.IsEqual;

                            break;

                        case EvaluationTypeArgs.Pattern:

                            result = System.Text.RegularExpressions.Regex.IsMatch(currentValue, directValue) ? ValidationResultArgs.Successful : ValidationResultArgs.MatchNotFound;

                            break;

                    }

                }
            }
            catch (Exception ex)
            {
                result = ValidationResultArgs.Error;
                ValidationError = ex.Message;
            }

            return result;

        }
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public class ValidationGroup : IValidationGroup
    {

        #region Local Variables
        /// <summary>
        /// 
        /// </summary>
        protected readonly IList<IValidationValue> _Items;
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        public ValidationGroup()
        {
            _Items = new List<IValidationValue>();
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IValidationValue this[int index]
        {
            get
            {
                if (index >= 0 && index < _Items.Count)
                {
                    return _Items[index];
                }

                return null;

            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Length => _Items.Count;

        /// <summary>
        /// 
        /// </summary>
        public BooleanComparisonTypeArgs ComparisonType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public EvaluationTypeArgs EvaluationType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DataTypeArgs DataType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DataValidationConfigSetting Setting { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ValidationError { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Add(IValidationValue value)
        {
            try
            {

                if (value != null)
                {
                    _Items.Add(value);
                }

            }
            catch
            {
                //  Suppress Error
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ValidationResultArgs Validate()
        {
            ValidationResultArgs result = ValidationResultArgs.Failed;

            try
            {
                switch (ComparisonType)
                {
                    case BooleanComparisonTypeArgs.AND:

                        foreach (IValidationValue value in _Items)
                        {
                            result = value.Validate();

                            if (result != ValidationResultArgs.Successful)
                            {
                                ValidationError = value.ValidationError;
                                break;
                            }

                        }

                        break;
                    case BooleanComparisonTypeArgs.OR:

                        foreach (IValidationValue value in _Items)
                        {
                            result = value.Validate();

                            if (result == ValidationResultArgs.Successful)
                            {
                                break;
                            }
                            else
                            {
                                ValidationError = (string.IsNullOrEmpty(ValidationError) ? value.ValidationError : ValidationError);
                            }

                        }

                        break;
                    case BooleanComparisonTypeArgs.XOR:
                        int successCount = 0;

                        foreach (IValidationValue value in _Items)
                        {
                            ValidationResultArgs valueResult = value.Validate();

                            if (valueResult == ValidationResultArgs.Successful)
                            {
                                successCount++;
                            }
                            else
                            {
                                ValidationError = (string.IsNullOrEmpty(ValidationError) ? value.ValidationError : ValidationError);
                            }

                        }

                        if (successCount == 0)
                        {
                            result = ValidationResultArgs.Failed;
                        }
                        else
                        {
                            result = (successCount % 2 == 0 ? ValidationResultArgs.Failed : ValidationResultArgs.Successful);
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                result = ValidationResultArgs.Error;
                ValidationError = ex.Message;
            }

            return result;

        }
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public class Validator : IValidator
    {

        #region Local Variables
        /// <summary>
        /// 
        /// </summary>
        protected readonly IList<IValidationValue> _Items;
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        public Validator()
        {
            _Items = new List<IValidationValue>();
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IValidationValue this[int index]
        {
            get
            {
                if (index >= 0 && index < _Items.Count)
                {
                    return _Items[index];
                }

                return null;

            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Length => _Items.Count;

        /// <summary>
        /// 
        /// </summary>
        public string ValidationError { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void Add(IValidationValue value)
        {
            try
            {

                if (value != null)
                {
                    _Items.Add(value);
                }

            }
            catch
            {
                //  Suppress Error
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            bool result = true;

            try
            {

                foreach (IValidationValue value in _Items)
                {
                    ValidationResultArgs validationResult = value.Validate();
                    result = (validationResult == ValidationResultArgs.Successful);
                    if (result == false)
                    {
                        ValidationError = value.ValidationError;
                        break;
                    }
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

    /// <summary>
    /// 
    /// </summary>
    public class ValidatorFactory : IValidatorFactory
    {
        
        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Context"></param>
        /// <param name="appName"></param>
        /// <param name="connStringKey"></param>
        /// <returns></returns>
        public IValidator CreateObjectValidator(ICommonObject Context, string appName, string connStringKey)
        {
            IValidator validator = null;

            try
            {

                if (Context != null)
                {
                    string[] parts = Context.ToString().Split(new[] { "." }, StringSplitOptions.None);
                    string objectName = parts[parts.Length - 1];

                    IDataValidationSettingCollection Settings = DataValidationConfigManager.Settings[appName, objectName, connStringKey];

                    if (Settings != null)
                    {
                        Dictionary<string, IValidationValue> validationValues = new Dictionary<string, IValidationValue>();
                        validator = new Validator();

                        foreach (System.Reflection.PropertyInfo property in Context.GetType().GetProperties())
                        {
                            IDataValidationConfigSetting setting = Settings[property.Name];
                            
                            if (setting != null)
                            {
                                IValidationValue value = GetValidationValue(setting.DataType, Context, property);
                                value.EvaluationType = setting.EvaluationType;
                                value.DataType = setting.DataType;
                                value.Setting = (DataValidationConfigSetting)setting;

                                if (validationValues.ContainsKey(setting.LogicalComparisonGroup))
                                {
                                    ((IValidationGroup)validationValues[setting.LogicalComparisonGroup]).Add(value);
                                }
                                else
                                {
                                    IValidationGroup group = new ValidationGroup();
                                    group.ComparisonType = setting.ComparisonType;
                                    group.Add(value);
                                    validationValues.Add(setting.LogicalComparisonGroup, group);
                                }

                            }

                        }

                        foreach (KeyValuePair<string, IValidationValue> kvp in validationValues)
                        {
                            IValidationGroup group = (IValidationGroup)kvp.Value;

                            if (string.IsNullOrEmpty(kvp.Key.Trim()))
                            {

                                for (int i = 0; i < group.Length; i++)
                                {
                                    validator.Add(group[i]);
                                }

                            }
                            else
                            {
                                validator.Add(group);
                            }

                        }

                    }

                }

            }
            catch
            {
                validator = null;
            }

            return validator;

        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="Context"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        protected virtual IValidationValue GetValidationValue(DataTypeArgs dataType, ICommonObject Context, System.Reflection.PropertyInfo property)
        {
            IValidationValue value = null;

            switch (dataType)
            {
                case DataTypeArgs.Boolean:
                    value = new ValidationValue<bool>();
                    ((ValidationValue<bool>)value).CurrentValue = (bool)property.GetValue(Context);
                    break;
                case DataTypeArgs.Date:
                    value = new ValidationValue<DateTime>();
                    ((ValidationValue<DateTime>)value).CurrentValue = (DateTime)property.GetValue(Context);
                    break;
                case DataTypeArgs.DateTime:
                    value = new ValidationValue<DateTime>();
                    ((ValidationValue<DateTime>)value).CurrentValue = (DateTime)property.GetValue(Context);
                    break;
                case DataTypeArgs.Decimal:
                    value = new ValidationValue<decimal>();
                    ((ValidationValue<decimal>)value).CurrentValue = (decimal)property.GetValue(Context);
                    break;
                case DataTypeArgs.Integer:
                    value = new ValidationValue<int>();
                    ((ValidationValue<int>)value).CurrentValue = (int)property.GetValue(Context);
                    break;
                case DataTypeArgs.String:
                    value = new ValidationValue<string>();
                    ((ValidationValue<string>)value).CurrentValue = (string)property.GetValue(Context);
                    break;
                case DataTypeArgs.Time:
                    value = new ValidationValue<TimeSpan>();
                    ((ValidationValue<TimeSpan>)value).CurrentValue = (TimeSpan)property.GetValue(Context);
                    break;
            }

            return value;

        }
        #endregion

    }

}
