using RPG.Managers.PersistentManagers.ClientSequences.AdditiveSceneSequence;

namespace RPG.SceneScripts.ClientSequenceInterfaces
{
    using RPG.Managers.PersistentManagers;

    public class Intro : AdditiveSceneMonoBehaviour {
        private IClientSequenceManager sequenceManager = null;
        [UnityEngine.SerializeField] private UnityEngine.GameObject skipPrompt = null;

        protected override void StartInterop() {
            if (sequenceManager != null) return;
            var _sequenceManager = ClientSequenceManager.Instance;
            if (_sequenceManager == null) return;
            if (_sequenceManager.IntroSequence == null) return;
            if (_sequenceManager.IntroSequence.State != Managers.GameSequence.State.ACTIVE) return;
            sequenceManager = _sequenceManager;
            sequenceManager.IntroSequence.NextSequenceReady += AttemptSkipPromptActivation;
        }

        protected override void StopInterop()
        {
            if (sequenceManager == null) return;
            sequenceManager.IntroSequence.NextSequenceReady -= AttemptSkipPromptActivation;
            sequenceManager = null;
        }

        private void Update()
        {
            if (skipPrompt != null && skipPrompt.activeSelf && UnityEngine.Input.anyKeyDown)
                IntroDone();
        }

        public void IntroDone() {
            if (sequenceManager == null) return;
            sequenceManager.IntroSequence?.OnIntroDone(this);
        }

        private void AttemptSkipPromptActivation(object sender, System.EventArgs _)
        {
            if (sequenceManager == null) return;
            sequenceManager.IntroSequence.NextSequenceReady -= AttemptSkipPromptActivation;
            if (!sequenceManager.IntroSequence.IsIntroDone && skipPrompt != null && ! skipPrompt.activeSelf)
            {
                skipPrompt.SetActive(true);
            }
        }
    }
}
