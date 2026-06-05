using UnityEngine;
using DivineDrift.Data;

namespace DivineDrift.Config
{
    /// <summary>
    /// Authoring asset for a single agent perk card. The player picks a limited
    /// number of these (see GameConfig.maxAgentPerks) when spawning an agent.
    /// </summary>
    [CreateAssetMenu(menuName = "DivineDrift/Agent Perk", fileName = "AgentPerk")]
    public class AgentPerkDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;

        [Header("Stat deltas while the agent lives")]
        public float deltaAttackPower;
        public float deltaWillToExpand;
        public float deltaCooperation;
        public float deltaResearchRate;
        public float deltaReligiousFervour;

        public bool requiresUnlock;

        public AgentPerk ToRuntime() => new AgentPerk
        {
            Id = id,
            DisplayName = displayName,
            Description = description,
            DeltaAttackPower = deltaAttackPower,
            DeltaWillToExpand = deltaWillToExpand,
            DeltaCooperation = deltaCooperation,
            DeltaResearchRate = deltaResearchRate,
            DeltaReligiousFervour = deltaReligiousFervour,
            RequiresUnlock = requiresUnlock
        };
    }
}
