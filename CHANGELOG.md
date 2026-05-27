# v0.1.0 
- No changes, built core package.

# v0.1.1 
- Fixed crucial bugs, first public release.

# v0.1.2
- Added XML tags for documentation.
- Fixed a major bug with deleting keys.
- Fixed a minor bug with parsing saved float values.

# v0.1.3
- Added simple Load() method to fallback to string.
- Added automatic backup reloading every 30 seconds.
- Very minor other tweaks.

# v0.2.0
- Added a full Events system with 6 subscribable Actions:
  - OnKeySaved(string key)
  - OnKeyDeleted(string key)
  - OnDataCleared
  - OnDataRestored
  - OnSaveFailed
  - OnStorageFull
- Added GetKeyType(string key) - Returns the Type a key was saved as, or null if not found.
- Added GetAllKeys() - Returns a string array of all currently saved keys.