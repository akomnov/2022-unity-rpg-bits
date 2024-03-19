using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Utils;
using RPG.Managers.PersistentManagers.ClientSequences.AdditiveSceneSequence;
using static RPG.Core.Shared.Utils.Functions;

namespace RPG.Managers.PersistentManagers.ClientSequences {
    public abstract class AdditiveSceneSequenceBase : ClientSequenceBase {
        public struct Id : IEquatable<Id> {
            public readonly LazySceneRef sceneRef;
            /// <summary>
            ///     Not really used for anything rn.
            /// </summary>
            public readonly int seed;
            public Id(LazySceneRef sceneRef, int seed = 0) {
                if (!sceneRef.LocationIsCached)
                    throw new Exception($"Cannot ID a sequence with {sceneRef} having no cached location!");
                this.sceneRef = sceneRef;
                this.seed = seed;
            }
            public bool Initialized => this.sceneRef != null;
            public override bool Equals(object obj) => (obj is Id other) && this.Equals(other);
            public bool Equals(Id p) {
                if (!p.sceneRef.LocationIsCached)
                    throw new Exception($"Cannot test equality against a sequence with {sceneRef} having no cached location!");
                return (sceneRef.LocationId == p.sceneRef.LocationId && seed == p.seed);
            }
            public override int GetHashCode() => (sceneRef.LocationId, seed).GetHashCode();
            public static bool operator ==(Id lhs, Id rhs) => lhs.Equals(rhs);
            public static bool operator !=(Id lhs, Id rhs) => !(lhs == rhs);
            public static IEnumerator FromUncachedSceneRef(LazySceneRef sceneRef, int seed = 0) {
                if (!sceneRef.LocationIsCached)
                    yield return sceneRef.LocateResource();
                yield return new Id(sceneRef, seed);
            }
        }
        public AdditiveSceneSequenceBase(IClientSequenceManager manager, Id id)
            : base(manager) => this.id = id;

        public readonly Id id;
        /// <summary>
        ///     Pairs up with sceneInstanceRoot, which is AdditiveSceneRoot component on its root GO.
        /// </summary>
        protected LazySceneRef.IInstance sceneInstance = null;
        /// <summary>
        ///     Pairs up with sceneInstance, which is the LazySceneRef.IInstance this component belongs to.
        /// </summary>
        protected AdditiveSceneRoot sceneInstanceRoot = null;

        /// <summary>
        ///     Similar to Component.GetComponentsInChildren, but over the whole additive scene.
        /// </summary>
        public IEnumerable<T> GetComponentsInChildren<T>(bool includeInactive = false) {
            return sceneInstanceRoot.GetComponentsInChildren<T>(includeInactive);
        }

        protected override IEnumerator PerformLoading(System.Action<float> progressCallback = null) {
            Exception _exc = null;
            yield return CoroutineAndCallBack(
                id.sceneRef.LoadAdditive(progressCallback),
                (e, r) => { _exc = e; sceneInstance = (LazySceneRef.IInstance)r; } );
            if (_exc != null)
                throw _exc;
            if (sceneInstance == null)
                throw new Exception("LazySceneRef.LoadAdditive did not throw but did not load a scene either");
            // We'd like to:
            // a) make sure the scene we just loaded does not interfere with currently
            // running scenes just because we've loaded it and
            // b) be able to change the loaded scene before it activates.
            // We can't do that using Addressables' API, as activateOnLoad:false
            // results in an empty scene you can't do anything with except activate
            // (upon which its GOs become available).
            // Therefore we're enforcing a couple of rules on our additive scenes:
            // 1. every additive scene MUST have a single active root game object
            // (so that (de-)activating the whole scene is trivial)
            // 2. this GO MUST have an AdditiveSceneRoot component
            // (so that we know for sure it behaves and does not start running
            // stuff until we say so).
            // This is certainly ass-backwards and tbh I'm hoping I got something
            // wrong and there is a better way, but none to be found so far.
            {
                var _gos = sceneInstance.Scene.GetRootGameObjects();
                if(_gos.Length != 1) throw new Exception(
                    $"{this} must have exactly 1 root GO (not {_gos.Length}) in order to be additively loaded.");
                sceneInstanceRoot = _gos[0].GetComponent<AdditiveSceneRoot>();
                if(sceneInstanceRoot == null) throw new Exception(
                     $"{this} must have AdditiveSceneRoot component on its root GO in order to be additively loaded.");
            }
        }

        protected override void PerformActivation(bool skipCamera = false) {
            if (sceneInstanceRoot == null) throw new System.Exception("Tried to activate a null scene");
            sceneInstanceRoot.PerformActivation(skipCamera);
        }

        protected override void PerformDeactivation() {
            if (sceneInstanceRoot == null) throw new Exception("Tried to deactivate a null scene");
            sceneInstanceRoot.PerformDeactivation();
        }

        protected override IEnumerator PerformUnloading() {
            if (sceneInstance == null) yield break;
            sceneInstanceRoot = null;
            yield return LazySceneRef.Unload(sceneInstance);
            sceneInstance = null;
        }
        public override string ToString() {
            return $"AdditiveSceneSequenceBase( sceneRef( {id.sceneRef} ), seed( {id.seed} ) )";
        }
    }
}
