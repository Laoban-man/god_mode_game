using System.Collections.Generic;
using UnityEngine;
using DivineDrift.Config;
using DivineDrift.Core;
using DivineDrift.Data;
using DivineDrift.Simulation;

namespace DivineDrift.Agents
{
    /// <summary>
    /// Manages the player's divine intervention: spawning an Agent (prophet / chosen
    /// one) with a DIRECTIVE plus a limited number of PERKS, enforcing the spawn
    /// cooldown, executing the directive each year, and expiring agents at end of life.
    ///
    /// Only the player spawns agents in the base design; AI populations rely on their
    /// utility heuristics. (Hook left to allow AI agents later.)
    /// </summary>
    public class AgentSystem
    {
        private readonly GameConfig _config;
        private readonly GameState _state;
        private readonly ExpansionSystem _expansion;
        private readonly CombatSystem _combat;
        private readonly DiplomacySystem _diplomacy;

        private float _lastSpawnYearPlayer = float.NegativeInfinity;
        private int _nextAgentId = 1;

        public AgentSystem(GameConfig config, GameState state, ExpansionSystem expansion,
                           CombatSystem combat, DiplomacySystem diplomacy)
        {
            _config = config;
            _state = state;
            _expansion = expansion;
            _combat = combat;
            _diplomacy = diplomacy;
        }

        public bool CanSpawnForPlayer()
        {
            var p = _state.Player;
            if (p == null || !p.IsAlive) return false;
            if (p.ActiveAgent != null && p.ActiveAgent.IsAlive) return false;
            return _state.CurrentYear - _lastSpawnYearPlayer >= _config.agentCooldownYears;
        }

        /// <summary>
        /// Returns the perks the player may currently choose: base perks plus any
        /// unlocked by the population's tech progress, capped by selection in the UI.
        /// </summary>
        public List<AgentPerk> AvailablePerksForPlayer()
        {
            var p = _state.Player;
            var list = new List<AgentPerk>();
            foreach (var def in _config.agentPerks)
            {
                if (def.requiresUnlock && !IsPerkUnlocked(p, def.id)) continue;
                list.Add(def.ToRuntime());
            }
            return list;
        }

        private bool IsPerkUnlocked(Population p, string perkId)
        {
            if (_config.techTree == null) return false;
            foreach (var nodeId in p.Tech.UnlockedNodeIds)
            {
                var node = _config.techTree.GetNode(nodeId);
                if (node != null && node.UnlocksAgentPerks.Contains(perkId)) return true;
            }
            return false;
        }

        /// <summary>
        /// Spawn an agent for the player. 'perks' must not exceed config.maxAgentPerks.
        /// </summary>
        public Agent SpawnPlayerAgent(AgentDirective directive, int targetId, List<AgentPerk> perks)
        {
            if (!CanSpawnForPlayer()) return null;

            var p = _state.Player;
            int allowed = Mathf.Min(perks.Count, _config.maxAgentPerks);
            var chosen = perks.GetRange(0, allowed);

            var agent = new Agent
            {
                Id = _nextAgentId++,
                OwnerPopulationId = p.Id,
                Directive = directive,
                TargetId = targetId,
                Perks = chosen,
                SpawnYear = _state.CurrentYear,
                LifespanYears = _config.agentLifespanYears,
                IsAlive = true
            };
            p.ActiveAgent = agent;
            _lastSpawnYearPlayer = _state.CurrentYear;
            Debug.Log($"[Agent] Player spawned agent #{agent.Id} ({directive}) with {chosen.Count} perks.");
            return agent;
        }

        /// <summary>Executes living agents' directives and expires the dead ones.</summary>
        public void TickYear()
        {
            foreach (var pop in _state.AlivePopulations())
            {
                var agent = pop.ActiveAgent;
                if (agent == null) continue;

                if (agent.ShouldExpire(_state.CurrentYear))
                {
                    agent.IsAlive = false;
                    pop.ActiveAgent = null;
                    continue;
                }
                ExecuteDirective(pop, agent);
            }
        }

        private void ExecuteDirective(Population pop, Agent agent)
        {
            switch (agent.Directive)
            {
                case AgentDirective.Expand:
                    _expansion.TickYear(pop, budget: 2); // agent accelerates expansion
                    break;

                case AgentDirective.ExpandToward:
                    // TODO: bias expansion/combat toward the target cell/population direction.
                    _expansion.TickYear(pop, budget: 2);
                    if (agent.TargetId >= 0) TryAttackTowardTarget(pop, agent.TargetId);
                    break;

                case AgentDirective.Ally:
                    if (agent.TargetId >= 0)
                    {
                        var target = _state.GetPopulation(agent.TargetId);
                        if (target != null && target.IsAlive)
                            _diplomacy.Propose(pop, target, DiplomacyState.Allied);
                    }
                    break;

                case AgentDirective.Defend:
                    // Defensive stance: small fervour boost, no expansion (handled via perks).
                    pop.Stats.ReligiousFervour += 0.02f;
                    break;

                case AgentDirective.Convert:
                    pop.Stats.ReligiousFervour += 0.05f;
                    break;
            }
            pop.Stats = pop.Stats.Clamped();
        }

        private void TryAttackTowardTarget(Population pop, int targetPopId)
        {
            foreach (int cellId in pop.FrontierCells)
                foreach (int n in _state.GetCell(cellId).Neighbors)
                {
                    var nb = _state.GetCell(n);
                    if (nb.OwnerPopulationId == targetPopId)
                    {
                        _state.SetRelation(pop.Id, targetPopId, DiplomacyState.AtWar);
                        _combat.ResolveBattleForCell(pop, _state.GetPopulation(targetPopId), n);
                        return;
                    }
                }
        }
    }
}
