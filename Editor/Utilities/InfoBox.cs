using UnityEngine;
using UnityEngine.UIElements;

namespace _JoykadeGames.Editor
{
    public class InfoBox : VisualElement
    {
        private Label _label;
        private Image _icon;

        public string text
        {
            get => _label.text;
            set => _label.text = value;
        }
        public Texture2D image
        {
            get => (Texture2D)_icon.image;
            set => _icon.image = value;
        }
        public InfoBox(string text = "")
        {
            VisualElement container = new VisualElement();
            container.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            container.style.alignItems = Align.Center;
            
            _icon = new Image();
            _icon.style.width = 16;
            _icon.style.height = 16;
            _icon.style.marginRight = 4;
            
            _label = new Label();
            _label.text = text;
            
            container.Add(_icon);
            container.Add(_label);
            
            Add(container);
        }

        public static explicit operator InfoBox(GUIContent content)
        {
            var element = new InfoBox();
            element.text = content.text;
            element.image = content.image as Texture2D;
            return element;
        }
        
    }
}