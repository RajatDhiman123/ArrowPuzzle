using UnityEngine;
using ArrowPuzzle.Interfaces;
using ArrowPuzzle.Core;
using ArrowPuzzle.Data;
using System.Collections.Generic;

namespace ArrowPuzzle.Managers
{
    public class GridManager : MonoBehaviour, IGridManager
    {
        [SerializeField] private GameSettings settings;
        [SerializeField] private Transform gridParent;

        private List<GameObject> spawnedArrows = new List<GameObject>();
        private ArrowPuzzle.Gameplay.Arrow[,] gridArrows;
        private int gridWidth;
        private int gridHeight;

        private void Awake()
        {
            ServiceLocator.Register<IGridManager>(this);
        }

        public void GenerateGrid(int width, int height)
        {
            ClearGrid();
            gridWidth = width;
            gridHeight = height;
            gridArrows = new ArrowPuzzle.Gameplay.Arrow[width, height];

            if (settings == null || settings.arrowPrefab == null)
            {
                Debug.LogError("GridManager: GameSettings or ArrowPrefab is missing!");
                return;
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    SpawnArrow(x, y);
                }
            }
            
            CenterGrid(width, height);
        }

        private void SpawnArrow(int x, int y)
        {
            Vector3 position = new Vector3(x * settings.spacing, 0.4f, y * settings.spacing);
            GameObject arrowObj = Instantiate(settings.arrowPrefab, position, Quaternion.identity, gridParent);
            spawnedArrows.Add(arrowObj);

            var arrowComponent = arrowObj.GetComponent<ArrowPuzzle.Gameplay.Arrow>();
            if (arrowComponent != null)
            {
                gridArrows[x, y] = arrowComponent;
                int directionIndex = GetValidDirection(x, y);

                Color randomColor = settings.arrowColors.Count > 0 
                    ? settings.arrowColors[Random.Range(0, settings.arrowColors.Count)] 
                    : Color.white;
                
                Vector3 rotation = settings.GetRotationForDirection(directionIndex);
                Vector3 dirVec = settings.GetDirectionVector(directionIndex);
                arrowComponent.Initialize(x, y, directionIndex, randomColor, rotation, settings.moveSpeed, dirVec, settings.splineFollowOffset);
            }
            else
            {
                Debug.LogError($"[GridManager] FAILED to find Arrow component on {arrowObj.name}! Make sure the Arrow.cs script is attached to the prefab.");
            }
        }

        private int GetValidDirection(int x, int y)
        {
            if (settings.rotationMode == RotationMode.Fixed)
                return (int)settings.fixedDirection;

            List<int> possibleDirections = new List<int> { 0, 1, 2, 3 };
             
            for (int i = 0; i < x; i++)
            {
                var leftArrow = gridArrows[i, y];
                if (leftArrow != null && (ArrowDirection)leftArrow.Direction == ArrowDirection.Right)
                { 
                    possibleDirections.Remove((int)ArrowDirection.Left);
                    break; 
                }
            }
             
            for (int j = 0; j < y; j++)
            {
                var bottomArrow = gridArrows[x, j];
                if (bottomArrow != null && (ArrowDirection)bottomArrow.Direction == ArrowDirection.Up)
                { 
                    possibleDirections.Remove((int)ArrowDirection.Down);
                    break;
                }
            }

            if (possibleDirections.Count == 0) return Random.Range(0, 4); 
            return possibleDirections[Random.Range(0, possibleDirections.Count)];
        }

        public bool CanMove(int x, int y, ArrowDirection direction)
        {
            int checkX = x;
            int checkY = y;

            // Continue checking in the direction until we hit the grid edge
            while (true)
            {
                switch (direction)
                {
                    case ArrowDirection.Up: checkY++; break;
                    case ArrowDirection.Right: checkX++; break;
                    case ArrowDirection.Down: checkY--; break;
                    case ArrowDirection.Left: checkX--; break;
                }

                // If the check position is outside the grid bounds, the path is clear!
                if (checkX < 0 || checkX >= gridWidth || checkY < 0 || checkY >= gridHeight)
                {
                    return true;
                }

                // If we find another arrow in the path, it's blocked
                if (gridArrows[checkX, checkY] != null)
                {
                    Debug.Log($"[GridManager] Path blocked for arrow at ({x},{y}) by arrow at ({checkX},{checkY})");
                    return false;
                }
            }
        }

        public void ClearCell(int x, int y)
        {
            if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            {
                gridArrows[x, y] = null;
            }
        }

        private void CenterGrid(int width, int height)
        {
            if (gridParent != null)
            {
                float offsetX = (width - 1) * settings.spacing / 2f;
                float offsetZ = (height - 1) * settings.spacing / 2f;
                gridParent.position = new Vector3(-offsetX, 0, -offsetZ);
            }
        }

        public void ClearGrid()
        {
            foreach (var arrow in spawnedArrows)
            {
                if (arrow != null) Destroy(arrow);
            }
            spawnedArrows.Clear();
        }
    }
}
