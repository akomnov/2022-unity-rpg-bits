using System.Collections;

namespace RPG.Managers
{
    /// <summary>
    ///     Classes implementing this basically tell Unity what to do: if a scene
    ///     should be loaded, or it's time to clean up assets, things like that.
    ///     
    ///     To keep track of things that should be happening implementers should
    ///     use a notion of a "sequence", see IGameSequence.
    ///     
    ///     Note: this is sort of like a state machine, but not really. States
    ///     are supposed to be exclusive, and sequences could very well run in
    ///     parallel, e.g. one is loading and the other is running to show a
    ///     loading screen, or we even could have several sequences running
    ///     alongside each other. Yes, some FSM impls allow composing states,
    ///     i.e. HSM, but that's based on a notion that a substate cannot
    ///     outlive the superstate, which makes e.g. loading sequences awkward
    ///     (like which state owns the loading screen? what if it's pre-loaded?
    ///     what if it's not?). That warrants concurrent FSMs, which is, well,
    ///     precisely what's happening here, if drilling down into sequence impl
    ///     yields a FSM (which it should for most of sequence types since
    ///     (un-)loading and (un-)pausing is generally involved).
    /// </summary>
    public interface IGameSequenceManager
    {
        /// <summary>
        ///     Pretty much identical to MonoBehaviour's StartCoroutine - this is
        ///     just a way to include that method as part of this interface.
        ///     Though one might consider using this instead of MonoBehaviour's
        ///     if the coroutine's runtime might extend past current GO runtime.
        /// </summary>
        UnityEngine.Coroutine StartCoroutine(IEnumerator routine);

        IEnumerator MakeSureSequenceIsLoaded(IGameSequence seq);
    }
}