using RPG.Managers.PersistentManagers.ClientSequences.AdditiveSceneSequence;
using static RPG.Core.Shared.Utils.Functions;

namespace RPG.Components.LevelDesign
{
    public class Portal : AdditiveSceneMonoBehaviour {
        private Managers.PersistentManagers.IClientSequenceManager sequenceManager = null;
        [UnityEngine.SerializeField] private ScriptableObjects.Destination destination = null;

        protected override void StartInterop() {
            if (sequenceManager != null) return;
            var _sequenceManager = Managers.PersistentManagers.ClientSequenceManager.Instance;
            if (_sequenceManager == null) return;
            if (_sequenceManager.CampaignSequence == null) return;
            if (_sequenceManager.CampaignSequence.State != Managers.GameSequence.State.ACTIVE) return;
            sequenceManager = _sequenceManager;
        }

        protected override void StopInterop() {
            sequenceManager = null;
        }

        private void OnTriggerEnter2D(UnityEngine.Collider2D other)
        {
            if (sequenceManager == null) return;
            var _go = other.gameObject;
            if (_go.CompareTag("Player")) {
                sequenceManager.StartCoroutine(
                    CoroutineAndCallBack(
                        sequenceManager.CampaignSequence.TeleportViaCurtain(destination, _go),
                        sequenceManager.HandleUnrecoverableException ) );
            }
        }
    }

}