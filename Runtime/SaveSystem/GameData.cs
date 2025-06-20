using System;

using UnityEngine;
using UnityEngine.Serialization;

namespace _JoykadeGames.Runtime.SaveSystem
{
    
    [Serializable]
    public struct SaveablePair
    {
        public string SceneId;
        public MonoBehaviour Instance;
        public SaveablePair(string sceneId, MonoBehaviour instance)
        {
            SceneId = sceneId;
            Instance = instance;
        }
    }

    public struct TransformData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public TransformData(Transform transform)
        {
            position = transform.localPosition;
            rotation = transform.localRotation;
            scale = transform.localScale;
        }
    }
    [Serializable]
    public struct RuntimeSaveable
    {
        public string PrefabID;
        public byte[] SceneID;
        public SaveableBehaviour InstantiatedObject;
        public SaveablePair[] SaveablePairs;

        public RuntimeSaveable(SaveableBehaviour saveable,string guid)
        {
            PrefabID = guid;
            SceneID = saveable.GetGuid().ToByteArray();
            InstantiatedObject = saveable;
            SaveablePairs = saveable.SaveablePair.ToArray();
        }
        public RuntimeSaveable(string prefabId, byte[] sceneID, SaveableBehaviour obj, SaveablePair[] pairs)
        {
            PrefabID = prefabId;
            SceneID = sceneID;
            InstantiatedObject = obj;
            SaveablePairs = pairs;
        }
    }
   
}