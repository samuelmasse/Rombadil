# Repository Instructions

## Scope

These instructions apply to this game repository. Keep this file copy-pasteable
across AlvorKit game repos. It should name only one sibling repository:
`../AlvorKit`.

Read this repo's `README`, solution, project files, scripts, and nearby source
before assuming local layout. Do not assume other sibling game repos exist.

## AlvorKit Relationship

This game consumes AlvorKit from `../AlvorKit` through relative project
references. AlvorKit owns the engine, UI, injection, windowing, GL lifetime,
maths, generated bindings, script tools, demos, documentation, AlvorSense, and
AlvorEye.

Do not treat AlvorKit as a fixed external dependency. We own that code too. If
the clean solution needs an engine API, UI primitive, script tool, harness
capability, math helper, resource abstraction, or docs update, make the right
change in `../AlvorKit` instead of forcing a game-local workaround. Keep truly
game-specific behavior in this repo.

Engine/tooling changes must follow `../AlvorKit/AGENTS.md` and any closer
scoped AlvorKit `AGENTS.md`. Keep game-repo and AlvorKit status, staging, and
commits separate.

## Working Rules

Read `../AlvorKit/AGENTS.md` before non-trivial work. Its Working Mode,
Commit Mode, C# defaults, allocation discipline, visual automation,
generated-output, coordination, and verification rules apply here unless this
file says otherwise.

Game-specific overrides:

- Game C# source files may be up to 750 lines when a cohesive game system,
  state, menu, renderer, or simulation reads better together.
- Test files may also be up to 750 lines when related scenarios read better
  together.
- Do not apply AlvorKit's coverage targets as mandatory game-repo gates. Add
  focused tests when useful; run coverage only when asked or required by this
  repo's own CI policy.
- Keep hot paths allocation-sensitive: update, render, input polling,
  emulation/simulation, resource lifetime, validation, bind/unbind, and
  teardown.
- This is game code. Follow the runtime allocation discipline in
  `../AlvorKit/AGENTS.md` and `../AlvorKit/src/AGENTS.md`: avoid managed
  allocations and GC pressure in per-frame, per-tick, polling, render,
  simulation, resource, and teardown paths unless the cost is intentional and
  accepted. Watch for arrays, `List<T>`, LINQ, closures, iterator blocks,
  boxing, `params`, string formatting, async state machines, and defensive
  copies.
- Use AlvorKit shapes directly: scopes, controls, vectors, maths types,
  `GlLayer`, UI menus, and engine lifecycle APIs.

## Game Ents And ECS

Game Ents must use AlvorKit ECS. Use `Ent` in every context. The word `Entity`
is banned; use `Ents` for the plural. This applies to prose, code identifiers,
type and member names, parameters and locals, filenames, directories, labels,
and compound names.

Model players, enemies, projectiles, items, chunks, and other mutable simulated
objects with generated `[Components]`, ECS handles and arenas, and
`AlvorKit.ECS.Indexed` when their component writes maintain bags, hooks, or
indexes. Do not introduce a parallel game Ent hierarchy, bespoke component
store, or alternate ECS.

Keep behavior in injected services and systems and keep Ent state in
components. Services, commands, configuration, assets, protocol records, and
ordinary value objects are not game Ents and should remain normal C# types.

Before creating or significantly changing game Ents, component declarations,
Ent handles or arenas, Indexed contexts, hooks, bags, indexes, or Ent lifetime,
read `../AlvorKit/docs/ECS.md`. Follow its ownership, registration, mutation,
iteration, and teardown contracts.

## Code Design Style

These are prescriptive defaults, not merely instructions to copy nearby code.
Apply them in new projects and packages even when no local precedent exists.

### Accessibility

- Prefer `public` over `internal` for game-code types and collaborating members.
  Game projects are not curated library API surfaces; assembly boundaries should
  not hide ordinary game systems, state, commands, or helpers.
- Keep details `private` when they are owned by one type. Use `internal` only
  when a deliberately small, curated assembly API is a real design requirement.

### Services And Composition

- Put runtime behavior in injected instance classes. A service should remain an
  instance even when it currently has no fields.
- Do not make a class or method static merely because it is stateless or because
  an analyzer recommends it.
- Use constructor injection. Do not introduce service locators, ambient
  containers, or hidden global dependencies.
- Keep composition in scopes, loaders, and entry points. Each loader should
  initialize its own layer instead of absorbing or replacing another loader's
  responsibilities.
- Keep domain services focused. Do not give them unrelated loading,
  persistence, rendering, protocol, or presentation responsibilities.
- Prefer a valid neutral initial state during normal binding over nullable
  placeholders and defensive access paths.

### Static Members And Constants

- Do not create static service classes, static mutable state, or broad
  collections of static helpers.
- Reserve static members for operators, extension methods, framework-required
  entry points, compile-time values, and pure value operations that are
  unambiguously owned by the type.
- Use a named constant for a repeated representation invariant or a value whose
  meaning matters. Put it on the type that owns the meaning.
- Do not promote one-off literals or runtime policy to global constants. Use
  injected configuration or instance state for values that can vary by runtime
  or composition.
- Disable analyzer rules that recommend making instance members static when
  they conflict with these conventions.

### Failure Semantics

- Internal code assumes its contracts are satisfied. Do not add redundant range
  checks, custom guard exceptions, debug assertions, or fallback behavior for
  states that should never occur.
- Let invalid internal or authoritative data fail naturally at the operation
  that cannot handle it.
- Validate external input only when validation is part of a real security,
  compatibility, or recoverable protocol boundary.
- Do not catch exceptions unless the code can perform a meaningful recovery.
  Do not catch merely to log, return a default, continue partially, or replace
  the exception.
- Use `try`/`finally` only when cleanup must still happen after an exception.
  Prefer ownership whose normal lifetime makes cleanup explicit.

### Ownership And Lifecycle

- Every mutable resource should have an obvious owner and one understandable
  allocation, replacement, clearing, and teardown path.
- Express lifecycle with domain operations such as `Load`, `Clear`, `Fill`,
  `Stop`, or `Unload` when those names describe the real operation.
- Use `IDisposable` only for types that participate in a genuine disposal
  contract, not as a generic marker for resetting state or returning storage.
- Similar resources should follow similar lifecycle patterns.
- Do not add wrapper properties or methods that merely expose information
  already publicly available.

### Concurrency

- Design concurrency from actual ownership and access paths. Prefer
  thread-owned, worker-owned, or scope-owned state over shared mutable state.
- Reuse per-worker buffers when their lifetime naturally matches a worker.
- Add locks only around state that is genuinely accessed concurrently and
  requires synchronization.
- Do not add volatile publication, snapshots, defensive copies, or additional
  locks for hypothetical races unsupported by the lifecycle.
- Do not assign unusual thread priorities without a measured scheduling
  requirement.

### Design Restraint

- Implement the smallest coherent design that satisfies the current system. Do
  not add speculative extensibility, defensive infrastructure, or future
  abstraction layers.
- Do not create a feature-specific protocol, synchronization channel, or side
  system when the concern belongs to a general system that has not been built
  yet.
- Keep package roles strict. Pure simulation, backend persistence, frontend
  presentation, protocol, and executable composition remain separate.
- Put derived presentation values in frontend packages instead of pure
  simulation packages.
- Give each class one clear responsibility. Move initialization, persistence,
  and presentation derivations to their respective owners instead of
  accumulating convenience methods on a domain object.
- Preserve unrelated behavior. A subsystem change should not also alter menus,
  loading presentation, scheduling policy, or other user-visible behavior.
- Prefer direct, readable code over infrastructure justified only by
  theoretical robustness.

## Documentation Router

Open the matching guide under `../AlvorKit/docs/` instead of re-inventing local
rules:

- `AlvorSense.md`: hidden, engine-native visual harness for AlvorKit games.
- `AlvorEye.md`: OS-level visual automation for real desktop windows.
- `ECS.md`: required game Ent components, handles, arenas, Indexed hooks and
  bags, iteration, ownership, and teardown.
- `ProjectSplitModel.md`: pure, frontend, menu, backend, server, protocol, and executable package split.
- `GameScopeOrganization.md`: DI scopes, scope prefixes, loader scopes, states,
  controls, seeding, and constructor ordering.
- `GlOwnership.md`: hierarchical `GlLayer` ownership and GPU object lifetime.
- `MenuAuthoring.md`: `AlvorKit.UI` menus and the one-public-`Create` shape.
- `AgentVerification.md`: lint, timing, coverage, artifacts, and report reading.
- `AgentCoordination.md`: leases, conflicts, complaints, staging discipline.
- `GeneratedOutputChecks.md`: generator and generated-output review workflow.

Use `../AlvorKit/demos/` as runnable examples for engine APIs.
Other docs and design references exist under `../AlvorKit/docs/`; open them
only when the task touches that area.

Before adding a `ProjectReference`, make sure it preserves the package's role.
It is vitally important that a project does not take on a dirty dependency in
the `.csproj` that defeats the purpose of the split. Pure packages must not
reference UI, GL, frontend, menu, audio, or windowing packages. Frontend
packages may depend on `AlvorKit.Engine`, but should not depend on
`AlvorKit.Engine.Loop`; loop ownership belongs in the executable, menus, or
another composition package.

## Visual Checks

Prefer AlvorSense when the game uses `RootLoop.RunGlfw`,
`AgentGlfwWindowHost`, or supports `ALVORKIT_WINDOWING_AGENT=1`. Read
`../AlvorKit/docs/AlvorSense.md`, run the CLI from this repo root, and pass
`--workdir .` so artifacts land under this repo's ignored `out/`.

```powershell
dotnet run --project ..\AlvorKit\scripts\AlvorKit.Script.AlvorSense -- start --id <game-id> --project <runnable-project-or-script> --workdir .
dotnet run --project ..\AlvorKit\scripts\AlvorKit.Script.AlvorSense -- send --id <game-id> --command "render" --command "screenshot out\shots\<name>.png"
dotnet run --project ..\AlvorKit\scripts\AlvorKit.Script.AlvorSense -- stop --id <game-id>
```

Use AlvorEye only for real desktop-window or OS-level input/focus behavior.

## Verification

Use this repo's solution, README, scripts, and CI workflow for normal build/test
commands. AlvorKit tools usually accept `--repo-root .`; read
`AgentVerification.md` first.

In Working Mode, prefer targeted builds, targeted tests, or an AlvorSense smoke
only when they directly support the requested change. Lint, coverage, broad
timing gates, generated-output checks, staging, and commits are explicit-request
or Commit Mode work.
