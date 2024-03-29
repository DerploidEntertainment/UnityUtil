﻿using UnityEngine;

namespace UnityUtil.Storage;

public class PlayerPrefsLocalPreferences : ILocalPreferences
{
    public void DeleteAll() => PlayerPrefs.DeleteAll();
    public void DeleteKey(string key) => PlayerPrefs.DeleteKey(getFullKey(key));
    public float GetFloat(string key, float defaultValue) => PlayerPrefs.GetFloat(getFullKey(key), defaultValue);
    public float GetFloat(string key) => PlayerPrefs.GetFloat(getFullKey(key));
    public int GetInt(string key, int defaultValue) => PlayerPrefs.GetInt(getFullKey(key), defaultValue);
    public int GetInt(string key) => PlayerPrefs.GetInt(getFullKey(key));
    public string GetString(string key, string defaultValue) => PlayerPrefs.GetString(getFullKey(key), defaultValue);
    public string GetString(string key) => PlayerPrefs.GetString(getFullKey(key));
    public bool HasKey(string key) => PlayerPrefs.HasKey(getFullKey(key));
    public void Save() => PlayerPrefs.Save();
    public void SetFloat(string key, float value) => PlayerPrefs.SetFloat(getFullKey(key), value);
    public void SetInt(string key, int value) => PlayerPrefs.SetInt(getFullKey(key), value);
    public void SetString(string key, string value) => PlayerPrefs.SetString(getFullKey(key), value);

    private static string getFullKey(string key) => key;   // We may want to concatenate other info with the provided key
}
