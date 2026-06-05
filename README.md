# Divine Drift — Unity Android God-Game (Architecture + Stubs)

A minimalist, pastel 3D "god game" for Android built in **Unity 2022 LTS (Built-in Render Pipeline)**.
You play a deity discreetly guiding one population across the ages on a procedurally
generated planet, expanding territory through war, cooperation, or defense until you
either cover the planet or are surrounded only by allies.

This package is a **full architecture with runnable stubs**: every system exists as a
complete, compiling class; some behaviours are intentionally simple with `TODO`
markers where you'll want to deepen the design.

---

## Quick start

1. **Create a new Unity project** with **2022.3 LTS** (Built-in Render Pipeline / 3D template).
2. Copy the `Assets/` and `Packages/manifest.json` from this package into your project
   (or open this folder directly as the project root).
3. In Unity, run the menu **DivineDrift → Generate Default Content**.
   - Creates `Assets/ScriptableObjects/GameConfig.asset` plus the three philosophies,
     a small tech tree, and agent perks.
4. Run **DivineDrift → Build Playable Scene**.
   - Creates a camera (with the black-outline post effect), a directional light, the
     planet object, an EventSystem, a Canvas with placeholder panels, and a wired
     `GameManager`. Save it as `Assets/Scenes/Main.unity`.
5. Lay out the HUD buttons/labels around the screen edges and assign the references on
   each panel (see **Scene wiring** below), then press **Play**.
6. For Android: **File → Build Settings → Android → Switch Platform**, then Build.

> Tip: start with `subdivisionLevel = 4` (~2.5k cells) while iterating; raise to `5`
> (~10k cells) for the dense look. The brief's 2,000–5,000 range sits between 4 and 5.

---

## How the game maps to the brief

| Brief requirement | Where it lives |
|---|---|
| God guiding one population among many | `GameState`, `Population`, `GameManager` |
| Start in Bronze Age (or other eras) | `GameConfig.startingEra`, `Era` enum |
| Pick Attack / Cooperation / Defense | `PhilosophySelectionPanel`, `PhilosophyDefinition` |
| Philosophy perks (attack/expand/tech/coop) | `PhilosophyDefinition` → `PopulationStats.ApplyPhilosophyProfile` |
| Goal: cover planet OR all neighbours allied | `WinConditionSystem` |
| Agent (prophet) with directive + limited perks | `AgentSystem`, `AgentPanel`, `Agent`, `AgentPerkDefinition` |
| Speed time 1–100 in-game years / real minute | `TimeManager`, `TimeScaleStep`, `GameConfig.YearsPerSecond` |
| Populations start in separate corners | `PopulationSpawner` (max-min separated seeds) |
| Battle on touching borders; winner takes loser | `CombatSystem` |
| Cooperate (trade, science) | `DiplomacySystem` |
| Strength = size + tech + fervour + terrain | `Population.EffectiveStrengthForCell` |
| Strength reduced by territory perimeter | `Population.PerimeterPenaltyFactor` |
| Territory must stay one piece | `TerritoryUtils` contiguity checks |
| Terrains: plains/desert/forest/mountain | `TerrainType`, `TerrainGenerator`, `Cell.TerrainCombatModifier` |
| Light tech tree per population | `TechTreeDefinition`, `TechProgress`, `GrowthSystem` |
| Rotate / zoom / click the planet | `PlanetCameraController`, `CellPicker` |
| Edge buttons (menu/speed/actions) | `HUDController` |
| Pastel, black edges, not realistic, no clouds | `PastelToon.shader`, `OutlinePostProcess.shader`, `OutlineEffect` |

---

## Runtime loop

Each simulated **year** (`TimeManager.OnYearTick`), `GameManager` runs in order:

1. `AgentSystem.TickYear` — the player's living agent executes its directive.
2. `GrowthSystem.TickYear` — population growth, research, tech unlocks, era advance.
3. `PopulationAI.TickYear` — each AI population scores **expand / battle / ally** and acts.
4. `DiplomacySystem.TickYear` — cooperating/allied pairs share science and trade.
5. `WinConditionSystem.Evaluate` — checks conquest, peace, or defeat.
6. `PlanetRenderer.RefreshAllColors` — repaints ownership tints.

---

## Scene wiring (manual references to finish)

The scene bootstrapper creates panels and a GameManager but leaves the *visual layout*
to you. For each panel script, drag the child buttons/labels onto its fields:

- **HUDController**: edge buttons (`menu`, `speedUp`, `speedDown`, `actions`) + status labels.
- **PhilosophySelectionPanel**: three buttons (+ optional description texts).
- **AgentPanel**: directive buttons, a `perkTogglePrefab` + `perkListRoot`, confirm button.
- **CellInfoPanel**: terrain/owner/relation labels + "set target" button.
- **GameOverPanel**: title/body + restart button (wire to `GameManager.Restart`).

Place the edge buttons around the screen borders (menu top-left, speed controls
bottom-center, actions bottom-right) per the brief.

---

## Tuning knobs (`GameConfig`)

- `subdivisionLevel` — planet cell density.
- `aiPopulationCount`, `startingTerritorySize` — match setup.
- `maxAgentPerks`, `agentLifespanYears`, `agentCooldownYears` — agent feel.
- `secondsPerMinute` — keep at 60 unless you redefine the time mapping.
- Philosophy multipliers live on the three `PhilosophyDefinition` assets.

---

## Known stubs / next steps (search for `TODO`)

- `CellPicker.FindNearestCell` is O(n); add a spatial hash for very high cell counts.
- `AgentDirective.ExpandToward` does not yet bias expansion *direction* toward a target.
- Tech tree is intentionally tiny; extend `DefaultContentGenerator.MakeTechTree`.
- No save/load yet (`GameState` is serialization-friendly by design).
- Borders are repainted fully each year; switch to dirty-cell updates for perf.
- AI never spawns agents (hook exists in `AgentSystem`).

See `FILE_MANIFEST.md` for a one-line description of every file.
