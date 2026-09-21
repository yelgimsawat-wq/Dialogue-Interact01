# Optional dialogue node modules

`DialogueGraphView.cs` owns shared graph behavior and calls optional partial registration methods. Each module supplies its own menu item and node factory. The core no longer constructs node classes by reflection.

| Module | Files (under `Assets/Dialogue/EasyDialogue`) |
| --- | --- |
| Single Choice | `DialogueGraphView.SingleChoice.cs`, `DialogueSingleChoiceNode.cs` |
| Multiple Choice | `DialogueGraphView.MultipleChoice.cs`, `DialogueMultipleChoiceNode.cs` |

To use Single Choice only, exclude both Multiple Choice files and their `.meta` files from the project. Restore that pair to enable Multiple Choice again. No changes to the graph core, exporter, or runtime canvas are required. Keep the existing `DialogueType` enum values for compatibility with saved data.

The optional module controls creation of new nodes in the editor. Exported Dialogue Containers retain all their choices and can still be played by `DialogueCanvas`, even without the Multiple Choice editor module. This does not add graph saving/loading; preserve your graph before changing scripts.

Export regression checks: `pwsh -NoProfile -File Tests/DialogueExporterRegression.ps1`.
