using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace RPG.Utils {
    [Serializable]
    public class LazySceneRef {
        /// <summary>
        ///     An interface for a wrapper over a wrapper. Yes, this is kinda ridiculous.
        ///     Point is, we won't use AddressableAssets-specific stuff outside of this class,
        ///     so we could switch implementations later if need be.
        /// </summary>
        public interface IInstance {
            Scene Scene { get; }
            bool Equals(object obj);
            bool Equals(IInstance other);
            int GetHashCode();
        }
        private readonly struct Instance : IInstance {
            public readonly SceneInstance sceneInstance;
            public Instance(SceneInstance sceneInstance) => this.sceneInstance = sceneInstance;
            public Scene Scene => sceneInstance.Scene;
            public override bool Equals(object obj) => (obj is IInstance iinstance) && iinstance.Equals(this);
            public bool Equals(IInstance other) => Scene.Equals(other.Scene);
            public override int GetHashCode() => Scene.GetHashCode();
            public static bool operator ==(IInstance lhs, Instance rhs) => lhs.Equals(rhs);
            public static bool operator !=(IInstance lhs, Instance rhs) => !(lhs == rhs);
            public static bool operator ==(Instance lhs, IInstance rhs) => rhs.Equals(lhs);
            public static bool operator !=(Instance lhs, IInstance rhs) => !(lhs == rhs);
        }

        /// <summary>
        ///     For assigning the reference in the Editor.
        /// </summary>
        [UnityEngine.SerializeField]
        private AssetReference addressable = null;
        /// <summary>
        ///     For the string-based constructor.
        /// </summary>
        private readonly string key = null;
        /// <summary>
        ///     "True" location of the resource.
        /// </summary>
        private IResourceLocation location = null;
        private bool locationLookupInProgress = false;

        /// <summary>
        ///     Default ctor for Unity-serialized objects.
        /// </summary>
        public LazySceneRef() { }
        public LazySceneRef(string key) {
            Debug.Assert(key != null);
            this.key = key;
        }
        public LazySceneRef(IResourceLocation location) {
            Debug.Assert(location != null);
            this.location = location;
        }
        public static LazySceneRef GUIDToLazySceneRef(string guid) {
            return new LazySceneRef() {
                addressable = new AssetReference(guid)
            };
        }
        public IEnumerator LoadAdditive(
            Action<float> progressCallback = null
        ) {
            yield return LocateResource();
            if (location == null) throw new Exception("LazySceneRef.LocateResource did not throw but did not locate anything either");
            Exception _loadExc = null;
            IInstance _sceneInstance = null;
            var _op = Addressables.LoadSceneAsync(
                location,
                LoadSceneMode.Additive,
                // Setting this to false in the hope that we can change the scene
                // before activating it simply does not work, as GOs in said scene
                // will not be accessible. Therefore, to support true delayed activation
                // and/or editing before activation, a helper script is necessary.
                // See AdditiveSceneSequenceBase::PerformLoading.
                activateOnLoad: true );
            {
                void _loadOpCompleted(AsyncOperationHandle<SceneInstance> op) {
                    op.Completed -= _loadOpCompleted;
                    if (_sceneInstance != null || _loadExc != null) {
                        UnityEngine.Debug.LogWarning(
                            $"Sanity check failed: entered the same closure multiple times while loading LazySceneRef '{location}'.");
                        return;
                    }
                    if (op.Status == AsyncOperationStatus.Failed) {
                        _loadExc = new Exception(
                            $"Failed to load LazySceneRef '{location}'",
                            op.OperationException);
                    } else {
                        _sceneInstance = new Instance(op.Result);
                    }
                }
                _op.Completed += _loadOpCompleted;
            }
            if (progressCallback != null) {
                progressCallback(0.0f);
                yield return null; // @akomnov: calling code tends to assume at least 1 frame with 0 progress
            }
            while (_sceneInstance == null && _loadExc == null) {
                progressCallback?.Invoke(_op.PercentComplete);
                yield return null;
            };
            if (_loadExc != null) {
                throw _loadExc;
            }
            if (progressCallback != null) {
                progressCallback(_op.PercentComplete);
                yield return null; // @akomnov: calling code tends to assume at least 1 frame with 100% progress
            }
            yield return _sceneInstance;
        }
        public static IEnumerator Unload(IInstance sceneInstance) {
            var _sceneInstance = (Instance)sceneInstance; // @akomnov: ok to throw if not an Instance
            Exception _exc = null;
            bool _ok = false;
            {
                void _opCompleted(AsyncOperationHandle<SceneInstance> op) {
                    op.Completed -= _opCompleted;
                    if (_ok || _exc != null) {
                        UnityEngine.Debug.LogWarning(
                            $"Sanity check failed: entered the same closure multiple times while unloading LazySceneRef '{_sceneInstance.sceneInstance}'.");
                        return;
                    }
                    if (op.Status == AsyncOperationStatus.Failed) {
                        _exc = new Exception(
                            $"Failed to unload '{_sceneInstance.sceneInstance}'",
                            op.OperationException);
                    } else {
                        _ok = true;
                    }
                }
                Addressables.UnloadSceneAsync(_sceneInstance.sceneInstance).Completed += _opCompleted;
            }
            while (!_ok && _exc == null) yield return null;
            if (_exc != null) throw _exc;
        }

        private static Dictionary<string, IResourceLocation> locationKeys = new Dictionary<string, IResourceLocation>();
        private static Dictionary<object, IResourceLocation> locationRuntimeKeys = new Dictionary<object, IResourceLocation>();
        public IEnumerator LocateResource() {
            if (location != null) yield break;
            Exception _addressResolutionExc = null;
            object _resourceKey = null;
            bool _resourceKeyWasAddressable = false;
            if (addressable != null && addressable.RuntimeKeyIsValid() && addressable.RuntimeKey != null) {
                _resourceKey = addressable;
                _resourceKeyWasAddressable = true;
                if (locationRuntimeKeys.ContainsKey(addressable.RuntimeKey)) {
                    location = locationRuntimeKeys[addressable.RuntimeKey];
                    yield break;
                }
            } else {
                _resourceKey = key;
                if (locationKeys.ContainsKey(key)) {
                    location = locationKeys[key];
                    yield break;
                }
            }
            if (!locationLookupInProgress) {
                locationLookupInProgress = true;
                void _addressResolutionCompleted(AsyncOperationHandle<IList<IResourceLocation>> op) {
                    op.Completed -= _addressResolutionCompleted;
                    if (location != null) {
                        // @akomnov: a concurrent LocateResource call got to setting this before us.
                        locationLookupInProgress = false; // assuming this runs on a different thread, this needs to be right before return
                        return;
                    }
                    if (_addressResolutionExc != null) {
                        UnityEngine.Debug.LogWarning(
                            $"Sanity check failed: entered the same closure multiple times while resolving LazySceneRef address '{_resourceKey}'.");
                        locationLookupInProgress = false; // assuming this runs on a different thread, this needs to be right before return
                        return;
                    }
                    if (op.Status == AsyncOperationStatus.Failed) {
                        _addressResolutionExc = new Exception(
                            $"Failed to resolve LazySceneRef address '{_resourceKey}'",
                            op.OperationException);
                        locationLookupInProgress = false; // assuming this runs on a different thread, this needs to be right before return
                        return;
                    }
                    location = op.Result[0];
                    locationLookupInProgress = false; // assuming this runs on a different thread, this needs to be right before return
                    Debug.Assert(op.Result.Count == 1);
                    Debug.Assert(location != null);
                }
                Addressables.LoadResourceLocationsAsync(_resourceKey).Completed += _addressResolutionCompleted;
            }
            while (locationLookupInProgress) {
                yield return null;
            }
            if (_addressResolutionExc != null) throw _addressResolutionExc;
            if (location == null) throw new Exception($"Failed to resolve LazySceneRef address '{_resourceKey}' (in a parallel operation)");
            if (_resourceKeyWasAddressable) {
                locationRuntimeKeys[((AssetReference)_resourceKey).RuntimeKey] = location;
            } else {
                locationKeys[key] = location;
            }
        }
        public bool? ReferencesAssetGUID(string guid) {
            if (addressable == null) return null;
            return (addressable.AssetGUID == guid);
        }
        public bool LocationIsCached => (location != null);
        public string LocationId => location?.PrimaryKey;
        public override string ToString() {
            if (addressable != null) {
                if (addressable.Asset != null) {
#pragma warning disable CS0162 // Unreachable code detected
#if UNITY_EDITOR
                    return $"LazySceneRef( path( {UnityEditor.AssetDatabase.GetAssetPath(addressable.Asset)} ) )";
#endif
                    return $"LazySceneRef( RuntimeKey( {addressable.RuntimeKey}), AssetGUID( {addressable.AssetGUID} ) )";
#pragma warning restore CS0162 // Unreachable code detected
                }
            }
            if (key != null) return $"LazySceneRef( key( {key} ) )";
            return $"LazySceneRef( PrimaryKey( {location?.PrimaryKey} ) )";
        }
    }
}
