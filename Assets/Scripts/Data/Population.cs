using System;
using System.Collections.Generic;
using UnityEngine;

namespace DivineDrift.Data
{
    /// <summary>
    /// A single civilization on the planet. One is controlled by the player (the
    /// "god"); the rest are driven by philosophy-based AI. Owns a contiguous set
    /// of cells, a stat block, a tech progress record, and an optional live agent.
    /// </summary>
    [Serializable]
    public class Population
    {
        public int Id;
        public string Name;
        public Color BannerColor;     // pastel tint used to paint owned cells
        public bool IsPlayer;

        public Philosophy Philosophy;
        public PopulationStats Stats = PopulationStats.Default;

        public Era Era = Era.Bronze;
        public TechProgress Tech = new TechProgress();

        /// <summary>Cells currently owned. Must stay a single contiguous piece.</summary>
        public HashSet<int> OwnedCells = new HashSet<int>();

        /// <summary>Frontier cell ids cached each tick for cheap expansion/combat scans.</summary>
        public List<int> FrontierCells = new List<int>();

        /// <summary>Active agent or null. Only one agent alive at a time per pop.</summary>
        public Agent ActiveAgent;

        public bool IsAlive => OwnedCells.Count > 0;

        // ---- Strength model ----
        // Strength = f(PopulationSize, Tech, Fervour, Terrain) reduced by border perimeter.

        /// <summary>
        /// Base (terrain-independent) strength from the three internal stats.
        /// Terrain is added per contested cell at combat time.
        /// </summary>
        public float BaseStrength()
        {
            float s = Stats.PopulationSize * 0.01f
                    + Stats.TechnologicalDevelopment * 0.6f
                    + Stats.ReligiousFervour * 0.4f;
            return s * Mathf.Max(0.1f, Stats.AttackPower);
        }

        /// <summary>
        /// Perimeter penalty: longer borders = harder to defend, so effective
        /// strength is divided by a factor growing with frontier length.
        /// </summary>
        public float PerimeterPenaltyFactor()
        {
            int perimeter = FrontierCells.Count;
            // Soft penalty: 1.0 with no border, asymptotically growing.
            return 1f + perimeter * 0.02f;
        }

        /// <summary>
        /// Effective strength when contesting a specific terrain cell.
        /// </summary>
        public float EffectiveStrengthForCell(Cell contested)
        {
            float terrain = contested != null ? contested.TerrainCombatModifier() : 0f;
            float raw = BaseStrength() * (1f + terrain);
            return raw / PerimeterPenaltyFactor();
        }
    }
}
