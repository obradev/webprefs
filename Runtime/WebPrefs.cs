using System.Runtime.InteropServices;
using System;
using UnityEngine;

namespace ObraDev.WebPrefs
{
    public static class WebPrefs
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            [DllImport("__Internal")]
            private static extern int SaveToLocalStorage(string masterKey, string masterString);

            [DllImport("__Internal")]
            private static extern IntPtr LocalStorageString(string masterKey);

            [DllImport("__Internal")]
            private static extern int SaveIDBKey(string masterKey, string key, string value);

            [DllImport("__Internal")]
            private static extern void LoadFromIDB(string dbName, string key);

            [DllImport("__Internal")]
            private static extern void DeleteFromIDB(string dbName, string key);

            [DllImport("__Internal")]
            private static extern void ClearAll(string masterKey);
        #endif
        
        private static string _idbString;
        private static bool _idbLoaded = false;
        
        private static string _masterKey;
        private static string MasterKey
        {
            get
            {
                if (string.IsNullOrEmpty(_masterKey))
                {
                    _masterKey = Application.identifier;
                    if (string.IsNullOrEmpty(_masterKey))
                        _masterKey = Application.productName.Replace(" ", "").ToLower();
                }
                return _masterKey;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Reload();
        }
        
        internal static void HandleIDBResult(string result)
        {
            _idbString = result;
            _idbLoaded = true;
            
            #if UNITY_WEBGL && !UNITY_EDITOR
                if (!string.IsNullOrEmpty(result) && result != "|")
                {
                    string master = FetchMasterString();
                    if (string.IsNullOrEmpty(master) || master == "|")
                    {
                        // localStorage was empty, restore from IDB
                        int writeResult = SaveToLocalStorage(MasterKey, result);
                        if (writeResult == 0)
                            Debug.LogError("WebPrefs failed to restore localStorage data from IDB backup.");
                        else
                            Debug.LogWarning("WebPrefs restored missing localStorage data from IDB backup.");
                    }
                }
            #endif
        }
        
        private static string FetchMasterString()
        {
            string master = "";
            #if UNITY_WEBGL && !UNITY_EDITOR
                            IntPtr ptr = LocalStorageString(MasterKey);
                            master = Marshal.PtrToStringUTF8(ptr);
                            Marshal.FreeHGlobal(ptr);
            #else
                        master = PlayerPrefs.GetString(MasterKey, "");
            #endif
            
            return master;
        }
        
        
        // PUBLIC INTERFACE

        /// <summary>
        /// Returns the full serialized localStorage string (useful for debugging).
        /// </summary>
        public static string GetRawData() => FetchMasterString();

        /// <summary>
        /// Reloads the data in memory from IndexedDB. (or saves via PlayerPrefs on non-web)
        /// </summary>
        public static void Reload()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
                 LoadFromIDB(MasterKey, "main");
            #else
                PlayerPrefs.Save();
            #endif
        }
        
        /// <summary>
        /// Saves a value of any supported type to persistent storage.
        /// On WebGL, saves to localStorage with an IndexedDB backup.
        /// On Standalone, saves through PlayerPrefs.
        /// </summary>
        /// <param name="key">The key to identify this value. Case-insensitive, spaces are removed automatically.</param>
        /// <param name="value">The value to save. Supported types: int, float, bool, string, Vector2, Vector3, Vector4, Quaternion, Color, Color32, SerializableTransform.</param>
        public static void Save(string key, object value)
        {
            string master = FetchMasterString();
            if (string.IsNullOrEmpty(master) || master == "|") 
                master = _idbLoaded ? _idbString : "";

            string serialized = WebPrefsSerializer.Serialize(key, value);

            if (WebPrefsSerializer.KeyExists(master, key))
                master = WebPrefsSerializer.RemoveEntry(master, key);

            master += serialized;

            #if UNITY_WEBGL && !UNITY_EDITOR
                if (System.Text.Encoding.UTF8.GetByteCount(master) > 10240)
                {
                    Debug.LogWarning("WebPrefs: Too much data in localStorage, saving to IndexedDB only.");
                    SaveIDBKey(MasterKey, "main", master);
                }
                else
                {
                    int result = SaveToLocalStorage(MasterKey, master);
                    if (result == 0)
                        Debug.LogError("WebPrefs: Failed to write to localStorage.");
                    SaveIDBKey(MasterKey, "main", master);
                }
            #else
                PlayerPrefs.SetString(MasterKey, master);
                PlayerPrefs.Save();
            #endif
        }
        
        /// <summary>
        /// Check if a key was saved anywhere.
        /// </summary>
        /// <param name="key">The key to look for.</param>
        /// <returns>The bool value on if the key was found.</returns>
        public static bool HasKey(string key)
        {
            return WebPrefsSerializer.KeyExists(FetchMasterString(), key);
        }

        /// <summary>
        /// Removes one key from the save data, and its value.
        /// </summary>
        /// <param name="key">The key to remove from the save data.</param>
        public static void DeleteKey(string key)
        {
            string master = FetchMasterString();
            master = WebPrefsSerializer.RemoveEntry(master, key);
    
            #if UNITY_WEBGL && !UNITY_EDITOR
                SaveToLocalStorage(MasterKey, master);
                SaveIDBKey(MasterKey, "main", master);
                _idbString = master;
            #else
                PlayerPrefs.SetString(MasterKey, master);
                PlayerPrefs.Save();
            #endif
        }

        /// <summary>
        /// Erases all player's save data.
        /// </summary>
        public static void ClearAllData()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
                ClearAll(MasterKey);
                _idbString = "";
                _idbLoaded = false;
            #else
                PlayerPrefs.DeleteAll();
            #endif
        }

        /// <summary>
        /// Loads a value from save data.
        /// </summary>
        /// <param name="key">The key the value was saved with.</param>
        /// <param name="defaultValue">Returned if the key doesn't exist. Defaults to the type's default value.</param>
        /// <returns>The saved value cast to T, or defaultValue if not found.</returns>
        public static T Load<T>(string key, T defaultValue = default(T))
        {
            string master = FetchMasterString();
            #if UNITY_WEBGL && !UNITY_EDITOR
                if (!WebPrefsSerializer.KeyExists(master, key))
                {
                    if (_idbLoaded)
                        master = _idbString;
                    else
                    {
                        Debug.LogWarning("WebPrefs could not find the key '" + key + "' in localStorage, and IndexedDB is not yet loaded. Reloading, please wait a moment before trying again.");
                        Reload();
                        return defaultValue;
                    }
                }

                if (!WebPrefsSerializer.KeyExists(master, key))
                {
                    Debug.LogWarning("WebPrefs could not find the key '" + key + "'. Make sure you saved this key before. Returned default value.");
                }
        
                return WebPrefsSerializer.Deserialize<T>(master, key, defaultValue);
            #else
                if (!WebPrefsSerializer.KeyExists(master, key))
                {
                    Debug.LogWarning("WebPrefs could not find the key '" + key + "'. Returned default value.");
                    return defaultValue;
                }
                return WebPrefsSerializer.Deserialize<T>(master, key, defaultValue);
            #endif
        }
        
        
        // LOAD ALTERNATIVE FUNCTIONS
        
        /// <summary>Loads a string value. Shorthand for Load&lt;string&gt;.</summary>
        public static string Load(string key, string defaultValue = null)
        {
            return Load<string>(key, defaultValue);
        }
        
        /// <summary>Loads a string value. Shorthand for Load&lt;string&gt;.</summary>
        public static string LoadString(string key, string defaultValue = null)
        {
            return Load<string>(key, defaultValue);
        }
        
        /// <summary>Loads an int value. Shorthand for Load&lt;int&gt;.</summary>
        public static int LoadInt(string key, int defaultValue = 0)
        {
            return Load<int>(key, defaultValue);
        }
        
        /// <summary>Loads a float value. Shorthand for Load&lt;float&gt;.</summary>
        public static float LoadFloat(string key, float defaultValue = 0f)
        {
            return Load<float>(key, defaultValue);
        }
        
        /// <summary>Loads a bool value. Shorthand for Load&lt;bool&gt;.</summary>
        public static bool LoadBool(string key, bool defaultValue = false)
        {
            return Load<bool>(key, defaultValue);
        }
        
        /// <summary>Loads a Vector2 value. Shorthand for Load&lt;Vector2&gt;.</summary>
        public static Vector2 LoadVector2(string key, Vector2 defaultValue = default(Vector2))
        {
            return Load<Vector2>(key, defaultValue);
        }
        
        /// <summary>Loads a Vector3 value. Shorthand for Load&lt;Vector3&gt;.</summary>
        public static Vector3 LoadVector3(string key, Vector3 defaultValue = default(Vector3))
        {
            return Load<Vector3>(key, defaultValue);
        }
        
        /// <summary>Loads a Vector4 value. Shorthand for Load&lt;Vector4&gt;.</summary>
        public static Vector4 LoadVector4(string key, Vector4 defaultValue = default(Vector4))
        {
            return Load<Vector4>(key, defaultValue);
        }
        
        /// <summary>Loads a Quaternion value. Shorthand for Load&lt;Quaternion&gt;.</summary>
        public static Quaternion LoadQuaternion(string key, Quaternion defaultValue = default(Quaternion))
        {
            return Load<Quaternion>(key, defaultValue);
        }
        
        /// <summary>Loads a Color value. Shorthand for Load&lt;Color&gt;.</summary>
        public static Color LoadColor(string key, Color defaultValue = default(Color))
        {
            return Load<Color>(key, defaultValue);
        }
        
        /// <summary>Loads a Color32 value. Shorthand for Load&lt;Color32&gt;.</summary>
        public static Color32 LoadColor32(string key, Color32 defaultValue = default(Color32))
        {
            return Load<Color32>(key, defaultValue);
        }
        
        /// <summary>Loads a SerializableTransform value. Shorthand for Load&lt;SerializableTransform&gt;.</summary>
        public static SerializableTransform LoadSTransform(string key, SerializableTransform defaultValue = default(SerializableTransform))
        {
            return Load<SerializableTransform>(key, defaultValue);
        }
    }

}