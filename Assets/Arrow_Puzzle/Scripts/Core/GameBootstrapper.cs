using UnityEngine;
using ArrowPuzzle.Interfaces;
using ArrowPuzzle.Core;
using ArrowPuzzle.Data;

namespace ArrowPuzzle.Core
{
    public class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private GameSettings gameSettings;
        
        private void Start()
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            Debug.Log("Game Bootstrapping...");
            
            // In a real production environment, you might have multiple managers to register.
            // They register themselves in Awake, and we use them in Start.
            
            try 
            {
                var gridManager = ServiceLocator.Get<IGridManager>();
                gridManager.GenerateGrid(gameSettings.defaultWidth, gameSettings.defaultHeight);
                Debug.Log($"Grid generated: {gameSettings.defaultWidth}x{gameSettings.defaultHeight}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Bootstrapping failed: {e.Message}");
            }
        }
    }
}
