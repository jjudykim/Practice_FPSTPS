using UnityEngine;
using UnityEngine.UI;

namespace jjudy
{
    [ExecuteInEditMode]
    public class FlexibleGridLayout : MonoBehaviour
    {
        [Header("Grid Settings")] 
        [SerializeField] private int columnCount = 4;
        [SerializeField] private float aspectRatio = 1f;
        
        private GridLayoutGroup gridLayoutGroup;
        private RectTransform rectTransform;

        private void Awake()
        {
            gridLayoutGroup = GetComponent<GridLayoutGroup>();
            rectTransform = GetComponent<RectTransform>();
            
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayoutGroup.constraintCount = columnCount;
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateCellSize();
        }
        
        #if UNITY_EDITOR
        private void Update()
        {
            if (Application.isPlaying == false)
                UpdateCellSize();
        }
        #endif

        private void UpdateCellSize()
        {
            if (gridLayoutGroup == null || rectTransform == null)
                return;
            
            float parentWidth = rectTransform.rect.width;
            float totalPadding = gridLayoutGroup.padding.left + gridLayoutGroup.padding.right;
            float totalSpacing = gridLayoutGroup.spacing.x * (columnCount - 1);

            float availableWidth = parentWidth - totalPadding - totalSpacing;
            float cellWidth = availableWidth / columnCount;

            if (cellWidth < 0)
                cellWidth = 0;

            Vector2 newSize = new Vector2(cellWidth, cellWidth / aspectRatio);
            gridLayoutGroup.cellSize = newSize;
        }

        public void SetColumnCount(int newCount)
        {
            columnCount = Mathf.Max(1, newCount);
            if (gridLayoutGroup != null)
                gridLayoutGroup.constraintCount = columnCount;
            UpdateCellSize();
        }
    }
}


