using UnityEngine;

namespace ArrowPuzzle.Gameplay
{
    public class Arrow : MonoBehaviour
    {
        [SerializeField] private Renderer arrowRenderer;
        
        public int Direction { get; private set; }
        public Color ArrowColor { get; private set; }

        public void Initialize(int direction, Color color, Vector3 rotation)
        {
            Direction = direction;
            ArrowColor = color;
            
            if (arrowRenderer != null)
            {
                arrowRenderer.material.color = color;
            }
            
            transform.localRotation = Quaternion.Euler(rotation);
            // Debug.Log($"Arrow Initialized: Dir={direction}, Rot={rotation}, FinalRot={transform.localRotation.eulerAngles}");
        }
    }
}
