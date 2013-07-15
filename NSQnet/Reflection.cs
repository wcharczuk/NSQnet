using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NSQnet.Utility
{
    public class Reflection
    {
        private static ConcurrentDictionary<Type, Func<object>> _ctorCache = new ConcurrentDictionary<Type, Func<object>>();
        private static ConcurrentDictionary<PropertyInfo, Action<Object, Object>>   _compiledSetActions     = new ConcurrentDictionary<PropertyInfo, Action<object, object>>();
        private static ConcurrentDictionary<PropertyInfo, Func<Object, Object>>     _compiledGetFunctions   = new ConcurrentDictionary<PropertyInfo, Func<object, object>>();
        private static ConcurrentDictionary<Type, Dictionary<String, Action<Object, Object>>> _setters = new ConcurrentDictionary<Type, Dictionary<String, Action<object, object>>>();
        private static ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new ConcurrentDictionary<Type, PropertyInfo[]>();

        private static Func<Type, Func<object>> _CtorHelperFunc = ConstructorCreationHelper;

        public static Func<object> ConstructorCreationHelper(Type target)
        {
            return Expression.Lambda<Func<object>>(Expression.New(target)).Compile();
        }

        public static object GetNewObject(Type toConstruct)
        {
            return _ctorCache.GetOrAdd(toConstruct, _CtorHelperFunc)();
        }

        public static T GetNewObject<T>()
        {
            var neededType = typeof(T);
            var ctor = _ctorCache.GetOrAdd(neededType, _CtorHelperFunc);

            return (T)ctor();
        }

        public static PropertyInfo[] GetProperties(Type type)
        {
            return _propertyCache.GetOrAdd(type, (t) =>
            {
                return type.GetProperties();
            });
        }

        public static Func<Object, Object> GetGetAction(PropertyInfo property)
        {
            return _compiledGetFunctions.GetOrAdd(property, (p) =>
            {
                return GenerateGetFunction(p);
            });
        }

        public static Action<Object, Object> GetSetAction(PropertyInfo property)
        {
            return _compiledSetActions.GetOrAdd(property, (p) =>
            {
                return GenerateSetAction(p);
            });
        }

        public static Dictionary<String, Action<Object, Object>> GetSetters(Type type)
        {
            return _setters.GetOrAdd(type, (t) =>
            {
                var dict = new Dictionary<String, Action<Object, Object>>();
                foreach (var pi in GetProperties(t))
                {
                    dict.Add(pi.Name.ToLower(), GetSetAction(pi));
                }
                return dict;
            });
        }

        public static Action<Object, Object> GenerateSetAction(PropertyInfo propertyInfo)
        {
            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var valueParameter = Expression.Parameter(typeof(object), "value");
            var lambda = Expression.Lambda<Action<object, object>>(
                Expression.Assign(
                    Expression.Property(Expression.Convert(instanceParameter, propertyInfo.DeclaringType), propertyInfo),
                    Expression.Convert(valueParameter, propertyInfo.PropertyType)),
                instanceParameter,
                valueParameter
            );

            return lambda.Compile();
        }

        public static Func<Object, Object> GenerateGetFunction(PropertyInfo propertyInfo)
        {
            var instance = Expression.Parameter(propertyInfo.DeclaringType, "instance");
            var property = Expression.Property(instance, propertyInfo);
            var convert = Expression.TypeAs(property, typeof(Object));
            return (Func<Object, Object>)Expression.Lambda(convert, instance).Compile();
        }

        public static T MarshallAs<T>(IDictionary<String, Object> parameters) where T : new()
        {
            var instance = GetNewObject<T>();
            var setters = GetSetters(typeof(T));

            foreach (var kvp in parameters)
            {
                if (setters.ContainsKey(kvp.Key.ToLower()))
                    setters[kvp.Key.ToLower()](instance, kvp.Value);
            }

            return instance;
        }
    }
}
