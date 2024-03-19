using UnityEngine;
using RPG.Managers.PersistentManagers.ClientSequences.AdditiveSceneSequence;
using RPG.Managers.PersistentManagers;

namespace RPG.SceneScripts {
    public class PlayNonDiegeticBackgroundSourceOnStartInterop : AdditiveSceneMonoBehaviour {
        protected override void StartInterop() {
            var _as = GetComponent<AudioSource>();
            if (_as == null) return;
            AudioSourceManager.Instance.PlayNonDiegeticBackgroundSource(
                AudioSourceManager.Instance.CreateManagedAudioSourceFromTemplate(_as));
        }
    }
}