# Product Requirements Document

## Game Title (working name)
Tidal Rush

## One Line Pitch
A fast paced memory matching game where every level gives you less time than the last, until speed itself becomes the real puzzle.

## Platform
Mobile, iOS primary target. Built in Unity so Android export stays possible later without a rebuild.

## Goal of This Project
This is a learning project first and a shippable product second. The purpose is to understand the full Unity workflow: project setup, scripting, level data, build pipeline, and store submission, using Claude Code as the development partner at every stage. The output should still be a real, polished, playable game, not a throwaway exercise.

## Target Audience
Casual mobile players who enjoy short session games (commute, waiting room, coffee break). Players who like games with a clear sense of increasing challenge and a personal best to chase, similar to endless runner or reflex game audiences rather than deep strategy players.

## Core Loop
1. Player sees a grid of face down cards.
2. Player taps two cards per turn to reveal them.
3. Matching pairs stay revealed, non matching pairs flip back after a short delay.
4. A countdown timer runs for the whole level. If all pairs are matched before time runs out, the level is won.
5. If time runs out before all pairs are matched, the level is failed and can be retried.
6. Winning unlocks the next level, which gives less time (see progression table).

## Level Progression Table

| Level | Time Limit | Grid Size | Notes |
|-------|-----------|-----------|-------|
| 1 | 60s | 4x4 | Tutorial pacing, generous time |
| 2 | 50s | 4x4 | |
| 3 | 45s | 4x4 | |
| 4 | 38s | 4x4 | |
| 5 | 32s | 4x4 | |
| 6 | 27s | 4x4 | |
| 7 | 23s | 4x4 | |
| 8 | 20s | 4x4 | |
| 9 | 17s | 4x4 | |
| 10 | 15s | 4x4 | Time floor reached, never goes lower |
| 11+ | 15s | 4x5, then 4x6, then increasing symbol similarity | Difficulty now comes from board size and visual complexity, not time |

Time floor at 15 seconds is a hard design rule. Below that the game stops being fun and becomes a reaction test rather than a memory test.

## MVP Feature Scope

In scope for first playable build:
- Single game mode, level based, sequential unlock
- Card flip mechanic with match and mismatch states
- Countdown timer with visual urgency cue in the last 5 seconds
- Level fail and retry flow
- Level select or straight progression screen
- Local save of furthest unlocked level and best time per level
- Basic main menu, level complete screen, level fail screen

Out of scope for MVP, revisit later:
- Multiplayer or leaderboards
- In app purchases or ads
- Daily challenges or events
- Multiple card themes or unlockable skins
- Sound design and music (hooks should exist in code, actual audio assets come later)

## Difficulty and Retention Design Notes
The hook of this game is the tightening time pressure. Every level should feel just barely possible on a good run, which pushes players to retry rather than quit. Because the time floor is fixed at 15 seconds, long term difficulty has to come from spatial complexity (more cards) and perceptual difficulty (similar looking symbols, symbol rotation, or color proximity) rather than an endless timer race, which would eventually become unplayable.

## Success Metrics (informal, personal project scale)
- A build that runs cleanly on a real iOS device
- Level 1 through 10 fully playable and correctly scaled in difficulty
- Clear, reusable level data structure so adding level 11 onward takes minutes, not hours
- A documented, repeatable Unity plus Claude Code workflow that can be reused for the next game

## Visual and Audio Direction
Full detail lives in the brand document. In short: dark navy background, teal and warm orange accent colors, clean geometric sans serif typography, nautical themed card symbols as the default visual set.

## Risks and Open Questions
- iOS builds require either a Mac or a cloud build service, since Windows alone cannot produce a final Xcode build
- Difficulty curve past level 10 needs playtesting once the base loop exists, numbers in this document are a starting hypothesis, not final
