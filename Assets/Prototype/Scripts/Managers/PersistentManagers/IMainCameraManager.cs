using RPG.Utils;

namespace RPG.Managers.PersistentManagers {
    public interface IMainCameraManager {
        /// <summary>
        ///     Call this when a "main" scene Camera becomes active and enabled.
        ///     IMainCameraManager will make sure only one Camera is
        ///     active at a time.
        /// </summary>
        void RegisterMainCamera(UnityEngine.Camera al);
        UnityEngine.Camera ActiveCamera { get; }
    }
}