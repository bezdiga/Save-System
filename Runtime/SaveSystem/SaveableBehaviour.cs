using System;
using System.Collections.Generic;
using UnityEngine;

namespace _JoykadeGames.Runtime.SaveSystem
{
    public class SaveableBehaviour : GuidComponent,ISaveable
    {
        private string _PrefabGuid;
        
        public List<SaveablePair> SaveablePair
        {
            get => m_saveablePair;
        }
        
        

        [SerializeField] private bool m_SavePosition;
        [SerializeField] private bool m_SaveRotation;
        
        [SerializeField] private List<SaveablePair> m_saveablePair = new ();

        public void Initialize(string guid)
        {
            _PrefabGuid = guid;
        }
        public void OnLoad(StorableCollection members)
        {
            var data = members.GetT<Data>("SavedData");
            SetGuid(data.SceneID);
            
            LoadTransform(transform,data.transform);
        }

        public StorableCollection OnSave()
        {
            StorableCollection members = new StorableCollection();
            Data data = new Data()
            {
                PrefabID = _PrefabGuid,
                SceneID = GetGuid().ToByteArray(),
                Name = name,
                transform = new TransformData(transform)
            };
           
            members.Add("SavedData",data);
            return members;
        }
        
        private void LoadTransform(Transform transform, TransformData data)
        {
            if(m_SavePosition)
                transform.localPosition = data.position;
            if(m_SaveRotation)
                transform.localRotation = data.rotation;
        }
        
        public void OnDestroy()
        {
            if(SaveGameManager.HasReference)
                SaveGameManager.UnregisterSaveable(this);
        }
    }
}