# Tidal Rush: Progress and Backlog

## Completed So Far

| Stage | What It Covers | Status |
|-------|----------------|--------|
| Stage 0 | Unity Hub, Unity 6 LTS, iOS Build Support module, VS Code, Unity MCP bridge connected to Claude Code | Done |
| Stage 1 | Project scaffolding: URP 2D pipeline, iOS build target, folder structure, MainGame scene, 9:16 aspect ratio | Done |
| Stage 2a | Card prefab, front/back visual states, brand colors applied | Done |
| Stage 2b | GameManager spawns a shuffled 4x4 grid of 16 cards, 8 pairs | Done |
| Stage 2c | Click/tap flip logic, match and mismatch resolution, input locking during resolution | Done |
| Stage 2d | Move counter and match counter UI, live updating | Done |
| Stage 3 | LevelData ScriptableObject system, 10 level assets (levels 1 to 10) with correct time values from PRD | Done |
| Stage 4a | Countdown timer UI, orange urgency color and pulse in the last 5 seconds | Done |
| Stage 4b | Level Complete and Level Failed panels, Continue and Retry buttons, mutually exclusive win/fail states | Done |
| Stage 5a | Local save system (PlayerPrefs), unlocked level persists across sessions, testing reset method | Done |
| Stage 5b | Basic MainMenu scene with a Play button, loads MainGame at the correct saved level | Done |
| Stage 6a | Difficulty scaling plan for levels 11 to 15 (grid size progression, symbol rotation lever) | Done |
| Stage 6b | Levels 11 to 15 implemented: width/height aware grid clamp, icon plus rotation pair identity, gradual rotation introduction, level cap fixed to 15 | Done |
| Stage 7a | Card flip animation and match confirmation pulse, both under BRAND.md's timing limits | Done |
| Stage 7b | BRAND.md color palette and Manrope heading font applied across all screens | Done |
| Stage 7c | Real nautical icon set (anchor, compass, wave, sailboat, shell, map, dolphin, lightning) replacing placeholder shapes | Done |
| Stage 7d | Empty audio hook methods wired into flip, match, mismatch, and level complete events; full text audit against BRAND.md writing rules, zero violations found | Done |

The core game loop is fully functional: menu, timed matching gameplay, win and fail states, progressive difficulty, local save, and a baseline visual identity.

---

## Backlog: Next Up

### A. Game flow and navigation gaps
1. **Current level indicator during gameplay.** Right now the player has no on-screen confirmation of which level they are playing.
2. **Level transition screen.** When advancing from one level to the next, show a brief transition or announcement rather than an instant cut.
3. **Level map / path screen.** A visual progression map (node-based path showing completed, current, and locked levels, similar in spirit to Candy Crush's level path) instead of, or in addition to, the current plain Continue flow.
4. **Main menu expansion.** Add New Game, Continue, and Settings as distinct options, not just a single Play button.
5. **Settings screen.** At minimum a mute toggle for sound, possibly a reset progress option.
6. **Quit/Exit button.** No current way to exit the app from within the game itself.

### B. Visual and legibility fixes
7. **Card icon size.** Icons render too small on flipped cards relative to the available card space, increase icon scale within the existing card bounds.

### C. Audio
8. **Source and wire real SFX.** Free Unity Asset Store packs identified (for example Dustyroom's Free Casual Game SFX Pack, SwishSwoosh's Free UI Click Sound Pack). Import, select flip/match/mismatch/level complete sounds, wire into the existing empty audio hook methods from Stage 7d.

### D. Repository setup (GitHub)
9. **README.md.** Project overview, setup instructions, tech stack, current status.
10. **Unity-specific .gitignore.** Exclude Library, Temp, Obj, Build, Logs, and other regenerated folders using GitHub's official Unity .gitignore template.
11. **Git LFS consideration.** Evaluate whether large binary assets (icons, future audio and music files) warrant Git LFS to keep repo size manageable.
12. **LICENSE file.** Decide on a license if the repository will be public.

### E. Visual redesign (external, separate track)
13. **Full design pass using external AI design tools.** Screenshots of each screen (main menu, gameplay, Level Complete, Level Failed) will be run through external design tools by the project owner, results brought back and implemented screen by screen in Unity.

### F. Deferred, not urgent right now
14. **Stage 8: iOS build pipeline (Unity Cloud Build).** Requires Mac access or cloud build account setup, paused until the game has a real visual identity.
15. **Game Center / cloud save sync.** Deferred earlier in favor of simple local device save, revisit only if cross-device progress becomes a real need.
16. **Custom music.** A friend may compose level-based dynamic music later, current architecture (centralized audio hooks) already supports adding this without rework.
17. **Medium article revision.** Once the design pass and remaining features are further along, revise the existing MEDIUM_ARTICLE.md draft into a more detailed piece with before/after screenshots and which AI tools were used at each step.
