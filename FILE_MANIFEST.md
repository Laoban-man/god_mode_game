# File Manifest — Divine Drift

Every code/asset file and its responsibility. Paths are relative to the project root.

## Data models — `Assets/Scripts/Data/`
- **Enums.cs** — `Philosophy`, `TerrainType`, `Era`, `AgentDirective`, `DiplomacyState`, `BattleResult`, `TimeScaleStep`.
- **PopulationStats.cs** — stat block (size, tech, fervour, attack, expand, coop, research) + `PhilosophyProfile`.
- **Cell.cs** — one icosphere/Goldberg tile: center, corners, neighbors, terrain, owner, frontier flag, terrain combat modifier.
- **TechTree.cs** — `TechNode` (era-gated, prereqs, stat deltas, perk unlocks) + per-population `TechProgress`.
- **Agent.cs** — `AgentPerk` runtime card + `Agent` (directive, target, perks, lifespan).
- **Population.cs** — a civilization: philosophy, stats, era, tech, owned cells, frontier, active agent, strength math.

## Config (ScriptableObjects) — `Assets/Scripts/Config/`
- **PhilosophyDefinition.cs** — authoring asset for one philosophy's stat multipliers.
- **TechTreeDefinition.cs** — authoring asset holding the shared tech graph.
- **AgentPerkDefinition.cs** — authoring asset for one agent perk card.
- **GameConfig.cs** — top-level tuning + references to all definitions; time-scale mapping.

## Core — `Assets/Scripts/Core/`
- **GameState.cs** — mutable match state: cells, populations, diplomacy relations, year/era, win flags. No Unity deps.
- **PopulationSpawner.cs** — places player + AI populations in separated corners with small starting territory.
- **GameManager.cs** — MonoBehaviour bootstrap; builds world, constructs systems, drives the per-year loop, handles win/defeat.

## Planet generation — `Assets/Scripts/Planet/`
- **IcosphereBuilder.cs** — subdivides an icosahedron into an icosphere (verts + triangles).
- **GoldbergPolyhedron.cs** — dual of the icosphere → hex/penta cells with centers, ordered corners, adjacency.
- **TerrainGenerator.cs** — assigns terrain + elevation via layered noise (deterministic per seed).
- **PlanetBuilder.cs** — façade: config → finished `Cell[]` + corner vertices (pure data).

## Simulation — `Assets/Scripts/Simulation/`
- **TimeManager.cs** — real-time → in-game-years clock, speed steps, per-year tick, era advance event.
- **TerritoryUtils.cs** — frontier recompute, contiguity checks (territory stays one piece), bordering-pops query.
- **GrowthSystem.cs** — yearly growth, research accumulation, tech unlocks, era advance, agent perk application.
- **CombatSystem.cs** — resolves battles for contested cells; transfers cells; enforces contiguity; handles elimination.
- **ExpansionSystem.cs** — peaceful claiming of unclaimed adjacent cells, weighted by will-to-expand and terrain cost.
- **DiplomacySystem.cs** — relationship transitions + cooperation benefits (science sharing, trade growth).
- **WinConditionSystem.cs** — conquest / peace victory and player-defeat evaluation.

## AI — `Assets/Scripts/AI/`
- **PopulationAI.cs** — philosophy-driven utility AI scoring expand / battle / ally and executing the best action.

## Agents — `Assets/Scripts/Agents/`
- **AgentSystem.cs** — spawn rules (cooldown, one at a time), available-perk gating, directive execution, expiry.

## Input — `Assets/Scripts/Input/`
- **PlanetCameraController.cs** — orbit rotate (drag), zoom (pinch/scroll), inertia. Touch-first + mouse fallback.
- **CellPicker.cs** — raycast the planet, map hit direction to nearest cell, raise selection event.

## UI — `Assets/Scripts/UI/`
- **HUDController.cs** — edge buttons (menu/speed/actions) + status labels (year/era/speed/pop/territory).
- **PhilosophySelectionPanel.cs** — start-of-game Attack/Cooperation/Defense picker.
- **AgentPanel.cs** — choose directive + up to N perks, enforce perk budget, spawn agent.
- **CellInfoPanel.cs** — selected cell's terrain/owner/relation; set agent target from a cell.
- **GameOverPanel.cs** — victory (conquest/peace) or defeat modal + restart hook.

## Rendering — `Assets/Scripts/Rendering/` + `Assets/Shaders/`
- **PlanetRenderer.cs** — builds the cell mesh (per-cell flat colors), collider, ownership/terrain tinting, selection highlight.
- **OutlineEffect.cs** — camera post-effect driving the black-edge outline shader.
- **PastelToon.shader** — banded pastel toon lighting using per-cell vertex colors.
- **OutlinePostProcess.shader** — screen-space depth+normal edge detection → black outlines.

## Editor tooling — `Assets/Scripts/Editor/`
- **DefaultContentGenerator.cs** — menu: generates philosophies, tech tree, perks, GameConfig.
- **SceneBootstrapper.cs** — menu: builds a runnable scene (camera, light, planet, canvas, GameManager).
- **DivineDrift.Editor.asmdef** — editor assembly referencing the runtime assembly.

## Assembly / project
- **Assets/Scripts/DivineDrift.asmdef** — runtime assembly (references Unity UI).
- **Assets/Materials/README_Materials.md** — notes on the auto-created PastelToon material.
- **Packages/manifest.json** — package dependencies (uGUI, physics, Android JNI, etc.).
- **ProjectSettings/ProjectVersion.txt** — pins Unity 2022.3 LTS.
- **README.md** — setup, brief mapping, runtime loop, wiring guide, TODOs.
