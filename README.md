# Bandhana — Eight Petals of the Mandala

A creature-collector RPG rooted in tantric tradition. Built in Unity 6.

> *"The bond is not what holds you. It is what you choose freely."*

## Status
- Phase: **M0 — Setup**
- Engine: Unity 6000.3.14f1 (will migrate to 6000.0 LTS when stable)
- Platform: Windows + Linux (primary), macOS (best-effort)

## Project layout
- `Assets/_Project/` — all game code, art, audio, data, scenes
- `Assets/_Project/Scripts/` — C# (organized by system: Core, Overworld, Battle, BondRite, Data, UI)
- `Assets/_Project/Data/` — ScriptableObject `.asset` instances (one per spirit, move, type, item)

## Plan
Full design + script lives in `~/.claude/plans/i-want-to-create-fuzzy-blum.md`.
Backlog (out-of-slice ideas) lives in `BACKLOG.md`.

## Conventions
- ScriptableObjects for all game data (spirits, moves, types, items, dialogue)
- One namespace per folder: `Bandhana.Core`, `Bandhana.Battle`, `Bandhana.BondRite`, `Bandhana.Data`, etc.
- LF line endings, UTF-8, 4-space indent.

## Vertical slice goal
Acts 0–opening of 2: village funeral → *The Helping* (acquire Damaru) → foothills → road to Vajrasana → gate-disciple challenge fight. ~30–40 min play.
