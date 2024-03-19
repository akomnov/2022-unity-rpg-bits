using System.Collections.Generic;

namespace RPG.Core.Shared.AI.Actions
{
    [System.Serializable]
    public class Idle : BaseAction
    {
        public int ticksToIdle = 50;
        private int ticksElapsed = 0;
        public override bool FixedUpdate()
        {
            if (IsDone) return true;
            ticksElapsed += 1;
            if (ticksElapsed > ticksToIdle)
            {
                ticksElapsed = ticksToIdle;
                IsDone = true;
            }
            return true;
        }
    }
}