using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Utils;

using static RPG.Core.Shared.Utils.Functions;

namespace RPG.Managers.PersistentManagers.ClientSequences
{
    /// <summary>
    ///     Lifetimes of these sequences correspond to the lifetime of the in-memory
    ///     state of player's game progression, starting with the moment the sequence
    ///     was first encountered during a game session.
    /// </summary>
    [System.Serializable]
    public class AreaSequence : AdditiveSceneSequenceBase
    {
        public AreaSequence(IClientSequenceManager manager, Id id) : base(manager, id) { }

        public void MoveObjectIntoSelf(UnityEngine.GameObject o)
        {
            var _wasActive = o.activeSelf;
            o.SetActive(false);
            o.transform.parent = null; // necessary for MoveGameObjectToScene to work
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(o, sceneInstance.Scene);
            if (_wasActive) o.SetActive(true);
        }

        /// <summary>
        ///     This should reset transitory state (i.e. stuff we wouldn't put in a save file).
        /// </summary>
        protected override void PerformResetting()
        {
            base.PerformResetting();
            foreach (var barkContainer in barkContainers.Values)
            {
                barkContainer.Destroy();
            }
            barkContainers = new Dictionary<UnityEngine.GameObject, BarkContainer>();
        }

        public class BarkContainer
        {
            private float secondsToFade;
            private float secondsToStay;
            private float secondsToShow = 0.0f;
            private float secondsElapsed = 0.0f;
            private bool done = false;
            private UnityEngine.TextMesh textMesh;

            public BarkContainer(UnityEngine.GameObject o) {
                var _textGO = new UnityEngine.GameObject("BarkContainer");
                textMesh = _textGO.AddComponent<UnityEngine.TextMesh>();
                textMesh.richText = true;
                textMesh.anchor = UnityEngine.TextAnchor.LowerCenter;
                textMesh.characterSize = 0.5f;
                var _mr = _textGO.GetComponent<UnityEngine.MeshRenderer>();
                _mr.receiveShadows = false;
                _mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                _mr.sortingLayerName = "UI";
                _mr.sortingOrder = -10;
                _textGO.transform.parent = o.transform;
                _textGO.transform.localPosition = new UnityEngine.Vector3(0, 1.0f, 0);
            }

            /// <param name="secondsToStay">
            ///     < 0 means calculate from message length
            /// </param>
            public void SetBark(string message, float secondsToFade = 1.0f, float secondsToStay = -1.0f)
            {
                message = message.Replace("<br>", "\n").Replace("<br/>", "\n");
                if (secondsToStay < 0.0f)
                {
                    secondsToStay = message.Length / 10.0f;
                }
                this.secondsToFade = System.Math.Max(secondsToFade, 0.0f);
                this.secondsToStay = (secondsToStay >= 0.0f) ? secondsToStay : (message.Length / 10.0f);
                secondsToShow = this.secondsToStay + this.secondsToFade;
                secondsElapsed = 0.0f;
                done = (secondsToShow == 0.0f);
                textMesh.text = message;
                textMesh.color = UnityEngine.Color.white;
            }
            public void Update()
            {
                if (done)
                    return;
                if (secondsElapsed >= secondsToShow)
                {
                    textMesh.color = UnityEngine.Color.clear;
                    done = true;
                    return;
                }
                if (secondsElapsed >= secondsToStay && secondsToFade != 0.0f)
                {
                    var _c = textMesh.color;
                    _c.a = 1.0f - (secondsElapsed - secondsToStay) / secondsToFade;
                    textMesh.color = _c;
                }
                secondsElapsed += UnityEngine.Time.deltaTime;
            }
            public void Destroy()
            {
                done = true;
                UnityEngine.Object.Destroy(textMesh.gameObject);
            }
        }

        /// <summary>
        ///     These are here because we want to explicitly manage their lifetime, and here is
        ///     a good place to do that for Unity-specific per-area objects.
        /// </summary>
        private Dictionary<UnityEngine.GameObject, BarkContainer> barkContainers = new Dictionary<UnityEngine.GameObject, BarkContainer>();

        public BarkContainer GetBarkContainer(UnityEngine.GameObject o)
        {
            if (!barkContainers.ContainsKey(o))
            {
                barkContainers[o] = new BarkContainer(o);
            }
            return barkContainers[o];
        }

        public void Update()
        {
            if (State != GameSequence.State.ACTIVE)
                return;
            if (barkContainers != null) foreach (var barkContainer in barkContainers.Values)
            {
                barkContainer.Update();
            }
        }
    }
}
