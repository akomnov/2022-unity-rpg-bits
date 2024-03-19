namespace RPG.SceneScripts
{
    public class LifeCycleTest : UnityEngine.MonoBehaviour
    {
        public void Awake()
        {
            UnityEngine.Debug.Log("[LifeCycleTest] Awake", this);
        }

        public void OnEnable()
        {
            UnityEngine.Debug.Log("[LifeCycleTest] OnEnable", this);
        }

        public void Reset()
        {
            UnityEngine.Debug.Log("[LifeCycleTest] Reset", this);
        }

        public void Start()
        {
            UnityEngine.Debug.Log("[LifeCycleTest] Start", this);
        }

        public void OnApplicationPause()
        {
            UnityEngine.Debug.Log("[LifeCycleTest] OnApplicationPause", this);
        }

        public void OnApplicationQuit()
        {
            UnityEngine.Debug.Log("[LifeCycleTest] OnApplicationQuit", this);
        }

        public void OnDisable()
        {
            UnityEngine.Debug.Log("[LifeCycleTest] OnDisable", this);
        }

        public void OnDestroy()
        {
            UnityEngine.Debug.Log("[LifeCycleTest] OnDestroy", this);
        }
    }
}
