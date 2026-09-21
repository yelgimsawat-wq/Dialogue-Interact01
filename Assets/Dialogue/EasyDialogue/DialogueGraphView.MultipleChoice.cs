using DS.Elementions;

namespace Unity.EasyDialogue
{
    public partial class DialogueGraphView
    {
        partial void RegisterMultipleChoice()
        {
            RegisterNodeType(DialogueType.MultipleChoice, "Add Dialogue (Multiple Choice)",
                () => new DialogueMultipleChoiceNode());
        }
    }
}
