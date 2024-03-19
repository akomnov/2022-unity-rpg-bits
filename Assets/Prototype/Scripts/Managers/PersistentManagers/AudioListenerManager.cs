using System.Collections.Generic;

namespace RPG.Managers.PersistentManagers {
    public class AudioListenerManager :
        PersistentManager<AudioListenerManager, IAudioListenerManager>,
        IAudioListenerManager
    {
        public UnityEngine.AudioListener ActiveAudioListener { get; private set; }
        public void RegisterActiveAudioListener(UnityEngine.AudioListener al) {
            if (!al.isActiveAndEnabled) {
                throw new System.Exception(
                    $"AudioListener in '{al.gameObject.scene.path}' must be active" +
                    $" and enabled in order to be registered as active!");
            }
            if (al == ActiveAudioListener) return;
            if (ActiveAudioListener != null && ActiveAudioListener.isActiveAndEnabled) {
                // This used to do this:
                // ActiveAudioListener.gameObject.SetActive(false);
                // But in the current impl object activity is handled by additive
                // scene flow. It's possible we'll revisit this if we want to
                // switch ALs on the go, but until then catching flow weirdness
                // is more valuable:
                throw new System.Exception(
                    $"AudioListener previously active in" +
                    $" '{ActiveAudioListener.gameObject.scene.path}' was not" +
                    $" deactivated before registering new active AudioListener in" +
                    $" '{al.gameObject.scene.path}'!");
            }
            ActiveAudioListener = al;
        }
    }
}