using System;
using System.Collections.Generic;

namespace DivineDrift.Data
{
    /// <summary>
    /// A single node in the light per-population tech tree. Nodes are gated by a
    /// minimum Era and a tech-point cost. Unlocking a node applies stat deltas
    /// and may unlock agent perks or new abilities.
    /// </summary>
    [Serializable]
    public class TechNode
    {
        public string Id;
        public string DisplayName;
        public Era RequiredEra;
        public float Cost;                 // in accumulated TechnologicalDevelopment points
        public List<string> Prerequisites = new List<string>(); // node ids

        // Effects applied on unlock (additive deltas to PopulationStats):
        public float DeltaAttackPower;
        public float DeltaWillToExpand;
        public float DeltaCooperation;
        public float DeltaResearchRate;
        public float DeltaReligiousFervour;

        /// <summary>Optional agent perk ids this node unlocks for selection.</summary>
        public List<string> UnlocksAgentPerks = new List<string>();
    }

    /// <summary>
    /// Per-population progression state over the (data-authored) tech graph.
    /// The graph definition itself lives in a TechTreeDefinition ScriptableObject;
    /// this class only tracks which nodes a given population has unlocked and
    /// its accumulated research toward the next.
    /// </summary>
    [Serializable]
    public class TechProgress
    {
        public HashSet<string> UnlockedNodeIds = new HashSet<string>();
        public float AccumulatedResearch;

        public bool IsUnlocked(string nodeId) => UnlockedNodeIds.Contains(nodeId);

        /// <summary>
        /// Returns true if every prerequisite and the era gate is satisfied and
        /// enough research is banked. Does not mutate state.
        /// TODO: full validation against the live TechTreeDefinition.
        /// </summary>
        public bool CanUnlock(TechNode node, Era currentEra)
        {
            if (IsUnlocked(node.Id)) return false;
            if ((int)currentEra < (int)node.RequiredEra) return false;
            if (AccumulatedResearch < node.Cost) return false;
            foreach (var pre in node.Prerequisites)
                if (!IsUnlocked(pre)) return false;
            return true;
        }
    }
}
