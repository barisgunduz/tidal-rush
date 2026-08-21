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

Usage rule: teal is the calm, positive color (matches, progress, confirm actions). Orange is reserved for urgency only (last 5 seconds of the timer, fail warnings). Do not use orange decoratively, it should always signal time pressure.

## Typography
- Headings: a clean geometric sans serif, for example Poppins or Manrope, medium to semi bold weight
- Body and UI text: a neutral, highly legible sans serif, for example Inter, regular weight
- Numbers (timer, score) should use a font or numeral style with consistent digit width (tabular numerals) so the countdown does not visually jitter as digits change

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
