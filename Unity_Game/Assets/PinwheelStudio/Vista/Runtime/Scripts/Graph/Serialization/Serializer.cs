#if VISTA
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    public static class Serializer
    {
        public struct TargetScope : IDisposable
        {
            public TargetScope(UnityEngine.Object target)
            {
                Serializer.target = target;
            }

            public void Dispose()
            {
                Serializer.target = null;
            }
        }

        [Serializable]
        public struct TypeInfo
        {
            [SerializeField]
            public string fullName;

            public bool IsValid
            {
                get
                {
                    return !string.IsNullOrEmpty(fullName);
                }
            }
        }

        [Serializable]
        public struct JsonObject : IEquatable<JsonObject>
        {
            [SerializeField]
            public TypeInfo typeInfo;

            [SerializeField]
            public string jsonData;

            public bool Equals(JsonObject other)
            {
                return String.Compare(typeInfo.fullName, other.typeInfo.fullName) == 0 &&
                        String.Compare(jsonData, other.jsonData) == 0;
            }

            public override string ToString()
            {
                return JsonUtility.ToJson(this);
            }
        }

        public static JsonObject NullElement
        {
            get
            {
                return new JsonObject()
                {
                    typeInfo = new TypeInfo(),
                    jsonData = null
                };
            }
        }

        public static UnityEngine.Object target { get; set; }

        public static TypeInfo GetTypeAsSerializedData(Type type)
        {
            return new TypeInfo
            {
                fullName = type.FullName
            };
        }

        public static Type GetTypeFromSerializedData(TypeInfo typeInfo)
        {
            if (!typeInfo.IsValid)
                return null;

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Type type = assembly.GetType(typeInfo.fullName);
                if (type != null)
                    return type;
            }

            return null;
        }

        public static JsonObject Serialize<T>(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item", $"Cannot serialize null item");
            }

            TypeInfo typeInfo = GetTypeAsSerializedData(item.GetType());
            string jsonData = JsonUtility.ToJson(item);

            JsonObject serializedElement = new JsonObject()
            {
                typeInfo = typeInfo,
                jsonData = jsonData
            };
            return serializedElement;
        }

        public static T Deserialize<T>(JsonObject item, params object[] constructorArgs) where T : class
        {
            if (!item.typeInfo.IsValid)
            {
                throw new ArgumentException($"Cannot deserialize the item, object type is invalid.");
            }

            Type type = GetTypeFromSerializedData(item.typeInfo);
            if (type == null)
            {
                throw new ArgumentException($"Cannot deserialize the item, the type [{item.typeInfo.fullName}] is not exist.");
            }

            T instance;
            try
            {
                CultureInfo culture = CultureInfo.CurrentCulture;
                BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                instance = Activator.CreateInstance(type, flags, null, constructorArgs, culture) as T;
            }
            catch
            {
                throw;
            }

            if (instance != null && !string.IsNullOrEmpty(item.jsonData))
            {
                JsonUtility.FromJsonOverwrite(item.jsonData, instance);
            }
            return instance;
        }

        public static List<JsonObject> Serialize<T>(IEnumerable<T> items)
        {
            List<JsonObject> serializedItems = new List<JsonObject>();
            if (items == null)
            {
                return serializedItems;
            }

            foreach (T i in items)
            {
                try
                {
                    serializedItems.Add(Serialize(i));
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e, target);
                }
            }
            return serializedItems;
        }

        public static List<T> Deserialize<T>(IEnumerable<JsonObject> items, params object[] constructorArgs) where T : class
        {
            List<T> deserializedItems = new List<T>();
            if (items == null)
            {
                return deserializedItems;
            }

            foreach (JsonObject i in items)
            {
                try
                {
                    T item = Deserialize<T>(i, constructorArgs);
                    deserializedItems.Add(item);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e, target);
                }
            }
            return deserializedItems;
        }
    }
}
#endif
