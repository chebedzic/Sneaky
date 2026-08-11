# CLAUDE.md

## Project overview

Unity project (Editor 6000.3.20f1 / Unity 6, URP). Fresh scaffold beyond Unity's default template assets (`Assets/TutorialInfo/`).

## Key dependencies

From `Packages/manifest.json`:

- **UniTask** (`com.cysharp.unitask`) — prefer over coroutines for async code.
- **VContainer** (`jp.hadashikick.vcontainer`) — DI; wire services/MonoBehaviours through it, not singletons or `FindObjectOfType`.
- **Input System** (`com.unity.inputsystem`) — action map in `Assets/InputSystem_Actions.inputactions`. No legacy `Input` class.
- **URP** (`com.unity.render-pipelines.universal`) — pipeline assets in `Assets/Settings/`.
- **AI Navigation** (`com.unity.ai.navigation`) — NavMesh.
- **Unity Test Framework** (`com.unity.test-framework`) — EditMode/PlayMode tests.

## Architecture: hexagonal (ports and adapters)

Hexagons (bounded features) live under `Assets/Features/[Hexagon]/`. For each:

- **`Domain/`** — plain C# (no `UnityEngine`, no `MonoBehaviour`), the business logic. **Ports** — interfaces the domain depends on — live in `Domain/Ports/{Input,Output}/`: `Ports/Input/` is what the domain reads from the world (e.g. `IMovementInputPort`), `Ports/Output/` is what it acts through (e.g. `IPlayerBodyPort`). Domain and ports never reference Unity APIs or other hexagons' internals.
- **`Adapters/`** — Unity-specific port implementations (MonoBehaviours, ScriptableObjects, engine calls), mirroring the ports: `Adapters/Input/`, `Adapters/Output/`. DI registration lives at `Adapters/Installer/` as a static `[Hexagon]Installer.Install(IContainerBuilder builder)` — not its own `LifetimeScope` (see "Dependency injection" for why). Presenter-style adapters that don't cleanly belong to input or output live at the `Adapters/` root. Adapters depend on the domain, never the reverse.
- **Cross-hexagon communication** happens through ports/events, never by reaching into another hexagon's internals directly. Two hexagons talking to each other means defining a port for it.

Ask which hexagon a new feature belongs to (or whether it's a new one) before writing code. Keep domain logic out of MonoBehaviours — they're thin adapters that delegate to `Domain/`.

**Testing and asmdefs:**

- Domain logic must be covered by EditMode tests (cheap: plain C#, no scene/play mode needed). Add/update tests whenever `Domain/` changes, and run them (Unity Test Runner, or `unity-mcp`'s `Unity_RunCommand`) before calling a change done.
- Each hexagon gets exactly two asmdefs: `[Hexagon].Domain.asmdef` (`noEngineReferences: true` — makes "no Unity in domain" a compile error, not a convention) and `[Hexagon].Adapters.asmdef` (covers `Adapters/` incl. `Installer/`, references `Domain` plus whatever engine/package assemblies it needs). The project will grow to many hexagons; without per-hexagon `Adapters` asmdefs, every hexagon's adapter code sits in one monolithic `Assembly-CSharp`, so any adapter change recompiles all of them and blocks Play Mode. Cheaper to do this from the start than retrofit later.
- Tests get **no asmdef** — put them in a folder literally named `Editor/` (e.g. `Assets/Tests/Editor/[Hexagon]/`). Unity auto-excludes `Editor/` from builds and auto-references `NUnit`/`TestRunner` there, so `[Test]` just works. One project-wide location, not one per hexagon — a shared test compile unit only costs you on test-file edits, not gameplay iteration. Split into a few *grouped* (by system, not per-hexagon) test asmdefs only if that's ever measured to be slow.

## Dependency injection: VContainer

- Wire dependencies (ports → adapters) through VContainer, not `FindObjectOfType`, singletons, or manual `new`-ing.
- **Hexagons don't get their own `LifetimeScope`.** A scope's container is visible only to itself and its descendants — never siblings, never its parent. A separate `PlayerLifetimeScope` nested under `LevelLifetimeScope` would leave a sibling `Enemy` hexagon unable to resolve anything Player registers, and siblings need to talk to each other constantly (targeting, detection). Instead, each hexagon exposes a static `[Hexagon]Installer.Install(builder)`, and whichever tier scope matches its lifetime calls it from its own `Configure()` — registration stays colocated per-hexagon while landing everyone in one flat, mutually-visible container per tier.
- Inject through constructors (plain C#) or `[Inject]` methods/fields (MonoBehaviours) — no service locators.
- **"Services" includes stateless `Domain/` classes, not just adapters.** A domain class with no identity that just computes from its inputs (e.g. `PlayerMoverService`) is a domain service — register it (`builder.Register<T>(...)`), don't `new` it up inside an adapter. Config (tunable values) belongs on the `Installer`, passed into the registration — not hardcoded on whichever adapter needed the service first. Domain services may depend on other domain services via constructor injection, as long as the dependency stays inside the domain layer.
- Suffix domain service names with `Service` (`PlayerMoverService`, not `PlayerMover`) so the role is visible at a glance.

## Composition roots: Global / Session / Level scopes

Cross-cutting composition, separate from hexagon installers, in `Assets/Core/Scopes/` (not a hexagon — no `Domain`/`Adapters`, just wiring):

- **`GlobalLifetimeScope`** — app-wide, no parent. Holds session-spanning things (e.g. `ClientConfig`).
- **`SessionLifetimeScope`** — parent `GlobalLifetimeScope`. Scoped to a play session/run.
- **`LevelLifetimeScope`** — parent `SessionLifetimeScope`. Scoped to the current level/scene. Hexagon installers for gameplay elements (e.g. `PlayerMovementInstaller`) get called from here.

Tiers attach to their parent via `FindParent()`, not the serialized `parentReference` field (that's for Inspector-driven cases): `protected override LifetimeScope FindParent() => Find<GlobalLifetimeScope>();`. VContainer's built-in extension point — lazy and order-safe, a child forces its parent to `Build()` first if needed, so Awake order doesn't matter. This is only for the three tier scopes; hexagons attach by having a tier's `Configure()` call their `Install(builder)`, not by nesting a scope.

Not yet built: surviving scene loads (`DontDestroyOnLoad` or VContainer's `VContainerSettings` root-prefab mechanism). Only one scene exists right now — revisit when multi-scene loading arrives, not before.

## Config: ClientConfig

`Assets/Features/ClientConfig/ClientConfig.cs` — a `[CreateAssetMenu]` `ScriptableObject` holding global, designer-tunable values (e.g. `PlayerMoveSpeed`). Registered as an instance on `GlobalLifetimeScope`, so any hexagon can resolve it (child scopes see parent registrations). Pure data, no behavior yet — no `Domain`/`Adapters` split or asmdef of its own until it grows logic.

A hexagon reads a value from it at the **installer** layer (e.g. `PlayerMovementInstaller` resolving `ClientConfig.PlayerMoveSpeed` to construct `PlayerMoverService`), passing the plain value into the domain service's constructor. Domain never references `ClientConfig`/`ScriptableObject` directly.

Watch for: one global config asset turning into a dumping ground as hexagons pile fields onto it. Fine for now — if it gets unwieldy, switch to per-hexagon config assets that `ClientConfig` aggregates references to.

## Coding style

- **No underscore-prefixed private fields.** Plain name (`moveSpeed`), disambiguate a same-named parameter with `this.`, not a prefix.
- **Adapters default to `UnityEngine` math types** (`Vector2`, `Vector3`, `Quaternion`, ...) — a bare `Vector2` in adapter code means `UnityEngine.Vector2`. Only fully-qualify `System.Numerics.Vector2`/`Vector3` inline where satisfying a `Domain`-owned port signature; don't alias it as the bare name via `using Vector2 = System.Numerics.Vector2;` — that inverts which type is "natural" in an otherwise-Unity file.
- **A type named the same as its innermost namespace segment is ambiguous under `using`** (`CS0118`) at every call site. E.g. class `ClientConfig` in namespace `Features.ClientConfig`: skip the `using`, fully-qualify (`Features.ClientConfig.ClientConfig`) instead.

## Async: UniTask

- `async UniTask`/`UniTaskVoid`, not `async void` or coroutines (`IEnumerator`/`StartCoroutine`).
- Don't mix `Task`/`Task<T>` with Unity code; convert at the boundary.
- Cancellation flows through `CancellationToken`s (e.g. `this.GetCancellationTokenOnDestroy()`), not manual `bool` flags.

## Working with this repo

- Meaningful changes are `.cs` scripts, or scene/prefab/asset YAML (`.unity`, `.asset`, `.prefab`). Avoid hand-editing generated YAML unless necessary — prefer scripting changes Unity serializes itself.
- Every asset has a paired `.meta` (GUID tracking) — move/create it alongside the asset, or Unity regenerates the GUID and breaks references.
- No command-line build/test setup; building is through the Editor (Build Settings). If a `unity-mcp` connection is available, use it (`Unity_RunCommand`, `Unity_GetConsoleLogs`) to compile and test programmatically instead of asking the user to do it manually.
- `Library/`, `Temp/`, `obj/`, generated `.csproj`/`.sln` are Editor-generated — don't hand-edit.
- Bar for "done": compiles clean (no Console errors), existing EditMode/PlayMode tests pass. **Never enter Play Mode to manually verify gameplay** — leave hands-on testing to the user.
- **Renaming a `Component`-derived class to a non-`Component` (e.g. a `LifetimeScope` into a `static` installer) while reusing the script's `.meta`/GUID breaks any scene that had it attached** — Unity logs `'X' is missing the class attribute 'ExtensionOfNativeClass'!`. `GameObjectUtility.RemoveMonoBehavioursWithMissingScript` won't catch it (the GUID still resolves to a real type, just not a valid `Component`), and `SerializedObject` can't touch `GameObject.m_Component` directly (`"It is not allowed to modify the data property"`). Simplest fix: recreate the affected GameObject.
- Commit in small, focused chunks.
- Prefix commit messages with the thing worked on in square brackets — the hexagon/feature/system, not a change type. `[PlayerMovement] add domain movement logic`, `[CLAUDE.md] document asmdef conventions` — not `[feat] ...`.

## Unity MCP tooling notes

Gotchas hit using `unity-mcp` in this environment — worth knowing before running `Unity_RunCommand`.

- Use `unity-mcp` (`Unity_RunCommand`, `Unity_GetConsoleLogs`, `Unity_Camera_Capture`), not `coplay-mcp` — the latter's bridge wasn't reachable here even with the Editor open.
- `Unity_RunCommand`'s sandbox rejects `System.Reflection` — use `GameObject.SendMessage` or restructure instead.
- Fully-qualify `UnityEngine.UI.Image` in sandbox scripts — a bare `using UnityEngine.UI;` + `Image` collides with an unrelated injected `Image` namespace (`CS0118`).
- `OnScreenStick`/`OnScreenControl` serialized field names differ from their public properties: `m_ControlPath`, `m_MovementRange`, not `controlPath`/`movementRange`.
- `TestRunnerApi` from `Unity_RunCommand` hits an interactive dialog MCP can't answer. Exercise `[Test]` methods directly instead (`new TestClass().TestMethod()` in a try/catch, tally pass/fail) — same assertions, no UI dependency.
- Newly-written files can get rewritten out from under you (namespace/body stripped to a bare template between write and next tool call — likely an IDE/analyzer auto-action). Re-read a new file after the first `AssetDatabase.Refresh()` before assuming your content is what's on disk.
- When an asmdef graph's topology changes, `Unity_RunCommand` can block for a long time with `COMPILATION_IN_PROGRESS`. Not a real hang: the MCP bridge's readiness check polls `EditorApplication.update`, which only ticks when the Editor window gets OS-level attention — while unfocused, its 120s timeout expires before it observes compilation finishing. Fix: ask the user to click into the Editor window, then retry.
