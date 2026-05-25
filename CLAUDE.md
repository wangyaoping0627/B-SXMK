# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

B-SXMK is a **coal mine museum exhibition kiosk** (白水西煤矿) built with Unity 2022.3.62f3. It's a single-scene UI application — an interactive touchscreen display showing historical images and background music about the mine's development history and old artifacts.

## Architecture

**Navigation**: Panel-based via `SetActive(true/false)` on GameObject hierarchies. 4 levels deep (一级目录 → 四级目录). No centralized navigation manager — each button script self-wires its `onClick` in `Start()` and references target panels via serialized fields assigned in the Inspector.

**BGM**: `BgmController` on the "静态画布" Canvas plays `tuncheng.mp3` on all pages except the home page. Every navigation script calls `FindObjectOfType<BgmController>()?.CheckBgm()` after navigating — this pattern means there must be exactly one BgmController in the scene.

**Content**: All images are static sprite references in the scene (from `Assets/Sources/`). No dynamic loading, no Addressables, no AssetBundles. No data models — everything is baked into the scene YAML.

## Key Files

| File | Purpose |
|------|---------|
| [Assets/Scripts/BackButton.cs](Assets/Scripts/BackButton.cs) | Back navigation: pops NavHistory stack first, falls back to Inspector-configured panels |
| [Assets/Scripts/BgmController.cs](Assets/Scripts/BgmController.cs) | Plays BGM when any second-level directory panel is active; polled by all nav scripts |
| [Assets/Scripts/NavigateToPanel.cs](Assets/Scripts/NavigateToPanel.cs) | Forward navigation: pushes to NavHistory stack, then shows/hides panels |
| [Assets/Scripts/NavHistory.cs](Assets/Scripts/NavHistory.cs) | Static stack-based navigation history: Push on forward, Pop reverses show/hide arrays |
| [Assets/Scenes/SampleScene.unity](Assets/Scenes/SampleScene.unity) | The only scene — contains all UI and game logic |
| [Assets/Sources/](Assets/Sources/) | Raw PNG images + `tuncheng.mp3` BGM track |

## Development Notes

- **No prefabs** are used — all UI is built directly in the scene. When adding new panels, expect to configure references via Inspector drag-and-drop.
- **No namespace** usage in scripts. All 4 scripts are flat in `Assets/Scripts/` with no assembly definition files.
- **No tests** exist for this project.
- The project is **not a git repository** — consider initializing one before making changes.
- `FindObjectOfType` is used for BGM access — be aware this scans the entire scene on each navigation. If performance becomes an issue, consider a static reference or singleton pattern.
