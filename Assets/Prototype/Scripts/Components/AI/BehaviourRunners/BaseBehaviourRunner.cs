using RPG.Managers.PersistentManagers.ClientSequences.AdditiveSceneSequence;

namespace RPG.Components.AI.BehaviourRunners
{
    [UnityEngine.DisallowMultipleComponent]
    public abstract partial class BaseBehaviourRunner : AdditiveSceneMonoBehaviour, IBehaviourRunner {
        private Managers.PersistentManagers.IClientSequenceManager sequenceManager = null;

        protected override void StartInterop() {
            sequenceManager = Managers.PersistentManagers.ClientSequenceManager.Instance;
        }

        protected override void StopInterop() {
            sequenceManager = null;
        }
    }
}