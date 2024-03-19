using System;
using System.Collections;
using RPG.Managers.PersistentManagers.ClientSequences;
using static RPG.Core.Shared.Utils.Functions;
using RPG.Utils;

namespace RPG.Managers.PersistentManagers
{
    /// <summary>
    ///     Defines "intro - main menu - loading - campaign" flow.
    ///     
    ///     Should not be ever disabled during runtime.
    /// </summary>
    ///
    /// TODO can't see shit in the editor due to sequences having an abstract base
    ///     might solve this via SO-s
    ///         or not
    ///         let's see how much pain this brings
    ///
    [System.Serializable] public class ClientSequenceManager
        : PersistentManager<ClientSequenceManager, IClientSequenceManager>, IClientSequenceManager
    {
        private IntroSequence introSequence = null;
        public IntroSequence IntroSequence { get { return introSequence; } private set { introSequence = value; } }
        [UnityEngine.SerializeField] private LazySceneRef introSequenceScene = null;

        private MainMenuSequence mainMenuSequence = null;
        public MainMenuSequence MainMenuSequence { get { return mainMenuSequence; } private set { mainMenuSequence = value; } }
        [UnityEngine.SerializeField] private LazySceneRef mainMenuSequenceScene = null;

        private LoadingSequence loadingSequence = null;
        public LoadingSequence LoadingSequence { get { return loadingSequence; } private set { loadingSequence = value; } }
        [UnityEngine.SerializeField] private LazySceneRef loadingSequenceScene = null;

        private CampaignSequence campaignSequence = null;
        public CampaignSequence CampaignSequence { get { return campaignSequence; } private set { campaignSequence = value; } }
        [UnityEngine.SerializeField] private ScriptableObjects.Destination campaignStart = null;
#if UNITY_EDITOR
        public static ScriptableObjects.Destination CampaignStartOverride;
#endif

        private CurtainSequence curtainSequence = null;
        public CurtainSequence CurtainSequence { get { return curtainSequence; } private set { curtainSequence = value; } }
        [UnityEngine.SerializeField] private LazySceneRef curtainSequenceScene = null;

        [UnityEngine.SerializeField] private UnityEngine.GameObject playerPrefab;
        public UnityEngine.GameObject PlayerPrefab { get => playerPrefab; private set { playerPrefab = value; } }

        protected override void Awake() {
            base.Awake();
            if (PlayerPrefab == null) {
                throw new System.Exception("ClientSequenceManager: Player prefab is not set");
            }
#if UNITY_EDITOR
            if (CampaignStartOverride != null) campaignStart = CampaignStartOverride;
#endif
        }
        protected void Start() {
            { // Set up curtain seq and start loading it.
                IEnumerator _coroutine() {
                    var _seq_id = new AdditiveSceneSequenceBase.Id();
                    Exception _e = null;
                    yield return CoroutineAndCallBack(
                        AdditiveSceneSequenceBase.Id.FromUncachedSceneRef(curtainSequenceScene),
                        (e, r) => { _e = e; _seq_id = (AdditiveSceneSequenceBase.Id)r; } );
                    if (_e != null)
                        throw _e;
                    if (!_seq_id.Initialized)
                        throw new Exception(
                            $"Id.FromUncachedSceneRef({curtainSequenceScene}) did not throw but did not produce a valid id either");
                    CurtainSequence = new CurtainSequence(this, _seq_id);
                    yield return CurtainSequence.Load();
                }
                StartCoroutine(
                    CoroutineAndCallBack(
                        _coroutine(),
                        HandleUnrecoverableException ) );
            }
            {
                IEnumerator _coroutine() {
                    IEnumerator _main_menu_coroutine() {
                        var _seq_id = new AdditiveSceneSequenceBase.Id();
                        Exception _e = null;
                        yield return CoroutineAndCallBack(
                            AdditiveSceneSequenceBase.Id.FromUncachedSceneRef(mainMenuSequenceScene),
                            (e, r) => { _e = e; _seq_id = (AdditiveSceneSequenceBase.Id)r; });
                        if (_e != null)
                            throw _e;
                        if (!_seq_id.Initialized)
                            throw new Exception(
                                $"Id.FromUncachedSceneRef({mainMenuSequenceScene}) did not throw but did not produce a valid id either");
                        MainMenuSequence = new MainMenuSequence(this, _seq_id);
                        yield return MainMenuSequence.Load();
                    }
                    IEnumerator _loading_coroutine() {
                        var _seq_id = new AdditiveSceneSequenceBase.Id();
                        Exception _e = null;
                        yield return CoroutineAndCallBack(
                            AdditiveSceneSequenceBase.Id.FromUncachedSceneRef(loadingSequenceScene),
                            (e, r) => { _e = e; _seq_id = (AdditiveSceneSequenceBase.Id)r; });
                        if (_e != null)
                            throw _e;
                        if (!_seq_id.Initialized)
                            throw new Exception(
                                $"Id.FromUncachedSceneRef({loadingSequenceScene}) did not throw but did not produce a valid id either");
                        LoadingSequence = new LoadingSequence(this, _seq_id);
                        yield return LoadingSequence.Load();
                    }
                    IEnumerator _intro_coroutine() {
                        var _seq_id = new AdditiveSceneSequenceBase.Id();
                        Exception _e = null;
                        yield return CoroutineAndCallBack(
                            AdditiveSceneSequenceBase.Id.FromUncachedSceneRef(introSequenceScene),
                            (e, r) => { _e = e; _seq_id = (AdditiveSceneSequenceBase.Id)r; });
                        if (_e != null)
                            throw _e;
                        if (!_seq_id.Initialized)
                            throw new Exception(
                                $"Id.FromUncachedSceneRef({introSequenceScene}) did not throw but did not produce a valid id either");
                        IntroSequence = new IntroSequence(this, _seq_id);
                        yield return IntroSequence.Load();
                        IntroSequence.Activate();
                    }
                    // Set up intro seq and start loading it. Activate when done.
                    // While intro is loading/playing, set up and load main menu and loading screen.
                    Exception _intro_e = null;
                    Exception _main_menu_e = null;
                    Exception _loading_e = null;
                    var _intro = StartCoroutine(
                        CoroutineAndCallBack(
                            _intro_coroutine(),
                            (e, r) => { _intro_e = e; }));
                    var _main_menu = StartCoroutine(
                        CoroutineAndCallBack(
                            _main_menu_coroutine(),
                            (e, r) => { _main_menu_e = e; }));
                    var _loading = StartCoroutine(
                        CoroutineAndCallBack(
                            _loading_coroutine(),
                            (e, r) => { _loading_e = e; }));
                    // We'd like both to be ready before we show the main menu.
                    yield return _main_menu;
                    if (_main_menu_e != null)
                        throw _main_menu_e;
                    yield return _loading;
                    if (_loading_e != null)
                        throw _loading_e;
                    yield return _intro;
                    if (_intro_e != null)
                        throw _intro_e;
                    IntroSequence.OnNextSequenceReady();
                    { // Start main menu and unload intro as soon as the former is ready to be shown and the latter is done.
                        void _cb(object sender, EventArgs _) {
                            if (!IntroSequence.IsIntroDone) return;
                            if (!IntroSequence.IsNextSequenceReady) return;
                            IntroSequence.IntroDone -= _cb;
                            IntroSequence.NextSequenceReady -= _cb;
                            IntroSequence.Deactivate();
                            MainMenuSequence.Activate();
                            StartCoroutine(
                                CoroutineAndCallBack(
                                    IntroSequence.Unload(),
                                    HandleUnrecoverableException));
                        }
                        IntroSequence.IntroDone += _cb;
                        IntroSequence.NextSequenceReady += _cb;
                    }
                }
                StartCoroutine(
                    CoroutineAndCallBack(
                        _coroutine(),
                        HandleUnrecoverableException ) );
            }
        }

        public void NewGame(object sender) {
            MainMenuSequence.Deactivate();
            StartCoroutine(
                CoroutineAndCallBack(
                    MainMenuSequence.Unload(),
                    HandleUnrecoverableException ) );
            LoadingSequence.Reset();
            LoadingSequence.Activate();
            { // create a CampaignSequence instance and start loading it
                // @akomnov TODO: would be nice to store campaignStart in the default profile SO,
                // so that it could be redefined in local override files.
                // Don't have the infra yet, therefore this.
                IEnumerator _coroutine(
                    Action<float> progressCallback = null
                ) {
                    var _seq_id = new AdditiveSceneSequenceBase.Id();
                    Exception _e = null;
                    yield return CoroutineAndCallBack(
                        campaignStart.CalculateSequenceId(),
                        (e, r) => { _e = e; _seq_id = (AdditiveSceneSequenceBase.Id)r; } );
                    if (_e != null)
                        throw _e;
                    if (!_seq_id.Initialized)
                        throw new Exception(
                            $"{campaignStart}.CalculateSequenceId did not throw but did not produce a valid id either" );
                    CampaignSequence = new CampaignSequence(this, new AreaSequence(this, _seq_id));
                    // TODO teleport to campaignStart once player is separate from starting scene
                    yield return CampaignSequence.Load(
                        (p) => LoadingSequence.NextSequenceProgress = p );
                }
                StartCoroutine(
                    CoroutineAndCallBack(
                        _coroutine(),
                        (e, r) => {
                            if (HandleUnrecoverableException(e)) return;
                            LoadingSequence.OnNextSequenceReady(); } ) );
            }
            { // as soon as loading screen is ready to fade out, activate the campaign sequence
                void _cb(object _s, EventArgs _) {
                    // TODO add "press any button" prompt
                    LoadingSequence.NextSequenceReady -= _cb;
                    LoadingSequence.Deactivate();
                    CampaignSequence.Activate();
                    StartCoroutine(
                        CoroutineAndCallBack(
                            LoadingSequence.Unload(),
                            HandleUnrecoverableException ) );
                }
                LoadingSequence.NextSequenceReady += _cb;
            }
        }

        #region Misc utils
        /// <summary>
        ///     Wrapper for directly using this method as CoroutineAndCallBack's onDone.
        /// </summary>
        public void HandleUnrecoverableException(System.Exception e, object current) {
            HandleUnrecoverableException(e);
        }

        public bool HandleUnrecoverableException(System.Exception e) {
            if (e == null) return false;
            UnityEngine.Debug.LogException(e, this);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPaused = true;
#else
            UnityEngine.Application.Quit();
#endif
            return true;
        }

        #endregion

#region IGameSequenceManager impl
        public IEnumerator MakeSureSequenceIsLoaded(IGameSequence seq) {
            while (seq.State == GameSequence.State.UNLOADING || seq.State == GameSequence.State.LOADING) {
                yield return null;
            }
            if (seq.State == GameSequence.State.UNLOADED)
                yield return seq.Load();
        }
        #endregion

        private void Update()
        {
            if (
                CampaignSequence != null
                && CampaignSequence.State == GameSequence.State.ACTIVE
                && !CampaignSequence.IsPaused
            ) CampaignSequence.Update();
        }
        protected void FixedUpdate()
        {
            CampaignSequence?.FixedUpdate();
        }

    }
}