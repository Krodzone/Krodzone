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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Krodzone.Attributes;
using Krodzone.SQL;

namespace Krodzone
{

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

        public void something()
        {
            System.Data.DataRow row = null;
        }
        /// <summary>
        /// Creates an instance of T, which MUST have a public static method named CreateInstance 
        /// that accepts a single parameter of type System.Object[], and returns an instance of T
        /// </summary>
        /// <typeparam name="T">The type to be created</typeparam>
        /// <param name="data">The array of object that is to be passed to the CreateInstance static method</param>
        /// <returns></returns>
        public static T CreateObject<T>(object[] data) 
            where T : ICommonObject, new()
        {
            Type typeOfT = typeof(T);
            T protoType = (T)Activator.CreateInstance(typeOfT, null);
            T obj = default(T);

            try
            {
                obj = (T)protoType.GetType().GetMethod("CreateInstance").Invoke(protoType, new object[] { data });
            }
            catch (Exception e)
            {
                obj = default(T);
            }

            return obj;

        }
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

            this._Key = provider.Key;
            this._Vector = provider.IV;

            this._EncryptionKey = Convert.ToBase64String(provider.Key);
            this._EncryptionVector = Convert.ToBase64String(provider.IV);

            provider.Dispose();

        }

        public EncryptionManager(byte[] key, byte[] vector)
        {
            this._Key = key;
            this._Vector = vector;

            this._EncryptionKey = Convert.ToBase64String(key);
            this._EncryptionVector = Convert.ToBase64String(vector);
        }

        public EncryptionManager(string key, string vector)
        {
            this._Key = Convert.FromBase64String(key);
            this._Vector = Convert.FromBase64String(vector);

            this._EncryptionKey = key;
            this._EncryptionVector = vector;
        }
        #endregion

        #region Properties
        public byte[] Key
        {
            get { return this._Key; }
        }

        public byte[] Vector
        {
            get { return this._Vector; }
        }

        public string EncryptionKey
        {
            get { return this._EncryptionKey; }
        }

        public string EncryptionVector
        {
            get { return this._EncryptionVector; }
        }
        #endregion

        #region Public Methods
        public string Encrypt(string originalString)
        {
            TripleDESCryptoServiceProvider provider = new TripleDESCryptoServiceProvider();

            provider.Key = this.Key;
            provider.IV = this.Vector;

            byte[] bytes = Encoding.ASCII.GetBytes(originalString);
            ICryptoTransform encryptor = provider.CreateEncryptor();
            byte[] results = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);

            provider.Clear();
            provider.Dispose();

            return Convert.ToBase64String(results);

        }

        public string Decrypt(string str)
        {
            TripleDESCryptoServiceProvider provider = new TripleDESCryptoServiceProvider();

            provider.Key = this.Key;
            provider.IV = this.Vector;

            byte[] bytes = Convert.FromBase64String(str);
            ICryptoTransform decryptor = provider.CreateDecryptor();
            byte[] results = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);

            provider.Clear();
            provider.Dispose();

            return Encoding.ASCII.GetString(results);

    }
        #endregion

}
}
