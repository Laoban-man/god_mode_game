using UnityEngine;
using DivineDrift.Data;

namespace DivineDrift.Planet
{
    /// <summary>
    /// Assigns terrain types and elevation to cells using layered 3D value noise
    /// sampled at each cell center. Deterministic for a given seed. No water layer
    /// in the base scenario (whole sphere is land split into the four terrains),
    /// but a sea-level threshold hook is left for future expansion.
    /// </summary>
    public static class TerrainGenerator
    {
        public static void Assign(Cell[] cells, int seed)
        {
            var rng = new System.Random(seed);
            // Random offsets so each seed produces a distinct world.
            Vector3 oElev = RandomOffset(rng);
            Vector3 oMoist = RandomOffset(rng);
            Vector3 oTemp = RandomOffset(rng);

            foreach (var cell in cells)
            {
                Vector3 p = cell.Center * 1.7f; // noise frequency
                float elev = Fbm(p + oElev);
                float moisture = Fbm(p * 1.3f + oMoist);
                // Latitude proxy: |y| → cooler poles; combine with noise for variety.
                float temp = (1f - Mathf.Abs(cell.Center.y)) * 0.7f + Fbm(p * 0.8f + oTemp) * 0.3f;

                cell.Elevation = elev;
                cell.Terrain = Classify(elev, moisture, temp);
            }
        }

        private static TerrainType Classify(float elev, float moisture, float temp)
        {
            if (elev > 0.72f) return TerrainType.Mountain;     // high ground
            if (temp < 0.30f && moisture < 0.45f) return TerrainType.Desert; // hot/dry-cold barren
            if (moisture > 0.58f) return TerrainType.Forest;   // wet lowlands
            return TerrainType.Plains;                         // default
        }

        private static Vector3 RandomOffset(System.Random rng)
            => new Vector3(
                (float)rng.NextDouble() * 1000f,
                (float)rng.NextDouble() * 1000f,
                (float)rng.NextDouble() * 1000f);

        // --- Fractal value noise in [0,1] using Unity's 2D Perlin on projections ---
        private static float Fbm(Vector3 p)
        {
            float total = 0f, amp = 1f, freq = 1f, max = 0f;
            for (int o = 0; o < 4; o++)
            {
                total += Sample3(p * freq) * amp;
                max += amp;
                amp *= 0.5f;
                freq *= 2f;
            }
            return Mathf.Clamp01(total / max);
        }

        // Pseudo-3D noise by averaging three orthogonal Perlin slices.
        private static float Sample3(Vector3 p)
        {
            float xy = Mathf.PerlinNoise(p.x, p.y);
            float yz = Mathf.PerlinNoise(p.y, p.z);
            float zx = Mathf.PerlinNoise(p.z, p.x);
            return (xy + yz + zx) / 3f;
        }
    }
}
