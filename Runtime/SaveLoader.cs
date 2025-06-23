using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _JoykadeGames.Runtime.SaveSystem
{
    public class SaveLoader : MonoBehaviour
    {
        public string NewGameSceneName;
        public bool NewGameRemoveSaves;
        
        public bool LoadAtStart;
        [SerializeField] Button ContinueButton;
        
        private SavedGameInfo? lastSave;
        
        private bool isLoading;
        [Header("Events")]
        public UnityEvent OnSavesBeingLoaded;
        public UnityEvent OnSavesLoaded;
        public UnityEvent OnSavesEmpty;
        
        private async void Start()
        {
            if (LoadAtStart)
            {
                // load saves process
                await LoadAllSaves();

                // enable or disable continue button when last save exists
                if(ContinueButton != null)
                    ContinueButton.gameObject.SetActive(lastSave.HasValue);
            }
        }
        

        /// <summary>
        /// Load last saved game.
        /// </summary>
        public void LoadLastSave()
        {
            if (!lastSave.HasValue || isLoading) 
                return;

            SaveGameManager.SetLoadGameState(lastSave.Value.Scene, lastSave.Value.Foldername);
            LoadGame();
            
            isLoading = true;
        }
        
        public void NewGame()
        {
            if (isLoading) return;

            isLoading = true;
            LoadNewGame();
        }
        void LoadNewGame()
        {
            if(NewGameRemoveSaves) 
                SaveGameManager.RemoveAllSaves();
            SaveGameManager.LoadSceneName = NewGameSceneName;
            SceneManager.LoadScene(SaveGameManager.LMS);
        }
        private async Task LoadAllSaves()
        {
            // load saves in another thread
            var savedGames = await SaveGameManager.WriteRead.ReadAllSaves();

            foreach (var saved in savedGames)
            {
                Debug.LogError($"{saved.Foldername} - time: {saved.TimeSaved}");
            }
            // set last saved game
            if(savedGames.Length > 0)
                lastSave = savedGames[0];
            
            if(savedGames.Length > 0)
            {
                OnSavesLoaded?.Invoke();
            }
            else
            {
                OnSavesEmpty?.Invoke();
            }
        }
        
        void LoadGame()
        {
            //adaugare fade
            SceneManager.LoadScene(SaveGameManager.LMS);
        }

    }
}