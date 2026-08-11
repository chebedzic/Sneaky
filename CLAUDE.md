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

- **Domain/core** — plain C# (no `UnityEngine`, no `MonoBehaviour`) holding the business logic and **ports**: interfaces the domain depends on (e.g. `IInputPort`, `IPersistencePort`, `IAudioPort`). The domain must not reference Unity APIs or other hexagons' internals directly.
- **Adapters** — Unity-specific implementations of the ports (MonoBehaviours, ScriptableObjects, engine calls), living in the same hexagon folder (e.g. an `Adapters/` subfolder). Adapters depend on the domain, never the other way around.
- **Cross-hexagon communication** happens through ports/interfaces (or events), not by one hexagon reaching directly into another's internals. If two hexagons need to talk, define a port for it rather than adding a direct reference.

When adding a new feature, ask which hexagon it belongs to (or whether it's a new one) before writing code, and keep domain logic out of MonoBehaviours — MonoBehaviours should be thin adapters that delegate to the domain/core.

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
- There is no command-line build/test setup in this repo; building, running, and testing is done through the Unity Editor (Unity Test Runner for tests, Build Settings for builds).
- `Library/`, `Temp/`, `obj/`, and the generated `.csproj`/`.sln` files are Unity Editor-generated and should not be hand-edited.
- After making changes, check that the project compiles (no errors in the Unity Console) and run any existing EditMode/PlayMode tests via the Unity Test Runner before considering the change done.
- Commit in small, focused chunks rather than batching unrelated changes into one commit.
