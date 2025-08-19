using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using _JoykadeGames.Code.Runtime.Scriptables;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace _JoykadeGames.Runtime.SaveSystem
{
    [DefaultExecutionOrder(-100000)]
    public class SaveGameManager : Singleton<SaveGameManager>
    {
        public static string TOKEN_SEPARATOR = "-";
        private byte[] GameData;
        internal static SerializationAsset _serializationAsset => SerializationUtillity.SerializationAsset;
        private ObjectDataBase _OnjectReference => SerializationUtillity.SerializationObjectDatabase;
        public List<SaveablePair> worldSaveables = new();
        public List<RuntimeSaveable> runtimeSaveables = new();
        
        internal static StorableCollection _worldStateBuffer;
        //protected static IWriterReader WriteRead => Instance._writeRead;
        private IWriterReader _writeRead;
        private string currentScene;
        public static string LoadSceneName;
        public static string LoadFolderName;
        
        /// <summary>
        /// Shortcut to Level Manager Scene/Scene Loader
        /// </summary>
        public static string LMS => _serializationAsset.LevelManagerScene;
        
        [Inject]
        public void Construct(IWriterReader writeRead)
        {
            _writeRead = writeRead;
            Debug.LogError("Successfully injected SaveGameManager with IWriterReader");
        }
        private void Awake()
        {
            currentScene = SceneManager.GetActiveScene().name;
            if(_worldStateBuffer != null)
                LoadSaveable(_worldStateBuffer);
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
        
        public static void SaveGame()
        {
            Instance.PrepareAndSaveGame();
        }

        /// <summary>
        /// Set the load type to load the game state.
        /// <br>The game state and player data will be loaded from a saved game.</br>
        /// </summary>
        /// <param name="sceneName">The name of the scene to be loaded.</param>
        /// <param name="folderName">The name of the saved game folder.</param>
        public static void SetLoadGameState(string sceneName, string folderName)
        {
            LoadSceneName = sceneName;
            LoadFolderName = folderName;
        }
        

        /*/// <summary>
        /// Try to Deserialize and Validate Game State
        /// </summary>
        public static void TryDeserializeGameStateAsync(string folderName)
        {
            // get saves path
            string savesPath = _serializationAsset.GetSavesPath();
            string saveFolderPath = Path.Combine(savesPath, folderName);

            // check if directory exists
            if (!Directory.Exists(saveFolderPath))
            {
                Debug.LogError("Save folder does not exist: " + saveFolderPath);
                return;
            }

            // deserialize saved game info
            string filePath = Path.Combine(saveFolderPath, _serializationAsset.SaveDataName + _serializationAsset.SaveExtension);
            StorableCollection worldData = WriteRead.LoadFromSaveFile(filePath);
            if (worldData != null)
            {
                if (worldData.ContainsKey("worldState"))
                    _worldStateBuffer = (worldData["worldState"] as StorableCollection);
            }
        }*/
        
        /// <summary>
        /// Remove all saved games.
        /// </summary>
        /*public static async Task RemoveAllSaves()
        {
            await WriteRead.RemoveAllSaves();
        }*/
        
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
        private async void PrepareAndSaveGame()
        {
            GetSavedFolder(out string saveFolderName);
            string saveFolderPath = Path.Combine(SavedGamePath, saveFolderName);
            if (!Directory.Exists(saveFolderPath))
                Directory.CreateDirectory(saveFolderPath);
            
            string saveId = UniqueID.GetUniqueGuid();
            StorableCollection saveInfoData = new()
            {
                { "id", saveId },
                { "scene", currentScene },
                { "dateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                { "timePlayed", 0 },
                { "data", "" },
                { "thumbnail", "" }
            };
            StorableCollection saveablesBuffer = new()
            {
                { "id", saveId },
                { "worldState", GetWorldStateBuffer() }
            };
            await SerializeGameState(saveInfoData, saveablesBuffer, saveFolderPath);
        }

        private async Task SerializeGameState(StorableCollection saveInfo, StorableCollection saveBuffer, string folderPath)
        {
            string saveInfoFileName = _serializationAsset.SaveInfoName + _serializationAsset.SaveExtension;
            string saveDataFileName = _serializationAsset.SaveDataName + _serializationAsset.SaveExtension;

            saveInfo["data"] = saveDataFileName;
            
            // serialize save info to file
            string saveInfoPath = Path.Combine(folderPath, saveInfoFileName);
            _writeRead.SerializeData(saveInfo, saveInfoPath);

            // serialize save data to file
            string saveDataPath = Path.Combine(folderPath, saveDataFileName);
            _writeRead.SerializeData(saveBuffer, saveDataPath);
        }
        
        private void GetSavedFolder(out string saveFolderName)
        {
            if(_serializationAsset == null) Debug.LogError("Not found Serialization Asset, please assign it in the inspector!");
            
            saveFolderName = _serializationAsset.SaveFolderPrefix;

            if (!_serializationAsset.SingleSave)
            {
                string[] directories = Directory.GetDirectories(SavedGamePath, $"{saveFolderName}*");
                saveFolderName += directories.Length.ToString("D3");//saveFolderName += (directories.Length - 1).ToString("D3");
            }
            // if single save and use of scene names is enabled, the scene name is used as the save name
            else if (_serializationAsset.UseSceneNames)
            {
                saveFolderName += currentScene.Replace(" ", "");
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
            Instance.runtimeSaveables.Add(runtimeSaveable);
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