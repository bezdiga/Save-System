﻿using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

namespace _JoykadeGames.Runtime.SaveSystem
{
    public class GuidComponent : MonoBehaviour, ISerializationCallbackReceiver
    {
        // System guid we use for comparison and generation
        System.Guid guid = System.Guid.Empty;

        // Unity's serialization system doesn't know about System.Guid, so we convert to a byte array
        // Fun fact, we tried using strings at first, but that allocated memory and was twice as slow
        [SerializeField, HideInInspector]
        private byte[] serializedGuid;


        public bool IsGuidAssigned()
        {
            return guid != System.Guid.Empty;
        }

        public void SetGuid(byte[] guid)
        {
            serializedGuid = guid;
            this.guid = new System.Guid(serializedGuid);
        }


        // When de-serializing or creating this component, we want to either restore our serialized GUID
        // or create a new one.
        void CreateGuid()
        {
            // if our serialized data is invalid, then we are a new object and need a new GUID
            if (serializedGuid == null || serializedGuid.Length != 16)
            {
#if UNITY_EDITOR
                // if in editor, make sure we aren't a prefab of some kind
                if (IsAssetOnDisk())
                {
                    return;
                }
                Undo.RecordObject(this, "Added GUID");
#endif
                guid = System.Guid.NewGuid();
                serializedGuid = guid.ToByteArray();

#if UNITY_EDITOR
                // If we are creating a new GUID for a prefab instance of a prefab, but we have somehow lost our prefab connection
                // force a save of the modified prefab instance properties
                if (PrefabUtility.IsPartOfNonAssetPrefabInstance(this))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(this);
                }
#endif
            }
            else if (guid == System.Guid.Empty)
            {
                // otherwise, we should set our system guid to our serialized guid
                guid = new System.Guid(serializedGuid);
            }
            
        }

#if UNITY_EDITOR
        private bool IsEditingInPrefabMode()
        {
            if (EditorUtility.IsPersistent(this))
            {
                // if the game object is stored on disk, it is a prefab of some kind, despite not returning true for IsPartOfPrefabAsset =/
                return true;
            }
            else
            {
                // If the GameObject is not persistent let's determine which stage we are in first because getting Prefab info depends on it
                var mainStage = StageUtility.GetMainStageHandle();
                var currentStage = StageUtility.GetStageHandle(gameObject);
                if (currentStage != mainStage)
                {
                    var prefabStage = PrefabStageUtility.GetPrefabStage(gameObject);
                    if (prefabStage != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsAssetOnDisk()
        {
            return PrefabUtility.IsPartOfPrefabAsset(this) || IsEditingInPrefabMode();
        }
#endif

        // We cannot allow a GUID to be saved into a prefab, and we need to convert to byte[]
        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            // This lets us detect if we are a prefab instance or a prefab asset.
            // A prefab asset cannot contain a GUID since it would then be duplicated when instanced.
            if (IsAssetOnDisk())
            {
                serializedGuid = null;
                guid = System.Guid.Empty;
            }
            else
#endif
            {
                if (guid != System.Guid.Empty)
                {
                    serializedGuid = guid.ToByteArray();
                }
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

        protected virtual void Awake()
        {
            CreateGuid();
        }

        void OnValidate()
        {
#if UNITY_EDITOR
            // similar to on Serialize, but gets called on Copying a Component or Applying a Prefab
            // at a time that lets us detect what we are
            if (IsAssetOnDisk())
            {
                serializedGuid = null;
                guid = System.Guid.Empty;
            }
            else
#endif
            {
                CreateGuid();
            }
        }

        // Never return an invalid GUID
        public System.Guid GetGuid()
        {
            if (guid == System.Guid.Empty && serializedGuid != null && serializedGuid.Length == 16)
            {
                guid = new System.Guid(serializedGuid);
            }

            return guid;
        }

    }
}