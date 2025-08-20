using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;

namespace _JoykadeGames.Runtime.SaveSystem
{
    public class SaveLoader : MonoBehaviour
    {
        public string NewGameSceneName;
        public bool NewGameRemoveSaves;
        
        public bool LoadAtStart;
        [SerializeField] Button ContinueButton;
        
        private SavedGameInfo? lastSave;
        
        private IWriterReader _writerReader;
        
        private bool isLoading;
        [Header("Events")]
        public UnityEvent OnSavesBeingLoaded;
        public UnityEvent OnSavesLoaded;
        public UnityEvent OnSavesEmpty;

        [Inject]
        public void Construct(IWriterReader wr)
        {
            if(wr == null)
            {
                Debug.LogError("Writer is null");
                return;
            }
            _writerReader = wr;
        }
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
                _writerReader.RemoveAllSaves();
            SaveGameManager.LoadSceneName = NewGameSceneName;
            SceneManager.LoadScene(SaveGameManager.LMS);
        }
        private async Task LoadAllSaves()
        {
            // load saves in another thread
            var savedGames = await _writerReader.ReadAllSaves();

            foreach (var saved in savedGames)
            {
                Debug.LogError($"{saved.Foldername} - time: {saved.TimeSaved}");
            }
            // set last saved game
            if (savedGames.Length > 0)
            {
                lastSave = savedGames[0];
                ContinueButton.gameObject.SetActive(lastSave.HasValue);
            }

            Debug.LogError("Last save: " + (lastSave.HasValue ? lastSave.Value.Foldername : "null"));
            
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