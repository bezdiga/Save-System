using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using static _JoykadeGames.Runtime.SaveSystem.SaveGameManager;

namespace _JoykadeGames.Runtime.SaveSystem
{
    public class LevelManager : MonoBehaviour
    {
        public bool Debugging;
        public UnityEvent<float> OnProgressUpdate;
        public UnityEvent OnLoadingDone;
        
        private void Start()
        {
            Time.timeScale = 1f;
            Application.backgroundLoadingPriority = ThreadPriority.High;
            string sceneName = LoadSceneName;
            if (!string.IsNullOrEmpty(sceneName))
            {
                StartCoroutine(LoadLevelAsync(sceneName));
            }
        }
        
        private IEnumerator LoadLevelAsync(string sceneName)
        {
            yield return new WaitForEndOfFrame();

            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
            asyncOp.allowSceneActivation = false;

            while (!asyncOp.isDone)
            {
                float progress = asyncOp.progress / 0.9f;
                OnProgressUpdate?.Invoke(progress);

                if (progress >= 1f) break;
                yield return null;
            }

            yield return DeserializeSavedGame();
            
            asyncOp.allowSceneActivation = true;
            yield return null;
        }
        
        private IEnumerator DeserializeSavedGame()
        {
            yield return new WaitForEndOfFrame();
            string saveFolder = LoadFolderName;
            if (!string.IsNullOrEmpty(saveFolder))
            {
                if (Debugging) Debug.Log($"[LevelManager] Trying to deserialize a save with the name '{saveFolder}'.");
                {
                    TryDeserializeGameStateAsync(saveFolder);
                    yield return new WaitForEndOfFrame();
                }
                if (Debugging) Debug.Log($"[LevelManager] The save was successfully deserialized. ");
            }
        }
    }
}