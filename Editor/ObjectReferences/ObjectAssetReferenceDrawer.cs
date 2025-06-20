
using System;
using _JoykadeGames.Code.Runtime.Scriptables;
using _JoykadeGames.Runtime.SaveSystem;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;


namespace _JoykadeGames.Editor
{
    [CustomPropertyDrawer(typeof(ObjectAssetReference))]
    public class ObjectAssetReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ObjectDataBase objectReferences = null;
            SerializedProperty GUID = property.FindPropertyRelative("PrefabGuid");
            SerializedProperty Object = property.FindPropertyRelative("saveable");

            string guid = GUID.stringValue;
            SaveableBehaviour obj = Object.objectReferenceValue as SaveableBehaviour;

            bool hasSaveManagerReference = SaveGameManager.HasReference;
            GUIContent fieldText = new GUIContent("None (ObjectReference)");

            if (hasSaveManagerReference)
            {
                objectReferences = SerializationUtillity.SerializationObjectDatabase;
                if (!string.IsNullOrEmpty(guid) && !objectReferences.HasReference(guid))
                {
                    fieldText.text = " Invalid GUID Reference";
                    fieldText.image = EditorGUIUtility.TrIconContent("console.erroricon.sml").image;
                }
                else if(obj != null)
                {
                    string title = $" {obj.name} ({guid})";
                    fieldText = EditorGUIUtility.TrTextContentWithIcon(title, "Prefab Icon");
                }
            }

            EditorGUI.BeginProperty(position, label, property);
            {
                position = EditorGUI.PrefixLabel(position, label);
                position.xMax -= EditorGUIUtility.singleLineHeight + 2f;

                ObjectReferencePicker objectReferencePicker = new(new AdvancedDropdownState(), objectReferences);
                objectReferencePicker.OnItemPressed += (reference) =>
                {
                    if (reference.HasValue)
                    {
                        Object.objectReferenceValue = reference.Value.saveable;
                        GUID.stringValue = reference.Value.PrefabGuid;
                    } 
                    else
                    {
                        Object.objectReferenceValue = null;
                        GUID.stringValue = string.Empty;
                    }

                    property.serializedObject.ApplyModifiedProperties();
                };

                if (ObjectField(position, fieldText))
                {
                    Rect pickerRect = position;
                    objectReferencePicker.Show(pickerRect);
                }

                if (obj != null)
                {
                    Event e = Event.current;
                    Rect pingRect = position;
                    pingRect.xMax -= 19;

                    if (pingRect.Contains(e.mousePosition) && e.type == EventType.MouseDown)
                    {
                        EditorGUIUtility.PingObject(obj);
                    }
                }

                Rect scriptablePingRect = position;
                scriptablePingRect.xMin = position.xMax + 2f;
                scriptablePingRect.width = EditorGUIUtility.singleLineHeight;
                scriptablePingRect.y += 1f;

                GUIContent scriptableIcon = EditorGUIUtility.TrIconContent("ScriptableObject Icon");
                if(objectReferences == null)
                {
                    scriptableIcon.tooltip = "The ObjectReferences asset is not assigned in SaveGameManager!";
                }

                using (new EditorGUI.DisabledGroupScope(objectReferences == null))
                {
                    if (GUI.Button(scriptablePingRect, scriptableIcon, EditorStyles.iconButton))
                    {
                        EditorGUIUtility.PingObject(objectReferences);
                    }
                }
            }
            EditorGUI.EndProperty();
        }
        
        public static bool ObjectField(Rect rect, GUIContent text, GUIContent tooltip = null)
        {
            using (new IconSizeScope(12f))
            {
                GUI.Box(rect, text, EditorStyles.objectField);

                GUIStyle buttonStyle = new GUIStyle("ObjectFieldButton") { richText = true };
                Rect buttonRect = buttonStyle.margin.Remove(new Rect(rect.xMax - 19, rect.y, 19, rect.height));

                return GUI.Button(buttonRect, tooltip ?? new GUIContent(), buttonStyle);
            }
        }

        public class IconSizeScope : GUI.Scope
        {
            private Vector2 prevIconSize;

            public IconSizeScope(Vector2 iconSize)
            {
                prevIconSize = EditorGUIUtility.GetIconSize();
                EditorGUIUtility.SetIconSize(iconSize);
            }

            public IconSizeScope(float iconSize)
            {
                prevIconSize = EditorGUIUtility.GetIconSize();
                EditorGUIUtility.SetIconSize(new Vector2(iconSize, iconSize));
            }

            protected override void CloseScope()
            {
                EditorGUIUtility.SetIconSize(prevIconSize);
            }
        }

        private class ObjectReferencePicker : AdvancedDropdown
        {
            private class ObjectReferenceElement : AdvancedDropdownItem
            {
                public ObjectAssetReference reference;
                public bool isNone;

                public ObjectReferenceElement(ObjectAssetReference reference, string displayName) : base(displayName)
                {
                    this.reference = reference;
                }

                public ObjectReferenceElement() : base("None")
                {
                    isNone = true;
                }
            }

            public string SelectedObjective;
            public string[] SelectedSubObjectives;
            public event Action<ObjectAssetReference?> OnItemPressed;

            private readonly ObjectDataBase objectReferencesAsset;

            public ObjectReferencePicker(AdvancedDropdownState state, ObjectDataBase objectReferencesAsset) : base(state)
            {
                this.objectReferencesAsset = objectReferencesAsset;
                minimumSize = new Vector2(200f, 250f);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem("Object References");
                root.AddChild(new ObjectReferenceElement()); // none selector

                if (objectReferencesAsset != null)
                {
                    foreach (var reference in objectReferencesAsset.References)
                    {
                        var referenceElement = new ObjectReferenceElement(reference, " " + reference.saveable.gameObject.name);
                        referenceElement.icon = (Texture2D)EditorGUIUtility.IconContent("Prefab Icon").image;
                        root.AddChild(referenceElement);
                    }
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                var element = item as ObjectReferenceElement;
                if (element.isNone) OnItemPressed?.Invoke(null);
                else OnItemPressed?.Invoke(element.reference);
            }
        }
        
    

    }
}