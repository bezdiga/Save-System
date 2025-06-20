using System;
using _JoykadeGames.Code.Runtime.Scriptables;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace _JoykadeGames.Editor
{
    using UnityEditor;
    
    [CustomEditor(typeof(ObjectDataBase))]
    public class ObjectDatabaseEditor : Editor
    {
        private static ObjectDataBase _Target;

        private void OnEnable()
        {
            _Target = target as ObjectDataBase;
        }
        
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceId);

            if (obj != null && obj is ObjectDataBase)
            {
                OpenWindow();
                return true;
            }

            return false;
        }

        static void OpenWindow()
        {
            if (_Target == null)
            {
                Debug.LogError("Scriptable Object is not initialized");
            }

            ObjectReferencesWindow objRefWindow = EditorWindow.GetWindow<ObjectReferencesWindow>(false, _Target.name, true);
            Rect position = objRefWindow.position;
            position.width = 800;
            position.height = 450;

            objRefWindow.minSize = new Vector2(850, 450);
            objRefWindow.maxSize = new Vector2(850, 450);
            objRefWindow.position = position;
            objRefWindow.Init(_Target);
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement rootVisual = new VisualElement();

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.hatchstudio.save-system/Editor/Utilities/Styles/EditorStyle.uss");
            
            rootVisual.styleSheets.Add(styleSheet);
            
            Label header = new Label("Object References".ToUpper());
            header.style.fontSize = 24;
            header.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
            header.style.paddingBottom = 10;
            header.style.paddingTop = 10;
            
            //header.style.b
            rootVisual.Add(header);

            VisualElement container = new VisualElement();
            container.AddToClassList("container");
            Label title = new Label($"References count {_Target.References.Count}");
            title.AddToClassList("label-header");
            container.Add(title);
            
            rootVisual.Add(container);
            
            Button btn = new Button(OpenWindow)
            {
                text = "Open Object References window"
            };
            
            rootVisual.Add(btn);
            return rootVisual;
        }

    }
}