using System.Collections.Generic;
using _JoykadeGames.Runtime.SaveSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace _JoykadeGames.Code.Runtime.Scriptables
{
    [CreateAssetMenu(fileName = "ObjectDatabase", menuName = "JoykadeGames/Save/ObjectDatabase")]
    public class ObjectDataBase : ScriptableObject
    {
        public List<ObjectAssetReference> References = new List<ObjectAssetReference>();

        public ObjectAssetReference? GetAssetReferences(string guid)
        {
            foreach (var asset in References)
            {
                if (asset.PrefabGuid == guid)
                    return asset;
            }

            return null;
        }
        
        public bool HasReference(string guid)
        {
            foreach (var elm in References)
            {
                if (elm.PrefabGuid == guid)
                    return true;
            }

            return false;
        }
        
    }
    [System.Serializable]
    public struct ObjectAssetReference
    {
        public string PrefabGuid;
        public SaveableBehaviour saveable;
    }
    
    
}