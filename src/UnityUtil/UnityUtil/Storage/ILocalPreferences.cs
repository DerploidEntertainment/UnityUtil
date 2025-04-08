namespace UnityUtil.Storage;

public interface ILocalPreferences
{
    /// <summary>
    /// Removes all keys and values from the preferences. Use with caution.
    /// </summary>
    void DeleteAll();
    /// <summary>
    /// Removes key and its corresponding value from the preferences.
    /// </summary>
    /// <param name="key"></param>
    void DeleteKey(string key);

    /// <summary>
    /// Returns the value corresponding to key in the preference file if it exists; otherwise, <paramref name="defaultValue"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    float GetFloat(string key, float defaultValue);
    /// <summary>
    /// Returns the value corresponding to key in the preference file if it exists; otherwise, zero.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    float GetFloat(string key);

    /// <summary>
    /// Returns the value corresponding to key in the preference file if it exists; otherwise, <paramref name="defaultValue"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    int GetInt(string key, int defaultValue);
    /// <summary>
    /// Returns the value corresponding to key in the preference file if it exists; otherwise, zero.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    int GetInt(string key);

    /// <summary>
    /// Returns the value corresponding to key in the preference file if it exists; otherwise, <paramref name="defaultValue"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    string GetString(string key, string defaultValue);
    /// <summary>
    /// Returns the value corresponding to key in the preference file if it exists; otherwise, <see cref="string.Empty"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    string GetString(string key);

    /// <summary>
    /// Returns true if key exists in the preferences.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    bool HasKey(string key);

    /// <summary>
    /// Writes all modified preferences to disk.
    /// </summary>
    void Save();

    /// <summary>
    /// Sets the value of the preference identified by key.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    void SetFloat(string key, float value);
    /// <summary>
    /// Sets the value of the preference identified by key.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    void SetInt(string key, int value);
    /// <summary>
    /// Sets the value of the preference identified by key.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    void SetString(string key, string value);
}
