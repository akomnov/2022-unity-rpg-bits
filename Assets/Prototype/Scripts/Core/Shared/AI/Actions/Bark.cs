using System.Collections.Generic;

namespace RPG.Core.Shared.AI.Actions
{
    [System.Serializable]
    public class Bark : BaseAction
    {
        public string message = "";
        public override bool FixedUpdate()
        {
            if (IsDone) return true;
            Context.Bark(Subject, message);
            IsDone = true;
            return true;
        }
    }
}