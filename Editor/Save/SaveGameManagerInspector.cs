using System;
using System.Linq;
using UnityEditor.UIElements;

namespace _JoykadeGames.Editor
{
    using Runtime.SaveSystem;
    using UnityEditor;
    using UnityEngine.UIElements;
    using UnityEngine;
    
    
    [CustomEditor(typeof(SaveGameManager))]
    public class SaveGameManagerInspector : Editor
    {
        private SaveGameManager manager;
        private void OnEnable()
        {
            manager = (SaveGameManager)target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.hatchstudio.save-system/Editor/Utilities/Styles/EditorStyle.uss");
            VisualElement root = new VisualElement();
            root.styleSheets.Add(styleSheet);
            var container = CreateContainer();
            
            container.Add(CreateHeader("Saveables Searchers".ToUpper()));
            container.Add(IconLabelContainer($"World Saveables - {manager.worldSaveables.Count}",out Label worldSaveable));
            container.Add(IconLabelContainer($"Runtime Saveables - {manager.runtimeSaveables.Count}",out Label runtimeSaveable));
            Button findSaveables = new Button(() => FindSaviables(worldSaveable))
            {
                text = "Find Saveables"
            };
            container.Add(findSaveables);
            
            root.Add(container);
            
            //myLabel.style.backgroundColor = new StyleColor(Color.cyan);
            
            return root;
        }
        [MenuItem("GameObject/Managers/SaveGameManager")]
        static void CreateSaveManager()
        {
            GameObject managers;
            if (GameObject.Find("Managers"))
            {
                managers = GameObject.Find("Managers");
            }
            else managers = new GameObject("Managers");
            if (FindObjectOfType<SaveGameManager>())
            {
                var obj = FindObjectOfType<SaveGameManager>().gameObject;
                EditorUtility.DisplayDialog("SaveGameManager Error", $"There should never be more than 1 reference of SaveGameManager!", "OK");
                Selection.activeGameObject = obj;
            }
            else
            {
                managers.transform.position = Vector3.zero;
                GameObject newObject = new GameObject("SaveGameManager");
                newObject.transform.position = Vector3.zero;
                newObject.transform.SetParent(managers.transform);
                newObject.AddComponent<SaveGameManager>();

                Selection.activeGameObject = newObject;
            }
        }
        private void FindSaviables(Label worldSaveable)
        {
            MonoBehaviour[] monos = FindObjectsOfType<MonoBehaviour>(true);
            var saveables = from mono in monos
                let type = mono.GetType()
                where typeof(ISaveable).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract
                //let token = $"{type.Name}{SaveGameManager.TOKEN_SEPARATOR}{Guid.NewGuid()}"
                select mono;
            
            var saveablesObjects = manager.worldSaveables;

            saveablesObjects.RemoveAll(saveablePair => saveablePair.Instance == null || !saveables.Contains(saveablePair.Instance));
            foreach (var saveable in saveables)
            {
                var existing = saveablesObjects.FirstOrDefault(x => x.Instance == saveable);
                if (existing.Instance == null)
                {
                    string token = $"{saveable.GetType().Name}{SaveGameManager.TOKEN_SEPARATOR}{Guid.NewGuid()}";
                    saveablesObjects.Add(new SaveablePair(token,saveable));
                    Debug.Log("Token " + token);
                }
                else
                {
                    Debug.Log("Exist Object ");
                }
            }
            worldSaveable.text = $"World Saveables - {manager.worldSaveables.Count}";
            EditorUtility.SetDirty(manager);
            Repaint();
        }
        
        private VisualElement CreateContainer()
        {
            VisualElement container = new VisualElement();
            container.AddToClassList("container");
            return container;
        }

        private Label CreateHeader(string text)
        {
            Label header = new Label(text);
            header.AddToClassList("label-header");
            return header;
        }

        private VisualElement IconLabelContainer(string text, out Label label)
        {
            VisualElement element = new VisualElement();
            Image infoIcon = new Image();
            infoIcon.AddToClassList("icon");
            infoIcon.image = EditorGUIUtility.IconContent("_Help").image;
            
            label = new Label(text);
            
            element.Add(infoIcon);
            element.Add(label);
            element.AddToClassList("icon-label-container");
            return element;
        }
    }
}