using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace _JoykadeGames.Editor
{
    
    public class TableView : VisualElement
    {
        private VisualElement header;
        private ScrollView  content;
        private List<string> columnNames = new List<string>();
        private VisualElement selectedRow;
        public event Action<VisualElement> OnRowSelected;
        public new class UxmlFactory : UxmlFactory<TableView, UxmlTraits> { }
        
        public TableView()
        {
            header = new VisualElement();
            header.AddToClassList("table-header");

            content = new ScrollView ();
            content.AddToClassList("table-content");
            content.style.flexGrow = 1;
            Add(header);
            Add(content);
            
            this.RegisterCallback<ClickEvent>(OnClick);
        }
        
        public void AddColumn(string columnName, StyleLength width)
        {
            columnNames.Add(columnName);
            var columnHeader = new Label(columnName);
            columnHeader.AddToClassList("table-column-header");
         
            columnHeader.style.width = width;
            columnHeader.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
            header.Add(columnHeader);
            
            foreach (var row in content.Children())
            {
                var cell = new Label();
                cell.AddToClassList("table-cell");
                row.Add(cell);
            }
        }
        
        public void SetItemsSource<T>(List<T> items, System.Func<T, VisualElement[]> itemToCellValues)
        {
            content.Clear();
            
            foreach (var item in items)
            {
                var row = new VisualElement();
                row.AddToClassList("table-row");
                row.style.flexDirection = FlexDirection.Row;
                var cellValues = itemToCellValues(item);
                int index = 0;
                foreach (var value in cellValues)
                {
                    value.AddToClassList("table-cell");
                    value.style.width = header[index].style.width;
                    if (value is Button button)
                    {
                        button.style.width = 25;
                        button.style.height = 25;
                        button.style.flexShrink = 0; 
                        button.style.marginLeft = 5;
                        button.style.marginRight = 5;
                        
                    }
                    row.Add(value);
                    index++;
                }
                
                int numOfColumns = header.childCount;
                int numOfCells = row.childCount;

                for (int i = numOfCells; i < numOfColumns; i++)
                {
                    var cell = new Label();
                    cell.AddToClassList("table-cell");
                    row.Add(cell);
                }

                row.RegisterCallback<MouseEnterEvent>(evt => OnRowHoverEnter(row));
                row.RegisterCallback<MouseLeaveEvent>(evt => OnRowHoverLeave(row));
                content.Add(row);
            }
        }
        
        private void OnClick(ClickEvent evt)
        {
            var clickedElement = evt.target as VisualElement;
            if (clickedElement != null && clickedElement.ClassListContains("table-cell"))
            {
                var parentRow = clickedElement.parent;
                if (parentRow != null && parentRow.ClassListContains("table-row"))
                {
                    if (selectedRow != null)
                    {
                        selectedRow.RemoveFromClassList("selected-row");
                    }
                    
                    selectedRow = parentRow;
                    selectedRow.AddToClassList("selected-row");
                    
                    OnRowSelected?.Invoke(selectedRow);
                }
                
            }
        }
        
        private void OnRowHoverEnter(VisualElement row)
        {
            row.AddToClassList("selected-row");
        }
        
        private void OnRowHoverLeave(VisualElement row)
        {
            row.RemoveFromClassList("selected-row");
        }
    }
}