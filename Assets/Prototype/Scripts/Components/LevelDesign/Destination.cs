using RPG.Managers.PersistentManagers.ClientSequences.AdditiveSceneSequence;

namespace RPG.Components.LevelDesign
{
    public class Destination : AdditiveSceneMonoBehaviour
    {
        [UnityEngine.SerializeField] private ScriptableObjects.Destination destination = null;
        public ScriptableObjects.Destination DestinationSO { get { return destination; } }
        private Managers.PersistentManagers.IClientSequenceManager sequenceManager = null;

        protected override void StartInterop() {
            if (sequenceManager != null) return;
            var _sequenceManager = Managers.PersistentManagers.ClientSequenceManager.Instance;
            if (_sequenceManager == null) return;
            if (_sequenceManager.CampaignSequence == null) return;
            if (_sequenceManager.CampaignSequence.State != Managers.GameSequence.State.ACTIVE) return;
            sequenceManager = _sequenceManager;
            if (destination != null)
                sequenceManager.CampaignSequence.RegisterDestination(destination, this.gameObject);
        }

        protected override void StopInterop() {
            if (sequenceManager == null) return;
            if (destination != null)
                sequenceManager.CampaignSequence.UnregisterDestination(destination);
            sequenceManager = null;
        }

#if UNITY_EDITOR
        void OnValidate() {
            if (destination != null) {
                if (
                    gameObject.scene != null
                    && gameObject.scene.path != null
                    && !destination.ReferencesSceneGUID(
                            UnityEditor.AssetDatabase.AssetPathToGUID(gameObject.scene.path)
                        ).GetValueOrDefault()
                ) {
                    destination = null;
                    throw new System.Exception(
                        $"Cannot assign destination '{destination}'"
                        + $" onto an object from scene '{gameObject.scene.path}'" );
                }
            }
        }
#endif
    }

}