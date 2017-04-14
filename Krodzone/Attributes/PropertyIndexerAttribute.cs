using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Krodzone.Attributes
{
    
    public interface IPropertyIndexerAttribute
    {

        #region Properties
        int Sequence { get; set; }
        object DefaultValue { get; set; }
        #endregion

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyIndexerAttribute : Attribute, IPropertyIndexerAttribute
    {

        #region Constructor
        public PropertyIndexerAttribute(int sequence, object defaultValue) : base()
        {
            Sequence = sequence;
            DefaultValue = defaultValue;
        }
        #endregion

        #region Properties
        public int Sequence { get; set; }
        public object DefaultValue { get; set; }
        #endregion

        #region Public Static Methods
        public static object[] ToValueArray<T>(T obj) where T : ICommonObject, new()
        {
            IList<PropertyInfoOrganizer> propertyItems = new List<PropertyInfoOrganizer>();
            IList<object> items = new List<object>();

            if (obj != null)
            {

                foreach (PropertyInfo pi in obj.GetType().GetProperties())
                {

                    foreach (object piAttr in pi.GetCustomAttributes(true))
                    {

                        if (piAttr.GetType() == typeof(PropertyIndexerAttribute))
                        {
                            PropertyIndexerAttribute attr = (PropertyIndexerAttribute)piAttr;
                            propertyItems.Add(new PropertyInfoOrganizer(attr, pi));
                        }

                    }

                }


                foreach (PropertyInfoOrganizer pio in (from pi in propertyItems orderby pi.Attribute.Sequence select pi))
                {
                    object value = null;

                    try
                    {
                        value = pio.Property.GetValue(obj);
                    }
                    catch
                    {
                        value = pio.Attribute.DefaultValue;
                    }

                    items.Add(value);

                }

            }

            return items.ToArray();

        }

        public static object[] ToValueArray<T>(ObjectAuditBase<T> obj) where T : ICommonObject, new()
        {
            IList<PropertyInfoOrganizer> propertyItems = new List<PropertyInfoOrganizer>();
            IList<object> items = new List<object>();

            if (obj != null)
            {

                foreach (PropertyInfo pi in obj.GetType().GetProperties())
                {

                    foreach (object piAttr in pi.GetCustomAttributes(true))
                    {

                        if (piAttr.GetType() == typeof(PropertyIndexerAttribute))
                        {
                            PropertyIndexerAttribute attr = (PropertyIndexerAttribute)piAttr;
                            propertyItems.Add(new PropertyInfoOrganizer(attr, pi));
                        }

                    }

                }


                foreach (PropertyInfoOrganizer pio in (from pi in propertyItems orderby pi.Attribute.Sequence select pi))
                {
                    object value = null;

                    try
                    {
                        value = pio.Property.GetValue(obj);
                    }
                    catch
                    {
                        value = pio.Attribute.DefaultValue;
                    }

                    items.Add(value);

                }

            }

            return items.ToArray();

        }
        #endregion

        #region Nested Objects
        internal class PropertyInfoOrganizer
        {

            #region Local Variables
            protected readonly PropertyIndexerAttribute _Attribute;
            protected readonly PropertyInfo _Property;
            #endregion

            #region Constructor
            public PropertyInfoOrganizer(PropertyIndexerAttribute attribute, PropertyInfo property)
            {
                _Attribute = attribute;
                _Property = property;
            }
            #endregion

            #region Properties
            public PropertyIndexerAttribute Attribute => _Attribute;
            public PropertyInfo Property => _Property;
            #endregion

        }
        #endregion

    }

}
