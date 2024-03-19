namespace RPG.Core.Shared.AI
{
    /// <summary>
    ///     This is to be used for dynamic queries made by actions while they run,
    ///     and also as a service locator into Unity-specific functionality.
    /// </summary>
    public partial interface IActionContext
    {
        // TODO understand context use cases better, maybe compose it instead of implementing in *BehaviourRunner
    }
}