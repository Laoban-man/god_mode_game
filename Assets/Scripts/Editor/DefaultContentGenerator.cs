#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DivineDrift.Config;
using DivineDrift.Data;

namespace DivineDrift.EditorTools
{
    /// <summary>
    /// One-click generator for the default data assets so the project runs out of the
    /// box: three PhilosophyDefinitions, a small TechTreeDefinition, a handful of
    /// AgentPerkDefinitions, and a GameConfig wiring them together. Run from the menu
    /// "DivineDrift/Generate Default Content". Safe to re-run (overwrites the assets).
    ///
    /// These values implement the brief's philosophy profiles:
    ///   Attack:      +attack, +expand, -tech, -coop
    ///   Cooperation: +tech,   +coop,  -attack, slightly -expand
    ///   Defense:     -expand,  +attack, +tech, medium coop
    /// </summary>
    public static class DefaultContentGenerator
    {
        private const string Root = "Assets/ScriptableObjects";

        [MenuItem("DivineDrift/Generate Default Content")]
        public static void Generate()
        {
            EnsureFolder(Root);

            var attack = MakePhilosophy("Philosophy_Attack", Philosophy.Attack,
                "Strong attack and expansion; weaker technology and cooperation.",
                attackMul: 1.5f, expandMul: 1.4f, coopMul: 0.6f, researchMul: 0.7f);

            var coop = MakePhilosophy("Philosophy_Cooperation", Philosophy.Cooperation,
                "High technology and cooperation; lower attack and slightly lower expansion.",
                attackMul: 0.7f, expandMul: 0.9f, coopMul: 1.6f, researchMul: 1.5f);

            var defense = MakePhilosophy("Philosophy_Defense", Philosophy.Defense,
                "Lower expansion; strong attack and technology; medium cooperation.",
                attackMul: 1.3f, expandMul: 0.7f, coopMul: 1.0f, researchMul: 1.3f);

            var perks = MakePerks();
            var tech = MakeTechTree(perks);

            var config = ScriptableObject.CreateInstance<GameConfig>();
            config.subdivisionLevel = 5;       // ~10k cells; lower to 4 for ~2.5k
            config.planetRadius = 10f;
            config.aiPopulationCount = 7;
            config.startingTerritorySize = 3;
            config.philosophies = new List<PhilosophyDefinition> { attack, coop, defense };
            config.techTree = tech;
            config.agentPerks = perks;
            config.maxAgentPerks = 2;
            config.agentLifespanYears = 40f;
            config.agentCooldownYears = 30f;
            config.startingEra = Era.Bronze;
            CreateOrReplace(config, $"{Root}/GameConfig.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[DivineDrift] Default content generated under " + Root +
                      ". Assign GameConfig.asset to the GameManager.");
        }

        private static PhilosophyDefinition MakePhilosophy(
            string name, Philosophy p, string desc,
            float attackMul, float expandMul, float coopMul, float researchMul)
        {
            var asset = ScriptableObject.CreateInstance<PhilosophyDefinition>();
            asset.philosophy = p;
            asset.description = desc;
            asset.attackPowerMul = attackMul;
            asset.willToExpandMul = expandMul;
            asset.cooperationMul = coopMul;
            asset.researchRateMul = researchMul;
            CreateOrReplace(asset, $"{Root}/{name}.asset");
            return asset;
        }

        private static List<AgentPerkDefinition> MakePerks()
        {
            var list = new List<AgentPerkDefinition>();
            list.Add(MakePerk("perk_zealot", "Zealot", "Inspires fervour and aggression.",
                dAtk: 0.4f, dFerv: 0.5f));
            list.Add(MakePerk("perk_sage", "Sage", "Accelerates research.",
                dRes: 0.6f));
            list.Add(MakePerk("perk_diplomat", "Diplomat", "Improves cooperation.",
                dCoop: 0.6f));
            list.Add(MakePerk("perk_pioneer", "Pioneer", "Drives expansion.",
                dExp: 0.6f));
            list.Add(MakePerk("perk_warlord", "Warlord", "Greatly boosts attack (requires unlock).",
                dAtk: 0.9f, requiresUnlock: true));
            return list;
        }

        private static AgentPerkDefinition MakePerk(string id, string name, string desc,
            float dAtk = 0, float dExp = 0, float dCoop = 0, float dRes = 0, float dFerv = 0,
            bool requiresUnlock = false)
        {
            var a = ScriptableObject.CreateInstance<AgentPerkDefinition>();
            a.id = id; a.displayName = name; a.description = desc;
            a.deltaAttackPower = dAtk; a.deltaWillToExpand = dExp;
            a.deltaCooperation = dCoop; a.deltaResearchRate = dRes;
            a.deltaReligiousFervour = dFerv; a.requiresUnlock = requiresUnlock;
            CreateOrReplace(a, $"{Root}/AgentPerk_{id}.asset");
            return a;
        }

        private static TechTreeDefinition MakeTechTree(List<AgentPerkDefinition> perks)
        {
            var t = ScriptableObject.CreateInstance<TechTreeDefinition>();
            t.nodes = new List<TechNode>
            {
                new TechNode { Id="bronze_tools", DisplayName="Bronze Tools",
                    RequiredEra=Era.Bronze, Cost=8f, DeltaResearchRate=0.2f },
                new TechNode { Id="writing", DisplayName="Writing",
                    RequiredEra=Era.Bronze, Cost=15f, Prerequisites={"bronze_tools"},
                    DeltaResearchRate=0.3f, DeltaCooperation=0.2f },
                new TechNode { Id="iron_working", DisplayName="Iron Working",
                    RequiredEra=Era.Iron, Cost=25f, Prerequisites={"bronze_tools"},
                    DeltaAttackPower=0.4f, UnlocksAgentPerks={"perk_warlord"} },
                new TechNode { Id="philosophy", DisplayName="Philosophy",
                    RequiredEra=Era.Classical, Cost=40f, Prerequisites={"writing"},
                    DeltaCooperation=0.4f, DeltaResearchRate=0.4f },
            };
            CreateOrReplace(t, $"{Root}/TechTree.asset");
            return t;
        }

        // ---- asset helpers ----
        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
        }

        private static void CreateOrReplace(Object asset, string path)
        {
            var existing = AssetDatabase.LoadMainAssetAtPath(path);
            if (existing != null) AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(asset, path);
        }
    }
}
#endif
