using RPG.Utils.PropertyAttributes;

namespace RPG.Components.AI.BehaviourRunner.Slots
{
    /// <summary>
    ///     A slot that only supports IFixed behaviours serialized by reference
    ///     (i.e. configured inline inside the Editor component inspector).
    ///     We want to be able to compose an ordered list of behaviours from both
    ///     inline objects and ScriptableObjects, but Unity can only serialize
    ///     a list of references to one or the other, hence the need for a layer
    ///     of indirection.
    /// </summary>
    [System.Serializable]
    public class FixedSerialized : IFixed {
        [UnityEngine.SerializeReference]
        [SelectImplementation]
        private BehaviourDefinitions.IFixed behaviourDefinition = null;
        public BehaviourDefinitions.IFixed BehaviourDefinition
        {
            get { return behaviourDefinition; }
        }
        IBehaviourDefinition ISlot.BehaviourDefinition
        {
            get { return BehaviourDefinition; }
        }
    }
}