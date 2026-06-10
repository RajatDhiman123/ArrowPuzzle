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
        private int[,] gridDirections;

        private void Awake()
        {
            ServiceLocator.Register<IGridManager>(this);
        }

        public void GenerateGrid(int width, int height)
        {
            ClearGrid();
            gridDirections = new int[width, height];

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
                int directionIndex = GetValidDirection(x, y);
                gridDirections[x, y] = directionIndex;

                Color randomColor = settings.arrowColors.Count > 0 
                    ? settings.arrowColors[Random.Range(0, settings.arrowColors.Count)] 
                    : Color.white;
                
                Vector3 rotation = settings.GetRotationForDirection(directionIndex);
                Vector3 dirVec = settings.GetDirectionVector(directionIndex);
                arrowComponent.Initialize(directionIndex, randomColor, rotation, settings.moveSpeed, dirVec, settings.splineFollowOffset);
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
            
            // Check neighbor to the left (x-1, y)
            if (x > 0)
            {
                int leftNeighborDir = gridDirections[x - 1, y];
                if (leftNeighborDir == (int)ArrowDirection.Right)
                {
                    possibleDirections.Remove((int)ArrowDirection.Left);
                }
            }

            // Check neighbor below (x, y-1)
            if (y > 0)
            {
                int bottomNeighborDir = gridDirections[x, y - 1];
                if (bottomNeighborDir == (int)ArrowDirection.Up)
                {
                    possibleDirections.Remove((int)ArrowDirection.Down);
                }
            }

            if (possibleDirections.Count == 0) return Random.Range(0, 4); // Fallback
            return possibleDirections[Random.Range(0, possibleDirections.Count)];
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
