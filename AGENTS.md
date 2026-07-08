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

## Documentation Router

Open the matching guide under `../AlvorKit/docs/` instead of re-inventing local
rules:

- `AlvorSense.md`: hidden, engine-native visual harness for AlvorKit games.
- `AlvorEye.md`: OS-level visual automation for real desktop windows.
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
