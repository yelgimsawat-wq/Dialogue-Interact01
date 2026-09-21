using DS.Elementions;

namespace Unity.EasyDialogue
{
    public partial class DialogueGraphView
    {
        partial void RegisterMultipleChoice()
        {
            RegisterNodeType(DialogueType.MultipleChoice, "add Node (Multiple Choice)",
                () => new DialogueMultipleChoiceNode());
        }
    }
}
