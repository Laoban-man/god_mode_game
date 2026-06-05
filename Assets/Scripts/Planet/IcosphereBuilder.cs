using System.Collections.Generic;
using UnityEngine;

namespace DivineDrift.Planet
{
    /// <summary>
    /// Builds a subdivided icosahedron (icosphere) and exposes the triangle/vertex
    /// topology needed to construct the dual Goldberg polyhedron (hexagons +
    /// 12 pentagons) used as game cells.
    ///
    /// This is the geometry foundation. The dual construction lives in
    /// GoldbergPolyhedron; terrain assignment in TerrainGenerator.
    /// </summary>
    public static class IcosphereBuilder
    {
        public struct MeshData
        {
            public List<Vector3> Vertices;     // unit-sphere positions
            public List<int> Triangles;        // flat index triplets
            // For each vertex: the list of triangle indices touching it (filled by caller if needed)
        }

        /// <summary>
        /// Generate an icosphere at the given subdivision level.
        /// Level 0 = base icosahedron (12 verts, 20 tris).
        /// Each level multiplies triangle count by 4.
        /// </summary>
        public static MeshData Build(int subdivisions)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            CreateBaseIcosahedron(verts, tris);

            var midpointCache = new Dictionary<long, int>();
            for (int i = 0; i < subdivisions; i++)
            {
                var newTris = new List<int>(tris.Count * 4);
                for (int t = 0; t < tris.Count; t += 3)
                {
                    int a = tris[t], b = tris[t + 1], c = tris[t + 2];
                    int ab = Midpoint(a, b, verts, midpointCache);
                    int bc = Midpoint(b, c, verts, midpointCache);
                    int ca = Midpoint(c, a, verts, midpointCache);

                    newTris.AddRange(new[] { a, ab, ca });
                    newTris.AddRange(new[] { b, bc, ab });
                    newTris.AddRange(new[] { c, ca, bc });
                    newTris.AddRange(new[] { ab, bc, ca });
                }
                tris = newTris;
            }

            return new MeshData { Vertices = verts, Triangles = tris };
        }

        private static int Midpoint(int i1, int i2, List<Vector3> verts, Dictionary<long, int> cache)
        {
            long key = i1 < i2 ? ((long)i1 << 32) + i2 : ((long)i2 << 32) + i1;
            if (cache.TryGetValue(key, out int existing)) return existing;

            Vector3 mid = ((verts[i1] + verts[i2]) * 0.5f).normalized;
            int idx = verts.Count;
            verts.Add(mid);
            cache[key] = idx;
            return idx;
        }

        private static void CreateBaseIcosahedron(List<Vector3> verts, List<int> tris)
        {
            float t = (1f + Mathf.Sqrt(5f)) / 2f;

            verts.Add(new Vector3(-1, t, 0).normalized);
            verts.Add(new Vector3(1, t, 0).normalized);
            verts.Add(new Vector3(-1, -t, 0).normalized);
            verts.Add(new Vector3(1, -t, 0).normalized);

            verts.Add(new Vector3(0, -1, t).normalized);
            verts.Add(new Vector3(0, 1, t).normalized);
            verts.Add(new Vector3(0, -1, -t).normalized);
            verts.Add(new Vector3(0, 1, -t).normalized);

            verts.Add(new Vector3(t, 0, -1).normalized);
            verts.Add(new Vector3(t, 0, 1).normalized);
            verts.Add(new Vector3(-t, 0, -1).normalized);
            verts.Add(new Vector3(-t, 0, 1).normalized);

            int[] faces =
            {
                0,11,5, 0,5,1, 0,1,7, 0,7,10, 0,10,11,
                1,5,9, 5,11,4, 11,10,2, 10,7,6, 7,1,8,
                3,9,4, 3,4,2, 3,2,6, 3,6,8, 3,8,9,
                4,9,5, 2,4,11, 6,2,10, 8,6,7, 9,8,1
            };
            tris.AddRange(faces);
        }
    }
}
