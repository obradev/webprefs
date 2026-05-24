mergeInto(LibraryManager.library, {
    
    SaveToLocalStorage: function(masterKey, masterString)
    {
        try {
            localStorage.setItem(UTF8ToString(masterKey), UTF8ToString(masterString));
            return 1;
        } catch (e) { return 0; }
    },

    LocalStorageString: function(masterKey)
    {
        try {
            var value = localStorage.getItem(UTF8ToString(masterKey));
            if (value === null) value = "";
            var lengthBytes = lengthBytesUTF8(value) + 1;
            var ptr = _malloc(lengthBytes);
            stringToUTF8(value, ptr, lengthBytes);
            return ptr;
        } catch (e) {
            var empty = "";
            var lengthBytes = lengthBytesUTF8(empty) + 1;
            var ptr = _malloc(lengthBytes);
            stringToUTF8(empty, ptr, lengthBytes);
            return ptr;
        }
    },

    SaveIDBKey: function(masterKey, key, value)
    {
        try {
            var dbname = UTF8ToString(masterKey);
            var k = UTF8ToString(key);
            var v = UTF8ToString(value);
            var request = indexedDB.open(dbname, 1);
            request.onupgradeneeded = function (e) {
                e.target.result.createObjectStore('data');
            };
            request.onsuccess = function (e) {
                var db = e.target.result;
                var tx = db.transaction('data', 'readwrite');
                tx.objectStore('data').put(v, k);
            };
            return 1;
        }
        catch (e) { return 0; }
    },

    LoadFromIDB: function (dbName, key) {
        try {
            var db_name = UTF8ToString(dbName);
            var k = UTF8ToString(key);
            var request = indexedDB.open(db_name, 1);
            request.onupgradeneeded = function (e) {
                e.target.result.createObjectStore('data');
            };
            request.onsuccess = function (e) {
                var db = e.target.result;
                var tx = db.transaction('data', 'readonly');
                var get = tx.objectStore('data').get(k);
                get.onsuccess = function () {
                    var result = get.result !== undefined ? get.result : "";
                    SendMessage("WebPrefs_Communication", "OnIDBLoad", result);
                };
            };
        } catch (e) {
            SendMessage("WebPrefs_Communication", "OnIDBLoad", "");
        }
    },

    DeleteFromIDB: function (dbName, key) {
        try {
            var db_name = UTF8ToString(dbName);
            var k = UTF8ToString(key);
            var request = indexedDB.open(db_name, 1);
            request.onupgradeneeded = function (e) {
                e.target.result.createObjectStore('data');
            };
            request.onsuccess = function (e) {
                var db = e.target.result;
                var tx = db.transaction('data', 'readwrite');
                tx.objectStore('data').delete(k);
            };
        } catch (e) {}
    },

    ClearAll: function (masterKey) {
        try {
            localStorage.removeItem(UTF8ToString(masterKey));
        } catch (e) {}
        try {
            indexedDB.deleteDatabase(UTF8ToString(masterKey));
        } catch (e) {}
    },
    
});