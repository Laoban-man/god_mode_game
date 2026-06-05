using System.Collections.Generic;
using UnityEngine;
using DivineDrift.Data;

namespace DivineDrift.Planet
{
    /// <summary>
    /// Constructs the dual of a subdivided icosphere: a Goldberg polyhedron made
    /// of mostly hexagonal cells with exactly 12 pentagons. Each icosphere VERTEX
    /// becomes one CELL; the cell's corners are the centroids of the triangles
    /// surrounding that vertex; two cells are adjacent if their source vertices
    /// shared an edge.
    ///
    /// Output:
    ///  - Cell[] with Center, CornerVertexIndices (into a packed corner-vertex list),
    ///    and Neighbors filled in.
    ///  - A flat render mesh (triangulated cells) with per-cell submesh grouping
    ///    handled later by PlanetRenderer.
    /// </summary>
    public static class GoldbergPolyhedron
    {
        public struct BuildResult
        {
            public Cell[] Cells;
            public Vector3[] CornerVertices;  // triangle centroids, on unit sphere
            // Mesh assembly (triangle fans per cell) is produced by PlanetRenderer
            // using Cells[i].CornerVertexIndices ordered CCW.
        }

        public static BuildResult Build(IcosphereBuilder.MeshData ico)
        {
            int vertexCount = ico.Vertices.Count;
            int triCount = ico.Triangles.Count / 3;

            // 1. Corner vertices = centroid of each icosphere triangle, on sphere.
            var corners = new Vector3[triCount];
            for (int t = 0; t < triCount; t++)
            {
                int a = ico.Triangles[t * 3];
                int b = ico.Triangles[t * 3 + 1];
                int c = ico.Triangles[t * 3 + 2];
                corners[t] = ((ico.Vertices[a] + ico.Vertices[b] + ico.Vertices[c]) / 3f).normalized;
            }

            // 2. For each icosphere vertex, gather surrounding triangles (its cell corners)
            //    and neighboring vertices (its cell neighbors).
            var trianglesAtVertex = new List<int>[vertexCount];
            var neighborsAtVertex = new HashSet<int>[vertexCount];
            for (int v = 0; v < vertexCount; v++)
            {
                trianglesAtVertex[v] = new List<int>();
                neighborsAtVertex[v] = new HashSet<int>();
            }

            for (int t = 0; t < triCount; t++)
            {
                int a = ico.Triangles[t * 3];
                int b = ico.Triangles[t * 3 + 1];
                int c = ico.Triangles[t * 3 + 2];
                trianglesAtVertex[a].Add(t);
                trianglesAtVertex[b].Add(t);
                trianglesAtVertex[c].Add(t);

                neighborsAtVertex[a].Add(b); neighborsAtVertex[a].Add(c);
                neighborsAtVertex[b].Add(a); neighborsAtVertex[b].Add(c);
                neighborsAtVertex[c].Add(a); neighborsAtVertex[c].Add(b);
            }

            // 3. Build cells.
            var cells = new Cell[vertexCount];
            for (int v = 0; v < vertexCount; v++)
            {
                var cell = new Cell
                {
                    Id = v,
                    Center = ico.Vertices[v].normalized
                };
                cell.Neighbors.AddRange(neighborsAtVertex[v]);

                // Order the surrounding triangle-corners CCW around the cell center
                var ordered = SortCornersCCW(cell.Center, trianglesAtVertex[v], corners);
                cell.CornerVertexIndices.AddRange(ordered);
                cells[v] = cell;
            }

            return new BuildResult { Cells = cells, CornerVertices = corners };
        }

        /// <summary>
        /// Sort the triangle-centroid corners of a cell into consistent winding
        /// order around the cell normal, so the renderer can fan-triangulate them.
        /// </summary>
        private static List<int> SortCornersCCW(Vector3 center, List<int> cornerTris, Vector3[] corners)
        {
            // Build a tangent basis at the cell center.
            Vector3 normal = center;
            Vector3 tangent = Vector3.Cross(normal, Vector3.up);
            if (tangent.sqrMagnitude < 1e-6f) tangent = Vector3.Cross(normal, Vector3.right);
            tangent.Normalize();
            Vector3 bitangent = Vector3.Cross(normal, tangent);

            var withAngle = new List<(int idx, float angle)>(cornerTris.Count);
            foreach (int t in cornerTris)
            {
                Vector3 dir = corners[t] - center;
                float x = Vector3.Dot(dir, tangent);
                float y = Vector3.Dot(dir, bitangent);
                withAngle.Add((t, Mathf.Atan2(y, x)));
            }
            withAngle.Sort((p, q) => p.angle.CompareTo(q.angle));

            var result = new List<int>(withAngle.Count);
            foreach (var w in withAngle) result.Add(w.idx);
            return result;
        }
    }
}
