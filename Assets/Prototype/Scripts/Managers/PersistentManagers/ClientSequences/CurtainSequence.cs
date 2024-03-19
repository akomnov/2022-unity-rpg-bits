using RPG.Utils;

namespace RPG.Managers.PersistentManagers.ClientSequences
{
    [System.Serializable]
    public class CurtainSequence : AdditiveSceneSequenceBase
    {
        public CurtainSequence(IClientSequenceManager manager, Id id)
            : base(manager, id) { }
        public bool HasLowered { get; protected set; } = false;
        public bool IsReadyToLift { get; protected set; } = false;
        public bool HasLifted { get; protected set; } = false;

        protected System.EventHandler lowered;
        protected System.EventHandler readyToLift;
        protected System.EventHandler lifted;

        /// <summary>
        /// Will call the provided callback immediately if already lowered
        /// </summary>
        /// <param name="cb">The callback to add</param>
        public event System.EventHandler Lowered
        {
            add
            {
                lowered += value;
                if (HasLowered) value(this, System.EventArgs.Empty);
            }
            remove { lowered -= value; }
        }

        /// <summary>
        /// Will call the provided callback immediately if already ready to lift
        /// </summary>
        /// <param name="cb">The callback to add</param>
        public event System.EventHandler ReadyToLift
        {
            add
            {
                readyToLift += value;
                if (IsReadyToLift) value(this, System.EventArgs.Empty);
            }
            remove { readyToLift -= value; }
        }

        /// <summary>
        /// Will call the provided callback immediately if already lifted
        /// </summary>
        /// <param name="cb">The callback to add</param>
        public event System.EventHandler Lifted
        {
            add
            {
                lifted += value;
                if (HasLifted) value(this, System.EventArgs.Empty);
            }
            remove { lifted -= value; }
        }

        public void OnLowered(object sender)
        {
            if (HasLowered) return;
            HasLowered = true;
            lowered?.Invoke(sender, System.EventArgs.Empty);
        }

        public void OnReadyToLift(object sender)
        {
            if (IsReadyToLift) return;
            IsReadyToLift = true;
            readyToLift?.Invoke(sender, System.EventArgs.Empty);
        }

        public void OnLifted(object sender)
        {
            if (HasLifted) return;
            HasLifted = true;
            lifted?.Invoke(sender, System.EventArgs.Empty);
        }

        protected override void PerformResetting()
        {
            base.PerformResetting();
            HasLowered = false;
            IsReadyToLift = false;
            HasLifted = false;
        }
    }
}
