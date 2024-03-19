namespace RPG.Components.AI.BehaviourRunner
{
    public interface IBehaviourDefinition
    {
        Core.Shared.AI.IBehaviour InstantiateBehaviour(Core.Shared.AI.IActionContext ctx, object subject);
    }
}