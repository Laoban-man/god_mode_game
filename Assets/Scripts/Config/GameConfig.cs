using System.Collections.Generic;
using UnityEngine;
using DivineDrift.Data;

namespace DivineDrift.Config
{
    /// <summary>
    /// Top-level tuning asset. One instance wired into the GameManager. Holds
    /// references to all data definitions plus global balance knobs.
    /// </summary>
    [CreateAssetMenu(menuName = "DivineDrift/Game Config", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Planet generation")]
        [Tooltip("Icosphere subdivision level. ~level 5 ≈ 10,242 verts; tune for 2k–5k cells.")]
        public int subdivisionLevel = 5;
        [Tooltip("Planet radius in world units.")]
        public float planetRadius = 10f;
        public int randomSeed = 12345;

        [Header("Populations")]
        public int aiPopulationCount = 7;
        [Tooltip("Starting owned cells per population.")]
        public int startingTerritorySize = 3;

        [Header("Definitions")]
        public List<PhilosophyDefinition> philosophies = new List<PhilosophyDefinition>();
        public TechTreeDefinition techTree;
        public List<AgentPerkDefinition> agentPerks = new List<AgentPerkDefinition>();

        [Header("Agents")]
        [Tooltip("Max perks the player may attach to one agent (the 'limited number').")]
        public int maxAgentPerks = 2;
        public float agentLifespanYears = 40f;
        [Tooltip("Minimum in-game years between agent spawns.")]
        public float agentCooldownYears = 30f;

        [Header("Time scale (real minute -> in-game years)")]
        public float secondsPerMinute = 60f;

        [Header("Starting conditions")]
        public Era startingEra = Era.Bronze;

        /// <summary>Maps a TimeScaleStep to in-game years per real second.</summary>
        public float YearsPerSecond(TimeScaleStep step)
        {
            float yearsPerMinute = step switch
            {
                TimeScaleStep.Paused => 0f,
                TimeScaleStep.Years1PerMinute => 1f,
                TimeScaleStep.Years5PerMinute => 5f,
                TimeScaleStep.Years20PerMinute => 20f,
                TimeScaleStep.Years50PerMinute => 50f,
                TimeScaleStep.Years100PerMinute => 100f,
                _ => 0f
            };
            return yearsPerMinute / secondsPerMinute;
        }

        public PhilosophyDefinition GetPhilosophy(Philosophy p)
            => philosophies.Find(d => d.philosophy == p);
    }
}
