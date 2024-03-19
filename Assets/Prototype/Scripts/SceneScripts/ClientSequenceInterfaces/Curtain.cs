using RPG.Managers.PersistentManagers.ClientSequences.AdditiveSceneSequence;

namespace RPG.SceneScripts.ClientSequenceInterfaces
{
    using RPG.Managers.PersistentManagers;

    public class Curtain : AdditiveSceneMonoBehaviour {
        private IClientSequenceManager sequenceManager = null;
        private UnityEngine.Animator animator;

        protected override void StartInterop() {
            if (sequenceManager != null) return;
            var _sequenceManager = ClientSequenceManager.Instance;
            if (_sequenceManager == null) return;
            if (_sequenceManager.CurtainSequence == null) return;
            if (_sequenceManager.CurtainSequence.State != Managers.GameSequence.State.ACTIVE) return;
            animator = GetComponent<UnityEngine.Animator>();
            if (animator == null) return;
            sequenceManager = _sequenceManager;
            {
                void _cb(object sender, System.EventArgs e) {
                    sequenceManager.CurtainSequence.ReadyToLift -= _cb;
                    animator.SetTrigger("readyToLift");
                };
                sequenceManager.CurtainSequence.ReadyToLift += _cb;
            }
        }

        protected override void StopInterop()
        {
            sequenceManager = null;
        }

        public void Lowered() {
            if (sequenceManager == null) return;
            sequenceManager.CurtainSequence?.OnLowered(this);
        }

        public void Lifted() {
            if (sequenceManager == null) return;
            sequenceManager.CurtainSequence?.OnLifted(this);
        }
    }
}
