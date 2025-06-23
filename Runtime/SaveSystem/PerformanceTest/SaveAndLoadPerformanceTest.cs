
using System.Diagnostics;
using _JoykadeGames.Code.Runtime.Scriptables;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace _JoykadeGames.Runtime.SaveSystem.PerformanceTest
{
    public class SaveAndLoadPerformanceTest : MonoBehaviour
    {
        [SerializeField] private int count = 10;

        /*private void Awake()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("Canvas");
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
                
                if (FindObjectOfType<EventSystem>() == null)
                {
                    GameObject eventSystem = new GameObject("EventSystem");
                    eventSystem.AddComponent<EventSystem>();
                    eventSystem.AddComponent<StandaloneInputModule>();
                }
            }
            
            GameObject buttonObject = new GameObject("Button");
            buttonObject.transform.SetParent(canvas.transform);
            
            Button button = buttonObject.AddComponent<Button>();
            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(160, 30); 
            
            rectTransform.anchoredPosition = new Vector2(0, 0);
            
            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(buttonObject.transform);

            Text buttonText = textObject.AddComponent<Text>();
            buttonText.text = "Generate Object !";
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            buttonText.alignment = TextAnchor.MiddleCenter;
            RectTransform textRectTransform = textObject.GetComponent<RectTransform>();
            textRectTransform.sizeDelta = rectTransform.sizeDelta; 
            textRectTransform.anchoredPosition = Vector2.zero;
            
            button.onClick.AddListener(CreateSaveableObject);
        }*/
        
        private void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 150, 30), "Generate Saveable"))
            {
                CreateSaveableObject();
            }
            
            if (GUI.Button(new Rect(170, 10, 150, 30), "Save"))
            {
                SaveGame();
            }
            if (GUI.Button(new Rect(330, 10, 150, 30), "Save"))
            {
                LoadGame();
            }
        }

        public void CreateSaveableObject()
        {
            for (int i = 0; i < count; i++)
            {
                CreateObject(SerializationUtillity.SerializationObjectDatabase.References[Random.Range(0, 5)]);
            }
        }

        public void SaveGame()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            SaveGameManager.SaveGame();
            stopwatch.Stop();
            UnityEngine.Debug.Log("Save Time: " + stopwatch.ElapsedMilliseconds + " ms");
        }

        public void LoadGame()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            SaveGameManager.SetLoadGameState(SceneManager.GetActiveScene().name,"");
            stopwatch.Stop();
            UnityEngine.Debug.Log("Load Time: " + stopwatch.ElapsedMilliseconds + " ms");
        }
        private void CreateObject(ObjectAssetReference reference)
        {
            var obj = SaveGameManager.InstantiateSaveable(reference);
            obj.gameObject.transform.position =
                new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), Random.Range(-100, 100));
        }
    }
}