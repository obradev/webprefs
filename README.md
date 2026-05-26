# WebPrefs

WebPrefs is a package which serves as a replacement to Unity's PlayerPrefs saving system made to integrate much better into WebGL games. WebPrefs makes your save data harder to lose after publishing new game builds, and gives you much more power and flexibility with the data you are saving.

By default, PlayerPrefs simply store data inside the `IndexedDB`, but this is not great because we lose access to this data if we ever make a new build of the game. Instead this gets around that by using a JavaScript bridge to save to the browser's `localStorage` instead, along with keeping backups and fallbacks inside the `IndexedDB`, so your data is super hard to lose!

Even though this was made for WebGL games, simply calling WebPrefs to save your data will also properly channel it through PlayerPrefs, using the custom serialization to allow for all of the more advanced data types this lets you save, so don't worry about any PC port headaches for your web game.
 
# HOW TO INSTALL:

1. Open the Package Manager inside Unity.
2. Click on the '+' icon in the top-left corner, then click 'Install package from git URL'.
3. Paste 'https://github.com/obradev/WebPrefs.git' into the textbox.
4. It should be installed now, you can head over to 'Samples' for an optional demo scene.

# GETTING STARTED:

Getting started is as simple as adding the using directive to the top of your script

`using ObraDev.WebPrefs;`

After this you instantly start using the package to start saving data.

**SAVING VALUES:**

Saving data is as simple as doing

`WebPrefs.Save(string key, object value);` - Key is the name you will know the data by, for example 'coins' will represent the player's coin counter, and value can be various different types of data, such as string, int, float, bool, etc.

To save the player's coins we can simply do `WebPrefs.Save("coins", 100);`

**LOADING VALUES:**

There are 2 ways to load data:

1. `WebPrefs.Load<T>(string key, T defaultValue = default(T))`, if we wanted to for example load those coins from before, we would do `int coins = WebPrefs.Load<int>("coins")` (we use `<int>` to specify we want the data back as an int value)
   - `defaultValue` is an optional parameter you can add to specify what it should return in case the key isn't found. Example: `int coins = WebPrefs.Load<int>("coins", 0)`
   
2. Premade methods, such as `WebPrefs.LoadInt(string key)`, similar idea, we do `int coins = WebPrefs.LoadInt("coins")`


**CHECKING VALUES:**

`WebPrefs.HasKey(string key)` - Checks if a value with the key exists on the device. (returns a simple bool value)


**DELETING VALUES:**

`WebPrefs.DeleteKey(string key)` - Clears the data stored with that key.

`WebPrefs.ClearAllData()` - Clears all the data.

**OTHER METHODS:**

`WebPrefs.Reload()` - Reloads the data from the backup IndexedDB, or saves the PlayerPrefs data if not on a web build.

`WebPrefs.GetRawData()` - Returns the full serialized string from the `localStorage`, useful for debugging.


# AVAILABLE DATA TYPES

`int`
`float`
`string`
`bool`
`Vector2`
`Vector3`
`Vector4`
`Quaternion`
`Color`
`Color32`

.. And there is a custom data type `SerializableTransform` which you can also store, it can be instantiated as `new SerializableTransform(transform)` or by assigning Position, Rotation, Scale, and it can hold all of those values together under one key.

# FINAL NOTES

- Keys are case insensitive, so you don't need to worry about capitalization, spaces will also be removed from keys automatically.
  
- The project's 'Product Name' is used as the key for the localStorage database, name your project something unique so it won't ever overlap with other games or data.

## That's just about everything to get you started!
