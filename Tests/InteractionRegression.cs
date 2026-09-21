// Exercise the real interaction dispatch and quest manager with Unity/UI doubles.
using System;
using System.Reflection;
using UnityEngine;
namespace UnityEngine {
    public class MonoBehaviour {
        public GameObject gameObject = new GameObject();
        protected static T FindFirstObjectByType<T>() where T : class => null;
    }
    public class GameObject { public string name = "Test NPC"; }
    public struct Vector3 { }
    public class ScriptableObject { }
    public interface ISerializationCallbackReceiver { void OnBeforeSerialize(); void OnAfterDeserialize(); }
    public class SerializeField : Attribute { }
    public class HideInInspector : Attribute { }
    public class MinAttribute : Attribute { public MinAttribute(float n) { } }
    public class TooltipAttribute : Attribute { public TooltipAttribute(string s) { } }
    public class TextAreaAttribute : Attribute { public TextAreaAttribute(int a, int b) { } }
    public class CreateAssetMenuAttribute : Attribute { public string fileName; public string menuName; }
    public static class Debug { public static void LogWarning(object value, object context) { } }
}
public partial class InteractableObject {
    public int DialogueCalls, LightCalls, DoorCalls;
    partial void DialogueUpdate() { DialogueCalls++; }
    partial void LightUpdate() { LightCalls++; }
    partial void DoorUpdate() { DoorCalls++; }
}
public static class InteractionRegression {
    static void Set(object target, string name, object value) => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    public static void Run() {
        foreach (InteractableType legacy in Enum.GetValues(typeof(InteractableType))) {
            var old = new InteractableObject();
            Set(old, "InteractableType", legacy);
            old.OnAfterDeserialize();
            old.Interact();
            Check(old.DialogueCalls == (legacy == InteractableType.Dialogue ? 1 : 0), "Legacy dialogue migration failed");
            Check(old.LightCalls == (legacy == InteractableType.Light ? 1 : 0), "Legacy light migration failed");
            Check(old.DoorCalls == (legacy == InteractableType.Door ? 1 : 0), "Legacy door migration failed");
        }
        var system = new QuestSystem();
        typeof(QuestSystem).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(system, null);
        var quest = new QuestSet { QuestId = "talk" };
        quest.Objectives.Add(new QuestObjective_Child { EventId = "talked", TargetId = "npc", requiredAmount = 2 });
        var npc = new InteractableObject();
        npc.OnAfterDeserialize();
        Set(npc, "interactionTypes", InteractionActions.Dialogue | InteractionActions.Quest);
        Set(npc, "questSystem", system);
        Set(npc, "questDefinition", quest);
        npc.OnBeforeSerialize();
        npc.OnAfterDeserialize();
        npc.Interact();
        Check(npc.DialogueCalls == 1 && system.Manager.GetQuest("talk") != null, "Dialogue and quest must run together");
        npc.Interact();
        Check(system.Manager.GetAllQuests().Count == 1 && npc.DialogueCalls == 2, "Repeat interaction duplicated quest or blocked dialogue");
        Set(npc, "questActions", QuestInteractionActions.ReportEvent);
        Set(npc, "questEventId", "talked");
        Set(npc, "questTargetId", "wrong");
        npc.Interact();
        Check(system.Manager.GetQuest("talk").progressions[0].GetCurrentAmount() == 0, "Event target mismatch advanced quest");
        Set(npc, "questTargetId", "npc");
        npc.Interact();
        Check(system.Manager.GetQuest("talk").progressions[0].GetCurrentAmount() == 1, "Interaction did not report progress");
        npc.Interact();
        Check(system.Manager.IsQuestCompleted("talk"), "Interaction did not complete quest");
        Set(npc, "interactionTypes", InteractionActions.None);
        npc.OnBeforeSerialize(); npc.OnAfterDeserialize();
        int count = npc.DialogueCalls;
        npc.Interact();
        Check(npc.DialogueCalls == count, "Serialization reset None selection");
        Set(npc, "interactionTypes", InteractionActions.Dialogue | InteractionActions.Light | InteractionActions.Door | InteractionActions.Quest);
        Set(npc, "questSystem", null);
        npc.Interact();
        Check(npc.DialogueCalls == count + 1 && npc.LightCalls == 1 && npc.DoorCalls == 1, "Missing quest system blocked other actions");
        var combined = new InteractableObject();
        combined.OnAfterDeserialize();
        Set(combined, "interactionTypes", InteractionActions.Quest);
        var anotherSystem = new QuestSystem();
        typeof(QuestSystem).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(anotherSystem, null);
        Set(combined, "questSystem", anotherSystem);
        Set(combined, "questDefinition", quest);
        Set(combined, "questActions", QuestInteractionActions.AcceptQuest | QuestInteractionActions.ReportEvent);
        Set(combined, "questEventId", "talked");
        Set(combined, "questTargetId", "npc");
        combined.Interact();
        Check(anotherSystem.Manager.GetQuest("talk").progressions[0].GetCurrentAmount() == 1, "Accept must run before report event");
    }
}
