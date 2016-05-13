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
        bool Save();
        bool Delete();
        T Get(Expression<Func<T, bool>> predicate);
        IList<T> GetList(Expression<Func<T, bool>> predicate);
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
        DateTime UpdatedDate { get; set; }
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
        
        #region Properties
        /// <summary>
        /// Gets or sets the user id of the person who created an entry
        /// </summary>
        [SqlTableColumn("IsActive", SqlDbType.Bit, false)]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the user id of the person who created an entry
        /// </summary>
        [SqlTableColumn("CreatedBy", SqlDbType.Int, false)]
        public int CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date & time the entry was created
        /// </summary>
        [SqlTableColumn("DateCreated", SqlDbType.DateTime, false)]
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Gets or sets the user id of the person who updated an entry
        /// </summary>
        [SqlTableColumn("UpdatedBy", SqlDbType.Int, false)]
        public int UpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date & time the entry was updated
        /// </summary>
        [SqlTableColumn("DateUpdated", SqlDbType.DateTime, false)]
        public DateTime DateUpdated { get; set; }
        #endregion

    }

    /// <summary>
    /// Base class for all objects in this solution
    /// </summary>
    public abstract class ObjectAuditBase<T> : IAuditBase<T>
    {

        #region Local Variables
        protected static Type parameterType;
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
        [SqlTableColumn("IsActive", SqlDbType.Bit, false)]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the user id of the person who created an entry
        /// </summary>
        [SqlTableColumn("CreatedBy", SqlDbType.VarChar, false, Length = 100)]
        public T CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date & time the entry was created
        /// </summary>
        [SqlTableColumn("CreatedDate", SqlDbType.DateTime, false)]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the user id of the person who updated an entry
        /// </summary>
        [SqlTableColumn("UpdatedBy", SqlDbType.VarChar, false, Length = 100)]
        public T UpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date & time the entry was updated
        /// </summary>
        [SqlTableColumn("UpdatedDate", SqlDbType.DateTime, false)]
        public DateTime UpdatedDate { get; set; }
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
