using UnityEngine;
using DivineDrift.Data;

namespace DivineDrift.Config
{
    /// <summary>
    /// Data asset describing one philosophy's stat profile and flavour.
    /// Create three of these (Attack, Cooperation, Defense) in the project.
    ///
    /// Design intent (from the game brief):
    ///  - Attack:      strong attack, strong expansion, low tech, low cooperation
    ///  - Cooperation: high tech, high cooperation, low attack, slightly low expansion
    ///  - Defense:     low expansion, high attack, high tech, medium cooperation
    /// </summary>
    [CreateAssetMenu(menuName = "DivineDrift/Philosophy Definition", fileName = "Philosophy")]
    public class PhilosophyDefinition : ScriptableObject
    {
        public Philosophy philosophy;
        [TextArea] public string description;

        [Header("Stat multipliers")]
        [Tooltip("Offensive combat multiplier.")]
        public float attackPowerMul = 1f;
        [Tooltip("Tendency to push borders outward.")]
        public float willToExpandMul = 1f;
        [Tooltip("Diplomacy likelihood/benefit.")]
        public float cooperationMul = 1f;
        [Tooltip("Proxy for technological skill / research speed.")]
        public float researchRateMul = 1f;

        public PhilosophyProfile ToProfile() => new PhilosophyProfile
        {
            AttackPowerMul = attackPowerMul,
            WillToExpandMul = willToExpandMul,
            CooperationMul = cooperationMul,
            ResearchRateMul = researchRateMul
        };
    }
}
