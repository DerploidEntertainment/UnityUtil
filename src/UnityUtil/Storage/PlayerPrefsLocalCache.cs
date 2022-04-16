namespace UnityEngine.Storage;

public class PlayerPrefsLocalCache : ILocalCache
{
    public void DeleteAll() => PlayerPrefs.DeleteAll();
    public void DeleteKey(string key) => PlayerPrefs.DeleteKey(key);
    public float GetFloat(string key, float defaultValue) => PlayerPrefs.GetFloat(key, defaultValue);
    public float GetFloat(string key) => PlayerPrefs.GetFloat(key);
    public int GetInt(string key, int defaultValue) => PlayerPrefs.GetInt(key, defaultValue);
    public int GetInt(string key) => PlayerPrefs.GetInt(key);
    public string GetString(string key, string defaultValue) => PlayerPrefs.GetString(key, defaultValue);
    public string GetString(string key) => PlayerPrefs.GetString(key);
    public bool HasKey(string key) => PlayerPrefs.HasKey(key);
    public void Save() => PlayerPrefs.Save();
    public void SetFloat(string key, float value) => PlayerPrefs.SetFloat(key, value);
    public void SetInt(string key, int value) => PlayerPrefs.SetInt(key, value);
    public void SetString(string key, string value) => PlayerPrefs.SetString(key, value);
}
