using System;
using System.Linq;
using System.Text;
using _JoykadeGames.Runtime.SaveSystem;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace _JoykadeGames.Editor
{
    using UnityEditor;
    [CustomEditor(typeof(SaveableBehaviour))]
    public class SaveableBehaviourInspector : Editor
    {
        private SaveableBehaviour _Target;
        private Label title;
        private void OnEnable()
        {
            _Target = (SaveableBehaviour)target;
            title = new Label();
        }

        public override VisualElement CreateInspectorGUI()
        {
            
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.hatchstudio.save/Editor/Utilities/Styles/EditorStyle.uss");
            VisualElement rootVisual = new VisualElement();
            rootVisual.styleSheets.Add(styleSheet);
            
            VisualElement container = new VisualElement();
            container.AddToClassList("container");
            title.text = $"References count: {_Target.SaveablePair?.Count}";
            title.AddToClassList("label-header");
            container.Add(title);
            
            VisualElement propertyContainer = new VisualElement();
            propertyContainer.AddToClassList("container");
            SerializedProperty myProp = serializedObject.FindProperty("m_SavePosition");
            SerializedProperty rotProp = serializedObject.FindProperty("m_SavePosition");
            PropertyField myField = new PropertyField(myProp);
            PropertyField rotationField = new PropertyField(rotProp);
            myField.Bind(serializedObject);
            rotationField.Bind(serializedObject);
            propertyContainer.Add(myField);
            propertyContainer.Add(rotationField);
            
            rootVisual.Add(container);
            rootVisual.Add(propertyContainer);
            var findSaveable = new Button(FindSaveableInChildrean)
            {
                text = "Find Saveables Component"
            };
            
            rootVisual.Add(findSaveable);
            
            return rootVisual;
        }
        
        private void FindSaveableInChildrean()
        {
            GameObject root = _Target.gameObject;
            var saveablesObjects = _Target.SaveablePair;

            var saveables = from mono in root.GetComponentsInChildren<MonoBehaviour>(true)
                let type = mono.GetType()
                where typeof(ISaveable).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface && type != typeof(SaveableBehaviour)
                select mono;

            saveablesObjects.RemoveAll(x => x.Instance == null || !saveables.Contains(x.Instance));
            //new SaveablePair(GetRelativePath(mono.transform), mono)
            foreach (var saveable in saveables)
            {
                var existing = saveablesObjects.FirstOrDefault(x => x.Instance == saveable);

                if (existing.Instance == null)
                {
                    saveablesObjects.Add(new SaveablePair(GetRelativePath(saveable.transform),saveable));
                }
            }
            
            EditorUtility.SetDirty(_Target);
            title.text = $"References count: {saveablesObjects.Count()}";
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        private string GetRelativePath(Transform target)
        {
            
            StringBuilder path = new StringBuilder(target.name);
            
            while (target.parent != null && target != _Target.transform)
            {
                target = target.parent;
                path.Insert(0, target.name + "/");
            }
            
            return HashRelativePath(path.ToString());
        }
        private string HashRelativePath(string path)
        {
            int hash = path.GetHashCode();
            return hash.ToString("X");
        }
        
    }
}