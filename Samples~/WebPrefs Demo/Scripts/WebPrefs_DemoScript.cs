using ObraDev.WebPrefs;
using TMPro;
using UnityEngine;

public class WebPrefs_DemoScript : MonoBehaviour
{
    // Text Displays
    [SerializeField] private TextMeshProUGUI _storageDisplay;
    [SerializeField] private TextMeshProUGUI _loadDisplay;

    [SerializeField] private Transform sphere;

    // Values from the scene inputs
    
    private float numberValue = 0f;
    private string stringValue = "";
    private bool boolValue = true;
    
    private Vector2 vector2Value = Vector2.zero;
    private Vector3 vector3Value = Vector3.zero;
    private Vector4 vector4Value = Vector4.zero;
    
    // Keys we remember from the Input Fields
    private string _activeSaveKey;
    private string _activeLoadKey;
    private string _activeDeleteKey;
    
    void Start()
    {
        RefreshDisplay();
        
        // Loading the Sphere's Rotation and Scale
        SerializableTransform sTransform = WebPrefs.LoadSTransform("sphere");
        sphere.eulerAngles = sTransform.rotation;
        sphere.localScale = sTransform.scale;
    }

    void RefreshDisplay()
    {
        
        // Uses WebPrefs.FetchMasterString() to get the full string of save data.
        _storageDisplay.text = WebPrefs.GetRawData();
        Debug.Log(_storageDisplay.text);
        
        SerializableTransform sTransform = WebPrefs.LoadSTransform("sphere");
        sphere.eulerAngles = sTransform.rotation;
        sphere.localScale = sTransform.scale;
    }

    public void Load()
    {
        // Loads the value at _activeLoadKey as a string and writes it to the display, if the value exists
        _loadDisplay.text = WebPrefs.Load<string>(_activeLoadKey);
        
        // Alternatively use WebPrefs.LoadString(_activeLoadKey) to load the same way.
    }
    
    public void Delete()
    {
        // Uses WebPrefs.DeleteKey with our key to... delete the save data at that key.
        WebPrefs.DeleteKey(_activeDeleteKey);
        
        // Refreshing our full string to show the new change.
        RefreshDisplay();
    }

    public void ClearAll()
    {
        // This clears all of the data the player ever saved.
        WebPrefs.ClearAllData();
        RefreshDisplay();
    }
    
    public void Reload() { WebPrefs.Reload(); RefreshDisplay(); } // WebPrefs.Reload() just reloads the backup data from IndexedDB, usually not important.
    
    // Saving
    
    public void SaveNumber()
    {
        // Check if it's a whole number, so it can be saved as int instead
        if (numberValue % 1 == 0)
        {
            // Simply saving the value with the key, nothing special is happening here or changing. You can do this the same with all the types below.
            WebPrefs.Save(_activeSaveKey, (int)numberValue);
            RefreshDisplay();
            return;
        }
        WebPrefs.Save(_activeSaveKey, numberValue);
        RefreshDisplay();
    }

    public void SaveString() { WebPrefs.Save(_activeSaveKey, stringValue); RefreshDisplay(); }
    public void SaveBool() { WebPrefs.Save(_activeSaveKey, boolValue); RefreshDisplay(); }
    public void SaveVector2() { WebPrefs.Save(_activeSaveKey, vector2Value); RefreshDisplay(); }
    public void SaveVector3() { WebPrefs.Save(_activeSaveKey, vector3Value); RefreshDisplay(); }
    public void SaveVector4() { WebPrefs.Save(_activeSaveKey, vector4Value); RefreshDisplay(); }
    public void SaveQuaternion() { WebPrefs.Save(_activeSaveKey, new Quaternion(vector4Value.x, vector4Value.y, vector4Value.z, vector4Value.w)); RefreshDisplay(); }
    public void SaveColor() { WebPrefs.Save(_activeSaveKey, new Color(vector4Value.x, vector4Value.y, vector4Value.z, vector4Value.w)); RefreshDisplay(); }
    public void SaveColor32() { WebPrefs.Save(_activeSaveKey, new Color32((byte)vector4Value.x, (byte)vector4Value.y, (byte)vector4Value.z, (byte)vector4Value.w)); RefreshDisplay(); }
    public void SaveSphere() { WebPrefs.Save("sphere", new SerializableTransform(sphere)); RefreshDisplay(); } // Yes, we can save Transform properties as one entry.
    
    // Getting Values
    
    public void SetStringValue(string newValue)
    {
        stringValue = newValue;
    }

    public void SetNumberValue(string newValue)
    {
        numberValue = float.Parse(newValue);
    }

    public void SetBoolValue(bool isOn)
    {
        boolValue = isOn;
    }
    
    // Vector2
    public void SetVector2ValueX(string newValue)
    {
        vector2Value.x = float.Parse(newValue);
    }

    public void SetVector2ValueY(string newValue)
    {
        vector2Value.y = float.Parse(newValue);
    }
    
    // Vector3
    public void SetVector3ValueX(string newValue)
    {
        vector3Value.x = float.Parse(newValue);
    }

    public void SetVector3ValueY(string newValue)
    {
        vector3Value.y = float.Parse(newValue);
    }
    
    public void SetVector3ValueZ(string newValue)
    {
        vector3Value.z = float.Parse(newValue);
    }
    
    // Vector3
    public void SetVector4ValueX(string newValue)
    {
        vector4Value.x = float.Parse(newValue);
    }

    public void SetVector4ValueY(string newValue)
    {
        vector4Value.y = float.Parse(newValue);
    }
    
    public void SetVector4ValueZ(string newValue)
    {
        vector4Value.z = float.Parse(newValue);
    }
    
    public void SetVector4ValueW(string newValue)
    {
        vector4Value.w = float.Parse(newValue);
    }
    
    // Get the keys for Saving, Loading, Deleting from the Input Fields
    
    public void SetSaveKey(string newKey)
    {
        _activeSaveKey = newKey;
    }
    
    public void SetLoadKey(string newKey)
    {
        _activeLoadKey = newKey;
    }
    
    public void SetDeleteKey(string newKey)
    {
        _activeDeleteKey = newKey;
    }

    // Randomizing Sphere Rotation and Scale
    public void RandomizeSphere()
    {
        sphere.eulerAngles = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
        sphere.localScale = new Vector3(Random.Range(0.2f, 1.2f), Random.Range(0.2f, 1.2f), Random.Range(0.2f, 1.2f));
    }
}
