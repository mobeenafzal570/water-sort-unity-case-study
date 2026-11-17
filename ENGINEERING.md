# Engineering notes

## Layout in one coordinate space

A bottle root and its visible artwork can have different sizes. Placing roots at equal intervals therefore does not guarantee that the glass will not overlap.

`BottleBoardLayout.Bounds` reads the glass rectangle's world corners and transforms them into a common parent. `SafeArea` reserves space for the HUD and power-up bar. Normal boards retain row ordering and scale to fit the available rectangle. Special boards reserve a central receiver and allocate cells above, below, left, and right of it.

The special board randomizes which small bottle occupies each cell, with bounded jitter inside that cell. This separates randomness from the space needed by each bottle.

**Tradeoff:** scene object names and game-specific references remain part of this implementation. A reusable package would accept explicit references and a layout configuration instead of discovering components.

## Stable data identifiers

Changing a color name can break serialized levels, locks, and receiver matching. `GamePalette` preserves the `Ivory` ID and changes its displayed color to teal. Logical matching continues to use the saved identifier; liquid and effects obtain their presentation from the same palette.

**Tradeoff:** the legacy name is less intuitive. A future data migration could introduce stable numeric IDs with separate display names, provided all serialized content migrates together.

## Undo and power-ups

A move history refers to a particular board state. Shuffle creates a new arrangement, so replaying old undo entries can put colors into the wrong bottles. Clearing history and selection on shuffle establishes a new baseline.

Special levels use a central goal collector. Undo eligibility is tied to actual receiver pours rather than any interaction with a bottle. A selection or rejected pour must not become an undoable move.

## Liquid presentation

Normal stacked layers and a shaped special collector have different rendering requirements. The collector fill must follow its silhouette and retain a continuous surface as its volume changes. Layout and rendering checks must include intermediate pours, rather than only empty and full states.

The level validator models discrete color units. It intentionally does not model clipping, animation timing, or surface geometry; those require Unity visual checks.

## Progression and tutorials

Special levels are an explicit set: 10, 20, and 30. A divisibility rule such as `level % 10 == 0` would incorrectly include 40 and 50 in this project's schedule.

The journey displays 100 nodes while clearly marking the final 50 as upcoming. Displaying a map node and making a level playable are distinct progression states.

Tutorial panels need a stable screen-space parent. Their pour demonstrations also need the stream aligned to the receiving bottle's opening throughout the animation.

## Next improvements

- Replace component discovery with serialized or injected layout references.
- Add a reproducible visual capture matrix for portrait aspect ratios and intermediate pour frames.
- Keep a versioned level-data schema shared by Unity exports and the validator.
- Benchmark search budgets and report unresolved searches separately from proven dead ends.
