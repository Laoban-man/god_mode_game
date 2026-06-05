using UnityEngine;

namespace DivineDrift.InputControl
{
    /// <summary>
    /// Orbit-style camera around the planet at origin. Supports:
    ///  - Drag (one finger / left mouse) to rotate the view around the planet.
    ///  - Pinch (two fingers) / scroll wheel to zoom in and out.
    /// Designed for Android touch first, with mouse fallback in the editor.
    /// </summary>
    public class PlanetCameraController : MonoBehaviour
    {
        [Header("Targets")]
        public Transform planet;              // planet at world origin
        public Camera cam;

        [Header("Orbit")]
        public float rotateSpeed = 0.2f;
        public float inertiaDamping = 4f;

        [Header("Zoom")]
        public float minDistance = 12f;
        public float maxDistance = 40f;
        public float zoomSpeed = 0.02f;
        public float scrollZoomSpeed = 2f;

        private float _distance = 22f;
        private float _yaw;
        private float _pitch = 10f;
        private Vector2 _rotateVelocity;

        private void Reset()
        {
            cam = GetComponent<Camera>();
        }

        private void Update()
        {
            HandleTouch();
            HandleMouse();
            ApplyInertia();
            ApplyTransform();
        }

        private void HandleTouch()
        {
            if (Input.touchCount == 1)
            {
                var t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Moved && !PointerOverUI(t.position))
                {
                    _yaw += t.deltaPosition.x * rotateSpeed;
                    _pitch -= t.deltaPosition.y * rotateSpeed;
                    _rotateVelocity = new Vector2(t.deltaPosition.x, -t.deltaPosition.y) * rotateSpeed;
                }
            }
            else if (Input.touchCount == 2)
            {
                var a = Input.GetTouch(0);
                var b = Input.GetTouch(1);
                Vector2 aPrev = a.position - a.deltaPosition;
                Vector2 bPrev = b.position - b.deltaPosition;
                float prevMag = (aPrev - bPrev).magnitude;
                float curMag = (a.position - b.position).magnitude;
                float delta = prevMag - curMag;
                _distance = Mathf.Clamp(_distance + delta * zoomSpeed, minDistance, maxDistance);
            }
        }

        private void HandleMouse()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButton(0) && !PointerOverUI(Input.mousePosition))
            {
                _yaw += Input.GetAxis("Mouse X") * rotateSpeed * 20f;
                _pitch -= Input.GetAxis("Mouse Y") * rotateSpeed * 20f;
            }
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.0001f)
                _distance = Mathf.Clamp(_distance - scroll * scrollZoomSpeed * 10f, minDistance, maxDistance);
#endif
        }

        private void ApplyInertia()
        {
            if (Input.touchCount == 0 && !Input.GetMouseButton(0))
            {
                _yaw += _rotateVelocity.x;
                _pitch += _rotateVelocity.y;
                _rotateVelocity = Vector2.Lerp(_rotateVelocity, Vector2.zero, Time.deltaTime * inertiaDamping);
            }
            _pitch = Mathf.Clamp(_pitch, -85f, 85f);
        }

        private void ApplyTransform()
        {
            Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 pos = rot * new Vector3(0f, 0f, -_distance);
            transform.position = pos;
            transform.LookAt(Vector3.zero);
        }

        /// <summary>True if the screen point is over a UI element (so we don't rotate).</summary>
        private bool PointerOverUI(Vector2 screenPos)
        {
            var es = UnityEngine.EventSystems.EventSystem.current;
            if (es == null) return false;
            return es.IsPointerOverGameObject();
        }
    }
}
