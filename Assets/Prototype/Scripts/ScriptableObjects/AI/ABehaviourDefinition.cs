namespace RPG.ScriptableObjects.AI
{
    /// <summary>
    ///     Necessary because Unity Editor seems to be having trouble with fields
    ///     of interface type for ScriptableObjects.
    /// </summary>
    public abstract class ABehaviourDefinition : UnityEngine.ScriptableObject
    {
        public abstract Components.AI.BehaviourRunner.IBehaviourDefinition BehaviourDefinition { get; }
    }
}