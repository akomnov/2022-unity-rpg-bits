namespace RPG.Components.AI.BehaviourRunner.Slots
{
    /// <summary>
    ///     A slot that only supports IFixed behaviours
    ///     (needed since FixedBehaviourRunner only supports these).
    /// </summary>
    public interface IFixed : ISlot
    {
        new BehaviourDefinitions.IFixed BehaviourDefinition { get; }
    }
}