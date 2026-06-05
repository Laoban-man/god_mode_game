using System.Collections.Generic;
using UnityEngine;
using DivineDrift.Core;
using DivineDrift.Data;
using DivineDrift.Planet;

namespace DivineDrift.Rendering
{
    /// <summary>
    /// Builds the planet render mesh from Goldberg cells and keeps per-cell vertex
    /// colors in sync with terrain + ownership each tick. Each cell is fan-triangulated
    /// from its center to its corner vertices; all cell vertices are duplicated per cell
    /// so each can carry an independent flat color (flat pastel shading + crisp borders).
    ///
    /// Also exposes the cell-center array for the CellPicker, adds a SphereCollider for
    /// raycasting, and highlights the currently selected cell.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class PlanetRenderer : MonoBehaviour
    {
        [Header("Material")]
        public Material pastelToonMaterial;   // uses DivineDrift/PastelToon

        [Header("Terrain palette (pastel)")]
        public Color plainsColor   = new Color(0.78f, 0.87f, 0.62f);
        public Color desertColor   = new Color(0.93f, 0.85f, 0.62f);
        public Color forestColor   = new Color(0.55f, 0.78f, 0.58f);
        public Color mountainColor = new Color(0.80f, 0.78f, 0.82f);

        [Header("Ownership tinting")]
        [Range(0, 1)] public float ownerTintStrength = 0.55f;
        public Color selectionColor = new Color(1f, 1f, 0.6f);

        private GameState _state;
        private float _radius;
        private Mesh _mesh;

        // Maps cell id -> the contiguous range of mesh vertices for that cell,
        // so we can recolor a single cell cheaply without rebuilding geometry.
        private int[] _cellVertexStart;
        private int[] _cellVertexCount;
        private Color[] _colors;
        private int _selectedCell = -1;

        public void Build(GameState state, PlanetBuilder.PlanetData planet)
        {
            _state = state;
            _radius = planet.Radius;
            BuildMesh(planet);
            AddCollider();
            RefreshAllColors();
        }

        private void BuildMesh(PlanetBuilder.PlanetData planet)
        {
            var cells = planet.Cells;
            var corners = planet.CornerVertices;

            var verts = new List<Vector3>();
            var norms = new List<Vector3>();
            var tris = new List<int>();
            _cellVertexStart = new int[cells.Length];
            _cellVertexCount = new int[cells.Length];

            for (int c = 0; c < cells.Length; c++)
            {
                var cell = cells[c];
                int start = verts.Count;
                _cellVertexStart[c] = start;

                // Center vertex.
                Vector3 center = cell.Center * _radius;
                verts.Add(center);
                norms.Add(cell.Center);

                int n = cell.CornerVertexIndices.Count;
                for (int k = 0; k < n; k++)
                {
                    Vector3 cw = corners[cell.CornerVertexIndices[k]] * _radius;
                    verts.Add(cw);
                    norms.Add(cell.Center); // flat-shaded: all share cell normal
                }
                _cellVertexCount[c] = 1 + n;

                // Fan triangles (center, k, k+1).
                for (int k = 0; k < n; k++)
                {
                    int a = start;                 // center
                    int b = start + 1 + k;
                    int d = start + 1 + ((k + 1) % n);
                    tris.Add(a); tris.Add(b); tris.Add(d);
                }
            }

            _mesh = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            _mesh.SetVertices(verts);
            _mesh.SetNormals(norms);
            _mesh.SetTriangles(tris, 0);
            _colors = new Color[verts.Count];
            _mesh.colors = _colors;

            GetComponent<MeshFilter>().sharedMesh = _mesh;
            if (pastelToonMaterial != null)
                GetComponent<MeshRenderer>().sharedMaterial = pastelToonMaterial;
        }

        private void AddCollider()
        {
            var col = GetComponent<SphereCollider>();
            if (col == null) col = gameObject.AddComponent<SphereCollider>();
            col.center = Vector3.zero;
            col.radius = _radius;
        }

        // ---- Coloring ----

        public void RefreshAllColors()
        {
            for (int c = 0; c < _state.Cells.Length; c++)
                WriteCellColor(c, ComputeCellColor(c));
            _mesh.colors = _colors;
        }

        /// <summary>Recolor only the cells owned by a population (after a border change).</summary>
        public void RefreshCells(IEnumerable<int> cellIds)
        {
            foreach (int c in cellIds) WriteCellColor(c, ComputeCellColor(c));
            _mesh.colors = _colors;
        }

        public void SetSelectedCell(int cellId)
        {
            int prev = _selectedCell;
            _selectedCell = cellId;
            if (prev >= 0) WriteCellColor(prev, ComputeCellColor(prev));
            if (cellId >= 0) WriteCellColor(cellId, selectionColor);
            _mesh.colors = _colors;
        }

        private Color ComputeCellColor(int cellId)
        {
            var cell = _state.Cells[cellId];
            Color terrain = TerrainColor(cell.Terrain);
            if (cell.IsOwned)
            {
                var owner = _state.GetPopulation(cell.OwnerPopulationId);
                if (owner != null)
                    return Color.Lerp(terrain, owner.BannerColor, ownerTintStrength);
            }
            return terrain;
        }

        private Color TerrainColor(TerrainType t)
        {
            switch (t)
            {
                case TerrainType.Plains:   return plainsColor;
                case TerrainType.Desert:   return desertColor;
                case TerrainType.Forest:   return forestColor;
                case TerrainType.Mountain: return mountainColor;
                default:                   return plainsColor;
            }
        }

        private void WriteCellColor(int cellId, Color color)
        {
            int start = _cellVertexStart[cellId];
            int count = _cellVertexCount[cellId];
            for (int i = 0; i < count; i++) _colors[start + i] = color;
        }
    }
}
