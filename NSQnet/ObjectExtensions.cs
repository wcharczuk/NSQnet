using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSQnet.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Returns an object as a dynamic object. Use this to convert from an anonymous type parameter to a dynamic object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static dynamic ToDynamic(this Object obj)
        {
            if (obj == null)
                return null;

            dynamic value = new AgileObject();

            Type objectType = obj.GetType();

            foreach (var property in objectType.GetProperties())
            {
                var propertyName = property.Name;
                var propertyValue = property.GetValue(obj, null);

                ((IDictionary<string, object>)value)[propertyName] = propertyValue;
            }

            foreach (var field in objectType.GetFields())
            {
                var fieldName = field.Name;
                var fieldValue = field.GetValue(obj);

                ((IDictionary<string, object>)value)[fieldName] = fieldValue;
            }

            return value;
        }
    }
}
