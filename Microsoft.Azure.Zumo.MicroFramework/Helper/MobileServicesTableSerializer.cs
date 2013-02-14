using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.MobileServices;
using System.Text;
using System.Collections;

namespace Microsoft.Azure.Zumo.MicroFramework.Helper
{
    internal class MobileServicesTableSerializer
    {
        public static string Serialize(object obj)
        {
            bool terminatingcomma = false;
            StringBuilder json = new System.Text.StringBuilder("{");
            Type type = obj.GetType();

            Regex regex = new Regex(@"\<(.*?)\>");

            foreach (System.Reflection.FieldInfo fieldInfo in
                //TODO: NH flag selection could be optimized and prob want to ignore nonpublic
                //wtf is the GetProperties method in .NET MF?
               type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                string rawName = fieldInfo.Name;
                //only want properties
                if (regex.IsMatch(rawName))
                {                  
                    var propertyName = regex.Match(rawName).Groups[1].Value;

                    //TODO:  hack for stripping Id when it has a default value
                    if (propertyName.ToLower() != SerializableType.IdPropertyName || (propertyName.ToLower() == SerializableType.IdPropertyName && !SerializableType.IsDefaultIdValue(fieldInfo.GetValue(obj))))
                    {
                        json.Append("\"");
                        json.Append(propertyName);
                        json.Append("\":");
                        FormatValue(json, fieldInfo.GetValue(obj));
                        json.Append(",");
                    }
                    terminatingcomma = true;
                }
            }
            if (terminatingcomma)
                 json.Length -= 1;

            json.Append("}");  

            return json.ToString();
        }

        public static object Deserialize(string json, Type type)
        {
            object instance = null;
            //TODO:
            return instance;
        }

        //no System.Activator in .NET MF
        internal static object CreateInstance(Type t)
        {
            ConstructorInfo info = t.GetConstructor(null);
            return info.Invoke(null);
        }

        public static void Deserialize(Hashtable value, object instance, bool ignoreCustomSerialization = false)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            else if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }          
        }

        private static string TODOEscapeJSONString(string input)
        {
            return input;
        }

        private static void FormatValue(System.Text.StringBuilder json, object value)
        {
            Type type = value.GetType();


            if (type == typeof(System.String))
            {
                json.Append("\"");
                json.Append(TODOEscapeJSONString((string)value));
                json.Append("\"");
            }
            else if (type == typeof(System.Int16) ||
                     type == typeof(System.Int32) ||
                     type == typeof(System.Int64) ||
                     type == typeof(System.Double) ||
                     type == typeof(System.Single))
            {
                json.Append(value.ToString());
            }
            else if (type == typeof(System.DateTime))
            {
                json.Append("\"");
                json.Append(((DateTime)value).ToRoundtripDateString());
                json.Append("\"");
            }
            else if (type.IsArray)
            {
                bool terminatingComma = false;
                json.Append("[");

                foreach (object o in (Array)value)
                {
                    FormatValue(json, o);
                    json.Append(",");
                    terminatingComma = true;
                }

                if (terminatingComma)
                    json.Length -= 1;

                json.Append("]");
            }
            else
            {
                json.Append(TODOEscapeJSONString(value.ToString()));
            }
        }


    }
}
