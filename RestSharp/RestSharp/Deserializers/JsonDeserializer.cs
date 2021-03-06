﻿namespace RestSharp.Deserializers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Xml;
    using RestSharp.Extensions;

    public class JsonDeserializer : IDeserializer
    {
        public JsonDeserializer()
        {
            this.Culture = CultureInfo.InvariantCulture;
        }

        public string RootElement { get; set; }

        public string Namespace { get; set; }

        public string DateFormat { get; set; }

        public CultureInfo Culture { get; set; }

        public T Deserialize<T>(IRestResponse response)
        {
            var target = Activator.CreateInstance<T>();

            if (target is IList)
            {
                var objType = target.GetType();

                if (this.RootElement.HasValue())
                {
                    var root = this.FindRoot(response.Content);
                    target = (T)this.BuildList(objType, root);
                }
                else
                {
                    var data = SimpleJson.DeserializeObject(response.Content);
                    target = (T)this.BuildList(objType, data);
                }
            }
            else if (target is IDictionary)
            {
                var root = this.FindRoot(response.Content);
                target = (T)this.BuildDictionary(target.GetType(), root);
            }
            else
            {
                var root = this.FindRoot(response.Content);
                target = (T)this.Map(target, (IDictionary<string, object>)root);
            }

            return target;
        }

        private object FindRoot(string content)
        {
            var data = (IDictionary<string, object>)SimpleJson.DeserializeObject(content);

            if (this.RootElement.HasValue() && data.ContainsKey(this.RootElement))
            {
                return data[this.RootElement];
            }

            return data;
        }

        private object Map(object target, IDictionary<string, object> data)
        {
            var objType = target.GetType();
            var props = objType.GetProperties().Where(p => p.CanWrite).ToList();

            foreach (var prop in props)
            {
                var type = prop.PropertyType;
                var attributes = prop.GetCustomAttributes(typeof(DeserializeAsAttribute), false);
                string name;

                if (attributes.Length > 0)
                {
                    var attribute = (DeserializeAsAttribute)attributes[0];
                    name = attribute.Name;
                }
                else
                {
                    name = prop.Name;
                }

                var parts = name.Split('.');
                var currentData = data;
                object value = null;

                for (var i = 0; i < parts.Length; ++i)
                {
                    var actualName = parts[i].GetNameVariants(this.Culture).FirstOrDefault(currentData.ContainsKey);

                    if (actualName == null)
                    {
                        break;
                    }

                    if (i == parts.Length - 1)
                    {
                        value = currentData[actualName];
                    }
                    else
                    {
                        currentData = (IDictionary<string, object>)currentData[actualName];
                    }
                }

                if (value != null)
                {
                    prop.SetValue(target, this.ConvertValue(type, value), null);
                }
            }

            return target;
        }

        private IDictionary BuildDictionary(Type type, object parent)
        {
            var dict = (IDictionary)Activator.CreateInstance(type);
            var keyType = type.GetGenericArguments()[0];
            var valueType = type.GetGenericArguments()[1];

            foreach (var child in (IDictionary<string, object>)parent)
            {
                var key = keyType != typeof(string) ?
                    Convert.ChangeType(child.Key, keyType, CultureInfo.InvariantCulture) :
                    child.Key;

                object item;

                if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    item = this.BuildList(valueType, child.Value);
                }
                else
                {
                    item = this.ConvertValue(valueType, child.Value);
                }

                dict.Add(key, item);
            }

            return dict;
        }

        private IList BuildList(Type type, object parent)
        {
            var list = (IList)Activator.CreateInstance(type);
            var listType = type.GetInterfaces().First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));
            var itemType = listType.GetGenericArguments()[0];

            if (parent is IList)
            {
                foreach (var element in (IList)parent)
                {
                    if (itemType.IsPrimitive)
                    {
                        var item = this.ConvertValue(itemType, element);
                        list.Add(item);
                    }
                    else if (itemType == typeof(string))
                    {
                        if (element == null)
                        {
                            list.Add(null);
                            continue;
                        }

                        list.Add(element.ToString());
                    }
                    else
                    {
                        if (element == null)
                        {
                            list.Add(null);
                            continue;
                        }

                        var item = this.ConvertValue(itemType, element);
                        list.Add(item);
                    }
                }
            }
            else
            {
                list.Add(this.ConvertValue(itemType, parent));
            }

            return list;
        }

        private object ConvertValue(Type type, object value)
        {
            var stringValue = Convert.ToString(value, this.Culture);

            // check for nullable and extract underlying type
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // Since the type is nullable and no value is provided return null
                if (string.IsNullOrEmpty(stringValue))
                {
                    return null;
                }

                type = type.GetGenericArguments()[0];
            }

            if (type == typeof(object) && value != null)
            {
                type = value.GetType();
            }

            if (type.IsPrimitive)
            {
                return value.ChangeType(type, this.Culture);
            }

            if (type.IsEnum)
            {
                return type.FindEnumValue(stringValue, this.Culture);
            }

            if (type == typeof(Uri))
            {
                return new Uri(stringValue, UriKind.RelativeOrAbsolute);
            }

            if (type == typeof(string))
            {
                return stringValue;
            }

            if (type == typeof(DateTime)
#if !PocketPC
                || type == typeof(DateTimeOffset)
#endif
                )
            {
                DateTime dt;

                if (this.DateFormat.HasValue())
                {
                    dt = DateTime.ParseExact(stringValue, this.DateFormat, this.Culture);
                }
                else
                {
                    // try parsing instead
                    dt = stringValue.ParseJsonDate(this.Culture);
                }

#if PocketPC
                return dt;
#else
                if (type == typeof(DateTime))
                {
                    return dt;
                }

                if (type == typeof(DateTimeOffset))
                {
                    return (DateTimeOffset)dt;
                }
#endif
            }
            else if (type == typeof(decimal))
            {
                if (value is double)
                {
                    return (decimal)((double)value);
                }

                if (stringValue.Contains("e"))
                {
                    return decimal.Parse(stringValue, NumberStyles.Float, this.Culture);
                }

                return decimal.Parse(stringValue, this.Culture);
            }
            else if (type == typeof(Guid))
            {
                return string.IsNullOrEmpty(stringValue) ? Guid.Empty : new Guid(stringValue);
            }
            else if (type == typeof(TimeSpan))
            {
                TimeSpan timeSpan;
                if (TimeSpan.TryParse(stringValue, out timeSpan))
                {
                    return timeSpan;
                }

                // This should handle ISO 8601 durations
                return XmlConvert.ToTimeSpan(stringValue);
            }
            else if (type.IsGenericType)
            {
                var genericTypeDef = type.GetGenericTypeDefinition();

                if (genericTypeDef == typeof(List<>))
                {
                    return this.BuildList(type, value);
                }

                if (genericTypeDef == typeof(Dictionary<,>))
                {
                    var keyType = type.GetGenericArguments()[0];

                    // only supports Dict<string, T>()
                    if (keyType == typeof(string))
                    {
                        return this.BuildDictionary(type, value);
                    }
                }
                else
                {
                    // nested property classes
                    return this.CreateAndMap(type, value);
                }
            }
            else if (type.IsSubclassOfRawGeneric(typeof(List<>)))
            {
                // handles classes that derive from List<T>
                return this.BuildList(type, value);
            }
            else if (type == typeof(JsonObject))
            {
                // simplify JsonObject into a Dictionary<string, object> 
                return this.BuildDictionary(typeof(Dictionary<string, object>), value);
            }
            else
            {
                // nested property classes
                return this.CreateAndMap(type, value);
            }

            return null;
        }

        private object CreateAndMap(Type type, object element)
        {
            var instance = Activator.CreateInstance(type);

            this.Map(instance, (IDictionary<string, object>)element);

            return instance;
        }
    }
}
