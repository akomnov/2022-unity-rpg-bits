using RPG.Utils;

namespace RPG.Managers.PersistentManagers.ClientSequences
{
    [System.Serializable]
    public class IntroSequence : AdditiveSceneSequenceBase
    {
        public IntroSequence(IClientSequenceManager manager, Id id)
            : base(manager, id) { }
        public bool IsIntroDone { get; protected set; } = false;
        public bool IsNextSequenceReady { get; protected set; } = false;

        protected System.EventHandler introDone;

        /// <summary>
        /// Will call the provided callback immediately if intro is already done
        /// </summary>
        /// <param name="cb">The callback to add</param>
        public event System.EventHandler IntroDone
        {
            add
            {
                introDone += value;
                if (IsIntroDone) value(this, System.EventArgs.Empty);
            }
            remove { introDone -= value; }
        }

        public void OnIntroDone(object sender)
        {
            if (IsIntroDone) return;
            IsIntroDone = true;
            introDone?.Invoke(sender, System.EventArgs.Empty);
        }

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
            throw new System.Exception("We aren't supposed to reset the intro scene, are we?");
        }
    }
}
