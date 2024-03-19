namespace RPG.Managers
{
    /// <summary>
    ///     This type of component is supposed to start with the game and never
    ///     be disabled during runtime until the game shuts down.
    ///     
    ///     E.g. app state manager, or input manager.
    ///     
    ///     There can be only one of these in memory, so it makes sense to expose
    ///     them in a singleton-esque manner (this should not prevent testing as
    ///     long as such a class implements an interface, and references to these
    ///     can be injected into dependants).
    ///     
    ///     These all should probably sit on a special game object in the startup
    ///     scene and be configured from there.
    ///     
    ///     Then, in a build, accessing a non-initialized static instance of this
    ///     means there is a bug in initialization logic.
    ///     
    ///     Previous version allowed accessing (and auto-initializing) these in
    ///     editor play mode when startup scene is not present. However, this
    ///     potentially introduces an additional initialization codepath into every
    ///     PersistentManager. Instead of this, now PersistentManager-s rely on
    ///     being explicitly created in Awake (of the startup scene).
    /// </summary>
    public class PersistentManager<TSelf, TInterface> :
        UnityEngine.MonoBehaviour
        where TSelf : PersistentManager<TSelf, TInterface>, TInterface
        where TInterface : class
    {
        private static readonly object threadLock = new object();
        private static TSelf instance = null;
        public static TSelf Instance {
            get {
                if (instance == null) {
                    string _err = (
                        "[PersistentManager] Instance"
                        +$" '{typeof(TSelf)} : {typeof(TInterface)}' accessed"
                        + " before assignment in Awake. This should not happen." );
#if UNITY_EDITOR
                    _err += (
                        " If you're trying to enter play mode with an additive"
                        + " scene in the editor, make sure it has an"
                        + " AdditiveSceneRoot component to initialize the game"
                        + " correctly.");
#endif
                    UnityEngine.Debug.LogError(_err);
                    throw new System.Exception(_err);
                }
                return instance;
            }
        }
        public static bool InstanceReady { get { return instance != null; } }
        private void SetInstance(TSelf value) {
            lock (threadLock) {
                if (instance != null) {
                    string _err = (
                        "[PersistentManager] Instance"
                        +$" '{typeof(TSelf)} : {typeof(TInterface)}' already"
                        + " assigned. This should not happen." );
                    UnityEngine.Debug.LogError(_err, instance.gameObject);
                    throw new System.Exception(_err);
                }
                instance = value;
            }
        }

        protected virtual void Awake() {
            SetInstance((TSelf)this);
        }
    }
}