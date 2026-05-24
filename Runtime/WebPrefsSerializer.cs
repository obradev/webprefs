using System;
using UnityEngine;

namespace ObraDev.WebPrefs
{
    internal static class WebPrefsSerializer
    {
        internal static string Serialize(string key, object value)
        {
            key = NormalizeKey(key);
            ValidateKey(key);
            if (value is string val) ValidateStringValue(val);

            key = "|" + key;
            
            string entry = value switch
            {
                int i    => key + "|0" + i,
                float f  => key + "|1" + f,
                bool b   => key + "|2" + (b ? "1" : "0"),
                string s => key + "|3" + s,
            
                Vector2 v2  => key + "|4" + 
                               v2.x.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," + 
                               v2.y.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
            
                Vector3 v3 => key + "|5" + 
                              v3.x.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," + 
                              v3.y.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," +  
                              v3.z.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
            
                Vector4 v4 => key + "|6" + 
                              v4.x.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," + 
                              v4.y.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," +  
                              v4.z.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," +
                              v4.w.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
            
                Quaternion q => key + "|7" +
                                q.x.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," +
                                q.y.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," +
                                q.z.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," +
                                q.w.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
            
                Color c => key + "|8" + ((Color32)c).r + "," + ((Color32)c).g + "," + ((Color32)c).b + "," + ((Color32)c).a,
                Color32 k => key + "|9" + k.r + "," + k.g + "," + k.b + "," + k.a,
                
                SerializableTransform t => key + "|T" +
                               t.position.x.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," +
                               t.position.y.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," +
                               t.position.z.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," +
                               t.rotation.x.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," +
                               t.rotation.y.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," +
                               t.rotation.z.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," +
                               t.scale.x.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," +
                               t.scale.y.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "," +
                               t.scale.z.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
            
                _ => throw new WebPrefsException("Cannot save value of type '" + value.GetType().Name + "', please check the docs for all currently supported types.")
            };
            return entry;
        }
        
        internal static string NormalizeKey(string value)
        {
            return value.Replace(" ", "").ToLower();
        }

        internal static bool KeyExists(string masterString, string key)
        {
            key = NormalizeKey(key);
            return masterString.IndexOf("|" + key + "|") != -1;
        }
        
        private static void ValidateKey(string key)
        {
            key = NormalizeKey(key);
            if (string.IsNullOrEmpty(key))
                throw new WebPrefsException("Key cannot be null or empty.");
    
            if (key.Contains("|"))
                throw new WebPrefsException("Key '" + key + "' contains an invalid character '|'. This character is reserved for internal serialization!");
        }
        
        private static void ValidateStringValue(string value)
        {
            if (value != null && value.Contains("|"))
                throw new WebPrefsException("String value contains an invalid character '|'. This character is reserved for internal serialization!");
        }
        
        internal static T Deserialize<T>(string masterString, string key, T defaultValue = default(T))
        {
            key = NormalizeKey(key);
            string search = "|" + key + "|";
            int keyIndex = masterString.IndexOf(search);
    
            if (keyIndex == -1)
                return defaultValue;
    
            int typeIndex = keyIndex + search.Length;
            char typeChar = masterString[typeIndex];
    
            int valueStart = typeIndex + 1;
            int valueEnd = masterString.IndexOf('|', valueStart);
            if (valueEnd == -1) valueEnd = masterString.Length;
            
            string rawValue = masterString.Substring(valueStart, valueEnd - valueStart);
    
            float[] values = (typeChar != '0' && typeChar != '1' && typeChar != '2' && typeChar != '3')
                ? ParseFloats(rawValue)
                : null;
            
            object result = typeChar switch
            {
                '0' => int.Parse(rawValue),
                '1' => float.Parse(rawValue),
                '2' => rawValue == "1",
                '3' => rawValue,
                '4' => new Vector2(values[0], values[1]),
                '5' => new Vector3(values[0], values[1], values[2]),
                '6' => new Vector4(values[0], values[1], values[2], values[3]),
                '7' => new Quaternion(values[0], values[1], values[2], values[3]),
                '8' => (Color)new Color32((byte)values[0], (byte)values[1], (byte)values[2], (byte)values[3]),
                '9' => new Color32((byte)values[0], (byte)values[1], (byte)values[2], (byte)values[3]),
                
                'T' => new SerializableTransform(
                    new Vector3(values[0], values[1], values[2]), 
                    new Vector3(values[3], values[4], values[5]), 
                       new Vector3(values[6], values[7], values[8])
                    ),
                
                _   => throw new WebPrefsException("Encountered invalid type value during loading. The save data is likely corrupted or saved incorrectly.")
            };
    
            try
            {
                return (T)result;
            }
            catch (InvalidCastException)
            {
                if (typeof(T) == typeof(string))
                    return (T)(object)result.ToString().ToLower();;
    
                throw new WebPrefsException("Key '" + key + "' is saved as type '" + result.GetType().Name + "', it cannot be cast to a '" + typeof(T).Name + "'.");
            }
        }
        
        internal static string RemoveEntry(string masterString, string key)
        {
            key = NormalizeKey(key);
            if (string.IsNullOrEmpty(masterString)) return masterString;
            
            string search = "|" + key + "|";
            int entryStart = masterString.IndexOf(search);
    
            if (entryStart == -1) return masterString;

            int typeCharIndex = entryStart + search.Length;
            int valueStart = typeCharIndex + 1;
            int valueEnd = masterString.IndexOf('|', valueStart);
            if (valueEnd == -1) valueEnd = masterString.Length;

            return masterString.Substring(0, entryStart) + masterString.Substring(valueEnd);
        }
        
        private static float[] ParseFloats(string raw)
        {
            string[] p = raw.Split(',');
            float[] result = new float[9];
            for (int i = 0; i < p.Length && i < 9; i++)
                result[i] = float.Parse(p[i], System.Globalization.CultureInfo.InvariantCulture);
            return result;
        }
    }
}