using UnityEngine;

namespace RPG.Managers.PersistentManagers.ClientSequences.AdditiveSceneSequence {
    /// <summary>
    ///     See comments in AdditiveSceneRoot.Awake
    /// </summary>
    public class AdditiveSceneMonoBehaviour : MonoBehaviour {
        protected bool canInterop = false;
        protected virtual void OnEnable() {
            if(canInterop) StartInterop();
        }
        protected virtual void Start() {
            canInterop = true;
            StartInterop();
        }
        protected virtual void OnDisable() {
            if (canInterop) StopInterop();
        }
        protected virtual void StartInterop() {
            // does nothing by default
        }
        protected virtual void StopInterop() {
            // does nothing by default
        }
    }
}