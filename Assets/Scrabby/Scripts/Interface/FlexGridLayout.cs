using System;
using UnityEngine;
using UnityEngine.UI;

namespace Scrabby.Interface
{
    public class FlexGridLayout : LayoutGroup
    {
        public enum FitType
        {
            Uniform,
            Width,
            Height,
            FixedRows,
            FixedColumns
        }

        public FitType fitType;
        public int rows;
        public int columns;
        public Vector2 cellSize;
        public Vector2 spacing;

        public bool fitX;
        public bool fitY;

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            if (fitType is FitType.Width or FitType.Height)
            {
                var sqrt = Mathf.Sqrt(transform.childCount);
                rows = Mathf.CeilToInt(sqrt);
                columns = Mathf.CeilToInt(sqrt);
                fitX = fitY = true;
            }

            switch (fitType)
            {
                case FitType.FixedColumns:
                case FitType.Width:
                    rows = Mathf.CeilToInt(transform.childCount / (float)columns);
                    break;
                case FitType.FixedRows:
                case FitType.Height:
                    columns = Mathf.CeilToInt(transform.childCount / (float)rows);
                    break;
                case FitType.Uniform:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var rect = rectTransform.rect;
            var parentWidth = rect.width;
            var parentHeight = rect.height;
            var cellWidth = parentWidth / columns - spacing.x / columns * (columns - 1) - padding.left / (float)columns - padding.right / (float)columns;
            var cellHeight = parentHeight / rows  - spacing.y / rows * (rows - 1) - padding.top / (float)rows - padding.bottom / (float)rows;
            cellWidth = fitX ? cellWidth : cellSize.x;
            cellHeight = fitY ? cellHeight : cellSize.y;
            cellSize = new Vector2(cellWidth, cellHeight);

            for (var i = 0; i < rectChildren.Count; i++)
            {
                var rowCount = i / columns;
                var columnCount = i % columns;
                
                var item = rectChildren[i];
                var xPos = cellSize.x * columnCount + spacing.x * columnCount + padding.left;
                var yPos = cellSize.y * rowCount + spacing.y * rowCount + padding.top;
                SetChildAlongAxis(item, 0, xPos, cellSize.x);
                SetChildAlongAxis(item, 1, yPos, cellSize.y);
            }
        }

        public override void CalculateLayoutInputVertical()
        {
        }

        public override void SetLayoutHorizontal()
        {
        }

        public override void SetLayoutVertical()
        {
        }
    }
}