# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

This is a Unity project (Editor version 6000.3.20f1 / Unity 6) using the Universal Render Pipeline (URP). It is currently a fresh project scaffold — no gameplay scripts exist yet beyond Unity's default template assets (`Assets/TutorialInfo/`).

## Key dependencies

Declared in `Packages/manifest.json`:

- **UniTask** (`com.cysharp.unitask`) — async/await for Unity, prefer over coroutines for new async code.
- **VContainer** (`jp.hadashikick.vcontainer`) — dependency injection framework; use for wiring up services/MonoBehaviours rather than singletons or `FindObjectOfType`.
- **Input System** (`com.unity.inputsystem`) — new Input System is in use; action map defined in `Assets/InputSystem_Actions.inputactions`. Do not use the legacy `Input` class for new input code.
- **URP** (`com.unity.render-pipelines.universal`) — rendering pipeline; render pipeline assets live in `Assets/Settings/`.
- **AI Navigation** (`com.unity.ai.navigation`) — NavMesh tooling.
- **Unity Test Framework** (`com.unity.test-framework`) — for any EditMode/PlayMode tests.

## Architecture: hexagonal (ports and adapters)

This project is structured as a set of hexagons (bounded features/domains), each isolated under its own folder:

```
Assets/Features/[Hexagon]/
```

For each hexagon:

- **Domain/core** — plain C# (no `UnityEngine`, no `MonoBehaviour`) holding the business logic, in a `Domain/` subfolder. **Ports** — interfaces the domain depends on — live in their own `Domain/Ports/` subfolder, split further into `Ports/Input/` (what the domain reads from the world, e.g. `IMovementInputPort`) and `Ports/Output/` (what the domain acts through, e.g. `IPlayerBodyPort`). The domain (including its ports) must not reference Unity APIs or other hexagons' internals directly.
- **Adapters** — Unity-specific implementations of the ports (MonoBehaviours, ScriptableObjects, engine calls), living in the same hexagon's `Adapters/` subfolder, split to mirror the ports: `Adapters/Input/` implements `Ports/Input/`, `Adapters/Output/` implements `Ports/Output/`. The VContainer `LifetimeScope` for the hexagon lives at `Adapters/Installer/` (it's a Unity-specific composition adapter, not a separate top-level concept). Orchestrating/presenter-style adapters that don't cleanly belong to either input or output (e.g. a per-frame presenter driving both) live at the `Adapters/` root. Adapters depend on the domain, never the other way around.
- **Cross-hexagon communication** happens through ports/interfaces (or events), not by one hexagon reaching directly into another's internals. If two hexagons need to talk, define a port for it rather than adding a direct reference.

When adding a new feature, ask which hexagon it belongs to (or whether it's a new one) before writing code, and keep domain logic out of MonoBehaviours — MonoBehaviours should be thin adapters that delegate to the domain/core.

- **Domain/core logic must be covered by tests.** Since it's plain C# with no Unity dependencies, it's cheap to unit test with EditMode tests — no scene, no play mode required. Whenever you add or change logic in a hexagon's `Domain/` folder, add or update the corresponding tests, and run them (via the Unity Test Runner, or `unity-mcp`'s `Unity_RunCommand` when available) before considering the change done.
- **Give each hexagon exactly two asmdefs: `Domain` and `Adapters`.** `[Hexagon]/Domain/[Hexagon].Domain.asmdef` has `noEngineReferences: true` — that's what makes "no UnityEngine in domain code" a compile error instead of a convention someone forgets. `[Hexagon]/Adapters/[Hexagon].Adapters.asmdef` covers `Adapters/` (including `Installer/`), references the `Domain` asmdef plus whatever engine/package assemblies it needs (e.g. `VContainer`, `Unity.InputSystem`). This project is expected to grow to many (eventually hundreds of) hexagons, so without per-hexagon `Adapters` asmdefs, all adapter code across every hexagon would sit in one monolithic default `Assembly-CSharp` — meaning a change to any one hexagon's adapter forces a recompile of literally all of them, and blocks entering Play Mode while it happens. That's worth avoiding from the start; retrofitting it after hundreds of hexagons exist is much more painful than doing it consistently now.
- **Don't give tests an asmdef at all — put them in a folder literally named `Editor/`.** Unity auto-excludes anything under an `Editor/` folder from player builds and auto-references `NUnit`/`UnityEngine.TestRunner`/`UnityEditor.TestRunner` for it, so `[Test]` methods just work with zero asmdef. Use one project-wide location (e.g. `Assets/Tests/Editor/[Hexagon]/`) rather than one per hexagon — a monolithic test compile unit only costs you when editing test files (it doesn't block Play Mode or gameplay iteration), so it's fine to leave ungrouped until it's actually measured to be slow. If that day comes, split into a handful of *grouped* test asmdefs (by system, not by individual hexagon) rather than mirroring every hexagon 1:1.

## Dependency injection: VContainer

- Wire up dependencies (ports → adapters) via VContainer `LifetimeScope`s, not `FindObjectOfType`, static singletons, or manual `new`-ing of services.
- Each hexagon should register its own ports/adapters in its own scope (or a scoped installer), keeping registration colocated with the hexagon rather than centralized in one giant root installer.
- Inject dependencies through constructors (for plain C# classes) or `[Inject]` methods/fields (for MonoBehaviours) rather than reaching for service locators.

## Async: UniTask

- Use UniTask (and UniTask extensions, e.g. for Addressables, Input System, etc.) for all async code — `async UniTask`/`UniTaskVoid` instead of `async void` or coroutines (`IEnumerator`/`StartCoroutine`).
- Avoid mixing `Task`/`Task<T>` from `System.Threading.Tasks` with Unity code; convert at the boundary if interacting with a `Task`-based API.
- Cancellation should flow through `CancellationToken`s (e.g. tied to a `MonoBehaviour`'s lifetime via `this.GetCancellationTokenOnDestroy()`), not manual `bool` flags.

## Working with this repo

- This is a Unity project: most meaningful changes happen by editing `.cs` scripts under `Assets/`, or scene/prefab/asset files (YAML-based `.unity`, `.asset`, `.prefab`). Scene and asset files are large generated YAML — avoid hand-editing them unless necessary, and prefer scripting changes that Unity will serialize itself.
- Every asset has a paired `.meta` file (GUID tracking) — when adding or moving asset files, the corresponding `.meta` file must move/be created with it, or Unity will regenerate a new GUID and break references.
- There is no command-line build/test setup in this repo; building is done through the Unity Editor (Build Settings). If a `unity-mcp` connection to a running Editor is available, use it (`Unity_RunCommand`, `Unity_GetConsoleLogs`) to compile and run tests programmatically instead of asking the user to do it manually.
- `Library/`, `Temp/`, `obj/`, and the generated `.csproj`/`.sln` files are Unity Editor-generated and should not be hand-edited.
- After making changes, check that the project compiles (no errors in the Unity Console) and run any existing EditMode/PlayMode tests via the Unity Test Runner. **Do not enter Play Mode to manually verify gameplay/behavior** — compiling cleanly and passing existing tests is the bar for "done"; leave hands-on testing to the user.
- Commit in small, focused chunks rather than batching unrelated changes into one commit.
- Prefix commit messages with the name of the thing worked on in square brackets — the hexagon/feature/system, not a generic change type. E.g. `[PlayerMovement] add domain movement logic`, `[CLAUDE.md] document asmdef conventions`, not `[feat] add player movement domain`.

## Lessons learned (session notes — read before using Unity MCP tooling)

- **Use `unity-mcp` tools (`Unity_RunCommand`, `Unity_GetConsoleLogs`, `Unity_Camera_Capture`, etc.), not `coplay-mcp`.** In this environment the `coplay-mcp` bridge was not reachable ("Unity Editor is not running at the specified project root") even though a Unity Editor was actually open; `unity-mcp`'s `Unity_RunCommand` (compiles and runs arbitrary C# in-editor) is the tool that actually works here.
- **Never enter Play Mode to test a change** (see rule above). If you already did before this was written: don't — it doesn't prove anything useful in this setup anyway, see next point.
- **The automated Unity Editor doesn't tick frames while unfocused.** Entering Play Mode via script and checking behavior "over time" across multiple tool calls will show `Time.frameCount` barely moving — that's an editor-focus/throttling artifact, not a code bug. Not worth chasing, and moot now that play-testing is out of scope.
- **`Unity_RunCommand` script sandbox rejects `System.Reflection`.** Don't import it; use `GameObject.SendMessage` or restructure the check instead.
- **`Unity_RunCommand` scripts: fully-qualify `UnityEngine.UI.Image`.** A bare `using UnityEngine.UI;` plus a bare `Image` reference collides with an unrelated `Image` namespace injected into the sandboxed script's compilation context (`CS0118`). Write `UnityEngine.UI.Image` explicitly.
- **`OnScreenStick`/`OnScreenControl` serialized field names differ from their public property names.** When setting them via `SerializedObject` in an editor script, use `m_ControlPath` and `m_MovementRange`, not `controlPath`/`movementRange`.
- **Running tests via `TestRunnerApi` (`UnityEditor.TestTools.TestRunner.Api`) from `Unity_RunCommand` hits an interactive dialog MCP can't answer** ("User interactions are not supported for MCP tool calls"). Until that's solved, exercise the `[Test]` methods directly (`new TestClass().TestMethod()` in a try/catch, tally pass/fail) as a stand-in for a real Test Runner pass — same assertions, same domain code, no UI dependency.
- **Newly-written files can get rewritten out from under you.** A freshly created `LifetimeScope` script had its namespace/using directives/body stripped down to a bare template in between being written and the next tool call (likely an IDE/analyzer auto-action in this environment). After the first `AssetDatabase.Refresh()` following a new file creation, re-read the file before assuming your original content is what's on disk.
