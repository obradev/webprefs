using System.Runtime.InteropServices;
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

        private static readonly string MasterKey = Application.identifier;
        private static string _idbString;
        private static bool _idbLoaded = false;

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
                            Debug.Log("WebPrefs restored localStorage data from IDB backup.");
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
                        master = PlayerPrefs.GetString(MasterKey, "|");
            #endif
            
            return master;
        }
        
        
        // PUBLIC INTERFACE

        public static string GetRawData() => FetchMasterString();

        public static void Reload()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
                 LoadFromIDB(MasterKey, "main");
            #else
                PlayerPrefs.Save();
            #endif
        }
        
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

        public static bool HasKey(string key)
        {
            return WebPrefsSerializer.KeyExists(FetchMasterString(), key);
        }

        public static void DeleteKey(string key)
        {
            string master = FetchMasterString();
            master = WebPrefsSerializer.RemoveEntry(master, key);
    
            #if UNITY_WEBGL && !UNITY_EDITOR
                SaveToLocalStorage(MasterKey, master);
                DeleteFromIDB(MasterKey, key);
                _idbString = WebPrefsSerializer.RemoveEntry(_idbString, key);
            #else
                PlayerPrefs.SetString(MasterKey, master);
                PlayerPrefs.Save();
            #endif
        }

        public static void ClearAllData()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
                ClearAll(MasterKey);
            #endif
            PlayerPrefs.DeleteAll();
        }

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

        public static string LoadString(string key, string defaultValue = null)
        {
            return Load<string>(key, defaultValue);
        }
        
        public static int LoadInt(string key, int defaultValue = 0)
        {
            return Load<int>(key, defaultValue);
        }
        
        public static float LoadFloat(string key, float defaultValue = 0f)
        {
            return Load<float>(key, defaultValue);
        }
        
        public static bool LoadBool(string key, bool defaultValue = false)
        {
            return Load<bool>(key, defaultValue);
        }
        
        public static Vector2 LoadVector2(string key, Vector2 defaultValue = default(Vector2))
        {
            return Load<Vector2>(key, defaultValue);
        }
        
        public static Vector3 LoadVector3(string key, Vector3 defaultValue = default(Vector3))
        {
            return Load<Vector3>(key, defaultValue);
        }
        
        public static Vector4 LoadVector4(string key, Vector4 defaultValue = default(Vector4))
        {
            return Load<Vector4>(key, defaultValue);
        }
        
        public static Quaternion LoadQuaternion(string key, Quaternion defaultValue = default(Quaternion))
        {
            return Load<Quaternion>(key, defaultValue);
        }
        
        public static Color LoadColor(string key, Color defaultValue = default(Color))
        {
            return Load<Color>(key, defaultValue);
        }
        
        public static Color32 LoadColor32(string key, Color32 defaultValue = default(Color32))
        {
            return Load<Color32>(key, defaultValue);
        }
        
        public static SerializableTransform LoadSTransform(string key, SerializableTransform defaultValue = default(SerializableTransform))
        {
            return Load<SerializableTransform>(key, defaultValue);
        }
    }

}