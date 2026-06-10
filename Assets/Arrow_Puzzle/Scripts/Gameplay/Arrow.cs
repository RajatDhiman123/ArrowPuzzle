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

        private bool isMoving = false;
        private Vector3 moveDirection;
        private float moveSpeed;
        private float followOffset;
        private bool attachedToSpline = false;

        public void Initialize(int direction, Color color, Vector3 rotation, float speed, Vector3 dirVec, float offset)
        {
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
            Debug.Log($"[Arrow] Initialized: Name={gameObject.name}, Direction={direction}, MoveDir={dirVec}, Speed={speed}");
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
                        isMoving = true;
                        Debug.Log("[Arrow] Movement Started!");
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[Arrow] Trigger Entered with: {other.gameObject.name}");
            if (isMoving && !attachedToSpline)
            {
                SplineComputer spline = other.GetComponentInParent<SplineComputer>();
                if (spline != null)
                {
                    Debug.Log($"[Arrow] SPLINE FOUND: {spline.gameObject.name}. Attaching...");
                    AttachToSpline(spline);
                }
            }
        }

        private void AttachToSpline(SplineComputer spline)
        {
            isMoving = false;
            attachedToSpline = true;

            SplineFollower follower = gameObject.AddComponent<SplineFollower>();
            follower.spline = spline;
            follower.followSpeed = moveSpeed;
            follower.followMode = SplineFollower.FollowMode.Uniform;
            follower.wrapMode = SplineFollower.Wrap.Loop;
            
            // Apply Y offset relative to the spline
            follower.motion.offset = new Vector2(0f, followOffset);
            follower.preserveUniformSpeedWithOffset = true;

            // Find the nearest point on the spline and start from there
            SplineSample result = spline.Project(transform.position);
            follower.SetPercent(result.percent);

            follower.follow = true;
        }
    }
}
