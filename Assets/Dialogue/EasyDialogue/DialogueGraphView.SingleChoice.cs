using DS.Elementions;

namespace Unity.EasyDialogue
{
    public partial class DialogueGraphView
    {
        partial void RegisterSingleChoice()
        {
            RegisterNodeType(DialogueType.SingleChoice, "add Node (Single Choice)",
                () => new DialogueSingleChoiceNode());
        }
    }
}
