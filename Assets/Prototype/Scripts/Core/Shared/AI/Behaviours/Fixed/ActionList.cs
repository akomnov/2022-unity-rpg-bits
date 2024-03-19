using System.Collections.Generic;

namespace RPG.Core.Shared.AI.Behaviours.Fixed
{
    /// <summary>
    ///     A fixed ordered action list.
    /// </summary>
    public class ActionList : IFixed {
        public bool IsDone { get; private set; } = false;
        protected double successProbability = -1.0; // success_probability < 0 means "need to recalculate"
        public double SuccessProbability
        {
            get
            {
                if (successProbability < 0.0)
                {
                    if (actions.Count == 0)
                        return 0.0;
                    successProbability = 1.0;
                    foreach (var action in actions)
                    {
                        successProbability *= action.SuccessProbability;
                        if (successProbability == 0.0) break;
                    }
                }
                return successProbability;
            }
        }
        private bool gaveUp = false;
        private int currentActionIdx = 0;
        private List<IAction> actions = null;
        private int actionsCount = 0;
        /// <param name="actions">
        ///     Must be fresh instances that are safe to modify!
        /// </param>
        public ActionList(List<IAction> actions)
        {
            this.actions = actions;
            actionsCount = actions.Count;
        }
        public bool FixedUpdate()
        {
            if (gaveUp) return false;
            if (currentActionIdx >= actionsCount) return false;
            var _currentAction = actions[currentActionIdx];
            if (_currentAction.FixedUpdate() == false)
            {
                gaveUp = true;
                return false;
            }
            if (_currentAction.IsDone == false) return true;
            if (currentActionIdx + 1 >= actionsCount)
            {
                IsDone = true;
            }
            else
            {
                currentActionIdx += 1;
            }
            return true;
        }
    }
}