# Rombadil

Rombadil is a cycle accurate NES emulator implemented in C# using OpenTK (OpenGL, OpenAL, GLFW).

<table>
  <tr>
    <td>
      <img src="res/Icon.png" width="80" alt="Rombadil Icon">
    </td>
    <td style="padding-left: 12px;">
      <strong>“It's aight”</strong><br>
      <em>– Tom</em>
    </td>
  </tr>
</table>

## Utils

- `rombadilasm` is a 6502 assembler
- `rombadil6502` is a standalone 6502 CPU emulator

## Developer Scripts

Use AlvorSense for visual capture:

```powershell
dotnet run --project ..\AlvorKit\scripts\AlvorKit.Script.AlvorSense -- start --id rombadil --project scripts\Rombadil.Script.Dev --workdir .
dotnet run --project ..\AlvorKit\scripts\AlvorKit.Script.AlvorSense -- send --id rombadil --command "render" --command "screenshot out\shots\rombadil.png"
dotnet run --project ..\AlvorKit\scripts\AlvorKit.Script.AlvorSense -- stop --id rombadil
```
