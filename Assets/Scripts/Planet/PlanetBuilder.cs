using UnityEngine;
using DivineDrift.Config;
using DivineDrift.Data;

namespace DivineDrift.Planet
{
    /// <summary>
    /// High-level façade that turns a GameConfig into a finished planet: builds the
    /// icosphere, derives the Goldberg cells, assigns terrain, and hands back the
    /// data the renderer and simulation need. Pure data — no GameObjects spawned
    /// here (PlanetRenderer handles meshes).
    /// </summary>
    public static class PlanetBuilder
    {
        public struct PlanetData
        {
            public Cell[] Cells;
            public Vector3[] CornerVertices;
            public float Radius;
        }

        public static PlanetData Build(GameConfig config)
        {
            var ico = IcosphereBuilder.Build(config.subdivisionLevel);
            var goldberg = GoldbergPolyhedron.Build(ico);
            TerrainGenerator.Assign(goldberg.Cells, config.randomSeed);

            Debug.Log($"[PlanetBuilder] Built planet: {goldberg.Cells.Length} cells, " +
                      $"{goldberg.CornerVertices.Length} corners (subdiv {config.subdivisionLevel}).");

            return new PlanetData
            {
                Cells = goldberg.Cells,
                CornerVertices = goldberg.CornerVertices,
                Radius = config.planetRadius
            };
        }
    }
}
