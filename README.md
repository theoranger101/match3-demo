🧩 Match-3 Case Study — Unity Project

A modular, event-driven match-3 sample with clean separation of concerns, dependency injection, additive scene flow, pooled gameplay objects, deterministic level data, and a deadlock-free shuffle planner.

⸻

✨ Highlights
	•	Additive scene flow (Main ⟷ Game) via SceneTransitioner (UniTask).
	•	Dependency Injection (lightweight Utilities.DI.Container) with Installers.
	•	Event Bus (GEM) for decoupled systems (Grid, UI, Levels, Views).
	•	Grid runtime with pooling, gravity refill, and deadlock-free shuffle.
	•	Generalized skins: SkinLibrary → SkinTable → SkinSet + editor auto-builder.
	•	Custom Level authoring: rich LevelDefinitionEditor grid painter.
	•	Robust analyzers: GridAnalyzer (group sizes, matchables, appearance slots).

⸻

🗺️ Project Layout 

Assets/Scripts
├─ Blocks/
│  ├─ Block.cs (+ BlockEvents)
│  ├─ Types/ (MatchBlock, ObstacleBlock, PowerUpBlock)
│  ├─ UI/ (BlockView, BlockViewFactory)
│  └─ Data/ (SkinLibrary, SkinTable, SkinSet, SkinSetBuilder)
│
├─ Grid/
│  ├─ GridManager, GridRefillController, GridMath
│  ├─ GridAnalyzer, ShufflePlanner
│  ├─ ClickStrategies/ (MatchBlockClickStrategy, …)
│  └─ MatchDetectionStrategies/ (DFSMatchDetectionStrategy)
│
├─ Levels/
│  ├─ LevelManager, LevelController, LevelLoader
│  ├─ Data/ (LevelDefinition, LevelRules, GridGeometryConfig)
│  ├─ UI/ (MainLevelView, LevelUIController, LevelFinishView)
│  └─ Editor/ (LevelDefinitionEditor)
│
├─ Core/
│  ├─ SceneTransitioner (UniTask-based)
│  ├─ Services/ (MainSceneInstaller, GameSceneInstaller, ContainerProvider)
│  └─ Scene Events (SceneEventType/SceneEvent)
│
└─ Utilities/
   ├─ DI/ (Container, IInstaller, [Inject], InjectionExtensions)
   ├─ Events/ (GEM, Event<T>, EventDispatcher, ListenerCollection)
   ├─ Pooling/ (ListPool, HashSetPool, DictionaryPool, QueuePool, GameObjectPool)
   └─ ZzzLog (debug logging helpers)

⸻


🔧 Dependencies
	•	Unity: 2021 LTS or newer recommended.
	•	UniTask (for async scene loads): ensure the package is installed.
	•	DOTween (optional, used by views if you animate) — already guarded by usage.
	•	No external DI framework; a tiny custom Utilities.DI.

⸻

🧩 Gameplay

Grid
	•	GridManager keeps the 2D Block[,] state, applies clicks, pops, and refills.
	•	GridRefillController handles gravity and spawns new match blocks at the column head.

Analysis & Shuffle
	•	GridAnalyzer.Run(...):
	•	Computes connected groups, HasAnyPair, MatchGroupCounts, MatchableCells.
	•	Emits Appearances (slot indices per cell) — a generalized “tier”.
	•	If no pairs:
	•	ShufflePlanner.PlanShuffle(...) assigns pair-friendly colors fairly, without blind shuffles.
	•	Re-runs analyzer over changed cells and updates appearances.

Views & Skins
	•	BlockViewFactory spawns pooled BlockViews and injects a resolver:
Func<Block, int, Sprite> ResolveSprite = (block, slot) => SkinLibrary.Resolve(block, slot);
	•	BlockView listens for BlockAppearanceUpdated and refreshes sprite.
	•	SkinLibrary routes by block category to a SkinTable, which maps to a SkinSet (array of slots).

⸻

🎨 Skins & Editor Tools

Skin Assets
	•	SkinSet (ScriptableObject): { SkinId, Sprite[] Slots }.
	•	SkinTable: list of SkinSets, lookup by index or SkinId.
	•	SkinLibrary: three tables (Match, Obstacle, PowerUp) and a Resolve(...) method.

Auto-Builder (Editor)

Menu: Tools/Skins/Create SkinSets from Folder...
Pick a source root and destination under Assets/.
	•	Walks all subfolders; for any folder whose name starts with a leading number (e.g., 5_blue) it:
	•	Collects sprites directly under that folder.
	•	Uses the leading number in the sprite filename as slot index (e.g. 0_Default.png → slot 0).
	•	Saves a SkinSet asset to the chosen destination.

⸻

🧪 Level Authoring
	•	LevelDefinition:
	•	GridSize, list of Cells (type + data), LevelRules, SkinLibrary, MoveCount.
	•	OnValidate keeps GridSize in sync with LevelRules (Rows/Columns) and warns on ColorCount vs available match skins.
	•	LevelDefinitionEditor:
	•	Paint Match / Obstacle / PowerUp / Erase on a visual grid.
	•	Optional match group auto-increment.
	•	Validate button checks duplicates / out-of-bounds.

⸻

🧠 Events & DI

Events (GEM)
	•	Decoupled messaging across systems.
	•	Examples:
	•	Level: StartLevel, InitGrid, ResetGrid, ConsumeMove, RetryLevel, NextLevel, LevelFinished, ReturnToMenu.
	•	Grid: RequestAxis, RequestAdjacent, RequestSameType, ClearPosition, BlockMoved, TriggerRefill.
	•	Scene: Loading, Loaded.
	•	Block: BlockCreated, BlockClicked, BlockPopped, BlockAppearanceUpdated.

Remember to unsubscribe (BlockView does via instance listeners to avoid cross-pool leaks).

Dependency Injection
	•	ContainerProvider.Root holds the root Container.
	•	MainSceneInstaller and GameSceneInstaller register singletons and inject runtime components on scene load.
	•	Use [Inject] on fields, then container.InjectInto(target).

🎬 Scene Flow
	•	Main scene bootstraps UI and container bindings for menu.
	•	On Play:
	•	LevelManager → SceneTransitioner.ChangeSceneAsync(Game, additive: true).
	•	GameSceneInstaller registers grid, theme, rules for the active level, then injects.
	•	LevelController.StartLevel() dispatches InitGrid with spawn data.
	•	On finish / retry / next / quit: handled via LevelEvents + LevelUIController.

⸻

🧰 Logging
	•	Use ZzzLog helpers to enable/disable Unity logs. [Tools/Logging/Enable ZzzLog]
⸻

⚙️ Performance Notes
	•	Pooling everywhere (ListPool, DictionaryPool, HashSetPool, QueuePool, GameObjectPool, and block pools).
	•	Grid analysis uses stamp arrays to avoid clearing bool[,].
	•	No blind shuffles: ShufflePlanner distributes colors to adjacent pairs with fair quotas.
	•	Minimal allocation in hot paths (avoid LINQ, use pooled containers).

⸻

🧩 Extending
	•	Add new Block types:
	•	Implement a subclass (e.g., IceObstacleBlock), integrate into BlockExtensions/BlockFactory.
	•	Provide a SkinSet in the Obstacle or PowerUp table (via SkinLibrary).
	•	Add new click or match mechanics:
	•	Implement IBlockClickStrategy / IMatchDetectionStrategy, route in GridManager.
	•	Add power-ups:
	•	Fill out PowerUpBlock strategies (commented hooks are ready).

⸻

🗓️ Improvement Points
	•	Addressables for skins/configs and level content
	•	JSON import/export pipeline for level data
	•	Full power-up implementations 
	•	Visual polish: shuffle animation, pop/damage FX, transitions
	•	Editor UX: inline LevelRules editing in LevelDefinition, richer validators
	•	Player data persistence 
	•	Deeper separation of concerns (extract services/interfaces for grid, match, shuffle, spawn)
	•	Moves UI: on-screen counter with feedback
	•	Objectives framework: configurable goals (e.g., boxes cleared, balloons popped, score targets)

