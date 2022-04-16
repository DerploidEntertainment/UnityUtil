namespace UnityEngine.Storage;

public interface ILocalCache
{
    /// <summary>
    /// Removes all keys and values from the preferences. Use with caution.
    /// </summary>
    void DeleteAll();
    /// <summary>
    /// Removes key and its corresponding value from the preferences.
    /// </summary>
    void DeleteKey(string key);
    /// <summary>
    /// Returns the value corresponding to key in the preference file if it exists.
    /// </summary>
    float GetFloat(string key, float defaultValue);
    /// <summary>
    /// Returns the value corresponding to key in the preference file if it exists.
    /// </summary>
    float GetFloat(string key);
    /// <summary>
    /// Returns the value corresponding to key in the preference file if it exists.
    /// </summary>
    int GetInt(string key, int defaultValue);
    /// <summary>
    /// Returns the value corresponding to key in the preference file if it exists.
    /// </summary>
    int GetInt(string key);
    /// <summary>
    /// Returns the value corresponding to key in the preference file if it exists.
    /// </summary>
    string GetString(string key, string defaultValue);
    /// <summary>
    /// Returns the value corresponding to key in the preference file if it exists.
    /// </summary>
    string GetString(string key);
    /// <summary>
    /// Returns true if key exists in the preferences.
    /// </summary>
    bool HasKey(string key);
    /// <summary>
    /// Writes all modified preferences to disk.
    /// </summary>
    void Save();
    /// <summary>
    /// Sets the value of the preference identified by key.
    /// </summary>
    void SetFloat(string key, float value);
    /// <summary>
    /// Sets the value of the preference identified by key.
    /// </summary>
    void SetInt(string key, int value);
    /// <summary>
    /// Sets the value of the preference identified by key.
    /// </summary>
    void SetString(string key, string value);
}
