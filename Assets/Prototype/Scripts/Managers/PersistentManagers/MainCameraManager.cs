using System.Collections.Generic;

namespace RPG.Managers.PersistentManagers {
    public class MainCameraManager : PersistentManager<MainCameraManager, IMainCameraManager>, IMainCameraManager {
        public UnityEngine.Camera ActiveCamera { get; private set; }
        public void RegisterMainCamera(UnityEngine.Camera cam) {
            if (!cam.isActiveAndEnabled) {
                throw new System.Exception(
                    $"Camera in '{cam.gameObject.scene.path}' must be active and" +
                    $" enabled in order to be registered as active!");
            }
            if (cam == ActiveCamera) return;
            if (ActiveCamera != null && ActiveCamera.isActiveAndEnabled) {
                // This used to do this:
                // ActiveCamera.gameObject.SetActive(false);
                // But in the current impl object activity is handled by additive
                // scene flow. It's possible we'll revisit this if we want to
                // switch cameras on the go, but until then catching flow weirdness
                // is valuable:
                throw new System.Exception(
                    $"Camera previously active in" +
                    $" '{ActiveCamera.gameObject.scene.path}' was not deactivated" +
                    $" before registering new active camera in" +
                    $" '{cam.gameObject.scene.path}'!");
            }
            ActiveCamera = cam;
        }
    }
}