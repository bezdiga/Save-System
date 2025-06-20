
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using _JoykadeGames.Code.Runtime.Scriptables;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _JoykadeGames.Runtime.SaveSystem
{
    public class SaveGameManager : Singleton<SaveGameManager>
    {
        public static string TOKEN_SEPARATOR = "-";
        private byte[] GameData;
        private SerializationAsset _serializationAsset => SerializationUtillity.SerializationAsset;
        private ObjectDataBase _OnjectReference => SerializationUtillity.SerializationObjectDatabase;
        public List<SaveablePair> worldSaveables = new();
        public List<RuntimeSaveable> runtimeSaveables = new();
        private FileWriteRead _writeRead;

        private void Awake()
        {
            GetSavedFolder(out string saveFolderName, 0);
            string saveFolderPath = Path.Combine(SavedGamePath, saveFolderName);
            _writeRead = new FileWriteRead(saveFolderPath);
            LoadInfo();
        }

        private string SavedGamePath
        {
            get
            {
                string savesPath = _serializationAsset.GetSavesPath();
                if (!Directory.Exists(savesPath))
                    Directory.CreateDirectory(savesPath);

                return savesPath;
            }
        }

        #region Save/Load
        
        public static void SaveTest()
        {
            Instance.PrepareAndSaveGame(0);
        }

        public static void LoadInfo()
        {
            
            Instance.LoadGameState();
        }


        private void LoadGameState()
        {
            string saveDataFileName = _serializationAsset.SaveDataName + _serializationAsset.SaveExtention;
            
            StorableCollection worldData = _writeRead.LoadFromSaveFile(saveDataFileName);
            if (worldData == null)
            {
                SaveTest();
                Debug.LogError("Create New Game");
                LoadGameState();
                return;
            }
            if(worldData.ContainsKey("worldState"))
                LoadSaveable(worldData["worldState"] as StorableCollection);
        }
        private void LoadSaveable(StorableCollection worldData)
        {
            bool isTokenError = false;
            StorableCollection worldBuffer = worldData["worldBuffer"] as StorableCollection;
            StorableCollection runtimeBuffer = worldData["runtimeBuffer"] as StorableCollection;
            /*foreach (var world in worldBuffer)
            {
                Debug.LogError(world.Key + " : "+ world.Value);
            }*/
            foreach (var saveable in worldSaveables)
            {
                if (saveable.Instance == null || string.IsNullOrEmpty(saveable.SceneId))
                    return;

                if(!worldBuffer.TryGetValue(saveable.SceneId,out StorableCollection members))
                {
                    Debug.Log($"Could not find saveable with token '{saveable.SceneId}'. Drstroy Object");
                    Destroy(saveable.Instance.gameObject);
                    isTokenError = true;
                    continue;
                }
                ((ISaveable)saveable.Instance).OnLoad(members);
            }

            
            foreach (var saveable in runtimeBuffer)
            {
                var dataContainer = saveable.Value as StorableCollection;
                
                if(!dataContainer.TryGetValue("SavedData", out Data data))
                    continue;
                
                if(!runtimeSaveables.Exists(x => x.InstantiatedObject.GetGuid() == new Guid(data.SceneID)))
                {
                    ObjectAssetReference? reference = _OnjectReference.GetAssetReferences(data.PrefabID);
                    if (reference != null)
                    {
                        var obj = InstantiateSaveable(reference.Value);
                        obj.Initialize(data.PrefabID);
                        obj.OnLoad(saveable.Value as StorableCollection);
                        /*foreach (var saveablePair in dataContainer)
                        {
                            
                        }*/
                        foreach (var saveablePair in obj.SaveablePair)
                        {
                            ((ISaveable)saveablePair.Instance).OnLoad(dataContainer[saveablePair.SceneId] as StorableCollection);
                        }
                    }
                    else Debug.LogError($"The prefab with ID: {data.PrefabID} was not found");
                }
                else
                {
                    Debug.Log($"The prefab with identification number: {new Guid(data.SceneID)} already exists");
                }
            }
        }
        private async void PrepareAndSaveGame(int savedId)
        {
            GetSavedFolder(out string saveFolderName, savedId);
            string saveFolderPath = Path.Combine(SavedGamePath, saveFolderName);
            if (!Directory.Exists(saveFolderPath))
                Directory.CreateDirectory(saveFolderPath);
            
            StorableCollection saveInfoData = new()
            {
                { "id", savedId },
                { "scene", SceneManager.GetActiveScene().name },
                { "dateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                { "timePlayed", 0 },
                { "data", "" },
                { "thumbnail", "" }
            };
            StorableCollection saveablesBuffer = new()
            {
                { "id", savedId },
                { "worldState", GetWorldStateBuffer() }
            };
            await SerializeGameState(saveInfoData, saveablesBuffer, saveFolderName, saveFolderPath);
        }

        private async Task SerializeGameState(StorableCollection saveInfo, StorableCollection saveBuffer,
            string folderName, string folderPath)
        {
            string saveInfoFileName = _serializationAsset.SaveInfoName + _serializationAsset.SaveExtention;
            string saveDataFileName = _serializationAsset.SaveDataName + _serializationAsset.SaveExtention;

            saveInfo["data"] = saveDataFileName;
            
            // serialize save info to file
            string saveInfoPath = Path.Combine(folderPath, saveInfoFileName);
            _writeRead.SerializeData(saveInfo, saveInfoPath);

            // serialize save data to file
            string saveDataPath = Path.Combine(folderPath, saveDataFileName);
            _writeRead.SerializeData(saveBuffer, saveDataPath);
        }
        
        private void GetSavedFolder(out string saveFolderName, int savedId)
        {
            if(_serializationAsset == null) Debug.LogError("Is Null");
            saveFolderName = _serializationAsset.SaveFolderPrefix;

            if (!_serializationAsset.SingleSave)
            {
                string[] directories = Directory.GetDirectories(SavedGamePath, $"{saveFolderName}*");
                saveFolderName += (directories.Length - 1).ToString("D3");
            }
            else
            {
                saveFolderName += savedId.ToString("D3");
            }
        }
        private StorableCollection GetWorldStateBuffer()
        {
            StorableCollection saveablesBuffer = new StorableCollection();
            StorableCollection worldBuffer = new StorableCollection();
            StorableCollection runtimeBuffer = new StorableCollection();
            
            foreach (var saveable in worldSaveables)
            {
                if (saveable.Instance == null || string.IsNullOrEmpty(saveable.SceneId))
                    continue;

                var saveableData = ((ISaveable)saveable.Instance).OnSave();
                worldBuffer.Add(saveable.SceneId, saveableData);
            }

            foreach (var saveable in runtimeSaveables)
            {
                if (saveable.InstantiatedObject == null || string.IsNullOrEmpty(saveable.PrefabID))
                    continue;
                
                StorableCollection runtimeCollection = saveable.InstantiatedObject.OnSave();
                foreach (var saveablePair in saveable.SaveablePairs)
                {
                    if (saveablePair.Instance == null || string.IsNullOrEmpty(saveablePair.SceneId))
                        continue;
                    var saveableData = ((ISaveable)saveablePair.Instance).OnSave();
                    runtimeCollection.Add(saveablePair.SceneId,saveableData);
                }
                
                runtimeBuffer.Add(saveable.InstantiatedObject.GetGuid().ToString(),runtimeCollection);
            }

            saveablesBuffer.Add("worldBuffer",worldBuffer);
            saveablesBuffer.Add("runtimeBuffer",runtimeBuffer);
            
            return saveablesBuffer;
        }
        #endregion

        #region Instantiate Runtime

        public static SaveableBehaviour InstantiateSaveable(ObjectAssetReference reference, string name = null)
        {
            SaveableBehaviour saveable = Instantiate(reference.saveable);
            saveable.name = name ?? reference.saveable.gameObject.name;

            Instance.RegisterRuntimeSaveable(saveable,reference.PrefabGuid);
            return saveable;
        }
        private void RegisterRuntimeSaveable(SaveableBehaviour saveable,string guid)
        {
            RuntimeSaveable runtimeSaveable = new RuntimeSaveable(saveable, guid);
            saveable.Initialize(guid);
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;

            if (!Instance.runtimeSaveables.Contains(runtimeSaveable))
                Instance.runtimeSaveables.Add(runtimeSaveable);
            else
                Debug.LogWarning("Saveable is already registered!", saveable);
#else
            Instance.m_CurrentSceneSaveables.Add(runtimeSaveable);
#endif
        }
        
        public static void UnregisterSaveable(SaveableBehaviour saveable)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;

            if (Instance.runtimeSaveables.Exists(x => x.InstantiatedObject.GetGuid() == saveable.GetGuid()))
            {
                Instance.runtimeSaveables.RemoveAll(x => x.InstantiatedObject.GetGuid() == saveable.GetGuid());
            }
            else Debug.LogWarning("Saveable is not registered, no need for un-registering!");
#else
            Instance.runtimeSaveables.RemoveAll(x => x.InstantiatedObject.GetGuid() == saveable.GetGuid());
#endif
        }

        #endregion
    }
    
}