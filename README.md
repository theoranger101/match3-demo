# Match-3 Case Study 

> A systems-driven match-3 sample that showcases dependency injection, an event bus, additive scene flow, pooled objects, deterministic level data, and a deadlock-free shuffle.

---

## 🎥 Demo

- **Video:** 

---

## Table of Contents

- [Highlights](#-highlights)
- [Gameplay Overview](#-gameplay-overview)
- [Tech & Libraries](#-tech--libraries)
- [Architecture](#-architecture)
  - [Scene Flow](#scene-flow)
  - [Grid Pipeline](#grid-pipeline)
  - [Skins Pipeline](#skins-pipeline)
- [Design Choices](#-design-choices)
- [Editor Tooling](#-editor-tooling)
- [Improvement Points](#-improvement-points)

---

## ✨ Highlights

- **Additive scene flow** (`Main` ⟷ `Game`) with async loading (UniTask).
- **Lightweight DI** (`Utilities.DI.Container`) via scene installers.
- **Global Event Bus (GEM)** for decoupled systems (Grid, UI, Levels, Views).
- **Grid runtime** with gravity refill & **deadlock-free shuffle**.
- **Generalized skins**: `SkinLibrary → SkinTable → SkinSet` + editor auto-builder.
- **In-editor level painter** (`LevelDefinitionEditor`) with validation.
- **Analyzer** (`GridAnalyzer`) computes groups, matchables, and per-cell appearance slots.

---

## Gameplay Overview

- Tap **2+** adjacent (4-dir) **match blocks** to pop.
- Gravity pulls blocks down; empty cells are **refilled**.
- **Appearance slots** are computed by `GridAnalyzer` and pushed to views.
- **No blind shuffles**: `ShufflePlanner` redistributes colors fairly to create pairs.

> Obstacles & Power-ups are scaffolded. `ObstacleBlock` (e.g., WoodenBox) demonstrates state/damage. Power-ups are ready for strategy wiring.

---

## Tech & Libraries

- **Engine:** Unity 2022.3.62f
- **Language:** C#

**Third-party:**
- **UniTask** — async/await for Unity: <https://github.com/Cysharp/UniTask>  
- **DOTween** — tweening engine (optional/guarded): <https://dotween.demigiant.com/>

**Custom utilities:**
- `Utilities.DI` (tiny DI)
- `Utilities.Events` (GEM event bus)
- `Utilities.Pooling` (collection & GameObject pools)

---

## Architecture

### Scene Flow
- `LevelManager` orchestrates **Play → load Game additively** via `SceneTransitioner` → inject services → `LevelController.StartLevel()`.
- On finish / retry / next / quit: dispatch `LevelEvent`s; UI reacts; scenes unload as needed.

### Grid Pipeline
1. **Input** → click strategies (`IBlockClickStrategy`).
2. **Pop** & **Refill** with gravity (`GridRefillController`).
3. **Analyze** (`GridAnalyzer`) → connected groups, `HasAnyPair`, appearance slots.
4. If deadlocked → **`ShufflePlanner`** assigns pair-friendly colors → re-analyze & update.

### Skins Pipeline
- `BlockViewFactory` injects a sprite resolver:  
  `Func<Block, int, Sprite> ResolveSprite`
- `SkinLibrary` routes by category (Match/Obstacle/PowerUp) → `SkinTable` → `SkinSet.Slots[slotIndex]`.
- **Editor** `SkinSetBuilder` auto-creates `SkinSet` assets based on folder/filename conventions.

---

## Design Choices

- **Factories**  
  `BlockFactory` and `BlockViewFactory` centralize creation/release to enable pooling and reduce coupling.

- **Pooling**  
  Pooled collections minimize allocations in hot paths (click, refill, analyze).

- **Event-based structure (GEM)**  
  Strongly-typed events let systems evolve independently. Input → Grid → UI flows without direct references.

### Shuffle Planner

`ShufflePlanner` **plans** color assignments instead of doing blind random shuffles. The goal is to **maximize new adjacent pairs** while respecting each color’s available **quota**, with an optional **pair cap** via `maxPairs`.

1. **Collect candidates & quotas**
   - Gather all *matchable* cells (`matchableCells` if provided, otherwise scan the grid).
   - Build **`counts[groupId]`**:
     - If **`groupCounts`** is provided → copy it.
     - Otherwise → infer by counting each cell’s **`MatchGroupId`**.

2. **Randomize cell order**
   - **`ShuffleInPlace(cells)`** for spatial diversity (seeded RNG supported for reproducibility).

3. **Greedy pairing on the adjacency graph**
   - For each cell in **shuffled** order:
     - Collect its 4-neighbors that are **match blocks** and **unused**.
     - **Shuffle** neighbors; pick the **first free neighbor** to form a **pair**; otherwise mark the cell as a **single**.
     - **Stamp** both paired positions as used so they aren’t reused.

4. **Fair color distribution to pairs (quotas)**
   - Compute **pair quotas** per color: **`need = counts[color] / 2`**.
   - Build **`pairQuotas`** as `(color, need)` items.
   - **Shuffle** the list of pairs.
   - Iterate colors in **round-robin** while quotas remain to keep distribution **fair**.

5. **Assign pairs (respecting `maxPairs`)**
   - **If `maxPairs` < `int.MaxValue`:**
     - Round-robin across **`pairQuotas`**, assigning **pairs** while **`pairsMade < maxPairs`** and pairs remain.
     - For each assigned pair: **`counts[color] -= 2`**.
     - **All leftover pairs** (not consumed due to the cap) are **degraded to singles** by splitting each pair into **two singles**.
   - **If `maxPairs == int.MaxValue`:**
     - Round-robin across **`pairQuotas`**, assigning as many **pairs** as quotas allow, decrementing **`counts[color] -= 2`**.
     - For any **leftover pairs**: try **`TakeGroupWithAtLeast(counts, 2)`**; if none is available, **degrade to singles**.

6. **Place singles**
   - **Capped mode (`maxPairs` limited):** for each single, pick **`PickNonAdjacentOrAny(counts, grid, pos)`** to **avoid accidentally creating new pairs**; if none, use **`PickAny(counts)`**. Then **`counts[color] -= 1`** (if present).
   - **Maximize mode:** for each single, try **`TakeGroupWithAtLeast(counts, 1)`**; if none, use **`PickAny(counts)`**. Then **`counts[color] -= 1`**.

7. **Return a compact plan**
   - Emit **`ShuffleAssignment(GridPosition, MatchGroupId)`** for every assigned cell (pairs and singles).
   - Caller applies the plan, then runs **`GridAnalyzer`** once on the **dirty set** to update visuals/tiers.
  
### Resolution batching (grid scope)

`using (grid.ResolutionBatch)` wraps a burst of actions (e.g., multiple pops from a single click). Inside the scope we:

- **Accumulate** all side effects (cells that became empty, moves, clears, etc.).
- **Defer** expensive operations (refill + analyze) until the **outermost** scope exits.

This batching model also makes it easier to add effects/animations later (e.g., wait for a pop animation, then do a single refill/analyze).

### Stamp-based visitation 

`GridAnalyzer` avoids repeatedly scanning and clearing memory:

- Uses an `int[,] s_Visited` **stamp array** and a monotonically increasing `s_VisitStamp`.  
  - Marking a cell as visited = `s_Visited[x,y] = s_VisitStamp`.  
  - A fresh pass just increments `s_VisitStamp`—**no array clears**.
- Builds a **frontier** from either:
  - The whole grid (first scan), or
  - **Dirty cells + their 4-neighbors** (incremental scan).
- Runs DFS/BFS per component **once**, emitting:
  - `HasAnyPair`, `MatchGroupCounts`, `MatchableCells`.
  - `Appearances` (slot indices) to update all relevant views **in a single pass**.

---

## Editor Tooling

- **LevelDefinitionEditor**
  - Paint **Match / Obstacle / PowerUp / Erase** on a visual grid.
  - Optional **auto-increment** match group IDs.
  - **Validate** button checks duplicates & out-of-bounds.
  - `OnValidate` keeps `GridSize` in sync with `LevelRules` and warns on `ColorCount` vs available skins.
  - Find existing `LevelDefinition`s under `Assets/Data/Levels/LevelDefinitions/...`
  - Add new `LevelDefinition` asset to `LevelManager` in `Main` scene to test in game. (improvement point!)

- **SkinSetBuilder** (`Tools/Skins/Create SkinSets from Folder...`)
  - Walks subfolders; for each folder with a **leading number** (e.g., `5_blue`) collects sprites in that folder.
  - Uses **leading number in each sprite filename** as **slot index** (e.g., `0_Default.png` → slot `0`).
  - Saves `SkinSet` assets into your chosen destination.

- **Toggle Unity Logs** (`Tools/Logging/Enable Zzzlog`)

---

## Improvement Points

- **Addressables** for skins/configs and level content  
- **JSON** import/export pipeline for level data  
- Visual polish: **shuffle animation**, pop/damage FX, transitions  
- Deeper **separation of concerns** (extract services/interfaces for grid, match, shuffle, spawn)  

---
