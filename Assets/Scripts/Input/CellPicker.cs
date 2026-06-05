using UnityEngine;
using DivineDrift.Core;
using DivineDrift.Data;
using DivineDrift.Planet;

namespace DivineDrift.InputControl
{
    /// <summary>
    /// Converts a tap / click on the planet into a Cell selection. Raycasts against
    /// the planet's sphere collider, then maps the hit point (a direction from the
    /// planet center) to the nearest cell center. Raises OnCellSelected.
    /// </summary>
    public class CellPicker : MonoBehaviour
    {
        public Camera cam;
        public Transform planet;

        /// <summary>Raised with the selected cell id (or -1 if nothing hit).</summary>
        public event System.Action<int> OnCellSelected;

        private GameState _state;
        private PlanetRenderer _renderer; // provides cell-center lookup acceleration

        public void Initialize(GameState state, PlanetRenderer renderer)
        {
            _state = state;
            _renderer = renderer;
        }

        private void Update()
        {
            if (_state == null) return;

            bool tapped = false;
            Vector2 screenPos = default;

#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButtonDown(0) && !PointerOverUI(Input.mousePosition))
            {
                tapped = true; screenPos = Input.mousePosition;
            }
#endif
            if (Input.touchCount == 1)
            {
                var t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Began && !PointerOverUI(t.position))
                {
                    tapped = true; screenPos = t.position;
                }
            }

            if (tapped) Pick(screenPos);
        }

        private void Pick(Vector2 screenPos)
        {
            Ray ray = cam.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                // Hit direction from planet center (planet assumed at origin).
                Vector3 dir = (hit.point - planet.position).normalized;
                // Undo planet rotation so direction is in cell (local) space.
                Vector3 localDir = Quaternion.Inverse(planet.rotation) * dir;
                int cellId = FindNearestCell(localDir);
                OnCellSelected?.Invoke(cellId);
            }
            else
            {
                OnCellSelected?.Invoke(-1);
            }
        }

        /// <summary>
        /// Nearest-cell lookup. Naive O(n) scan is fine up to a few thousand cells;
        /// PlanetRenderer may provide a spatial hash for larger counts (TODO).
        /// </summary>
        private int FindNearestCell(Vector3 localDir)
        {
            int best = -1;
            float bestDot = -2f;
            var cells = _state.Cells;
            for (int i = 0; i < cells.Length; i++)
            {
                float d = Vector3.Dot(cells[i].Center, localDir);
                if (d > bestDot) { bestDot = d; best = i; }
            }
            return best;
        }

        private bool PointerOverUI(Vector2 screenPos)
        {
            var es = UnityEngine.EventSystems.EventSystem.current;
            return es != null && es.IsPointerOverGameObject();
        }
    }
}
