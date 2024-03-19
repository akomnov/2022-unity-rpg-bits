using RPG.Utils;

namespace RPG.Managers.PersistentManagers.ClientSequences
{
    [System.Serializable]
    public class LoadingSequence : AdditiveSceneSequenceBase
    {
        public LoadingSequence(IClientSequenceManager manager, Id id)
            : base(manager, id) { }
        public float NextSequenceProgress { get; set; } = 0.0f;
        public bool IsNextSequenceReady { get; protected set; } = false;

        protected System.EventHandler nextSequenceReady;

        /// <summary>
        /// Will call the provided callback immediately if next sequence is already ready
        /// </summary>
        /// <param name="cb">The callback to add</param>
        public event System.EventHandler NextSequenceReady
        {
            add
            {
                nextSequenceReady += value;
                if (IsNextSequenceReady) value(this, System.EventArgs.Empty);
            }
            remove { nextSequenceReady -= value; }
        }

        public void OnNextSequenceReady()
        {
            if (IsNextSequenceReady) return;
            IsNextSequenceReady = true;
            nextSequenceReady?.Invoke(this, System.EventArgs.Empty);
        }

        protected override void PerformResetting()
        {
            base.PerformResetting();
            NextSequenceProgress = 0.0f;
            IsNextSequenceReady = false;
        }
    }
}
