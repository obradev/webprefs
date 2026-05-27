using UnityEngine;

namespace ObraDev.WebPrefs
{
    internal class WebPrefsListener : MonoBehaviour
    {
        private static float _reloadTimer = 0f;
        private const float ReloadInterval = 30f;

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

        private void Update()
        {
            _reloadTimer += Time.deltaTime;
            if (_reloadTimer >= ReloadInterval)
            {
                _reloadTimer = 0f;
                WebPrefs.Reload();
            }
        }
    }
}