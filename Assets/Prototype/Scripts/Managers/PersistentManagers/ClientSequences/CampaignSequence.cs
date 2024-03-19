using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static RPG.Core.Shared.Utils.Functions;

namespace RPG.Managers.PersistentManagers.ClientSequences
{
    /// <summary>
    ///     Lifetime of this sequence corresponds to the lifetime of the in-memory
    ///     state of player's campaign progression.
    ///     State of specific scenes is to be factored out into AreaSequence states
    ///     and stored as areaSequences entries.
    /// </summary>
    [System.Serializable]
    public partial class CampaignSequence : IClientSequence
    {
        protected IClientSequenceManager manager = null;
        public GameEvents CampaignEvents { get; protected set; }
        public CombatHandler CampaignCombat { get; protected set; }
        public SpeakerHandler CampaignConvoLog { get; protected set; }

        public CampaignSequence(IClientSequenceManager manager, AreaSequence areaSequence)
        {
            this.manager = manager;
            currentAreaSequenceId = RegisterSequence(areaSequence).id;
            CampaignEvents = new GameEvents();
            CampaignCombat = new CombatHandler();
            CampaignConvoLog = new SpeakerHandler();
        }

        #region area sequences

        private AdditiveSceneSequenceBase.Id currentAreaSequenceId = default;

        /// <summary>
        ///     Since AreaSequences hold game progression state (specific to said areas),
        ///     this dict should only be added to as long as a campaign session lasts.
        ///     Not a set to hopefully catch more bugs (like dangling sequences).
        /// </summary>
        private Dictionary<AdditiveSceneSequenceBase.Id, AreaSequence> areaSequences = new Dictionary<AdditiveSceneSequenceBase.Id, AreaSequence>();

        private AreaSequence RegisterSequence(AreaSequence seq) {
            if (areaSequences.ContainsKey(seq.id))
                return areaSequences[seq.id];
            // FIXME: this does not look ok with multiple loaded sequences
            seq.StateChanged += (object sender, GameSequence.StateChangedEventArgs e) => stateChanged?.Invoke(this, e);
            areaSequences.Add(seq.id, seq);
            return seq;
        }

        private AreaSequence RegisterSequence(AdditiveSceneSequenceBase.Id seq_id) {
            if (areaSequences.ContainsKey(seq_id))
                return areaSequences[seq_id];
            return RegisterSequence(new AreaSequence(manager, seq_id));
        }

        public AreaSequence GetCurrentAreaSequence() {
            if (!currentAreaSequenceId.Initialized || areaSequences == null)
                return null;
            return areaSequences[currentAreaSequenceId];
        }

        #endregion

        #region IClientSequence impl

        public GameSequence.State State {
            get {
                var _currentAreaSequence = GetCurrentAreaSequence();
                if (_currentAreaSequence == null)
                    return GameSequence.State.UNLOADED;
                return _currentAreaSequence.State;
            }
        }

        protected System.EventHandler<GameSequence.StateChangedEventArgs> stateChanged;

        public event System.EventHandler<GameSequence.StateChangedEventArgs> StateChanged
        {
            add
            {
                stateChanged += value;
                value(this, new GameSequence.StateChangedEventArgs { oldState = State, newState = State });
            }
            remove { stateChanged -= value; }
        }

        public IEnumerator Load(Action<float> progressCallback = null) {
            yield return GetCurrentAreaSequence().Load(progressCallback);
        }

        public void Activate(bool skipCamera = false) {
            CampaignEvents.onCombatTriggerEnter += CampaignCombat.BeginCombat;
            GetCurrentAreaSequence().Activate(skipCamera);
        }

        public void Deactivate() {
            CampaignCombat.ExitCombat();
            CampaignEvents.onCombatTriggerEnter -= CampaignCombat.BeginCombat;
            GetCurrentAreaSequence().Deactivate();
        }

        public void Reset() {
            sentientActors.Clear();
            GetCurrentAreaSequence().Reset();
            // @akomnov: note that this does not reset non-current AreaSequence's, which may or may not be
            // what we want. Then again right now I'm not sure why would we reset campaign sequence.
            // Moreover, we don't really have any transitory state here to speak of, beside destinations.
            // Point is, this is a stand-in implementation. TODO
        }

        public IEnumerator Unload() {
            yield return GetCurrentAreaSequence().Unload();
            // @akomnov: note that this does not make sure that non-current AreaSequence's are unloaded,
            // which is probably not what we want.
            // TODO revisit this when we have exiting into main menu or something.
        }

        #endregion

        #region area persistence

        // TODO Looks like this should be part of AreaSequence state. Revisit this when doing save/load.

        private Dictionary<string, Core.Shared.Campaign.AreaState> areaStates = new Dictionary<string, Core.Shared.Campaign.AreaState>();

        public Core.Shared.Campaign.AreaState GetAreaState(string key)
        {
            if (!areaStates.ContainsKey(key))
            {
                areaStates[key] = new Core.Shared.Campaign.AreaState();
            }
            return areaStates[key];
        }

        public Core.Shared.Campaign.AreaState GetAreaState(UnityEngine.GameObject go)
        {
            return GetAreaState(go.scene.path);
        }

        #endregion

        #region tile state shortcuts

        public Core.Shared.Campaign.AreaStateData.ITileState GetTileState(
            UnityEngine.Tilemaps.Tilemap tilemap,
            UnityEngine.Vector3Int location
        )
        {
            return GetAreaState(tilemap.gameObject)?.GetTileState(
                new Core.Shared.Campaign.AreaState.TilePointer
                {
                    tilemap = tilemap.gameObject.name,
                    location = (location.x, location.y, location.z)
                }
            );
        }

        public Core.Shared.Campaign.AreaStateData.ITileState GetTileState(
            UnityEngine.Tilemaps.ITilemap tilemap,
            UnityEngine.Vector3Int location
        )
        {
            return GetTileState(
                tilemap.GetComponent<UnityEngine.Tilemaps.Tilemap>(),
                location
            );
        }

        public Core.Shared.Campaign.AreaStateData.ITileState RegisterTileState(
            UnityEngine.Tilemaps.Tilemap tilemap,
            UnityEngine.Vector3Int location,
            Core.Shared.Campaign.AreaStateData.ITileState state
        )
        {
            return GetAreaState(tilemap.gameObject)?.RegisterTileState(
                new Core.Shared.Campaign.AreaState.TilePointer
                {
                    tilemap = tilemap.gameObject.name,
                    location = (location.x, location.y, location.z)
                },
                state
            );
        }

        public Core.Shared.Campaign.AreaStateData.ITileState RegisterTileState(
            UnityEngine.Tilemaps.ITilemap tilemap,
            UnityEngine.Vector3Int location,
            Core.Shared.Campaign.AreaStateData.ITileState state
        )
        {
            return RegisterTileState(
                tilemap.GetComponent<UnityEngine.Tilemaps.Tilemap>(),
                location,
                state
            );
        }

        #endregion

        #region teleports and destinations

        private Dictionary<ScriptableObjects.Destination, UnityEngine.GameObject> destinations = new Dictionary<ScriptableObjects.Destination, UnityEngine.GameObject>();

        public IEnumerator TeleportViaCurtain(ScriptableObjects.Destination dest, UnityEngine.GameObject player) {
            if (manager.CurtainSequence == null) throw new System.Exception("Curtain sequence is not initialized!");
            if (IsPaused) throw new System.Exception("Tried to teleport while paused - it's probably a TeleportViaCurtain re-entry bug!");
            Pause();
            var _dest_seq_id = new AdditiveSceneSequenceBase.Id();
            { // FIXME move this behind curtain activation
                Exception _e = null;
                yield return CoroutineAndCallBack(
                    dest.CalculateSequenceId(),
                    (e, r) => { _e = e; _dest_seq_id = (AdditiveSceneSequenceBase.Id)r; } );
                if (_e != null)
                    throw _e;
            }
            if (!_dest_seq_id.Initialized)
                throw new Exception($"{dest}.CalculateSequenceId did not throw but did not produce a valid id either");
            AreaSequence _dest_seq = RegisterSequence(_dest_seq_id);
            bool _curtain_lowered = false;
            UnityEngine.Coroutine _dest_seq_loading = null;
            Exception _dest_seq_loading_exc = null;
            if (_dest_seq.id != currentAreaSequenceId) { // load target seq if necessary
                _dest_seq_loading = manager.StartCoroutine(
                    CoroutineAndCallBack(
                        manager.MakeSureSequenceIsLoaded(_dest_seq),
                        (e, r) => { _dest_seq_loading_exc = e; } ) );
            }
            { // activate the curtain, upon which it starts to lower
                void _cb(object sender, GameSequence.StateChangedEventArgs e) {
                    if (e.newState != GameSequence.State.INACTIVE) return;
                    manager.CurtainSequence.StateChanged -= _cb;
                    manager.CurtainSequence.Activate(skipCamera: true);
                };
                manager.CurtainSequence.StateChanged += _cb;
            }
            { // when it's lowered, we're ready to teleport if other parts are ready
                void _cb(object sender, EventArgs e) {
                    manager.CurtainSequence.Lowered -= _cb;
                    _curtain_lowered = true;
                }
                manager.CurtainSequence.Lowered += _cb;
            }
            while (!_curtain_lowered)
                yield return null;
            if (_dest_seq_loading != null)
                yield return _dest_seq_loading;
            if (_dest_seq_loading_exc != null)
                throw _dest_seq_loading_exc;
            if (_dest_seq.id != currentAreaSequenceId) {
                GetCurrentAreaSequence().Reset();
                var _tagetScenePc = _dest_seq.GetComponentsInChildren<Controllers.PlayerController>(includeInactive: true).DefaultIfEmpty(null).First();
                if (_tagetScenePc != null) {
#if UNITY_EDITOR
                    // If a GO with a PlayerController is already there in the scene, use that instead...
                    player = _tagetScenePc.gameObject;
                    UnityEngine.Debug.LogWarning(
                        "Reusing player object already present in additive scene; don't forget to remove it when shipping!",
                        player
                    );
#else
                    // @akomnov: lmk if we want to be permissive here.
                    throw new System.Exception(
                        $"Additive scene already has a player GO (scene '{_dest_seq}'); bailing so we don't get any weirdness"
                    );
#endif
                } else {
                    // ...otherwise move the player object
                    _dest_seq.MoveObjectIntoSelf(player);
                }
                // Unload current sequence. Might revisit this later (e.g. unload scenes 2 teleports away or something).
                manager.StartCoroutine(
                    CoroutineAndCallBack(
                        GetCurrentAreaSequence().Unload(),
                        manager.HandleUnrecoverableException ) );
                currentAreaSequenceId = _dest_seq.id;
                GetCurrentAreaSequence().Activate();
                { // interop does not start right away, so we need to wait for dest to register:
                    bool _destRegistered = false;
                    void _cb(ScriptableObjects.Destination regDest) {
                        if (regDest != dest) return;
                        DestinationRegistered -= _cb;
                        _destRegistered = true;
                    }
                    DestinationRegistered += _cb;
                    while (!_destRegistered)
                        yield return null;
                }
            }
            player.transform.position = destinations[dest].transform.position;
            manager.CurtainSequence.OnReadyToLift(this);
            { // lift the curtain
                void _cb(object sender, EventArgs e) {
                    manager.CurtainSequence.Lifted -= _cb;
                    _curtain_lowered = false;
                }
                manager.CurtainSequence.Lifted += _cb;
            }
            while (_curtain_lowered)
                yield return null;
            manager.CurtainSequence.Reset();
            Unpause();
        }

        protected event Action<ScriptableObjects.Destination> destinationRegistered;
        protected event Action<ScriptableObjects.Destination> DestinationRegistered {
            add {
                destinationRegistered += value;
                foreach(var _d in destinations.Keys) value(_d);
            }
            remove { destinationRegistered -= value; }
        }
        public void RegisterDestination(ScriptableObjects.Destination dest, UnityEngine.GameObject go)
        {
            if (destinations.ContainsKey(dest)) return;
            destinations.Add(dest, go);
            destinationRegistered?.Invoke(dest);
        }

        public void UnregisterDestination(ScriptableObjects.Destination dest)
        {
            if (!destinations.ContainsKey(dest)) return;
            destinations.Remove(dest);
        }

        #endregion

        #region gameplay

        public bool IsPaused { get; private set; } = false;

        private float pausedFixedDeltaTime;

        /// <summary>
        ///     Paused is not the same as inactive.
        ///     When sequence is paused, some stuff might still execute each tick.
        ///     Inactive sequence does not execute anything.
        /// </summary>
        public void Pause()
        {
            pausedFixedDeltaTime = UnityEngine.Time.fixedDeltaTime;
            IsPaused = true;
            UnityEngine.Time.timeScale = 0f;
            UnityEngine.Time.fixedDeltaTime = 0f;
        }

        public void Unpause()
        {
            IsPaused = false;
            UnityEngine.Time.timeScale = 1f;
            UnityEngine.Time.fixedDeltaTime = pausedFixedDeltaTime;
        }
        public void FixedUpdate()
        {
            if (IsPaused || State != GameSequence.State.ACTIVE) return;
            foreach (KeyValuePair<Components.Actors.Sentient, SentientActorState> sentientActor in sentientActors)
            {
                sentientActor.Value.FixedUpdate();
            }
        }
        #endregion

        public void Update()
        {
            if (State != GameSequence.State.ACTIVE || IsPaused)
                return;
            var _cas = GetCurrentAreaSequence();
            if (_cas == null || _cas.State != GameSequence.State.ACTIVE)
                return;
            _cas.Update();
            CampaignCombat.Update();
        }
    }
}
