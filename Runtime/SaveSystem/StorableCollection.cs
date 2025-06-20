using System;
using System.Collections.Generic;

namespace _JoykadeGames.Runtime.SaveSystem
{
    public class StorableCollection : Dictionary<string,object>
    {
        public T GetT<T>(string key)
        {
            if (TryGetValue(key, out var value))
                if (value is T valueT)
                    return valueT;

            return default(T);
            //throw new System.NullReferenceException($"Could not find item with key '{key}' or could not convert to type '{typeof(T).Name}'.");
        }
        
        public bool TryGetValue<T>(string key, out T value)
        {
            if (TryGetValue(key, out var valueO))
            {
                if (valueO is T valueT)
                {
                    value = valueT;
                    return true;
                }
            }

            value = default;
            return false;
        }
        
    }
    
    [Serializable]
    public struct Data
    {
        public string PrefabID;
        public byte[] SceneID;
        public string Name;

        public TransformData transform;
        public StorableCollection Components;
    }
}