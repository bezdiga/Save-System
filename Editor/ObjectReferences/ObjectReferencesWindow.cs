using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using _JoykadeGames.Code.Runtime.Scriptables;
using _JoykadeGames.Runtime.SaveSystem;
using UnityEditor;
using UnityEngine;

namespace _JoykadeGames.Editor
{
    public class ObjectReferencesWindow : EditorWindow
    {
        public struct ObjectReferenceElement
        {
            public GameObject Obj;
            public string path;
            public string PrefabGuid;
        }
        
        const float HEADER_HEIGHT = 25;
        const float ROW_HEIGHT = 20;
        private ObjectDataBase Target;
        
        
        private List<ObjectReferenceElement> ObjRefArray;
        private TableView tableView;

        public void Init(ObjectDataBase target)
        {
            Target = target;
            Refresh();
        }
        
        /*[MenuItem("Window/ObjectReferences")]
        public static void CreateWindow()
        {
            ObjectReferencesWindow wnd = GetWindow<ObjectReferencesWindow>();
            wnd.titleContent = new GUIContent("MyEditorWindow");
        }*/

        private void OnEnable()
        {
            var root = rootVisualElement;

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.hatchstudio.save-system//Editor/ObjectReferences/Style/TableStyle.uss");
            var styleSheetEditor = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.hatchstudio.save-system/Editor/Utilities/Styles/EditorStyle.uss");
            rootVisualElement.styleSheets.Add(styleSheet);
            rootVisualElement.styleSheets.Add(styleSheetEditor);

            Label label = new Label("To add elements to the list, Drag and Drop Folder or Prefabs to the windows.");
            label.AddToClassList("label-header");
            label.style.marginTop = 5;
            /*VisualElement element = IconLabelContainer("Test");
            rootVisualElement.Add(element);*/
            Label dropHere = new Label("Drop Here".ToUpper());
            dropHere.AddToClassList("label-header");
            dropHere.style.width = 100;
            dropHere.style.backgroundColor = new StyleColor(Color.clear);
            rootVisualElement.Add(label);
            var dropArea = new VisualElement
            {
                name = "DropArea",
            };
            dropArea.AddToClassList("dragArea");
            dropArea.style.height = 50;
            dropArea.Add(dropHere);
            dropArea.style.flexDirection = FlexDirection.Column;
            dropArea.style.justifyContent = Justify.Center; // Centrează pe verticală
            dropArea.style.alignItems = Align.Center;
            
            //drop area Event
            dropArea.RegisterCallback<DragEnterEvent>(evt => OnDragEnter(evt));
            dropArea.RegisterCallback<DragLeaveEvent>(evt => OnDragLeave(evt));
            dropArea.RegisterCallback<DragUpdatedEvent>(evt => OnDragUpdate(evt));
            dropArea.RegisterCallback<DragPerformEvent>(evt => OnDragPerform(evt));
            
            rootVisualElement.Add(dropArea);
            tableView = new TableView();
            rootVisualElement.Add(tableView);
            
            //list = CreateSampleData();
            tableView.AddColumn("Object",new StyleLength(100));
            tableView.AddColumn("Path",300);
            tableView.AddColumn("PrefabGuid",350);
            tableView.AddColumn("X",50);
            Refresh();

            rootVisualElement.Add(tableView);
        }

        #region Darg Logic

        private void OnDragEnter(DragEnterEvent evt)
        {
            var droparea = evt.currentTarget as VisualElement;
            //droparea.style.backgroundColor = Color.green;
        }

        private void OnDragLeave(DragLeaveEvent evt)
        {
            var dropArea = evt.currentTarget as VisualElement;
            //dropArea.style.backgroundColor = new Color(.2f,.2f,.2f);
        }
        private void OnDragUpdate(DragUpdatedEvent evt)
        {
            Object[] objects = DragAndDrop.objectReferences;
            string[] paths = DragAndDrop.paths;

            if (CanAcceptDrag(objects, paths)) 
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            else DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
        }
        private void OnDragPerform(DragPerformEvent evt)
        {
            Object[] objects = DragAndDrop.objectReferences;
            string[] paths = DragAndDrop.paths;

            if (CanAcceptDrag(objects, paths))
            {
                DragAndDrop.AcceptDrag();
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                foreach (string path in paths)
                {
                    Debug.Log($"Prefab dropat: {path}");
                    AddObjectToTarget(path);
                }
                EditorUtility.SetDirty(Target);
                AssetDatabase.SaveAssets();
                Refresh();
            }
            else
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            }
            
            
            var dropArea = evt.currentTarget as VisualElement;
            //dropArea.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        }

        private void AddObjectToTarget(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                string[] asset = AssetDatabase.FindAssets("t:GameObject", new[] { path });

                foreach (var pathGuid in asset)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(pathGuid);
                    GameObject obj = (GameObject)AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));

                    if (!Target.References.Any(x => x.PrefabGuid == pathGuid))
                    {
                        if (obj.TryGetComponent(out SaveableBehaviour saveable))
                        {
                            Target.References.Add(new Code.Runtime.Scriptables.ObjectAssetReference()
                            {
                                PrefabGuid = pathGuid,
                                saveable = saveable
                            });
                            EditorUtility.SetDirty(saveable);
                        }
                    }
                }
            }
            else
            {
                Object asset = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                if (asset.GetType() == typeof(GameObject))
                {
                    GameObject obj = (GameObject)asset;
                    string guid = AssetDatabase.AssetPathToGUID(path);
                    
                    if (!Target.References.Any(x => x.PrefabGuid == guid))
                    {
                        if (obj.TryGetComponent(out SaveableBehaviour saveable))
                        {
                            Target.References.Add(new Code.Runtime.Scriptables.ObjectAssetReference()
                            {
                                PrefabGuid = guid,
                                saveable = saveable
                            });
                            EditorUtility.SetDirty(saveable);
                        }
                    }
                }
            }
        }

        private bool CanAcceptDrag(Object[] drag, string[] paths)
        {
            bool flag1 = drag.Any(x => x is GameObject go && go.GetComponent<SaveableBehaviour>() != null);
            bool flag2 = paths.Any(x => AssetDatabase.IsValidFolder(x));
            

            return flag1 || flag2 ;
        }
        #endregion
        private void Refresh()
        {
            ObjRefArray = new List<ObjectReferenceElement>();
            if (Target == null) return;
            foreach (var kv in Target.References)
            {
                string path = AssetDatabase.GetAssetPath(kv.saveable.gameObject);
                ObjRefArray.Add(new ObjectReferenceElement()
                {
                    Obj = kv.saveable.gameObject,
                    path = path,
                    PrefabGuid = kv.PrefabGuid
                });
            }
            System.Func<ObjectReferenceElement, VisualElement[]> rowData = (item) =>
            {
                var labelIcon = IconLabelContainer(item.Obj.name);
                var label2 = new Label(item.path);
                var label3 = new Label(item.PrefabGuid);
                var button = new Button() { text = "X" };
                button.clicked += () =>
                {
                    var itemData = Target.References.Find(x => x.PrefabGuid == item.PrefabGuid);
                    Target.References.Remove(itemData);
                    Refresh();
                };
                return new VisualElement[] { labelIcon, label2,label3, button };
            };
            tableView.SetItemsSource(ObjRefArray, rowData);
        }
        
        
        private VisualElement IconLabelContainer(string text)
        {
            VisualElement element = new VisualElement();
            Image infoIcon = new Image();
            infoIcon.style.width = 20;
            infoIcon.style.height = 20;
            infoIcon.style.flexShrink = 0;
            Label label = new Label(text);
            label.style.height = 20;
            label.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft);
            
            infoIcon.image = EditorGUIUtility.IconContent("PrefabVariant Icon").image;
            element.Add(infoIcon);
            element.Add(label);
            element.style.alignItems = new StyleEnum<Align>(Align.FlexStart);
            element.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            element.style.overflow = new StyleEnum<Overflow>(Overflow.Hidden);
            return element;
        }
    }
}