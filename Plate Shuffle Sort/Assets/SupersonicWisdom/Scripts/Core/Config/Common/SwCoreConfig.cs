using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace SupersonicWisdomSDK
{
    [Serializable]
    internal class SwCoreConfig : ISwCoreInternalConfig
    {
        #region --- Constants ---

        private const string CONFIG_KEY = "config";

        #endregion


        #region --- Members ---

        /// <summary>
        ///     The dictionary under "config" key.
        ///     This cannot be simply deserialized since it's a dynamic dictionary
        /// </summary>
        [NotNull] protected Dictionary<string, object> _dynamicConfig;

        public SwAbConfig ab;

        #endregion


        #region --- Properties ---

        public Dictionary<string, object> DynamicConfig
        {
            get { return _dynamicConfig; }
            internal set { _dynamicConfig = value; }
        }

        public SwAbConfig Ab
        {
            get { return ab; }
        }

        #endregion


        #region --- Construction ---

        public SwCoreConfig(Dictionary<string, object> defaultDynamicConfig)
        {
            _dynamicConfig = defaultDynamicConfig;
        }

        #endregion


        #region --- Public Methods ---

        public void SetDynamicData(string json)
        {
            var responseDictionary = SwJsonParser.DeserializeToDictionary(json);
            _dynamicConfig = responseDictionary.SwSafelyGet(CONFIG_KEY, _dynamicConfig) as Dictionary<string, object> ?? _dynamicConfig;
        }

        public int GetValue(int defaultVal, params string[] keys)
        {
            if (keys == null || keys.Length == 0) return defaultVal;
            
            foreach (var key in keys)
            {
                if (_dynamicConfig.ContainsKey(key))
                {
                    return _dynamicConfig.GetValue(key, defaultVal);
                }
            }

            return defaultVal;
        }

        public int GetValue(string key, int defaultVal)
        {
            return _dynamicConfig.GetValue(key, defaultVal);
        }

        public float GetValue(string key, float defaultVal)
        {
            return _dynamicConfig.GetValue(key, defaultVal);
        }

        public bool GetValue(string key, bool defaultVal)
        {
            return _dynamicConfig.GetValue(key, defaultVal);
        }
        
        public bool GetValue(bool defaultVal, params string[] keys)
        {
            if (keys == null || keys.Length == 0) return defaultVal;
            
            foreach (var key in keys)
            {
                if (_dynamicConfig.ContainsKey(key))
                {
                    return _dynamicConfig.GetValue(key, defaultVal);
                }
            }

            return defaultVal;
        }

        public string GetValue(string key, string defaultVal)
        {
            return _dynamicConfig.GetValue(key, defaultVal);
        }

        public Dictionary<string, object> AsDictionary()
        {
            return _dynamicConfig.AsDictionary();
        }

        public bool HasConfigKey(string key)
        {
            return _dynamicConfig.HasConfigKey(key);
        }

        #endregion
    }
}