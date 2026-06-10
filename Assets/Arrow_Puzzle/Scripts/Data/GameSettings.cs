using UnityEngine;
using System.Collections.Generic;

namespace ArrowPuzzle.Data
{
    public enum RotationMode { Random, Fixed }
    public enum ArrowDirection { Up = 0, Right = 1, Down = 2, Left = 3 }

    [CreateAssetMenu(fileName = "GameSettings", menuName = "ArrowPuzzle/GameSettings")]
    public class GameSettings : ScriptableObject
    {
        [Header("Grid Settings")]
        public int defaultWidth = 12;
        public int defaultHeight = 12;
        public float spacing = 1.1f;

        [Header("Arrow Settings")]
        public GameObject arrowPrefab;
        public List<Color> arrowColors;
        public float fixedXRotation = 90f;
        public float moveSpeed = 10f;
        public float splineFollowOffset = 0.5f;
        
        [Header("Rotation Logic")]
        public RotationMode rotationMode = RotationMode.Random;
        public ArrowDirection fixedDirection = ArrowDirection.Up;

        public Vector3 GetRotationForDirection(int directionIndex)
        {
            // 0: Up (0 deg), 1: Right (90 deg), 2: Down (180 deg), 3: Left (270 deg)
            float yRotation = 90f * directionIndex;
            return new Vector3(fixedXRotation, yRotation, 0);
        }

        public Vector3 GetDirectionVector(int directionIndex)
        {
            switch ((ArrowDirection)directionIndex)
            {
                case ArrowDirection.Up: return Vector3.forward;
                case ArrowDirection.Right: return Vector3.right;
                case ArrowDirection.Down: return Vector3.back;
                case ArrowDirection.Left: return Vector3.left;
                default: return Vector3.zero;
            }
        }
    }
}
