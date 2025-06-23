using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace _JoykadeGames
{
    [Serializable]
    public sealed class UniqueID : ISerializationCallbackReceiver
    {
        Guid guid = System.Guid.Empty;

        [SerializeField]
        private byte[] serializedGuid;
        
        
        public UniqueID()
        {
            CreateGuid();
        }
        private void CreateGuid()
        {
            if (serializedGuid == null || serializedGuid.Length != 16)
            {

                guid = Guid.NewGuid();
                serializedGuid = guid.ToByteArray();
            }
            else if (guid == System.Guid.Empty)
            {
                // otherwise, we should set our system guid to our serialized guid
                guid = new System.Guid(serializedGuid);
            }
        }
        
        public string GetGuid()
        {
            if(guid == null)
                CreateGuid();
            Debug.LogError("Return Guid " + guid.ToString());
            return guid.ToString();
        }
        
        // We cannot allow a GUID to be saved into a prefab, and we need to convert to byte[]
        public void OnBeforeSerialize()
        {
            if (guid != System.Guid.Empty)
            {
                serializedGuid = guid.ToByteArray();
            }
        }

        // On load, we can go head a restore our system guid for later use
        public void OnAfterDeserialize()
        {
           
            if (serializedGuid != null && serializedGuid.Length == 16)
            {
                guid = new System.Guid(serializedGuid);
            }
        }
        
        /// <summary>
        /// Create new unique Guid.
        /// </summary>
        public static string GetUniqueGuid() => System.Guid.NewGuid().ToString("N");
        
    }
}