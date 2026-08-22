# Brand and Visual Identity Document

## Game Title (working name)
Tidal Rush

## Positioning
A casual mobile memory game that feels tense and satisfying rather than childish. It should feel closer to a reflex arcade game in tone than to a kids matching app, even though the mechanic itself (flip two cards, find pairs) is simple and familiar to everyone.

## Name Rationale
Tidal Rush ties into a nautical visual language (waves, tides, timing) which mirrors the time pressure mechanic. Working name only, can change once the build exists and the feel is confirmed.

## Tone of Voice
- Direct and calm in UI copy, no exclamation heavy language
- Short instructions, no long onboarding text
- Slightly competitive in framing (best time, personal record) without being aggressive
- No slang, no forced humor, no filler words
- No em dashes anywhere, in code comments, UI text, or documentation. Use a plain hyphen (-) for a break in a sentence, or a colon (:) when introducing an explanation

## Color Palette

| Role | Color | Hex |
|------|-------|-----|
| Background (primary) | Deep navy | #0F1420 |
| Card back (default) | Muted slate blue | #1E2A45 |
| Card back (hover/active) | Lighter slate blue | #263352 |
| Accent (primary, success/match) | Teal | #4FD1C5 |
| Accent (secondary, urgency/warning) | Warm orange | #F6AD55 |
| Text (primary) | Off white | #E8ECF4 |
| Text (muted/secondary) | Cool gray | #8B93A7 |
| Success state | Deep teal green | #1C5F4F |
| Card face-up background | Cream / off-white | #DEDDDB |
| Card face-up icon tint | Deep navy | #102640 |

Usage rule: teal is the calm, positive color (matches, progress, confirm actions). Orange is reserved for urgency only (last 5 seconds of the timer, fail warnings). Do not use orange decoratively, it should always signal time pressure.

Face-down cards use the original dark card back colors (default and hover/active) listed above. Face-up (flipped) cards use the cream background and navy icon tint listed above instead, this was adopted after the approved external design pass and should be treated as the current standard, not the original all-dark card back and off-white icon combination described in earlier drafts of this document.

## Typography
- Headings: Manrope, SemiBold weight, confirmed and implemented. Poppins was tried first but showed a font rendering issue in Unity (glyphs displayed with an incorrect script/language mapping), so this project has standardized on Manrope for all headings going forward
- Body and UI text: a neutral, highly legible sans serif, for example Inter, regular weight. Not yet swapped in as a real font asset, currently still rendering on Unity's default runtime font, this remains an open item
- Numbers (timer, score) should use a font or numeral style with consistent digit width (tabular numerals) so the countdown does not visually jitter as digits change. The approved mockups show a distinct digital/LCD style numeral treatment for the timer specifically, this has not yet been decided on or implemented, still an open style decision

## Iconography and Symbol Set
Default card symbol set follows a light nautical theme:
- Anchor
- Compass
- Wave
- Sailboat
- Shell
- Map
- Dolphin
- Lightning (used sparingly, represents speed/energy rather than the sea theme)

Symbols should be simple, single color line or flat icons, readable at small mobile card sizes (roughly 70 to 90 px). Avoid detailed illustration style, it will not read clearly at that scale.

These 8 symbols are the gameplay card icon set only. The project also uses a small set of separate UI action icons, not part of the card matching symbol pool: gear-hammer.png (Settings) and exit-door.png (Quit). These are stored in the same Assets/Sprites/Icons folder as the gameplay symbols but serve a different, menu-navigation purpose, do not confuse them with the 8 card symbols above when referencing "the icon set" in future prompts.

## Background and Logo Assets
Two illustration assets were produced externally (via AI design tools) and approved as the visual direction, both live under Assets/Art:
- Assets/Art/Backgrounds/bg_lighthouse_scene.png: a full screen nautical night scene (lighthouse on a cliff, dark navy sea and sky, moonlit water) used as the background on the Main Menu, Level Complete, and Level Failed screens
- Assets/Art/Logos/logo_wave.png: a standalone stylized teal wave mark, transparent background, used as a logo graphic above the "Tidal Rush" title on the Main Menu

Both assets are treated as final art for these screens, not placeholders, unless a future design pass replaces them.

## App Icon Direction
Dark navy background, a single bold symbol (compass or anchor), teal accent line work, no text on the icon itself. Should read clearly at the smallest iOS icon size.

## Motion and Feel
- Card flip: fast, under half a second, no bounce or elastic easing, this is a speed game and animation should never slow the player down
- Match confirmation: brief teal glow or scale pulse, no more than 200ms
- Timer urgency: at 5 seconds remaining, timer text shifts to orange and can pulse gently, no jarring flash or shake

## Sound Direction (for later implementation)
- Card flip: short, soft click
- Match: short positive chime, teal in feeling (not a full melody, a single clean tone)
- Mismatch: neutral short tone, not punishing or negative in character
- Level complete: short upbeat stinger, under 2 seconds
- Timer urgency: optional subtle heartbeat or tick under the last 5 seconds, must be possible to mute independently from music

## Writing Style Rules for All Game Text and Documentation
- No em dashes, use a hyphen (-) or a colon (:) instead
- No exclamation point stacking, one is enough if used at all
- Keep UI microcopy under 6 words where possible ("Level Complete", "Try Again", "Best Time: 42s")
- All documentation and in game text in English
