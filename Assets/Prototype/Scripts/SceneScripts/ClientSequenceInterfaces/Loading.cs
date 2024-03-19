using RPG.Managers.PersistentManagers.ClientSequences.AdditiveSceneSequence;

namespace RPG.SceneScripts.ClientSequenceInterfaces
{
    using RPG.Managers.PersistentManagers;

    public class Loading : AdditiveSceneMonoBehaviour {
        private IClientSequenceManager sequenceManager = null;
        [UnityEngine.SerializeField] private UnityEngine.UI.Slider progressSlider = null;


        protected override void StartInterop() {
            if (sequenceManager != null) return;
            var _sequenceManager = ClientSequenceManager.Instance;
            if (_sequenceManager == null) return;
            if (_sequenceManager.LoadingSequence == null) return;
            if (_sequenceManager.LoadingSequence.State != Managers.GameSequence.State.ACTIVE) return;
            sequenceManager = _sequenceManager;
        }

        protected override void StopInterop()
        {
            sequenceManager = null;
        }

        private void Update()
        {
            if (sequenceManager == null) return;
            if (progressSlider != null && sequenceManager.LoadingSequence != null)
            {
                progressSlider.value = sequenceManager.LoadingSequence.NextSequenceProgress;
            }
        }
    }
}
