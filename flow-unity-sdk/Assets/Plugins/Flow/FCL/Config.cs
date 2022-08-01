using System;
using System.Collections.Generic;
using System.Linq;

namespace Flow.FCL
{
    public class Config
    {
        private static Dictionary<string, string> _configDict = new Dictionary<string, string>();
        public Config Put(string key, string value)
        {
            Config._configDict.Add(key, value);
            return this;
        }
        
        public string Get(string key, string fallback)
        {
            return Config._configDict.ContainsKey(key)
                       ? Config._configDict[key]
                       : fallback;
        }
        
        public string Get(string key)
        {
            return Config._configDict.ContainsKey(key)
                ? Config._configDict[key]
                : string.Empty;
        }
        
        public Config Update(string key, Func<string, string> func)
        {
            Config.CheckKeyExist(key);

            var tmp = func.Invoke(Config._configDict[key]);
            Config._configDict[key] = tmp;
            return this;
        }


        public Config Delete(string key)
        {
            Config.CheckKeyExist(key);    
            Config._configDict.Remove(key);
            return this;
        }
        
        public List<string> Where(string key)
        {
            var keys = Config._configDict.Keys;
            var result = keys.Where(p => p.Contains(key)).ToList();
            return result;
        }
        
        public Dictionary<string, string> All()
        {
            return Config._configDict;
        }
        
        private static void CheckKeyExist(string key)
        {
            if (!Config._configDict.ContainsKey(key))
            {
                throw new Exception("Config key not exist.");
            }
        }
    }
}