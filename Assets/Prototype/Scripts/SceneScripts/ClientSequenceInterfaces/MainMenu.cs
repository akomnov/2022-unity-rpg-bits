using RPG.Managers.PersistentManagers.ClientSequences.AdditiveSceneSequence;

namespace RPG.SceneScripts.ClientSequenceInterfaces
{
    using RPG.Managers.PersistentManagers;

    public class MainMenu : AdditiveSceneMonoBehaviour {
        private IClientSequenceManager sequenceManager = null;

        protected override void StartInterop() {
            if (sequenceManager != null) return;
            var _sequenceManager = ClientSequenceManager.Instance;
            if (_sequenceManager == null) return;
            if (_sequenceManager.MainMenuSequence == null) return;
            if (_sequenceManager.MainMenuSequence.State != Managers.GameSequence.State.ACTIVE) return;
            sequenceManager = _sequenceManager;
        }

        protected override void StopInterop()
        {
            sequenceManager = null;
        }
        public void NewGame()
        {
            sequenceManager.MainMenuSequence?.OnNewGame(this);
        }

        public void Quit()
        {
            sequenceManager.MainMenuSequence?.OnQuit(this);
        }
    }
}
