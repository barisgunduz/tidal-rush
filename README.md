# Tidal Rush

A fast paced memory matching game where every level gives you less time than the last, until speed itself becomes the real puzzle.

## Status

- Core gameplay loop complete: card flip, match and mismatch resolution, countdown timer, win and fail states
- 15 levels implemented
- Local save of progress (PlayerPrefs)
- Basic main menu in place, Play button loads into the game
- Visual polish in progress: BRAND colors and typography applied, real nautical icons wired in, motion polish on flips and matches done, audio hooks exist in code but no sound files yet

## Tech Stack

- Unity 6 LTS
- Universal Render Pipeline, 2D Renderer
- New Input System
- C#

## Difficulty Progression

Levels 1 through 10 hold a fixed 4x4 grid and cut the time limit each level, from 60 seconds down to a hard floor of 15 seconds. That floor never goes lower: below it the game stops feeling like a memory challenge and turns into a pure reaction test.

From level 11 onward, time stays fixed at 15 seconds and difficulty comes from two other levers instead: a bigger board (4x5 up through 6x7 by level 15, meaning more pairs to track) and, starting at level 13, icon rotation, where matching symbols can appear at different angles so pairs have to be told apart by orientation as well as shape.

See `ProjectRequirementDocs/PRD.md` for the full level by level table and design reasoning.

## Documentation

Full project documentation lives in `ProjectRequirementDocs`:

- `PRD.md`: product requirements, core loop, level progression table, and design notes
- `BRAND.md`: visual identity, color palette, typography, iconography, motion, and writing style rules
- `CLAUDE_CODE_BUILD_GUIDE.md`: the stage by stage guide used to build this project with Unity and Claude Code
