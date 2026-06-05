using System;
using System.Collections.Generic;

namespace DivineDrift.Data
{
    /// <summary>
    /// Data describing a single selectable perk card the player can attach to an
    /// Agent at spawn time. Authored via AgentPerkDefinition ScriptableObjects.
    /// </summary>
    [Serializable]
    public class AgentPerk
    {
        public string Id;
        public string DisplayName;
        public string Description;

        // Additive deltas applied to the owning population while the agent lives:
        public float DeltaAttackPower;
        public float DeltaWillToExpand;
        public float DeltaCooperation;
        public float DeltaResearchRate;
        public float DeltaReligiousFervour;

        /// <summary>If true, only available once a tech node unlocks it.</summary>
        public bool RequiresUnlock;
    }

    /// <summary>
    /// A live Agent (prophet / chosen one) created by the player. Provides a
    /// directive plus a limited set of perks to its population for a finite
    /// lifespan (measured in in-game years).
    /// </summary>
    [Serializable]
    public class Agent
    {
        public int Id;
        public int OwnerPopulationId;

        public AgentDirective Directive;
        /// <summary>Target population/cell for ExpandToward / Ally directives (-1 if unused).</summary>
        public int TargetId = -1;

        /// <summary>The limited set of perks the player chose for this agent.</summary>
        public List<AgentPerk> Perks = new List<AgentPerk>();

        public float SpawnYear;
        public float LifespanYears = 40f;
        public bool IsAlive = true;

        public float DeathYear => SpawnYear + LifespanYears;

        public bool ShouldExpire(float currentYear) => currentYear >= DeathYear;
    }
}
