using System;

namespace DivineDrift.Data
{
    /// <summary>
    /// The mutable stat block for a population. These drive expansion, combat,
    /// diplomacy and tech progression. Values are kept as floats and clamped
    /// to sensible ranges by the simulation systems.
    /// </summary>
    [Serializable]
    public struct PopulationStats
    {
        // --- Core combat / growth inputs ---
        public float PopulationSize;          // raw headcount-ish abstraction
        public float TechnologicalDevelopment; // research level, gates tech tree
        public float ReligiousFervour;         // boosted by Agents / Convert
        // Terrain contribution is computed per-cell at combat time, not stored here.

        // --- Behavioural tendencies (0..1 multipliers) ---
        public float AttackPower;     // offensive multiplier in combat
        public float WillToExpand;    // how readily the pop pushes borders
        public float CooperationCapacity; // likelihood/benefit of diplomacy

        // --- Derived/economy ---
        public float ResearchRate;    // tech gained per simulated year

        public static PopulationStats Default => new PopulationStats
        {
            PopulationSize = 100f,
            TechnologicalDevelopment = 1f,
            ReligiousFervour = 1f,
            AttackPower = 1f,
            WillToExpand = 1f,
            CooperationCapacity = 1f,
            ResearchRate = 1f
        };

        /// <summary>
        /// Multiplies the behavioural tendencies by a philosophy profile.
        /// Called once at population creation.
        /// </summary>
        public void ApplyPhilosophyProfile(PhilosophyProfile p)
        {
            AttackPower *= p.AttackPowerMul;
            WillToExpand *= p.WillToExpandMul;
            CooperationCapacity *= p.CooperationMul;
            ResearchRate *= p.ResearchRateMul;
        }

        public PopulationStats Clamped()
        {
            PopulationSize = Math.Max(0f, PopulationSize);
            TechnologicalDevelopment = Math.Max(0f, TechnologicalDevelopment);
            ReligiousFervour = Math.Max(0f, ReligiousFervour);
            AttackPower = Math.Max(0f, AttackPower);
            WillToExpand = Math.Max(0f, WillToExpand);
            CooperationCapacity = Math.Max(0f, CooperationCapacity);
            ResearchRate = Math.Max(0f, ResearchRate);
            return this;
        }
    }

    /// <summary>
    /// Immutable multiplier profile describing how a Philosophy reshapes stats.
    /// Authored as data in PhilosophyDefinition ScriptableObjects.
    /// </summary>
    [Serializable]
    public struct PhilosophyProfile
    {
        public float AttackPowerMul;
        public float WillToExpandMul;
        public float CooperationMul;
        public float ResearchRateMul; // proxy for "technological skills"
    }
}
