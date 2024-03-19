using System.Collections.Generic;
using RPG.Utils.PropertyAttributes;

namespace RPG.Components.AI.BehaviourRunner.BehaviourDefinitions.Fixed
{
    /// <summary>
    ///     A fixed ordered action list with targets set in editor.
    /// </summary>
    [System.Serializable]
    public class ActionList : IFixed
    {
        [System.Serializable]
        public class Entry
        {
            [UnityEngine.SerializeReference]
            [SelectImplementation]
            public Core.Shared.AI.IAction action = null;
            public UnityEngine.GameObject target = null;
            public string targetTag = "";
        }
        [UnityEngine.SerializeField]
        private List<Entry> entries = new List<Entry>();
        public Core.Shared.AI.Behaviours.IFixed InstantiateBehaviour(
            RPG.Core.Shared.AI.IActionContext ctx,
            object subject
        ) {
            var _actions = new List<Core.Shared.AI.IAction>();
            if (entries.Count == 0)
                return null;
            foreach (var entry in entries)
            {
#if UNITY_EDITOR
                var _action = entry.action.SafeCopy();
#else
                var _action = entry.action.Copy();
#endif
                _action.Targets = (
                    entry.target == null
                    ? (entry.targetTag == "" ? null : new List<object>(UnityEngine.GameObject.FindGameObjectsWithTag(entry.targetTag)))
                    : new List<object> { entry.target }
                );
                _action.Context = ctx;
                _action.Subject = subject;
                _actions.Add(_action);
            }
            return new Core.Shared.AI.Behaviours.Fixed.ActionList(_actions);
        }
        Core.Shared.AI.IBehaviour IBehaviourDefinition.InstantiateBehaviour(
            RPG.Core.Shared.AI.IActionContext ctx,
            object subject
        ) {
            return InstantiateBehaviour(ctx, subject);
        }
    }
}