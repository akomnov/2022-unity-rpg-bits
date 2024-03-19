namespace RPG.ScriptableObjects.AI.Behaviourdefinitions.Fixed
{
    [UnityEngine.CreateAssetMenu(fileName = "New ActionList", menuName = "AI/BehaviourDefinitions/Fixed/ActionList")]
    public class ActionList : AFixed {
        [UnityEngine.SerializeField]
        private Components.AI.BehaviourRunner.BehaviourDefinitions.Fixed.ActionList behaviourDefinition = default;
        public override Components.AI.BehaviourRunner.BehaviourDefinitions.IFixed FixedBehaviourDefinition
        {
            get
            {
                return behaviourDefinition;
            }
        }
    }
}