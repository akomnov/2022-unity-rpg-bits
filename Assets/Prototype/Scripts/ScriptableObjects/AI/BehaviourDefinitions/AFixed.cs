namespace RPG.ScriptableObjects.AI.Behaviourdefinitions
{
    /// <summary>
    ///     Necessary because Unity Editor seems to be having trouble with fields
    ///     of interface type for ScriptableObjects.
    /// </summary>
    public abstract class AFixed : ABehaviourDefinition
    {
        public override Components.AI.BehaviourRunner.IBehaviourDefinition BehaviourDefinition
        {
            get
            {
                return FixedBehaviourDefinition;
            }
        }
        public abstract Components.AI.BehaviourRunner.BehaviourDefinitions.IFixed FixedBehaviourDefinition { get; }
    }
}