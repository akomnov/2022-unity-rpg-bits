using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPG.Managers.PersistentManagers.ClientSequences.AdditiveSceneSequence {
    /// <summary>
    ///     This component reflects the specifics of how we work with additive
    ///     scenes.
    ///     It's assumed that this sits on a root GO in an additive scene
    ///     (see AdditiveSceneSequenceBase::PerformLoading).
    ///     
    ///     When entering editor play mode, gameplay is not supposed to jump to
    ///     currently loaded scene - instead, normal campaign flow starts. This
    ///     is done so that managers have a single entry point, and to minimize
    ///     editor-specific code.
    ///     
    ///     Also, this attempts to take care of Camera and AudioListener management
    ///     when the scene activates.
    /// </summary>
    [DisallowMultipleComponent]
    public class AdditiveSceneRoot : MonoBehaviour {
        public const int StartupSceneBuildIndex = 0;

        [SerializeField] private Camera mainCamera;
        [SerializeField] private AudioListener activeAudioListener;

#if UNITY_EDITOR
        private void Reset() {
            mainCamera = GetComponentInChildren<Camera>(includeInactive: false);
            activeAudioListener = GetComponentInChildren<AudioListener>(includeInactive: false);
        }
#endif

        private void Awake() {
#if UNITY_EDITOR
            if(gameObject.scene.isDirty) Debug.LogWarning(
                $"'{gameObject.scene.path}' has unsaved changes. Since we're going" +
                $" to load scenes from disk, you won't see them during gameplay.");
            // Test for correct AdditiveSceneRoot placement:
            var _gos = gameObject.scene.GetRootGameObjects();
            if (
                _gos.Length != 1
                || _gos[0] != gameObject
                || GetComponentsInChildren<AdditiveSceneRoot>(includeInactive: true).Length > 1
            ) throw new Exception(
                $"'{gameObject.scene.path}' must have exactly 1 active root GO with" +
                " AdditiveSceneRoot component in order to be additively loaded." );
            // Test for correct main camera assignment:
            {
                bool _activePresent = false;
                bool _matchesActive = false;
                bool _extraActive = false;
                foreach(var _act in GetComponentsInChildren<Camera>(includeInactive:false)) {
                    _activePresent = true;
                    if(mainCamera == _act) {
                        _matchesActive = true;
                    } else {
                        _extraActive = true;
                    }
                }
                if (
                    !_matchesActive
                    && _activePresent
                ) throw new Exception(
                    $"'{gameObject.scene}' contains active cameras but none are" +
                    " referenced by mainCamera. Pick one or reset this component" +
                    " to auto-pick.");
                if (_extraActive) Debug.LogWarning(
                    $"'{gameObject.scene}' contains active cameras not referenced" +
                    " by mainCamera. Hope these are disabled in Start somewhere!",
                    this);
            }
            // Test for correct active audiolistener assignment:
            {
                bool _activePresent = false;
                bool _matchesActive = false;
                bool _extraActive = false;
                foreach (var _act in GetComponentsInChildren<AudioListener>(includeInactive: false)) {
                    _activePresent = true;
                    if (activeAudioListener == _act) {
                        _matchesActive = true;
                    } else {
                        _extraActive = true;
                    }
                }
                if (
                    !_matchesActive
                    && _activePresent
                ) throw new Exception(
                    $"'{gameObject.scene}' contains active audiolisteners but none" +
                    " are referenced by activeAudioListener. Pick one or reset" +
                    " this component to auto-pick.");
                if (_extraActive) Debug.LogWarning(
                    $"'{gameObject.scene}' contains active audiolisteners not" +
                    " referenced by activeAudioListener. Hope these are disabled" +
                    " in Start somewhere!",
                    this);
            }
            // Test for Startup load QOL shortcut:
            if (gameObject.scene == SceneManager.GetActiveScene()) {
                // We'll end up in this branch only if entering play mode with
                // an additive scene loaded in the editor.
                // If there's no Startup scene in the current setup, load it (in
                // the next frame) so that managers are initialized properly.
                // This is merely a QOL thing so that devs don't have to have
                // Startup scene loaded at all times in the editor.
                bool _startupPresent = false;
                ScriptableObjects.Destination _dest = null;
                for (int _idx = SceneManager.sceneCount - 1; _idx >= 0; --_idx) {
                    var _sc = SceneManager.GetSceneAt(_idx);
                    if (_sc.buildIndex == StartupSceneBuildIndex) _startupPresent = true;
                    // Another QOL thing: if destinations exist in loaded scenes,
                    // pick the first one and override campaign start with it.
                    if(_dest == null && ClientSequenceManager.CampaignStartOverride == null) {
                        foreach (var _go in _sc.GetRootGameObjects()) {
                            var _destGO = _go.GetComponentInChildren<RPG.Components.LevelDesign.Destination>();
                            _dest = _destGO?.DestinationSO;
                            if (_dest != null) {
                                Debug.LogWarning(
                                    $"'{_dest}' from '{_go.scene.path}' was picked" +
                                    $" to override starting destination of the" +
                                    $" campaign.", _destGO);
                                break;
                            }
                        }
                    }
                    if (_startupPresent
                        && (_dest != null || ClientSequenceManager.CampaignStartOverride != null)) break;
                }
                if (_dest != null && ClientSequenceManager.CampaignStartOverride == null)
                    ClientSequenceManager.CampaignStartOverride = _dest;
                if (!_startupPresent) SceneManager.LoadScene(
                    StartupSceneBuildIndex,
                    LoadSceneMode.Additive );
            }
#endif
            // Disable the whole scene as soon as possible so it can't do anything
            // meaningful until it's activated by the manager:
            gameObject.SetActive(false);
            // Note that this leads to the following Unity messages being sent
            // to scripts in additive scenes:
            // During loading (in this order for each script but with no order
            // between scripts!): Awake, OnEnable (skipped for root), OnDisable
            // During activation: OnEnable for every script, then Start for every
            // script.
            // This means that only after Start has been first called you can rely
            // on neighbours' initialization being done!
            // For this reason it's advised to use AdditiveSceneMonoBehaviour as
            // base Unity behaviour type instead of MonoBehaviour.
        }
        private Camera cameraDeactivatedDueToSkip;
        public void PerformActivation(bool skipCamera = false) {
            cameraDeactivatedDueToSkip = null;
            gameObject.SetActive(true);
            if (mainCamera != null && mainCamera.isActiveAndEnabled) {
                if (skipCamera) {
                    cameraDeactivatedDueToSkip = mainCamera;
                    cameraDeactivatedDueToSkip.gameObject.SetActive(false);
                } else {
                    var _mcm = MainCameraManager.Instance;
                    if (_mcm != null) _mcm.RegisterMainCamera(mainCamera);
                }
            }
            if (activeAudioListener != null && activeAudioListener.isActiveAndEnabled) {
                var _alm = AudioListenerManager.Instance;
                if (_alm != null) _alm.RegisterActiveAudioListener(activeAudioListener);
            }
        }
        public void PerformDeactivation() {
            gameObject.SetActive(false);
            if (cameraDeactivatedDueToSkip != null) cameraDeactivatedDueToSkip.gameObject.SetActive(true);
            cameraDeactivatedDueToSkip = null;
        }
    }
}