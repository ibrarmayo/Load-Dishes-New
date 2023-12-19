using System;
using Newtonsoft.Json;
using UnityEngine;

namespace SupersonicWisdomSDK
{
    public class SwPlayerPrefsStore : ISwKeyValueStore
    {
        #region --- Public Methods ---

        public void DeleteAll()
        {
            SwInfra.Logger.Log(EWisdomLogType.Cache, "DeleteAll");
            PlayerPrefs.DeleteAll();
        }

        public ISwKeyValueStore DeleteKey(string key)
        {
            if (key.SwIsNullOrEmpty()) return this;
            
            SwInfra.Logger.Log(EWisdomLogType.Cache, $"Key: {key}");
            PlayerPrefs.DeleteKey(key);
            
            return this;
        }

        public bool GetBoolean(string key, bool defaultValue = false)
        {
            if (key.SwIsNullOrEmpty()) return defaultValue;
            
            var value = PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
            SwInfra.Logger.Log(EWisdomLogType.KeyValueStore, $"SwPlayerPrefsStore | GetBoolean | {key} | {value}");

            return value;
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            if (key.SwIsNullOrEmpty()) return defaultValue;
            
            var value = PlayerPrefs.GetFloat(key, defaultValue);
            SwInfra.Logger.Log(EWisdomLogType.KeyValueStore, $"SwPlayerPrefsStore | GetFloat | {key} | {value}");
            
            return value;
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            if (key.SwIsNullOrEmpty()) return defaultValue;
            
            var value = PlayerPrefs.GetInt(key, defaultValue);
            SwInfra.Logger.Log(EWisdomLogType.KeyValueStore, $"SwPlayerPrefsStore | GetInt | {key} | {value}");
            
            return value;
        }

        public string GetString(string key, string defaultValue = "")
        {
            if (key.SwIsNullOrEmpty()) return defaultValue;
            
            var value = PlayerPrefs.GetString(key, defaultValue);
            SwInfra.Logger.Log(EWisdomLogType.KeyValueStore, $"SwPlayerPrefsStore | GetString | {key} | {value ?? "Null"}");
            
            return value;
        }

        public DateTime GetDate(string key, DateTime defaultValue)
        {
            var dateInString = GetString(key, string.Empty);

            try
            {
                return JsonConvert.DeserializeObject<DateTime>(dateInString, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                });
            }
            catch (Exception e)
            {
                SwInfra.Logger.Log(EWisdomLogType.KeyValueStore, $"{nameof(SwPlayerPrefsStore)} | {nameof(GetDate)} | {e.Message}");
                return defaultValue;
            }
        }

        public bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public void Save ()
        {
            PlayerPrefs.Save();
        }

        public ISwKeyValueStore SetBoolean(string key, bool value)
        {
            if (key.SwIsNullOrEmpty()) return this;
            
            SwInfra.Logger.Log(EWisdomLogType.KeyValueStore, $"{key} | {value}");
            PlayerPrefs.SetInt(key, value ? 1 : 0);

            return this;
        }

        public ISwKeyValueStore SetFloat(string key, float value)
        {
            if (key.SwIsNullOrEmpty()) return this;
            
            SwInfra.Logger.Log(EWisdomLogType.KeyValueStore, $"SwPlayerPrefsStore | SetFloat | {key} | {value}");
            PlayerPrefs.SetFloat(key, value);

            return this;
        }

        public ISwKeyValueStore SetInt(string key, int value)
        {
            if (key.SwIsNullOrEmpty()) return this;
            
            SwInfra.Logger.Log(EWisdomLogType.KeyValueStore, $"SwPlayerPrefsStore | SetInt | {key} | {value}");
            PlayerPrefs.SetInt(key, value);

            return this;
        }

        public ISwKeyValueStore SetString(string key, string value)
        {
            if (key.SwIsNullOrEmpty()) return this;
            
            SwInfra.Logger.Log(EWisdomLogType.Cache, $"{key} | {value ?? "Null"}");
            PlayerPrefs.SetString(key, value);

            return this;
        }

        public ISwKeyValueStore SetDate(string key, DateTime value)
        {
            var jsonString = JsonConvert.SerializeObject(value);
            return SetString(key, jsonString);
        }

        #endregion
    }
}