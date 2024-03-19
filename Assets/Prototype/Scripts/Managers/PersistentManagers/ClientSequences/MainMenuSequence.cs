using RPG.Utils;

namespace RPG.Managers.PersistentManagers.ClientSequences
{
    [System.Serializable]
    public class MainMenuSequence : AdditiveSceneSequenceBase
    {
        public MainMenuSequence(IClientSequenceManager manager, Id id)
            : base(manager, id) { }
        public bool IsSequenceExiting { get; protected set; } = false;

        protected override void PerformResetting()
        {
            throw new System.Exception("TODO: Unclear what to do here yet. Implement this or change this message.");
        }

        public void OnNewGame(object sender)
        {
            if (IsSequenceExiting) return;
            //IsSequenceExiting = true;
            Manager.NewGame(sender);
        }

        public void OnQuit(object sender)
        {
            if (IsSequenceExiting) return;
            IsSequenceExiting = true;
            // @akomnov: as it is now, does not do much, but maybe later we'll need
            // to add some cleanup here.
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }
    }
}
