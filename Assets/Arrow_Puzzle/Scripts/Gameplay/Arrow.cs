using UnityEngine;
using Dreamteck.Splines;
using ArrowPuzzle.Data;

namespace ArrowPuzzle.Gameplay
{
    public class Arrow : MonoBehaviour
    {
        [SerializeField] private Renderer arrowRenderer;
        
        public int Direction { get; private set; }
        public Color ArrowColor { get; private set; }

        private int gridX;
        private int gridY;
        private bool isMoving = false;
        private Vector3 moveDirection;
        private float moveSpeed;
        private float followOffset;
        private bool attachedToSpline = false;

        public void Initialize(int x, int y, int direction, Color color, Vector3 rotation, float speed, Vector3 dirVec, float offset)
        {
            gridX = x;
            gridY = y;
            Direction = direction;
            ArrowColor = color;
            moveSpeed = speed;
            moveDirection = dirVec;
            followOffset = offset;
            
            if (arrowRenderer != null)
            {
                arrowRenderer.material.color = color;
            }
            
            transform.localRotation = Quaternion.Euler(rotation);
            Debug.Log($"[Arrow] Initialized: Name={gameObject.name}, Pos=({x},{y}), Direction={direction}");
        }

        private void Update()
        {
            // Click Detection (Using Raycast for better compatibility)
            if (Input.GetMouseButtonDown(0))
            {
                CheckForClick();
            }

            if (isMoving && !attachedToSpline)
            {
                transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
            }
        }

        private void CheckForClick()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    Debug.Log($"[Arrow] CLICK DETECTED on {gameObject.name}");
                    if (!isMoving && !attachedToSpline)
                    {
                        var gridManager = ArrowPuzzle.Core.ServiceLocator.Get<ArrowPuzzle.Interfaces.IGridManager>();
                        if (gridManager.CanMove(gridX, gridY, (ArrowDirection)Direction))
                        {
                            isMoving = true;
                            gridManager.ClearCell(gridX, gridY);
                            Debug.Log("[Arrow] Movement Started!");
                        }
                        else
                        {
                            Debug.Log("[Arrow] Path Blocked!");
                        }
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isMoving && !attachedToSpline)
            {
                SplineComputer spline = other.GetComponentInParent<SplineComputer>();
                if (spline != null)
                {
                    StartCoroutine(SmoothAttachToSpline(spline));
                }
            }
        }

        private System.Collections.IEnumerator SmoothAttachToSpline(SplineComputer spline)
        {
            isMoving = false;
            attachedToSpline = true;

            // 1. Calculate target point on spline
            SplineSample result = spline.Project(transform.position);
            
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;
            
            // We want to align the arrow with the spline's direction
            Quaternion targetRot = result.rotation; 

            float duration = 0.4f; // Short duration for a "snappy" but smooth feel
            float elapsed = 0f;

            // 2. Smoothly interpolate to the spline point
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // Use SmoothStep for a more natural feel
                float smoothT = Mathf.SmoothStep(0f, 1f, t);
                
                transform.position = Vector3.Lerp(startPos, result.position, smoothT);
                transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);
                
                yield return null;
            }

            // 3. Finalize attachment
            transform.position = result.position;
            transform.rotation = targetRot;

            SplineFollower follower = gameObject.AddComponent<SplineFollower>();
            follower.spline = spline;
            follower.followSpeed = moveSpeed;
            follower.followMode = SplineFollower.FollowMode.Uniform;
            follower.wrapMode = SplineFollower.Wrap.Loop;
            follower.motion.offset = new Vector2(0f, followOffset);
            follower.preserveUniformSpeedWithOffset = true;

            follower.RebuildImmediate();
            follower.SetPercent(result.percent);
            follower.follow = true;

            Debug.Log($"[Arrow] Smoothly attached to {spline.gameObject.name} at {result.percent:P2}");
        }

        // Removed the old AttachToSpline method to avoid confusion
    }
}
