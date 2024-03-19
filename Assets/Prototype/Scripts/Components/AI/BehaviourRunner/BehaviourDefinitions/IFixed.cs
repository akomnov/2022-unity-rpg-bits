namespace RPG.Components.AI.BehaviourRunner.BehaviourDefinitions
{
    public interface IFixed : IBehaviourDefinition
    {
        new Core.Shared.AI.Behaviours.IFixed InstantiateBehaviour(Core.Shared.AI.IActionContext ctx, object subject);
    }
}