namespace RPG.Components.AI.BehaviourRunner.Slots
{
    /// <summary>
    ///     A slot that only supports IFixed behaviours that are ScriptableObjects
    ///     (i.e. pre-configured as separate assets).
    ///     We want to be able to compose an ordered list of behaviours from both
    ///     inline objects and ScriptableObjects, but Unity can only serialize
    ///     a list of references to one or the other, hence the need for a layer
    ///     of indirection.
    /// </summary>
    [System.Serializable]
    public class FixedScriptable : IFixed {
        [UnityEngine.SerializeField]
        private ScriptableObjects.AI.Behaviourdefinitions.AFixed behaviourDefinition;
        public BehaviourDefinitions.IFixed BehaviourDefinition
        {
            get { return behaviourDefinition.FixedBehaviourDefinition; }
        }
        IBehaviourDefinition ISlot.BehaviourDefinition
        {
            get { return BehaviourDefinition; }
        }
    }
}