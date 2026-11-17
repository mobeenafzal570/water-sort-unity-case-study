# WaterSort — Unity engineering case study

A case study of iterative improvements to a mobile water-sorting puzzle game: bottle placement, liquid presentation, special collectors, undo, onboarding, and progression.

**Stack:** Unity 6000.3.28f1, C#, uGUI, TextMeshPro, Android.

This repository contains engineering notes and selected integration code from an existing game project. It is a focused case study, rather than a complete Unity project or a drop-in package.

## Start here

- [BottleBoardLayout.cs](BottleBoardLayout.cs): calculates bounds in a common coordinate space, reserves room for controls, and arranges bottles around a special collector.
- [GamePalette.cs](GamePalette.cs): keeps serialized color identifiers stable while changing their visual presentation.
- [SpecialLevelRules.cs](SpecialLevelRules.cs): centralizes the special-level schedule.
- [Engineering notes](ENGINEERING.md): the bugs, fixes, and verification boundaries.
- [Companion level validator](https://github.com/mobeenafzal570/water-sort-level-validator): runnable search and replay tooling.

## Work documented

| Problem | Resulting behavior |
| --- | --- |
| Bottles overlap or shift when an extra bottle appears. | Layout uses actual glass bounds and available screen space, with separate normal and special-board arrangements. |
| Special-level liquid has gaps or extends outside its collector. | Collector-specific fill presentation and clipping keep the liquid within the visible shape. |
| Undo history can become inconsistent after a shuffle. | Shuffle invalidates the previous move history; special-level undo is tied to pouring into the collector. |
| Tutorial panels and completion labels drift outside their frames. | Centered panel anchoring and label geometry alignment keep the content within its intended bounds. |
| Levels contain insufficient units of a color to finish. | Inventory checks, solution search, and move replay expose invalid level data. |

## Content and progression

- 50 playable levels in the documented project snapshot.
- Special collectors at levels **10, 20, and 30**.
- A journey map displays levels 1–100; levels 51–100 are marked upcoming.
- Five castle-themed chambers, with chamber transitions at 20-level milestones.
- Beginner tutorials in the first three levels.
- A gradual color progression; the serialized `Ivory` identifier displays as deep teal.

## How to review the code

Read `Bounds`, `SafeArea`, and `Fit` in that order. The important layout decision is to compare every bottle in the same parent coordinate space, then fit the visible glass rather than assuming a fixed root size.

The excerpts retain their game integrations. `BottleBoardLayout` depends on the game's `Bottle` and `PowerUpManager` types. `GamePalette` expects project material resources. Those dependencies and the scenes are outside this repository, so these files cannot be compiled as a standalone application.

## Verification and limits

The project work included Unity compilation, targeted visual checks, and solver/replay checks against a 50-level exported snapshot. The companion validator makes the logical model reviewable and repeatable. It does not prove UI correctness, ad-server availability, or behavior on every Android device.

The implementation and portfolio preparation used AI assistance. The engineering notes describe concrete behavior and tradeoffs rather than attributing the original game's entire codebase to this iteration.
