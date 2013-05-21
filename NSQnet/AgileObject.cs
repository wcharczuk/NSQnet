using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Collections;

namespace NSQnet
{
    /// <summary>
    /// Underlying type for fully dynamic objects. Member 'misses' result in null rather than runtime exceptions for easier ref checking. 
    /// </summary>
    public sealed class AgileObject : DynamicObject, IDynamicMetaObjectProvider, IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Destrier.JsonObject" /> class.
        /// </summary>
        public AgileObject()
        {
            _members = new Dictionary<string, object>();
        }

        /// <summary>
        /// The internal member dictionary.
        /// </summary>
        private Dictionary<string, object> _members;
        /// <summary>
        /// Gets the <see cref="T:System.Object" /> at the specified index.
        /// </summary>
        /// <value />
        public object this[int index]
        {
            get
            {
                return _members.ToList<KeyValuePair<string, object>>()[index];
            }
        }

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <value>The keys.</value>
        public ICollection<string> Keys
        {
            get
            {
                return _members.Keys; ;
            }
        }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        public ICollection<object> Values
        {
            get
            {
                ICollection<object> values = _members.Values;
                return values;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="T:System.Object" /> with the specified key.
        /// </summary>
        /// <value />
        public object this[string key]
        {
            get
            {
                return _members[key];
            }
            set
            {
                _members[key] = value;
            }
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get
            {
                return _members.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Adds the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(string key, object value)
        {
            _members.Add(key, value);
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// <c>true</c> if the specified key contains key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(string key)
        {
            return _members.ContainsKey(key);
        }

        /// <summary>
        ///       Removes the specified key.
        ///       </summary>
        /// <param name="key">The key.</param>
        /// <returns />
        public bool Remove(string key)
        {
            return _members.Remove(key);
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns />
        public bool TryGetValue(string key, out object value)
        {
            return _members.TryGetValue(key, out value);
        }

        /// <summary>
        /// Adds the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(KeyValuePair<string, object> item)
        {
            _members.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            _members.Clear();
        }

        /// <summary>
        /// Determines whether [contains] [the specified item].
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// <c>true</c> if [contains] [the specified item]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(KeyValuePair<string, object> item)
        {
            return _members.Contains(item);
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns />
        public bool Remove(KeyValuePair<string, object> item)
        {
            return _members.Remove(item.Key);
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns />
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _members.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _members.GetEnumerator();
        }

        /// <summary>
        /// Returns a json <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        /// A json <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </returns>
        /// <ensures inheritedFrom="M:System.Object.ToString" inheritedFromTypeName="Object">Contract.Result&lt;string&gt;() != null</ensures>
        public override string ToString()
        {
            return JsonSerializer.Current.SerializeObject(this); ;
        }

        /// <summary>
        /// Provides implementation for type conversion operations. Classes derived from the <see cref="T:System.Dynamic.DynamicObject" /> class can override this method to specify dynamic behavior for operations that convert an object from one type to another.
        /// </summary>
        /// <param name="binder">Provides information about the conversion operation. The binder.Type property provides the type to which the object must be converted. For example, for the statement (String)sampleObject in C# (CType(sampleObject, Type) in Visual Basic), where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject" /> class, binder.Type returns the <see cref="T:System.String" /> type. The binder.Explicit property provides information about the kind of conversion that occurs. It returns true for explicit conversion and false for implicit conversion.</param>
        /// <param name="result">The result of the type conversion operation.</param>
        /// <returns>
        /// Alwasy returns true.
        /// </returns>
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }
            Type targetType = binder.Type;
            bool result2;
            if (targetType == typeof(IEnumerable) || targetType == typeof(IEnumerable<KeyValuePair<string, object>>) || targetType == typeof(IDictionary<string, object>) || targetType == typeof(IDictionary))
            {
                result = this;
                result2 = true;
            }
            else
            {
                result2 = base.TryConvert(binder, out result);
            }
            return result2;
        }

        /// <summary>
        /// Provides the implementation for operations that delete an object member. This method is not intended for use in C# or Visual Basic.
        /// </summary>
        /// <param name="binder">Provides information about the deletion.</param>
        /// <returns>
        /// Alwasy returns true.
        /// </returns>
        public override bool TryDeleteMember(DeleteMemberBinder binder)
        {
            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }
            bool result = _members.Remove(binder.Name);
            return result;
        }

        /// <summary>
        /// Provides the implementation for operations that get a value by index. Classes derived from the <see cref="T:System.Dynamic.DynamicObject" /> class can override this method to specify dynamic behavior for indexing operations.
        /// </summary>
        /// <param name="binder">Provides information about the operation.</param>
        /// <param name="indexes">The indexes that are used in the operation. For example, for the sampleObject[3] operation in C# (sampleObject(3) in Visual Basic), where sampleObject is derived from the DynamicObject class, <paramref name="indexes" /> is equal to 3.</param>
        /// <param name="result">The result of the index operation.</param>
        /// <returns>
        /// Alwasy returns true.
        /// </returns>
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            bool result2;
            if (indexes.Length == 1)
            {
                result = ((IDictionary<string, object>)this)[(string)indexes[0]];
                result2 = true;
            }
            else
            {
                result = null;
                result2 = true;
            }
            return result2;
        }

        /// <summary>
        /// Provides the implementation for operations that get member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject" /> class can override this method to specify dynamic behavior for operations such as getting a value for a property.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member on which the dynamic operation is performed. For example, for the Console.WriteLine(sampleObject.SampleProperty) statement, where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject" /> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
        /// <param name="result">The result of the get operation. For example, if the method is called for a property, you can assign the property value to <paramref name="result" />.</param>
        /// <returns>
        /// Alwasy returns true.
        /// </returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            object value;
            bool result2;
            if (_members.TryGetValue(binder.Name, out value))
            {
                result = value;
                result2 = true;
            }
            else
            {
                result = null;
                result2 = true;
            }
            return result2;
        }

        /// <summary>
        /// Provides the implementation for operations that set a value by index. Classes derived from the <see cref="T:System.Dynamic.DynamicObject" /> class can override this method to specify dynamic behavior for operations that access objects by a specified index.
        /// </summary>
        /// <param name="binder">Provides information about the operation.</param>
        /// <param name="indexes">The indexes that are used in the operation. For example, for the sampleObject[3] = 10 operation in C# (sampleObject(3) = 10 in Visual Basic), where sampleObject is derived from the <see cref="T:System.Dynamic.DynamicObject" /> class, <paramref name="indexes" /> is equal to 3.</param>
        /// <param name="value">The value to set to the object that has the specified index. For example, for the sampleObject[3] = 10 operation in C# (sampleObject(3) = 10 in Visual Basic), where sampleObject is derived from the <see cref="T:System.Dynamic.DynamicObject" /> class, <paramref name="value" /> is equal to 10.</param>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.
        /// </returns>
        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            bool result;
            if (indexes.Length == 1)
            {
                ((IDictionary<string, object>)this)[(string)indexes[0]] = value;
                result = true;
            }
            else
            {
                result = base.TrySetIndex(binder, indexes, value);
            }
            return result;
        }

        /// <summary>
        /// Provides the implementation for operations that set member values. Classes derived from the <see cref="T:System.Dynamic.DynamicObject" /> class can override this method to specify dynamic behavior for operations such as setting a value for a property.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member to which the value is being assigned. For example, for the statement sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject" /> class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
        /// <param name="value">The value to set to the member. For example, for sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject" /> class, the <paramref name="value" /> is "Test".</param>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)
        /// </returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }
            _members[binder.Name] = value;
            bool result = true;
            return result;
        }

        /// <summary>
        /// Returns the enumeration of all dynamic member names.
        /// </summary>
        /// <returns>
        /// A sequence that contains dynamic member names.
        /// </returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            IEnumerable<string> result = _members.Keys.AsEnumerable<string>();
            return result;
        }
    }
}
