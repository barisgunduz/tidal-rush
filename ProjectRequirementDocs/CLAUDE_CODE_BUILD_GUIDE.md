# Claude Code Build Guide: Tidal Rush

This document is a sequential, stage by stage guide for building the game with Unity and Claude Code. Each stage has a short goal, then a ready to use prompt you can paste into Claude Code once you reach that point. Do not skip stages, each one assumes the previous stage is working first.

Read PRD.md and BRAND.md before starting. Claude Code should be pointed at both files for context at the start of the project.

---

## Stage 0: Environment Setup

Goal: get Unity, an editor, and Claude Code all talking to each other before writing any game code.

Manual steps (not Claude Code prompts):
1. Install Unity Hub
2. Through Unity Hub, install the latest Unity LTS version (Unity 6000.x line)
3. Install Visual Studio Code if not already installed
4. In Unity Editor preferences, set VS Code as the External Script Editor
5. Install the Unity MCP package/bridge so Claude Code can talk to the running Unity Editor
6. Open the Unity project folder in VS Code, run `claude` in the integrated terminal, and accept the pending connection request inside Unity's MCP settings window

Prompt to use once Claude Code is connected and Unity Editor is open:

```
Confirm you are connected to my running Unity Editor through the Unity MCP bridge. List the current Unity version, render pipeline, and target platform set in Player Settings. Do not make any changes yet, just confirm the connection and report back what you see.
```

---

## Stage 1: Project Scaffolding

Goal: a clean 2D mobile project with the right settings and folder structure, no gameplay yet.

Prompt:

```
I am building a mobile memory matching game called Tidal Rush. Read PRD.md and BRAND.md in the project root for full context before doing anything.

Set up this Unity project for a 2D mobile game:
1. Confirm or set the 2D render pipeline
2. Set build target to iOS, with Android as a secondary target we may enable later
3. Create this folder structure under Assets: Scripts, Prefabs, Sprites, ScriptableObjects, Scenes, UI
4. Create a single scene called MainGame and set it as the active scene
5. Set the target aspect ratio to a standard portrait mobile ratio (9:16) in the Game view

Report the final folder structure and settings once done.
```

---

## Stage 2: Core Card Matching Mechanic

Goal: one hardcoded level, fixed grid, fixed time, playable end to end with no polish.

Prompt:

```
Read BRAND.md for the color palette and symbol set before writing any UI.

Build the core card matching mechanic as a first playable slice:
1. A Card prefab with a front face (hidden symbol) and back face (card back), using the color values from BRAND.md
2. A GameManager script that spawns a 4x4 grid (16 cards, 8 symbol pairs) using the nautical symbol set from BRAND.md
3. Tap/click input that flips a card, waits for a second card, checks for a match, and either keeps both revealed (match) or flips both back after a short delay (mismatch)
4. Track and display move count and matches found, no styling needed yet, plain text is fine
5. No timer yet, this stage is only about the flip and match logic being correct

Test this by pressing Play and confirm the full flip, match, mismatch cycle works before moving on.
```

---

## Stage 3: Level Data System

Goal: move level definitions (time limit, grid size) out of code and into data, so adding new levels is fast.

Prompt:

```
Read the level progression table in PRD.md.

Refactor the level setup into a data driven system:
1. Create a LevelData ScriptableObject with fields for level number, time limit in seconds, and grid dimensions (rows and columns)
2. Create one ScriptableObject asset per level for levels 1 through 10, using the exact time values from the PRD progression table
3. Update GameManager to load the correct LevelData asset for the current level instead of using hardcoded values
4. Confirm the grid still builds correctly at 4x4 for all of levels 1 through 10, only the time limit should change between them at this stage

Report back the list of LevelData assets created and their values.
```

---

## Stage 4: Timer and Win/Fail States

Goal: the actual core loop, timed pressure, win screen, fail screen.

Prompt:

```
Read BRAND.md for the timer urgency color rule (orange in the last 5 seconds) and the motion guidance for timer pulsing.

Add the timed win/fail loop:
1. A countdown timer UI tied to the current LevelData time limit, counting down from level start
2. When time reaches 5 seconds, timer text switches to the orange accent color and gently pulses, following the motion rules in BRAND.md
3. If all pairs are matched before time runs out, show a Level Complete screen with the time remaining and a Continue button that loads the next level
4. If time reaches zero before all pairs are matched, show a Level Failed screen with a Retry button that restarts the same level
5. Wire Retry and Continue buttons to correctly reload LevelData for the right level

Test levels 1 through 3 fully, including at least one intentional fail to confirm the fail state works.
```

---

## Stage 5: Progression and Save System

Goal: persist player progress locally so closing and reopening the app remembers where they left off.

Prompt:

```
Add a local save system using PlayerPrefs:
1. Save the highest level unlocked so far
2. Save the best completion time per level
3. On app launch, load saved progress and start the player at their furthest unlocked level rather than always level 1
4. Add a simple main menu scene with a Play button that goes to the correct furthest level, and a Level Select area if it is not too much extra work at this stage, otherwise skip level select for now and note it as a later addition

Confirm progress correctly persists across a Play mode stop and restart in the Unity Editor.
```

---

## Stage 6: Difficulty Scaling Beyond Level 10

Goal: implement the level 11+ scaling rule from the PRD, since time is capped at 15 seconds from level 10 onward.

Prompt:

```
Read the level progression table and difficulty design notes in PRD.md.

Implement the post level 10 difficulty scaling:
1. From level 11 onward, time limit stays fixed at 15 seconds
2. Grid size increases: level 11 to 4x5, level 14 to 4x6, or a similar reasonable step you can propose based on card count math, explain your reasoning before implementing
3. Add at least one additional difficulty lever beyond grid size, for example visually similar symbol pairs or rotated symbol variants, and explain the approach you chose before building it

Create LevelData assets for levels 11 through 15 as a first batch under this new scaling rule.
```

---

## Stage 7: Polish Pass

Goal: make the game feel good, not just function.

Prompt:

```
Read the motion and feel section of BRAND.md closely before this stage.

Apply a polish pass:
1. Card flip animation under half a second, no bounce or elastic easing
2. Match confirmation: brief teal glow or scale pulse under 200ms
3. Add empty audio hook methods (PlayFlipSound, PlayMatchSound, PlayMismatchSound, PlayLevelCompleteSound) even if no audio files exist yet, so sound can be dropped in later without touching gameplay code
4. Review all UI text against the writing style rules in BRAND.md, confirm no em dashes exist anywhere in code comments or UI strings, replace any found with a hyphen or colon as appropriate

List any BRAND.md rules you found violated in the current code and confirm you fixed them.
```

---

## Stage 8: iOS Build Pipeline

Goal: get an actual build path working, since Windows alone cannot produce a final iOS build.

Prompt:

```
Help me prepare this project for an iOS build through Unity Cloud Build, since I am developing on Windows and do not have direct Mac access.

1. Confirm Player Settings are correctly configured for iOS (bundle identifier, minimum iOS version, orientation locked to portrait)
2. Walk me through what needs to be set up in Unity Cloud Build for this project step by step
3. Flag any settings or assets that commonly cause iOS build failures so we can check them before the first build attempt
```

---

## Stage 9: Testing and QA Pass

Goal: catch obvious issues before considering this a finished learning milestone.

Prompt:

```
Do a QA pass on the full level 1 through 15 experience:
1. Check for any level where the timer math or grid size does not match PRD.md exactly
2. Check that Retry and Continue always load the correct level with no off by one errors
3. Check that saved progress correctly reflects the furthest completed level after multiple play sessions
4. Report anything that looks off, do not fix silently, list issues first so I can confirm the fix approach
```

---

## Stage 10: Store Preparation Notes

Goal: understand what is needed to actually publish, even if this specific build is a learning project.

Prompt:

```
Based on BRAND.md and PRD.md, draft App Store listing text for Tidal Rush: a short app description, a set of keywords, and a one line subtitle. Follow the writing style rules in BRAND.md, no em dashes anywhere. Keep the tone consistent with the positioning section of BRAND.md.
```

---

## Notes for Working With Claude Code Throughout

- Keep PRD.md and BRAND.md in the project root the entire time, referencing them by file name in prompts keeps Claude Code grounded in the actual spec instead of improvising
- Confirm each stage works in Play mode before moving to the next one, do not stack unverified stages
- If Claude Code proposes a design choice not covered in PRD.md or BRAND.md (like the exact grid scaling step in Stage 6), ask it to explain the reasoning before accepting it
