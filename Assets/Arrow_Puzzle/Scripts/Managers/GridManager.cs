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

        private void Awake()
        {
            ServiceLocator.Register<IGridManager>(this);
        }

        public void GenerateGrid(int width, int height)
        {
            ClearGrid();

            if (settings == null || settings.arrowPrefab == null)
            {
                Debug.LogError("GridManager: GameSettings or ArrowPrefab is missing!");
                return;
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    SpawnArrow(x, y);
                }
            }
            
            CenterGrid(width, height);
        }

        private void SpawnArrow(int x, int y)
        {
            Vector3 position = new Vector3(x * settings.spacing, 0, y * settings.spacing);
            GameObject arrowObj = Instantiate(settings.arrowPrefab, position, Quaternion.identity, gridParent);
            spawnedArrows.Add(arrowObj);

            var arrowComponent = arrowObj.GetComponent<ArrowPuzzle.Gameplay.Arrow>();
            if (arrowComponent != null)
            {
                int directionIndex = 0;
                if (settings.rotationMode == RotationMode.Random)
                {
                    directionIndex = Random.Range(0, 4);
                }
                else
                {
                    directionIndex = (int)settings.fixedDirection;
                }

                Color randomColor = settings.arrowColors.Count > 0 
                    ? settings.arrowColors[Random.Range(0, settings.arrowColors.Count)] 
                    : Color.white;
                
                Vector3 rotation = settings.GetRotationForDirection(directionIndex);
                arrowComponent.Initialize(directionIndex, randomColor, rotation);
            }
            else
            {
                Debug.LogWarning($"Arrow component not found on {arrowObj.name}. Rotation not applied.");
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
