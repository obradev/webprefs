using UnityEngine;

namespace ObraDev.WebPrefs
{
    internal class WebPrefsListener : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            GameObject listener = new GameObject("WebPrefs_Communication");
            listener.AddComponent<WebPrefsListener>();
            DontDestroyOnLoad(listener);
            listener.hideFlags = HideFlags.HideInHierarchy;
        }
        
        internal void OnIDBLoad(string values)
        {
            WebPrefs.HandleIDBResult(values);
        }
    }
}