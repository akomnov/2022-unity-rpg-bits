namespace RPG.Managers.PersistentManagers
{
    [System.Serializable]
    public class AudioSourceManager : PersistentManager<AudioSourceManager, IAudioSourceManager>, IAudioSourceManager
    {
        private class AudioSourceFade
        {
            public UnityEngine.AudioSource source;
            public UnityEngine.AudioSource target;
            public readonly float sourceVolume;
            public readonly float targetVolume;
            public readonly System.Func<float, float, float, float> interpolationFunction;
            public readonly float fadeInDuration;
            public readonly float fadeOutDuration;
            public readonly float crossfadeDelay;
            public readonly double startT;
            private bool sourceDone = false;
            private bool targetDone = false;
            public AudioSourceFade(
                UnityEngine.AudioSource source,
                UnityEngine.AudioSource target,
                float fadeInDuration = 0.0f,
                float fadeOutDuration = 0.0f,
                float crossfadeDelay = 0.0f,
                System.Func<float, float, float, float> interpolationFunction = null
            )
            {
                if (source == null && target == null)
                    throw new System.Exception("Tried to create AudioSourceFade from null to null");
                startT = UnityEngine.AudioSettings.dspTime;
                this.source = source;
                sourceVolume = 0.0f;
                this.target = target;
                targetVolume = 0.0f;
                this.interpolationFunction = interpolationFunction ?? UnityEngine.Mathf.Lerp;
                this.fadeInDuration = UnityEngine.Mathf.Max(fadeInDuration, 0.0f);
                this.fadeOutDuration = UnityEngine.Mathf.Max(fadeOutDuration, 0.0f);
                this.crossfadeDelay = UnityEngine.Mathf.Max(crossfadeDelay, 0.0f);
                if (this.source != null)
                {
                    sourceVolume = this.source.volume;
                }
                if (this.target != null)
                {
                    targetVolume = this.target.volume;
                    SetupTarget();
                }
            }
            private void SetupTarget()
            {
                if (target == null) return;
                target.volume = (fadeInDuration == 0.0f) ? targetVolume : 0.0f;
                if (source == null)
                {
                    target.Play();
                }
                else
                {
                    target.PlayScheduled(startT + crossfadeDelay);
                }
            }
            public bool ChangeTarget(UnityEngine.AudioSource target)
            {
                if (this.target == target) return false;
                if (this.target == null && target == null) return false;
                this.target = target;
                // @akomnov: probably ok with PlayScheduled not respecting time already elapsed?
                // switching targets on the fly is not supposed to be pretty anyway
                SetupTarget();
                return true;
            }
            public bool Update()
            {
                double dT = UnityEngine.AudioSettings.dspTime - startT;
                if (source == null) sourceDone = true;
                if (! sourceDone)
                {
                    if (dT >= fadeOutDuration)
                    {
                        source.volume = 0.0f;
                        source.Stop();
                        sourceDone = true;
                    }
                    else
                    {
                        source.volume = interpolationFunction(sourceVolume, 0.0f, (float)dT);
                    }
                }
                if (target == null) targetDone = true;
                if (! targetDone)
                {
                    if (dT >= crossfadeDelay + fadeInDuration)
                    {
                        target.volume = targetVolume;
                        targetDone = true;
                    }
                    else
                    {
                        source.volume = interpolationFunction(0.0f, targetVolume, (float)(dT - crossfadeDelay));
                    }
                }
                return ! (sourceDone && targetDone);
            }
        }

        private UnityEngine.AudioSource nonDiegeticBackground = null;
        private AudioSourceFade nonDiegeticBackgroundFade = null;
        public UnityEngine.AudioSource CreateManagedAudioSourceFromTemplate(
            UnityEngine.AudioSource template,
            bool disableTemplate = true
        )
        {
            if (disableTemplate) template.enabled = false;
            var result = gameObject.AddComponent<UnityEngine.AudioSource>();
            result.clip = template.clip;
            result.volume = template.volume;
            result.bypassEffects = template.bypassEffects;
            result.pitch = template.pitch;
            result.bypassListenerEffects = template.bypassListenerEffects;
            result.bypassReverbZones = template.bypassReverbZones;
            result.ignoreListenerPause = template.ignoreListenerPause;
            result.ignoreListenerVolume = template.ignoreListenerVolume;
            result.loop = template.loop;
            result.outputAudioMixerGroup = template.outputAudioMixerGroup;
            result.priority = template.priority;
            result.tag = template.tag;
            return result;
        }
        private bool NonDiegeticBackgroundIsPlaying()
        {
            return nonDiegeticBackground != null
                    && nonDiegeticBackground.isActiveAndEnabled
                    && nonDiegeticBackground.isPlaying;
        }
        public void PlayNonDiegeticBackgroundSource(
            UnityEngine.AudioSource audioSource,
            float fadeInDuration = 0.0f,
            float fadeOutDuration = 0.0f,
            float crossfadeDelay = 0.0f,
            System.Func<float, float, float, float> interpolationFunction = null
        )
        {
            if (audioSource != null)
            {
                if (! audioSource.isActiveAndEnabled)
                {
                    UnityEngine.Debug.Log(
                        "Requested to play '" + audioSource.clip.name.ToString() + "'"
                        + " as non-diegetic background while it's inactive or disabled, ignoring!"
                    );
                    return;
                }
                if (NonDiegeticBackgroundIsPlaying() && nonDiegeticBackground.clip == audioSource.clip)
                {
                    UnityEngine.Debug.Log(
                        "Requested to play '" + audioSource.clip.name.ToString() + "'"
                        + " as non-diegetic background while it's already playing as such, ignoring!"
                    );
                    return;
                    // @akomnov: might make sense to add an option to crossfade with
                    // self, e.g. to restart a clip without a sudden jump. Then
                    // again, to keep things simple, maybe it's better to add a
                    // Restart method, which fades the source out, restarts it 
                    // and fades it in without overlap - and therefore without 
                    // the need to duplicate the source component - and use that.
                }
                if (nonDiegeticBackgroundFade != null && nonDiegeticBackgroundFade.target.clip == audioSource.clip)
                {
                    UnityEngine.Debug.Log(
                        "Requested to play '" + audioSource.clip.name.ToString() + "'"
                        + " as non-diegetic background while it's already fading in as such, ignoring!"
                    );
                    return;
                }
                if (audioSource.isPlaying)
                {
                    UnityEngine.Debug.Log(
                        "Requested to play '" + audioSource.clip.name.ToString() + "'"
                        + " as non-diegetic background while it's already playing unmanaged, ignoring!"
                    );
                    return;
                }
            }
            else
            {
                if (! NonDiegeticBackgroundIsPlaying()) return;
            }
            if (nonDiegeticBackgroundFade == null)
            {
                nonDiegeticBackgroundFade = new AudioSourceFade(
                    source : NonDiegeticBackgroundIsPlaying() ? nonDiegeticBackground : null,
                    target : audioSource,
                    fadeInDuration,
                    fadeOutDuration,
                    crossfadeDelay,
                    interpolationFunction
                );
            }
            else
            {
                // @akomnov: if already transitioning, notify and switch the transition
                // target; to fail here would probably be incorrent even in production.
                UnityEngine.Debug.Log(
                    "Requested to play '" + audioSource.clip.name.ToString() + "'"
                    + " as non-diegetic background while '" + nonDiegeticBackgroundFade.target.clip.name.ToString() + "'"
                    + " is already fading in, switching!");
                var _oldTarget = nonDiegeticBackgroundFade.target;
                if (
                    nonDiegeticBackgroundFade.ChangeTarget(audioSource)
                    && _oldTarget.gameObject == gameObject
                ) Destroy(_oldTarget);
            }
        }
        private void Update()
        {
            if (nonDiegeticBackgroundFade != null)
            {
                if (! nonDiegeticBackgroundFade.Update())
                {
                    nonDiegeticBackground = nonDiegeticBackgroundFade.target;
                    if (
                        nonDiegeticBackgroundFade.source != null
                        && nonDiegeticBackgroundFade.source.gameObject == gameObject
                    ) Destroy(nonDiegeticBackgroundFade.source);
                    nonDiegeticBackgroundFade = null;
                }
            }
        }
    }
}