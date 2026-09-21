using DS.Elementions;

namespace Unity.EasyDialogue
{
    public partial class DialogueGraphView
    {
        partial void RegisterSingleChoice()
        {
            RegisterNodeType(DialogueType.SingleChoice, "Add Dialogue (Single Choice)",
                () => new DialogueSingleChoiceNode());
        }
    }
}
