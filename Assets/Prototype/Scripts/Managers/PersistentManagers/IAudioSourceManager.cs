
using RPG.Utils;

namespace RPG.Managers.PersistentManagers
{
    public interface IAudioSourceManager
    {
        /// <summary>
        ///     Creates an AudioSource component on the manager's GO and copies
        ///     template's field values to it.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="disableTemplate">template component will become disabled after this call unless this is false</param>
        /// <returns>the result of copy operation</returns>
        UnityEngine.AudioSource CreateManagedAudioSourceFromTemplate(
            UnityEngine.AudioSource template,
            bool disableTemplate = true
        );
        /// <summary>
        ///     Play an AS as a non-diegetic background. There can be only one 
        ///     of those at any given moment, except when cross-fading from previous
        ///     to current.
        /// </summary>
        /// <param name="audioSource">the AS to play; if null, this fades the current non-diegetic background, if any</param>
        /// <param name="fadeInDuration">new source fades in over this duration</param>
        /// <param name="fadeOutDuration">if there is another non-diegetic background source active, fade it out over this duration</param>
        /// <param name="crossfadeDelay">if there is another non-diegetic background source active, start fading in the new source with this delay</param>
        /// <param name="interpolationFunction">interpolation function to fade with; UnityEngine.Mathf.Lerp by default</param>
        void PlayNonDiegeticBackgroundSource(
            UnityEngine.AudioSource audioSource,
            float fadeInDuration = 0.0f,
            float fadeOutDuration = 0.0f,
            float crossfadeDelay = 0.0f,
            System.Func<float,float,float,float> interpolationFunction = null
        );
    }
}