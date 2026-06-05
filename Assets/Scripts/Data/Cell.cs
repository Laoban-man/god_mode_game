using System.Collections.Generic;
using UnityEngine;

namespace DivineDrift.Data
{
    /// <summary>
    /// A single tile on the icosphere. Most cells are hexagons; exactly 12 are
    /// pentagons (the original icosahedron vertices). Cells are the atomic unit
    /// of territory ownership and terrain.
    /// </summary>
    public class Cell
    {
        public int Id;

        /// <summary>Unit-sphere centroid (also the world direction from planet center).</summary>
        public Vector3 Center;

        /// <summary>Indices of mesh vertices that make up this cell's polygon (CCW).</summary>
        public List<int> CornerVertexIndices = new List<int>();

        /// <summary>Adjacent cell ids (share an edge). 5 for pentagons, 6 for hexagons.</summary>
        public List<int> Neighbors = new List<int>();

        public TerrainType Terrain = TerrainType.Plains;

        /// <summary>Owning population id, or -1 if unclaimed.</summary>
        public int OwnerPopulationId = -1;

        /// <summary>True if at least one neighbor is owned by a different population (or unclaimed).</summary>
        public bool IsFrontier;

        /// <summary>Elevation in [0,1], used for terrain assignment and subtle visual offset.</summary>
        public float Elevation;

        public bool IsOwned => OwnerPopulationId >= 0;

        /// <summary>
        /// Terrain combat modifier applied to the holder's strength.
        /// Mountains add, deserts subtract, forest small bonus, plains neutral.
        /// </summary>
        public float TerrainCombatModifier()
        {
            switch (Terrain)
            {
                case TerrainType.Mountain: return 0.35f;   // strong defensive terrain
                case TerrainType.Forest:   return 0.10f;
                case TerrainType.Plains:   return 0.0f;
                case TerrainType.Desert:   return -0.25f;
                default:                   return 0f;
            }
        }
    }
}
