using System.Collections;

namespace RPG.Managers
{
    using GameSequence;

    public interface IGameSequence
    {
        State State { get; }

        /// <summary>
        /// Should call the added callback immediately so that consumer could
        /// react to adding a callback when the sequence is already in the state
        /// they are interested in.
        /// </summary>
        /// <param name="cb">The callback to add</param>
        event System.EventHandler<StateChangedEventArgs> StateChanged;

        /// <remarks>
        ///     Note that if a scene is what's being loaded, it's impossible to
        ///     load a scene and not for it so start calling Awake and OnEnable
        ///     on it's GO-s, unless we're using allowSceneActivation flag, which
        ///     we should not, because it stops async ops thread dead until switched
        ///     back on (what a shitshow).
        /// </remarks>
        IEnumerator Load(System.Action<float> progressCallback = null);

        void Activate(bool skipCamera = false);

        void Deactivate();

        void Reset();

        /// <remarks>
        ///     Note that this probably should not call Resources.UnloadUnusedAssets.
        ///     That should be a concern for the Sequence Manager, as it has more
        ///     information on when it would be preferable to unload shared assets.
        /// </remarks>
        IEnumerator Unload();
    }
}