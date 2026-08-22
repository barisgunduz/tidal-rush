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
1. ~~Current level indicator during gameplay.~~ Done.
2. **Level transition screen.** Not needed as a separate screen, the existing Level Complete panel already fulfills this role (shows result, Continue button advances to the next level). Marked as satisfied, closed.
3. **Level map / path screen.** Superseded by item 28 below (full version with locks and star ratings, based on the approved mockup).
4. ~~Main menu expansion.~~ Done (New Game, Continue, Settings).
5. ~~Settings screen.~~ Done (baseline sound on/off toggle, Back button).
6. ~~Quit/Exit button.~~ Done.

### B. Visual and legibility fixes
7. ~~Card icon size.~~ Done.

### C. Audio
8. **Source and wire real SFX.** Deferred, moved to section F below. Priority is getting the structural gameplay and visual redesign solid first, audio is a polish layer to add once the game's shape is settled.

### D. Repository setup (GitHub)
9. **README.md.** Project overview, setup instructions, tech stack, current status.
10. **Unity-specific .gitignore.** Exclude Library, Temp, Obj, Build, Logs, and other regenerated folders using GitHub's official Unity .gitignore template.
11. **Git LFS consideration.** Evaluate whether large binary assets (icons, future audio and music files) warrant Git LFS to keep repo size manageable.
12. **LICENSE file.** Decide on a license if the repository will be public.

### E. Visual redesign (external, separate track)
13. **Full design pass using external AI design tools.** Screenshots of each screen (main menu, gameplay, Level Complete, Level Failed) will be run through external design tools by the project owner, results brought back and implemented screen by screen in Unity.

### E2. Visual redesign implementation (from approved mockups, ProjectRequirementDocs/Screenshots)
Seven mockup screens were generated externally (ChatGPT) and approved as the visual direction. Implementation is broken into ordered steps:

18. ~~Card flip color inversion.~~ Done. Flipped cards now use cream background (#DEDDDB) with navy icon tint (#102640), documented in BRAND.md.
19. ~~Background art integration.~~ Done. bg_lighthouse_scene.png and logo_wave.png produced externally, added under Assets/Art, documented in BRAND.md.
20. ~~Main Menu reskin.~~ Functionally done, matches mockup layout and specification (two-tone title, anchor divider, three consistent buttons with icons, correct uppercase text, proportions matching SS1's reference ratios). Flagged by the project owner as not fully satisfying visually yet, accepted for now to keep moving, worth a revisit pass once the other screens are further along.
21. **Gameplay screen reskin.** Apply the card back pattern (subtle compass rose motif visible on face-down cards), stat bar layout (Level, Time, Moves, Matches) matching the mockup's spacing and iconography.
22. **Level Complete / Level Failed reskin.** Apply the mockup's icon-in-circle header treatment (anchor icon for complete, hourglass icon for failed), stat box layout, and background scene.
23. **Settings screen reskin.** Apply the mockup's layout style, panel treatment, and section headers.
24. **Timer font style decision.** Mockups show a digital/LCD-style numeral font for the countdown timer, distinct from the body text font. Decide whether to adopt this or keep the current tabular-numeral body font, not urgent, can be revisited after the reskin above is done.

### E3. New mechanics implied by the approved mockups
These are real new features, not simple reskins, and should be built as their own stages after the visual reskin above is stable:

25. **Pause button and pause/resume flow.** Mockups show a pause control during gameplay. Requires new logic: freeze the timer and input, show a paused state, resume cleanly.
26. **Hint system.** Mockups show a hint counter (lightbulb icon plus a number) both during gameplay and on the level map. Requires defining what a hint does (for example briefly reveal one matching pair), a limited count, and how that count refills or is earned.
27. **Star rating per level.** Mockups show a 1 to 3 star rating per completed level on the level map. Requires defining the scoring criteria (for example based on time remaining or move efficiency) and persisting the best star rating per level alongside the existing unlocked-level save data.
28. **Level map screen with lock and progress states.** Upgrades the earlier simple "level map" backlog idea into the full version shown in the mockup: a connected path of level nodes, locked nodes for levels not yet reached, a distinct current-level marker, and star ratings displayed per completed node.
29. **Separate Music and Sound Effects toggles.** The current Settings screen has a single combined sound toggle. Mockups show Sound Effects and Music as two independent toggles. Music itself does not exist yet as an asset (deferred earlier to a friend composing it later), but the two separate toggle controls and their persisted state can be built now.

### G. Known technical issues and gotchas
Notes for future stages, so the same problem is not re-diagnosed from scratch:

30. **Image.Type.Sliced 9-slice distortion bug.** In this project's Unity setup, UI Image components using Sliced mode silently distort rounded-rectangle button/panel sprites into a full pill/capsule shape, regardless of correct sprite border metadata or pixels-per-unit settings, confirmed even with a Sprite.Create() call that bypassed the normal asset import pipeline. Workaround adopted: generate each button or panel shape as a pre-baked sprite at its exact final display size and use Image.Type.Simple instead of Sliced. Keep this in mind for any future screen (gameplay reskin, Level Complete/Failed reskin, Settings reskin, level map) that uses rounded rectangle buttons or panels, prefer Simple-type pre-sized sprites over Sliced from the start rather than discovering the same distortion again.
31. **Layered button Raycast Target gotcha.** When building a button out of multiple stacked Image layers (Backdrop, Fill, Border, Icon, Label), exactly one layer must have Raycast Target enabled and assigned as the Button component's targetGraphic, or clicks silently fail with no console error. A helper that uniformly disables Raycast Target across all decorative layers (correct for most of them) must not also disable it on the one layer meant to receive the click. When rebuilding any future screen's buttons with this same layered approach, verify at least one layer per button keeps Raycast Target on and is wired as targetGraphic, and test with a real click or a raycast simulation, not just onClick.Invoke(), since Invoke bypasses this exact failure path.

### F. Deferred, not urgent right now
8. **Source and wire real SFX.** Free Unity Asset Store packs identified (for example Dustyroom's Free Casual Game SFX Pack, SwishSwoosh's Free UI Click Sound Pack). Import, select flip/match/mismatch/level complete sounds, wire into the existing empty audio hook methods from Stage 7d. Deferred until the structural and visual redesign work below is settled, audio is a polish layer.
14. **Stage 8: iOS build pipeline (Unity Cloud Build).** Requires Mac access or cloud build account setup, paused until the game has a real visual identity.
15. **Game Center / cloud save sync.** Deferred earlier in favor of simple local device save, revisit only if cross-device progress becomes a real need.
16. **Custom music.** A friend may compose level-based dynamic music later, current architecture (centralized audio hooks) already supports adding this without rework.
17. **Medium article revision.** Once the design pass and remaining features are further along, revise the existing MEDIUM_ARTICLE.md draft into a more detailed piece with before/after screenshots and which AI tools were used at each step.
