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
using System.Data;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Krodzone.Attributes;
using Krodzone.SQL;
using Krodzone.Data;
using Krodzone.Validation;
using System.Reflection;
using Krodzone.Validation.Attributes;
using System.Linq;

namespace Krodzone
{

    /// <summary>
    /// 
    /// </summary>
    public enum EvaluationTypeArgs
    {
        /// <summary>
        /// 
        /// </summary>
        Exact = 0,
        /// <summary>
        /// 
        /// </summary>
        Negation = 1,
        /// <summary>
        /// 
        /// </summary>
        Range = 2,
        /// <summary>
        /// 
        /// </summary>
        Pattern = 3,
        /// <summary>
        /// 
        /// </summary>
        Custom = 100
    }

    #region Common Interface Objects
    /// <summary>
    /// Provides a base interface for many of the objects used by the data classes
    /// </summary>
    public interface ICommonObject
    {

        #region Methods
        /// <summary>
        /// Creates an array of object from the class properties
        /// </summary>
        /// <returns>Array of Object</returns>
        object[] ToArray();
        #endregion

    }

    public interface ICommonObject<T> : ICommonObject
    {

        #region Methods
        bool Save(IDataSetting setting);
        bool Delete(IDataSetting setting);
        #endregion

    }

    public interface IAudit
    {

        #region Properties
        int CreatedBy { get; set; }
        DateTime DateCreated { get; set; }
        int UpdatedBy { get; set; }
        DateTime DateUpdated { get; set; }
        #endregion

    }

    public interface IAuditBase
    {

        #region Properties
        bool IsActive { get; set; }
        int CreatedBy { get; set; }
        DateTime DateCreated { get; set; }
        int UpdatedBy { get; set; }
        DateTime DateUpdated { get; set; }
        #endregion

    }

    public interface IAuditBase<T>
    {

        #region Properties
        bool IsActive { get; set; }
        T CreatedBy { get; set; }
        DateTime CreatedDate { get; set; }
        T UpdatedBy { get; set; }
        DateTime DateUpdated { get; set; }
        #endregion

    }

    public interface IValueCollection : ICommonObject
    {

        #region Properties
        object this[string key] { get; }
        #endregion

        #region Methods
        void Add(string key, object value);
        #endregion

    }

    public interface IValueCollection<T> : ICommonObject
    {

        #region Properties
        T this[string key] { get; }
        #endregion

        #region Methods
        void Add(string key, T value);
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public interface ICommandExecuter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="W"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        W ExecuteCommand<T, W>(T obj)
            where T : ICommand<W>
            where W : new();
    }

    /// <summary>
    /// Base interface for all command objects
    /// </summary>
    /// <typeparam name="T">The type to be returned in method ExecuteCommand</typeparam>
    public interface ICommand<T>
    {

        #region Properties
        /// <summary>
        /// The ICommandString object executed by the command
        /// </summary>
        ICommandString CommandString { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Executes the command string
        /// </summary>
        /// <returns>An instance of T</returns>
        T ExecuteCommand();
        #endregion

    }

    /// <summary>
    /// Base interface for all CommandString objects
    /// </summary>
    public interface ICommandString
    {

        #region Methods
        /// <summary>
        /// Creates a command as a string to be executed
        /// </summary>
        /// <returns>System.String</returns>
        string GetCommandString();
        #endregion

    }
    #endregion

    /// <summary>
    /// Base class for all objects in this solution
    /// </summary>
    public abstract class ObjectAuditBase : IAuditBase
    {

        #region Local Variables
        protected IObjectValidationMessage _ValidationMessage;
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        public ObjectAuditBase() { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the user id of the person who created an entry
        /// </summary>
        [PropertyIndexer(1000, true)]
        [SqlTableColumn("IsActive", SqlDbType.Bit, false)]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the user id of the person who created an entry
        /// </summary>
        [PropertyIndexer(1001, 0)]
        [SqlTableColumn("CreatedBy", SqlDbType.Int, false)]
        public int CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date & time the entry was created
        /// </summary>
        [PropertyIndexer(1002, "1/1/1900")]
        [SqlTableColumn("DateCreated", SqlDbType.DateTime, false)]
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Gets or sets the user id of the person who updated an entry
        /// </summary>
        [PropertyIndexer(1003, 0)]
        [SqlTableColumn("UpdatedBy", SqlDbType.Int, false)]
        public int UpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date & time the entry was updated
        /// </summary>
        [PropertyIndexer(1004, "1/1/1900")]
        [SqlTableColumn("DateUpdated", SqlDbType.DateTime, false)]
        public DateTime DateUpdated { get; set; }

        public bool IsValid
        {
            get
            {
                this.Validate();
                return this._ValidationMessage.IsValid;
            }
        }

        public string ValidationMessage
        {
            get
            {
                IObjectValidationMessage ovm = _ValidationMessage;
                if (ovm != null)
                    return ovm.Message;
                else
                    return null;
            }
        }
        #endregion

        #region Public Methods
        public virtual ValidationResultArgs ValidateCreatedDate(IValidationValue value)
        {
            if (this.DateCreated.Year <= 1900)
            {
                this.DateCreated = DateTime.Now;
            }
            return ValidationResultArgs.Successful;
        }

        public virtual ValidationResultArgs ValidateUpdatedDate(IValidationValue value)
        {
            if (this.DateUpdated.Year < 1900)
            {
                this.DateUpdated = new DateTime(1900, 1, 1);
            }
            return ValidationResultArgs.Successful;
        }
        #endregion

        #region Protected Methods
        protected virtual void Validate()
        {
            IList<IRequiredPropertyAttribute> list = this.GetValidationAttributes().ToList<IRequiredPropertyAttribute>();
            StringBuilder builder = new StringBuilder();
            bool isValid = true;
            if ((list != null) && (list.Count > 0))
            {
                foreach (IRequiredPropertyAttribute attribute in list)
                {
                    if (attribute.EvaluationType == EvaluationTypeArgs.Custom)
                    {
                        if (attribute.ValidationMethod != null)
                        {
                            if (((ValidationResultArgs)attribute.ValidationMethod.Invoke(this, new object[] { attribute.ValidationValue })) != ValidationResultArgs.Successful)
                            {
                                isValid = false;
                                builder.Append(attribute.InvalidValueMessage + "\r\n");
                            }
                        }
                        else
                        {
                            isValid = false;
                            builder.Append("The method " + attribute.ValidationMethodName + " was not found on object " + base.GetType().Name + "\r\n");
                        }
                    }
                    else if (attribute.ValidationValue != null)
                    {
                        if (attribute.ValidationValue.Validate() != ValidationResultArgs.Successful)
                        {
                            isValid = false;
                            builder.Append(attribute.InvalidValueMessage + "\r\n");
                        }
                    }
                    else
                    {
                        isValid = false;
                        builder.Append("Unable to validate property " + attribute.PropertyName + " on object " + base.GetType().Name + "\r\n");
                    }
                }
                if (isValid)
                {
                    this._ValidationMessage = new ObjectValidationMessage(isValid, "Valid");
                }
                else
                {
                    this._ValidationMessage = new ObjectValidationMessage(isValid, builder.ToString());
                }
            }
            else
            {
                this._ValidationMessage = new ObjectValidationMessage(isValid, "There were no properties found to validate!");
            }
        }

        protected virtual IEnumerable<IRequiredPropertyAttribute> GetValidationAttributes()
        {
            IList<IRequiredPropertyAttribute> list = new List<IRequiredPropertyAttribute>();
            foreach (PropertyInfo info in base.GetType().GetProperties())
            {
                foreach (object obj2 in info.GetCustomAttributes(true))
                {
                    if (obj2.GetType() == typeof(RequiredPropertyAttribute))
                    {
                        try
                        {
                            IRequiredPropertyAttribute attribute = (IRequiredPropertyAttribute)obj2;
                            attribute.CurrentValue = this.GetPropertyValue(info);
                            attribute.PropertyName = info.Name;
                            if ((attribute.EvaluationType == EvaluationTypeArgs.Custom) && !string.IsNullOrEmpty(attribute.ValidationMethodName))
                            {
                                MethodInfo method = base.GetType().GetMethod(attribute.ValidationMethodName);
                                if (method != null)
                                {
                                    attribute.ValidationMethod = method;
                                }
                            }
                            attribute.ValidationValue = this.GetValidationObject(attribute);
                            list.Add(attribute);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            return list;
        }

        protected virtual IValidationValue GetValidationObject(IRequiredPropertyAttribute attribute)
        {
            IValidationValue value2 = null;
            if (attribute == null)
            {
                return value2;
            }
            switch (attribute.DataType)
            {
                case DataTypeArgs.Integer:
                    {
                        ValidationValue<int> value3 = new ValidationValue<int>
                        {
                            CurrentValue = (int)attribute.CurrentValue,
                            EvaluationType = attribute.EvaluationType,
                            DataType = DataTypeArgs.Integer
                        };
                        DataValidationConfigSetting setting = new DataValidationConfigSetting
                        {
                            EvaluationType = attribute.EvaluationType,
                            DataType = DataTypeArgs.Integer,
                            DirectValue = (attribute.EvaluationType == EvaluationTypeArgs.Exact) ? attribute.AllowedValue : ((attribute.EvaluationType == EvaluationTypeArgs.Negation) ? attribute.NegatedValue : ""),
                            MinValue = attribute.MinValue,
                            MaxValue = attribute.MaxValue
                        };
                        value3.Setting = setting;
                        return value3;
                    }
                case DataTypeArgs.Decimal:
                    {
                        ValidationValue<decimal> value4 = new ValidationValue<decimal>
                        {
                            CurrentValue = (decimal)attribute.CurrentValue,
                            EvaluationType = attribute.EvaluationType,
                            DataType = DataTypeArgs.Decimal
                        };
                        DataValidationConfigSetting setting2 = new DataValidationConfigSetting
                        {
                            EvaluationType = attribute.EvaluationType,
                            DataType = DataTypeArgs.Decimal,
                            DirectValue = (attribute.EvaluationType == EvaluationTypeArgs.Exact) ? attribute.AllowedValue : ((attribute.EvaluationType == EvaluationTypeArgs.Negation) ? attribute.NegatedValue : ""),
                            MinValue = attribute.MinValue,
                            MaxValue = attribute.MaxValue
                        };
                        value4.Setting = setting2;
                        return value4;
                    }
                case DataTypeArgs.Date:
                    {
                        ValidationValue<DateTime> value5 = new ValidationValue<DateTime>
                        {
                            CurrentValue = (DateTime)attribute.CurrentValue,
                            EvaluationType = attribute.EvaluationType,
                            DataType = DataTypeArgs.Date
                        };
                        DataValidationConfigSetting setting3 = new DataValidationConfigSetting
                        {
                            EvaluationType = attribute.EvaluationType,
                            DataType = DataTypeArgs.Date,
                            DirectValue = (attribute.EvaluationType == EvaluationTypeArgs.Exact) ? attribute.AllowedValue : ((attribute.EvaluationType == EvaluationTypeArgs.Negation) ? attribute.NegatedValue : ""),
                            MinValue = attribute.MinValue,
                            MaxValue = attribute.MaxValue
                        };
                        value5.Setting = setting3;
                        return value5;
                    }
                case DataTypeArgs.Time:
                    {
                        ValidationValue<DateTime> value6 = new ValidationValue<DateTime>
                        {
                            CurrentValue = (DateTime)attribute.CurrentValue,
                            EvaluationType = attribute.EvaluationType,
                            DataType = DataTypeArgs.Time
                        };
                        DataValidationConfigSetting setting4 = new DataValidationConfigSetting
                        {
                            EvaluationType = attribute.EvaluationType,
                            DataType = DataTypeArgs.Time,
                            DirectValue = (attribute.EvaluationType == EvaluationTypeArgs.Exact) ? attribute.AllowedValue : ((attribute.EvaluationType == EvaluationTypeArgs.Negation) ? attribute.NegatedValue : ""),
                            MinValue = attribute.MinValue,
                            MaxValue = attribute.MaxValue
                        };
                        value6.Setting = setting4;
                        return value6;
                    }
                case DataTypeArgs.DateTime:
                    {
                        ValidationValue<DateTime> value7 = new ValidationValue<DateTime>
                        {
                            CurrentValue = (DateTime)attribute.CurrentValue,
                            EvaluationType = attribute.EvaluationType,
                            DataType = DataTypeArgs.DateTime
                        };
                        DataValidationConfigSetting setting5 = new DataValidationConfigSetting
                        {
                            EvaluationType = attribute.EvaluationType,
                            DataType = DataTypeArgs.DateTime,
                            DirectValue = (attribute.EvaluationType == EvaluationTypeArgs.Exact) ? attribute.AllowedValue : ((attribute.EvaluationType == EvaluationTypeArgs.Negation) ? attribute.NegatedValue : ""),
                            MinValue = attribute.MinValue,
                            MaxValue = attribute.MaxValue
                        };
                        value7.Setting = setting5;
                        return value7;
                    }
                case DataTypeArgs.Boolean:
                    {
                        ValidationValue<bool> value8 = new ValidationValue<bool>
                        {
                            CurrentValue = (bool)attribute.CurrentValue,
                            EvaluationType = attribute.EvaluationType,
                            DataType = DataTypeArgs.Boolean
                        };
                        DataValidationConfigSetting setting6 = new DataValidationConfigSetting
                        {
                            EvaluationType = attribute.EvaluationType,
                            DataType = DataTypeArgs.Boolean,
                            DirectValue = (attribute.EvaluationType == EvaluationTypeArgs.Exact) ? attribute.AllowedValue : ((attribute.EvaluationType == EvaluationTypeArgs.Negation) ? attribute.NegatedValue : "")
                        };
                        value8.Setting = setting6;
                        return value8;
                    }
            }
            ValidationValue<string> value9 = new ValidationValue<string>
            {
                CurrentValue = (string)attribute.CurrentValue,
                EvaluationType = attribute.EvaluationType,
                DataType = DataTypeArgs.String
            };
            DataValidationConfigSetting setting7 = new DataValidationConfigSetting
            {
                EvaluationType = attribute.EvaluationType,
                DataType = DataTypeArgs.String,
                DirectValue = (attribute.EvaluationType == EvaluationTypeArgs.Exact) ? attribute.AllowedValue : ((attribute.EvaluationType == EvaluationTypeArgs.Negation) ? attribute.NegatedValue : attribute.ValidationPattern)
            };
            value9.Setting = setting7;
            return value9;
        }
        #endregion

        #region Private Mathods
        private object GetPropertyValue(PropertyInfo property)
        {
            try
            {
                switch (property.PropertyType.Name)
                {
                    case "Int32":
                        return this.GetInteger(property);

                    case "Int16":
                        return this.GetInt16(property);

                    case "Int64":
                        return this.GetInt64(property);

                    case "Decimal":
                        return this.GetDecimal(property);

                    case "DateTime":
                        return this.GetDateTime(property);

                    case "Boolean":
                        return this.GetBoolean(property);

                    case "TimeSpan":
                        return this.GetTimeSpan(property);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private bool GetBoolean(PropertyInfo property)
        {
            try
            {
                return (bool)property.GetValue(this);
            }
            catch
            {
                return false;
            }
        }

        private DateTime GetDateTime(PropertyInfo property)
        {
            try
            {
                return (DateTime)property.GetValue(this);
            }
            catch
            {
                return new DateTime(0x76c, 1, 1);
            }
        }

        private decimal GetDecimal(PropertyInfo property)
        {
            try
            {
                return (decimal)property.GetValue(this);
            }
            catch
            {
                return 0M;
            }
        }

        private short GetInt16(PropertyInfo property)
        {
            try
            {
                return (short)property.GetValue(this);
            }
            catch
            {
                return 0;
            }
        }

        private long GetInt64(PropertyInfo property)
        {
            try
            {
                return (long)property.GetValue(this);
            }
            catch
            {
                return 0L;
            }
        }

        private int GetInteger(PropertyInfo property)
        {
            try
            {
                return (int)property.GetValue(this);
            }
            catch
            {
                return 0;
            }
        }

        private TimeSpan GetTimeSpan(PropertyInfo property)
        {
            try
            {
                return (TimeSpan)property.GetValue(this);
            }
            catch
            {
                return new TimeSpan(0, 0, 0);
            }
        }
        #endregion

    }

    /// <summary>
    /// Base class for all objects in this solution
    /// </summary>
    public abstract class ObjectAuditBase<T> 
        : IAuditBase<string>, ICommonObject<T> 
        where T: ICommonObject, new()
    {

        #region Local Variables
        protected static Type parameterType;
        protected IObjectValidationMessage _ValidationMessage;
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        public ObjectAuditBase()
        {
            parameterType = typeof(T);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a value indicating if the record is active in the production system
        /// </summary>
        [PropertyIndexer(1000, true)]
        [SqlTableColumn("IsActive", SqlDbType.Bit, false)]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the user id of the person who created an entry
        /// </summary>
        [PropertyIndexer(1001, "")]
        [RequiredProperty(DataTypeArgs.String, EvaluationTypeArgs.Custom, ValidationMethodName = "ValidateCreatedBy", InvalidValueMessage = "Created By is invalid")]
        [SqlTableColumn("CreatedBy", SqlDbType.VarChar, false, Length = 100)]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date & time the entry was created
        /// </summary>
        [PropertyIndexer(1002, "1/1/1900")]
        [RequiredProperty(DataTypeArgs.DateTime, EvaluationTypeArgs.Custom, ValidationMethodName = "ValidateCreatedDate", InvalidValueMessage = "Created Date must be a value greater than 01/01/1900")]
        [SqlTableColumn("CreatedDate", SqlDbType.DateTime, false)]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the user id of the person who updated an entry
        /// </summary>
        [PropertyIndexer(1003, "")]
        [RequiredProperty(DataTypeArgs.String, EvaluationTypeArgs.Custom, ValidationMethodName = "ValidateUpdatedBy", InvalidValueMessage = "Updated By is invalid")]
        [SqlTableColumn("UpdatedBy", SqlDbType.VarChar, false, Length = 100)]
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date & time the entry was updated
        /// </summary>
        [PropertyIndexer(1004, "1/1/1900")]
        [RequiredProperty(DataTypeArgs.DateTime, EvaluationTypeArgs.Custom, ValidationMethodName = "ValidateDateUpdated", InvalidValueMessage = "Updated Date is in an incorrect format")]
        [SqlTableColumn("DateUpdated", SqlDbType.DateTime, false)]
        public DateTime DateUpdated { get; set; }

        public bool IsValid
        {
            get
            {
                this.Validate();
                return this._ValidationMessage.IsValid;
            }
        }

        public string ValidationMessage
        {
            get
            {
                IObjectValidationMessage ovm = _ValidationMessage;
                if (ovm != null)
                    return ovm.Message;
                else
                    return null;
            }
        }
        #endregion

        #region Abstract Methods
        public abstract bool Save(IDataSetting setting);
        public abstract bool Delete(IDataSetting setting);
        #endregion

        #region Public Methods
        public virtual object[] ToArray()
        {
            return PropertyIndexerAttribute.ToValueArray<T>(this);
        }
        
        public virtual ValidationResultArgs ValidateCreatedBy(IValidationValue value)
        {
            if (string.IsNullOrEmpty(this.CreatedBy))
            {
                return ValidationResultArgs.OutOfRange;
            }
            return ValidationResultArgs.Successful;
        }

        public virtual ValidationResultArgs ValidateCreatedDate(IValidationValue value)
        {
            if (this.CreatedDate.Year <= 1900)
            {
                this.CreatedDate = DateTime.Now;
            }
            return ValidationResultArgs.Successful;
        }

        public virtual ValidationResultArgs ValidateUpdatedBy(IValidationValue value)
        {
            if (string.IsNullOrEmpty(this.UpdatedBy))
            {
                this.UpdatedBy = "";
            }
            return ValidationResultArgs.Successful;
        }

        public virtual ValidationResultArgs ValidateDateUpdated(IValidationValue value)
        {
            if (this.DateUpdated.Year < 1900)
            {
                this.DateUpdated = new DateTime(1900, 1, 1);
            }
            return ValidationResultArgs.Successful;
        }
        #endregion

        #region Protected Methods
        protected virtual void Validate()
        {
            IList<IRequiredPropertyAttribute> list = this.GetValidationAttributes().ToList<IRequiredPropertyAttribute>();
            StringBuilder builder = new StringBuilder();
            bool isValid = true;
            if ((list != null) && (list.Count > 0))
            {
                foreach (IRequiredPropertyAttribute attribute in list)
                {
                    if (attribute.EvaluationType == EvaluationTypeArgs.Custom)
                    {
                        if (attribute.ValidationMethod != null)
                        {
                            if (((ValidationResultArgs)attribute.ValidationMethod.Invoke(this, new object[] { attribute.ValidationValue })) != ValidationResultArgs.Successful)
                            {
                                isValid = false;
                                builder.Append(attribute.InvalidValueMessage + "\r\n");
                            }
                        }
                        else
                        {
                            isValid = false;
                            builder.Append("The method " + attribute.ValidationMethodName + " was not found on object " + base.GetType().Name + "\r\n");
                        }
                    }
                    else if (attribute.ValidationValue != null)
                    {
                        if (attribute.ValidationValue.Validate() != ValidationResultArgs.Successful)
                        {
                            isValid = false;
                            builder.Append(attribute.InvalidValueMessage + "\r\n");
                        }
                    }
                    else
                    {
                        isValid = false;
                        builder.Append("Unable to validate property " + attribute.PropertyName + " on object " + base.GetType().Name + "\r\n");
                    }
                }
                if (isValid)
                {
                    this._ValidationMessage = new ObjectValidationMessage(isValid, "Valid");
                }
                else
                {
                    this._ValidationMessage = new ObjectValidationMessage(isValid, builder.ToString());
                }
            }
            else
            {
                this._ValidationMessage = new ObjectValidationMessage(isValid, "There were no properties found to validate!");
            }
        }

        protected virtual IEnumerable<IRequiredPropertyAttribute> GetValidationAttributes()
        {
            IList<IRequiredPropertyAttribute> list = new List<IRequiredPropertyAttribute>();
            foreach (PropertyInfo info in base.GetType().GetProperties())
            {
                foreach (object obj2 in info.GetCustomAttributes(true))
                {
                    if (obj2.GetType() == typeof(RequiredPropertyAttribute))
                    {
                        try
                        {
                            IRequiredPropertyAttribute attribute = (IRequiredPropertyAttribute)obj2;
                            attribute.CurrentValue = this.GetPropertyValue(info);
                            attribute.PropertyName = info.Name;
                            if ((attribute.EvaluationType == EvaluationTypeArgs.Custom) && !string.IsNullOrEmpty(attribute.ValidationMethodName))
                            {
                                MethodInfo method = base.GetType().GetMethod(attribute.ValidationMethodName);
                                if (method != null)
                                {
                                    attribute.ValidationMethod = method;
                                }
                            }
                            attribute.ValidationValue = this.GetValidationObject(attribute);
                            list.Add(attribute);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            return list;
        }

        protected virtual IValidationValue GetValidationObject(IRequiredPropertyAttribute attribute)
        {
            IValidationValue value2 = null;
            if (attribute == null)
            {
                return value2;
            }
            switch (attribute.DataType)
            {
                case DataTypeArgs.Integer:
                    {
                        ValidationValue<int> value3 = new ValidationValue<int>
                        {
                            CurrentValue = (int)attribute.CurrentValue,
                            EvaluationType = attribute.EvaluationType,
                            DataType = DataTypeArgs.Integer
                        };
                        DataValidationConfigSetting setting = new DataValidationConfigSetting
                        {
                            EvaluationType = attribute.EvaluationType,
                            DataType = DataTypeArgs.Integer,
                            DirectValue = (attribute.EvaluationType == EvaluationTypeArgs.Exact) ? attribute.AllowedValue : ((attribute.EvaluationType == EvaluationTypeArgs.Negation) ? attribute.NegatedValue : ""),
                            MinValue = attribute.MinValue,
                            MaxValue = attribute.MaxValue
                        };
                        value3.Setting = setting;
                        return value3;
                    }
                case DataTypeArgs.Decimal:
                    {
                        ValidationValue<decimal> value4 = new ValidationValue<decimal>
                        {
                            CurrentValue = (decimal)attribute.CurrentValue,
                            EvaluationType = attribute.EvaluationType,
                            DataType = DataTypeArgs.Decimal
                        };
                        DataValidationConfigSetting setting2 = new DataValidationConfigSetting
                        {
                            EvaluationType = attribute.EvaluationType,
                            DataType = DataTypeArgs.Decimal,
                            DirectValue = (attribute.EvaluationType == EvaluationTypeArgs.Exact) ? attribute.AllowedValue : ((attribute.EvaluationType == EvaluationTypeArgs.Negation) ? attribute.NegatedValue : ""),
                            MinValue = attribute.MinValue,
                            MaxValue = attribute.MaxValue
                        };
                        value4.Setting = setting2;
                        return value4;
                    }
                case DataTypeArgs.Date:
                    {
                        ValidationValue<DateTime> value5 = new ValidationValue<DateTime>
                        {
                            CurrentValue = (DateTime)attribute.CurrentValue,
                            EvaluationType = attribute.EvaluationType,
                            DataType = DataTypeArgs.Date
                        };
                        DataValidationConfigSetting setting3 = new DataValidationConfigSetting
                        {
                            EvaluationType = attribute.EvaluationType,
                            DataType = DataTypeArgs.Date,
                            DirectValue = (attribute.EvaluationType == EvaluationTypeArgs.Exact) ? attribute.AllowedValue : ((attribute.EvaluationType == EvaluationTypeArgs.Negation) ? attribute.NegatedValue : ""),
                            MinValue = attribute.MinValue,
                            MaxValue = attribute.MaxValue
                        };
                        value5.Setting = setting3;
                        return value5;
                    }
                case DataTypeArgs.Time:
                    {
                        ValidationValue<DateTime> value6 = new ValidationValue<DateTime>
                        {
                            CurrentValue = (DateTime)attribute.CurrentValue,
                            EvaluationType = attribute.EvaluationType,
                            DataType = DataTypeArgs.Time
                        };
                        DataValidationConfigSetting setting4 = new DataValidationConfigSetting
                        {
                            EvaluationType = attribute.EvaluationType,
                            DataType = DataTypeArgs.Time,
                            DirectValue = (attribute.EvaluationType == EvaluationTypeArgs.Exact) ? attribute.AllowedValue : ((attribute.EvaluationType == EvaluationTypeArgs.Negation) ? attribute.NegatedValue : ""),
                            MinValue = attribute.MinValue,
                            MaxValue = attribute.MaxValue
                        };
                        value6.Setting = setting4;
                        return value6;
                    }
                case DataTypeArgs.DateTime:
                    {
                        ValidationValue<DateTime> value7 = new ValidationValue<DateTime>
                        {
                            CurrentValue = (DateTime)attribute.CurrentValue,
                            EvaluationType = attribute.EvaluationType,
                            DataType = DataTypeArgs.DateTime
                        };
                        DataValidationConfigSetting setting5 = new DataValidationConfigSetting
                        {
                            EvaluationType = attribute.EvaluationType,
                            DataType = DataTypeArgs.DateTime,
                            DirectValue = (attribute.EvaluationType == EvaluationTypeArgs.Exact) ? attribute.AllowedValue : ((attribute.EvaluationType == EvaluationTypeArgs.Negation) ? attribute.NegatedValue : ""),
                            MinValue = attribute.MinValue,
                            MaxValue = attribute.MaxValue
                        };
                        value7.Setting = setting5;
                        return value7;
                    }
                case DataTypeArgs.Boolean:
                    {
                        ValidationValue<bool> value8 = new ValidationValue<bool>
                        {
                            CurrentValue = (bool)attribute.CurrentValue,
                            EvaluationType = attribute.EvaluationType,
                            DataType = DataTypeArgs.Boolean
                        };
                        DataValidationConfigSetting setting6 = new DataValidationConfigSetting
                        {
                            EvaluationType = attribute.EvaluationType,
                            DataType = DataTypeArgs.Boolean,
                            DirectValue = (attribute.EvaluationType == EvaluationTypeArgs.Exact) ? attribute.AllowedValue : ((attribute.EvaluationType == EvaluationTypeArgs.Negation) ? attribute.NegatedValue : "")
                        };
                        value8.Setting = setting6;
                        return value8;
                    }
            }
            ValidationValue<string> value9 = new ValidationValue<string>
            {
                CurrentValue = (string)attribute.CurrentValue,
                EvaluationType = attribute.EvaluationType,
                DataType = DataTypeArgs.String
            };
            DataValidationConfigSetting setting7 = new DataValidationConfigSetting
            {
                EvaluationType = attribute.EvaluationType,
                DataType = DataTypeArgs.String,
                DirectValue = (attribute.EvaluationType == EvaluationTypeArgs.Exact) ? attribute.AllowedValue : ((attribute.EvaluationType == EvaluationTypeArgs.Negation) ? attribute.NegatedValue : attribute.ValidationPattern)
            };
            value9.Setting = setting7;
            return value9;
        }
        #endregion

        #region Private Mathods
        private object GetPropertyValue(PropertyInfo property)
        {
            try
            {
                switch (property.PropertyType.Name)
                {
                    case "Int32":
                        return this.GetInteger(property);

                    case "Int16":
                        return this.GetInt16(property);

                    case "Int64":
                        return this.GetInt64(property);

                    case "Decimal":
                        return this.GetDecimal(property);

                    case "DateTime":
                        return this.GetDateTime(property);

                    case "Boolean":
                        return this.GetBoolean(property);

                    case "TimeSpan":
                        return this.GetTimeSpan(property);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private bool GetBoolean(PropertyInfo property)
        {
            try
            {
                return (bool)property.GetValue(this);
            }
            catch
            {
                return false;
            }
        }

        private DateTime GetDateTime(PropertyInfo property)
        {
            try
            {
                return (DateTime)property.GetValue(this);
            }
            catch
            {
                return new DateTime(0x76c, 1, 1);
            }
        }

        private decimal GetDecimal(PropertyInfo property)
        {
            try
            {
                return (decimal)property.GetValue(this);
            }
            catch
            {
                return 0M;
            }
        }

        private short GetInt16(PropertyInfo property)
        {
            try
            {
                return (short)property.GetValue(this);
            }
            catch
            {
                return 0;
            }
        }

        private long GetInt64(PropertyInfo property)
        {
            try
            {
                return (long)property.GetValue(this);
            }
            catch
            {
                return 0L;
            }
        }

        private int GetInteger(PropertyInfo property)
        {
            try
            {
                return (int)property.GetValue(this);
            }
            catch
            {
                return 0;
            }
        }

        private TimeSpan GetTimeSpan(PropertyInfo property)
        {
            try
            {
                return (TimeSpan)property.GetValue(this);
            }
            catch
            {
                return new TimeSpan(0, 0, 0);
            }
        }
        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class Krodzone
    {

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="W"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public W ExecuteSqlCreationCommand<T, W>(T obj)
            where T : ISqlObjectCommand<W>
            where W : new()
        {
            W ret = default(W);

            return ret;

        }

        #region Public Static Methods
        /// <summary>
        /// Creates an instance of T, which MUST have a public static method named CreateInstance 
        /// that accepts a single parameter of type System.Object[], and returns an instance of T
        /// </summary>
        /// <typeparam name="T">The type to be created</typeparam>
        /// <param name="row">The DataRow instance whose ItemArray will be passed to the CreateInstance static method</param>
        /// <returns></returns>
        public static T CreateObject<T>(DataRow row) 
            where T : ICommonObject, new()
        {
            Type typeOfT = typeof(T);
            T protoType = (T)Activator.CreateInstance(typeOfT, null);
            T obj = default(T);

            try
            {
                System.Reflection.MemberInfo method = protoType.GetType().GetMethod("CreateInstance");

                if (method != null)
                {
                    obj = (T)protoType.GetType().GetMethod("CreateInstance").Invoke(protoType, new object[] { row.ItemArray });
                }

            }
            catch
            {
                obj = default(T);
            }

            return obj;

        }


        /// <summary>
        /// Creates and return an instance of T
        /// </summary>
        /// <typeparam name="T">The type to be created</typeparam>
        /// <param name="row">The DataRow instance containing the data from which the instance will be created</param>
        /// <returns></returns>
        public static T CreateInstance<T>(DataRow row)
            where T : ICommonObject, new()
        {
            Type typeOfT = typeof(T);
            T obj;

            try
            {
                obj = (T)Activator.CreateInstance(typeOfT, null);

                if (row != null && obj != null)
                {

                    foreach (DataColumn col in row.Table.Columns)
                    {
                        System.Reflection.PropertyInfo property = obj.GetType().GetProperty(col.ColumnName);

                        property?.SetValue(obj, row[col.ColumnName]);
                    }

                }

            }
            catch
            {
                obj = default(T);
            }

            return obj;

        }
        #endregion

        #region Nested Objects
        internal interface IDataObjectController
        {

            #region Methods
            SqlExecutionCommandResult<object> BatchUpdate<T>(IDataSetting setting, IList<T> items) where T : ICommonObject, new();
            SqlExecutionCommandResult<object> SaveItem<T>(IDataSetting setting, T obj) where T : ICommonObject, new();
            SqlExecutionCommandResult<object> SaveItem(IDataSetting setting, object[] data);
            T GetScalar<T>(IDataSetting setting, object[] paramValues, T defaultValue);
            T GetItem<T>(IDataSetting setting, object[] paramValues) where T : ICommonObject, new();
            IList<T> GetList<T>(IDataSetting setting, object[] paramValues) where T : ICommonObject, new();
            #endregion

        }

        internal class ObjectController
        {

            #region Local Variables
            private static ObjectController _Controller;
            #endregion

            #region Constructor
            private ObjectController() { }
            #endregion

            #region Public Static Properties
            public IDataObjectController this[SqlExceptionEventHandler exceptionHandler] => (new DataObjectController(exceptionHandler));
            public static ObjectController Controller => _Controller ?? (_Controller = new ObjectController());
            #endregion

            #region Nested Objects
            internal class DataObjectController : IDataObjectController
            {

                #region Local Variables
                private SqlExceptionEventHandler _SqlExceptionHandler;
                #endregion

                #region Constructor
                internal DataObjectController(SqlExceptionEventHandler exceptionHandler)
                {
                    _SqlExceptionHandler = exceptionHandler;
                }
                #endregion
                
                #region Public Methods
                /// <summary>
                /// Performs a batch update for objects that implement the Orlans.ICommonObject interface and have a parameterless constructor
                /// </summary>
                /// <param name="setting">
                /// A Orlans.Data.IDataSetting implementation, containing the connection string, stored procedure name, list of parameter
                /// names and their subsequent data types, along with outbound parameter names and data types.
                /// </param>
                /// <param name="items">The list of objects containing the data to be saved</param>
                /// <returns></returns>
                public virtual SqlExecutionCommandResult<object> BatchUpdate<T>(IDataSetting setting, IList<T> items) where T : ICommonObject, new()
                {
                    SqlExecutionCommandResult<object> result = new SqlExecutionCommandResult<object>(SqlCommandResultArgs.Error, "Invalid Object");

                    if (items != null && setting != null)
                    {

                        using (SqlConnectionObject sco = new SqlConnectionObject(setting))
                        {

                            if (_SqlExceptionHandler != null)
                            {
                                sco.ExecutionError += _SqlExceptionHandler;
                            }

                            bool saveResult = true;

                            sco.Open();
                            sco.BeginTransaction();

                            foreach (ICommonObject item in items)
                            {
                                saveResult = sco.ExecuteNonQuery(item.ToArray());
                                if (!saveResult) { break; }
                            }

                            sco.EndTransaction(saveResult);

                            if (saveResult)
                            {
                                result = new SqlExecutionCommandResult<object>(SqlCommandResultArgs.Successful, "All Items Successfully Saved");
                            }

                        }

                    }

                    return result;

                }

                /// <summary>
                /// Saves an object, that implements the Orlans.ICommonObject interface and has a parameterless constructor, to the database
                /// </summary>
                /// <typeparam name="T">The object type to be saved</typeparam>
                /// <param name="setting">
                /// A Orlans.Data.IDataSetting implementation, containing the connection string, stored procedure name, list of parameter
                /// names and their subsequent data types, along with outbound parameter names and data types.
                /// </param>
                /// <param name="obj">The object containing the data to be saved</param>
                /// <returns></returns>
                public virtual SqlExecutionCommandResult<object> SaveItem<T>(IDataSetting setting, T obj) where T : ICommonObject, new()
                {
                    if (obj == null) { return new SqlExecutionCommandResult<object>(SqlCommandResultArgs.Error, "Object Not Initialized"); }

                    RegisterExceptionHandler();

                    SqlExecutionCommandResult<object> result = DatabaseConnector.Instance.SaveData(setting, obj.ToArray());

                    UnregisterExceptionHandler();

                    return result;

                }

                /// <summary>
                /// Saves the object data represented in parameter data to the database
                /// </summary>
                /// <param name="setting">
                /// A Orlans.Data.IDataSetting implementation, containing the connection string, stored procedure name, list of parameter
                /// names and their subsequent data types, along with outbound parameter names and data types.
                /// </param>
                /// <param name="data">The data to be saved</param>
                /// <returns></returns>
                public virtual SqlExecutionCommandResult<object> SaveItem(IDataSetting setting, object[] data)
                {
                    if (data == null) { return new SqlExecutionCommandResult<object>(SqlCommandResultArgs.Error, "Object Not Initialized"); }

                    RegisterExceptionHandler();

                    SqlExecutionCommandResult<object> result = DatabaseConnector.Instance.SaveData(setting, data);

                    UnregisterExceptionHandler();

                    return result;

                }

                /// <summary>
                /// Returns a single value retrieved as an outbound parameter
                /// </summary>
                /// <typeparam name="T">The value type to be returned</typeparam>
                /// <param name="setting">
                /// A Orlans.Data.IDataSetting implementation, containing the connection string, stored procedure name, list of parameter
                /// names and their subsequent data types, along with outbound parameter names and data types.
                /// </param>
                /// <param name="paramValues">The array of object containing the parameter values expected by the stored procedure</param>
                /// <param name="defaultValue">A default value to return</param>
                /// <returns></returns>
                public virtual T GetScalar<T>(IDataSetting setting, object[] paramValues, T defaultValue)
                {

                    RegisterExceptionHandler();

                    SqlExecutionCommandResult<object> result = DatabaseConnector.Instance.SaveData(setting, paramValues);
                    
                    if (result.Result == SqlCommandResultArgs.Successful)
                    {
                        return (T)result.Items[0];
                    }

                    UnregisterExceptionHandler();

                    return defaultValue;

                }

                /// <summary>
                /// Returns a single instance of T, that implements the Orlans.ICommonObject interface and has a parameterless constructor, retrieved from the database
                /// </summary>
                /// <typeparam name="T">The reference type to be returned</typeparam>
                /// <param name="setting">
                /// A Orlans.Data.IDataSetting implementation, containing the connection string, stored procedure name, list of parameter
                /// names and their subsequent data types, along with outbound parameter names and data types.
                /// </param>
                /// <param name="paramValues">The array of object containing the parameter values expected by the stored procedure</param>
                /// <returns></returns>
                public virtual T GetItem<T>(IDataSetting setting, object[] paramValues) where T : ICommonObject, new()
                {

                    RegisterExceptionHandler();

                    T item = default(T);

                    ISqlExecutionCommand<SqlExecutionCommandResult<T>> cmd = new SqlGetCommand<T>(setting, (paramValues != null ? true : false), paramValues);
                    IList<T> objects = DatabaseConnector.Instance.GetData<T>(cmd);

                    if (objects != null && objects.Count > 0)
                    {
                        item = objects[0];
                    }


                    UnregisterExceptionHandler();

                    return item;

                }

                /// <summary>
                /// Returns a list of T, that implements the Orlans.ICommonObject interface and has a parameterless constructor, retrieved from the database
                /// </summary>
                /// <typeparam name="T">The reference type to be returned</typeparam>
                /// <param name="setting">
                /// A Orlans.Data.IDataSetting implementation, containing the connection string, stored procedure name, list of parameter
                /// names and their subsequent data types, along with outbound parameter names and data types.
                /// </param>
                /// <param name="paramValues">The array of object containing the parameter values expected by the stored procedure</param>
                /// <returns></returns>
                public virtual IList<T> GetList<T>(IDataSetting setting, object[] paramValues) where T : ICommonObject, new()
                {

                    RegisterExceptionHandler();

                    ISqlExecutionCommand<SqlExecutionCommandResult<T>> cmd = new SqlGetCommand<T>(setting, (paramValues != null ? true : false), paramValues);
                    IList<T> items = (IList<T>)DatabaseConnector.Instance.GetData<T>(cmd);

                    UnregisterExceptionHandler();

                    return items;

                }
                #endregion

                #region Protected Methods
                protected virtual void RegisterExceptionHandler()
                {

                    if (_SqlExceptionHandler != null)
                    {
                        DatabaseConnector.Instance.ExecutionError += _SqlExceptionHandler;
                    }

                }

                protected virtual void UnregisterExceptionHandler()
                {

                    if (_SqlExceptionHandler != null)
                    {
                        DatabaseConnector.Instance.ExecutionError -= _SqlExceptionHandler;
                    }

                }
                #endregion

            }
            #endregion

        }
        #endregion

    }

    public class EncryptionManager
    {

        #region Local Variables
        protected readonly byte[] _Key;
        protected readonly byte[] _Vector;
        protected readonly string _EncryptionKey;
        protected readonly string _EncryptionVector;
        #endregion

        #region Constructor
        public EncryptionManager()
        {
            TripleDESCryptoServiceProvider provider = new TripleDESCryptoServiceProvider();

            provider.GenerateKey();
            provider.GenerateIV();

            _Key = provider.Key;
            _Vector = provider.IV;

            _EncryptionKey = Convert.ToBase64String(provider.Key);
            _EncryptionVector = Convert.ToBase64String(provider.IV);

            provider.Dispose();

        }

        public EncryptionManager(byte[] key, byte[] vector)
        {
            _Key = key;
            _Vector = vector;

            _EncryptionKey = Convert.ToBase64String(key);
            _EncryptionVector = Convert.ToBase64String(vector);
        }

        public EncryptionManager(string key, string vector)
        {
            _Key = Convert.FromBase64String(key);
            _Vector = Convert.FromBase64String(vector);

            _EncryptionKey = key;
            _EncryptionVector = vector;
        }
        #endregion

        #region Properties
        public byte[] Key => _Key;

        public byte[] Vector => _Vector;

        public string EncryptionKey => _EncryptionKey;

        public string EncryptionVector => _EncryptionVector;

        #endregion

        #region Public Methods
        public string Encrypt(string originalString)
        {
            TripleDESCryptoServiceProvider provider = new TripleDESCryptoServiceProvider
            {
                Key = Key,
                IV = Vector
            };


            byte[] bytes = Encoding.ASCII.GetBytes(originalString);
            ICryptoTransform encryptor = provider.CreateEncryptor();
            byte[] results = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);

            provider.Clear();
            provider.Dispose();

            return Convert.ToBase64String(results);

        }

        public string Decrypt(string str)
        {
            TripleDESCryptoServiceProvider provider = new TripleDESCryptoServiceProvider
            {
                Key = Key,
                IV = Vector
            };


            byte[] bytes = Convert.FromBase64String(str);
            ICryptoTransform decryptor = provider.CreateDecryptor();
            byte[] results = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);

            provider.Clear();
            provider.Dispose();

            return Encoding.ASCII.GetString(results);

    }
        #endregion

    }

    /// <summary>
    /// Provides extension methods for various system types
    /// </summary>
    public static class KrodzoneExtensions
    {

        #region Public Static Methods
        /// <summary>
        /// Formats output as hh:mm:ss
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string toShortTime(this TimeSpan value)
        {
            return $"{value.Hours.ToString("D2")}:{value.Minutes.ToString("D2")}:{value.Seconds.ToString("D2")}";
        }

        /// <summary>
        /// Formats output as Yes/No
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string toYesNo(this bool value)
        {
            return (value ? "Yes" : "No");
        }

        /// <summary>
        /// Formats output as On/Off
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string toOnOff(this bool value)
        {
            return (value ? "On" : "Off");
        }
        #endregion

    }

}
