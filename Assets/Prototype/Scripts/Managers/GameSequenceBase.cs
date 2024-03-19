using System.Collections;

namespace RPG.Managers
{
    using GameSequence;

    /// <summary>
    ///     A game sequence is basically a combination of message handlers and
    ///     callbacks to communicate its state back to the manager.
    ///     Sequence state transitions are defined as follows:
    ///     
    ///         (instantiation)
    ///            |
    ///            V
    ///          UNLOADED state <-----------------------
    ///            |                                    |
    ///          (Load)                                 |
    ///            |                                    |
    ///            V                                    |
    ///          LOADING state                          |
    ///            |                                    |
    ///            |        ----------------------      |
    ///            V       |                      |     |
    ///        - INACTIVE state <---------------(Reset) |
    ///       |    |              |               |     |
    ///       |  (Activate)   (Deactivate)        |     |
    ///       |    |              |               |     |
    ///       |    V              |               |     |
    ///       |  ACTIVE state --------------------      |
    ///       |                                         |
    ///        --(Unload)                               |
    ///            |                                    |
    ///            |                                    |
    ///            V                                    |
    ///          UNLOADING state                        |
    ///            |                                    |
    ///             ------------------------------------
    ///     
    ///     Load and Unload transitions are guaranteed to take at least a single
    ///     frame.
    ///
    ///     Activate, Deactivate and Reset transitions are supposed to finish
    ///     during the current frame.
    ///     
    ///     Note in relation to IGameSequenceManager: this is sort of like a
    ///     state, but not really: see the note in IGameSequenceManager for more
    ///     info.
    /// </summary>
    /// <remarks>
    ///     @akomnov: This could be moved to Core.Shared.
    /// </remarks>
    public abstract class GameSequenceBase : IGameSequence
    {
        public State State { get; protected set; } = State.UNLOADED;

        protected virtual IGameSequenceManager Manager { get; set; } = null;

        public GameSequenceBase(IGameSequenceManager manager)
        {
            this.Manager = manager;
        }

        private void ChangeState(State targetState)
        {
            if (targetState == State) throw new InvalidTransitionException(
                "Already in state " + State.ToString("g")
            );
            // Actual checks for whether current state can transition into
            // target state are done in state transition methods.
            var _oldState = State;
            State = targetState;
            stateChanged?.Invoke(
                this,
                new StateChangedEventArgs { oldState = _oldState, newState = State }
            );
        }

        #region Event management

        protected System.EventHandler<StateChangedEventArgs> stateChanged;

        public event System.EventHandler<StateChangedEventArgs> StateChanged
        {
            add
            {
                stateChanged += value;
                value(this, new StateChangedEventArgs { oldState = State, newState = State });
            }
            remove { stateChanged -= value; }
        }

        #endregion

        #region State transition methods

        public IEnumerator Load(System.Action<float> progressCallback = null) {
            while (State == State.LOADING)
                yield return null;
            if (State != State.UNLOADED && State != State.INACTIVE)
                throw new InvalidTransitionException($"Cannot Load while in '{State:g}'");
            if (State == State.INACTIVE)
                yield break;
            ChangeState(State.LOADING);
            yield return PerformLoading(progressCallback);
            ChangeState(State.INACTIVE);
        }

        public void Activate(bool skipCamera = false) {
            if (State == State.ACTIVE)
                return;
            if (State != State.INACTIVE)
                throw new InvalidTransitionException($"Cannot Activate while in '{State:g}'");
            ChangeState(State.ACTIVE);
            PerformActivation(skipCamera);
        }

        public void Deactivate() {
            if (State == State.INACTIVE)
                return;
            if (State != State.ACTIVE)
                throw new InvalidTransitionException($"Cannot Deactivate while in '{State:g}'");
            ChangeState(State.INACTIVE);
            PerformDeactivation();
        }

        public void Reset() {
            Deactivate();
            PerformResetting();
        }

        public IEnumerator Unload() {
            while (State == State.UNLOADING)
                yield return null;
            if (State != State.INACTIVE && State != State.UNLOADED)
                throw new InvalidTransitionException($"Cannot Unload while in '{State:g}'");
            if (State == State.UNLOADED)
                yield break;
            ChangeState(State.UNLOADING);
            yield return PerformUnloading();
            ChangeState(State.UNLOADED);
        }

        #endregion

        #region Work to be done during state transitions - override these!

        protected virtual IEnumerator PerformLoading(System.Action<float> progressCallback = null)
        {
            yield return null; // does nothing by default
        }

        /// <summary>
        ///     Has to finish during the current frame!
        /// </summary>
        protected virtual void PerformActivation(bool skipCamera = false)
        {
            // does nothing by default
        }

        /// <summary>
        ///     Has to finish during the current frame!
        /// </summary>
        protected virtual void PerformDeactivation()
        {
            // does nothing by default
        }

        /// <summary>
        ///     Has to finish during the current frame!
        /// </summary>
        protected virtual void PerformResetting()
        {
            // does nothing by default
        }

        protected virtual IEnumerator PerformUnloading()
        {
            yield return null; // does nothing by default
        }

        #endregion
    }
}