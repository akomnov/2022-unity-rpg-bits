namespace RPG.Core.Shared.AI.Actions
{
    /// <summary>
    ///     Navigates to the first target.
    /// </summary>
    [System.Serializable]
    public class NavigateTo : BaseAction
    {
        /// <summary>
        ///     Sets the agent's stoppingDistance.
        ///     This is just a general hint to tweak the NavMesh agent, not a hard rule!
        ///     I.e. the agent will probably stop further than this from the goal, but
        ///     not much further.
        /// </summary>
        public float stoppingDistance = 1f;
        /// <summary>
        ///     Sets the agent's autoRepath.
        ///     Even if true, the agent is unlikely to actually repath if giveUpAfterUnableToReachTicks
        ///     is <= 0 (since that makes us enter a failure state if stuck - which
        ///     will happen when the agent detects a need to repath).
        /// </summary>
        public bool autoRepath = true;
        /// <summary>
        ///     If false, evaluating the action will synchronously compute the path
        ///     (might take a while! and this path will not be reused unless autoRepath
        ///     is false!) to determine if it's reachable. Success probability 
        ///     will then be either 0 or 1.
        ///     If true, success probability is assumed to be 1.
        /// </summary>
        public bool assumeTargetIsReachable = false;
        /// <summary>
        ///     Final path point within this distance from the target will be deemed
        ///     close enough.
        ///     If assumeTargetIsReachable is false, this affects evaluation as well.
        ///     Don't set this to zero - that will cause the agent will stop a bit
        ///     further than stoppingDistance from its goal and then think it's stuck!
        /// </summary>
        public float allowedUndershoot = 2f;
        /// <summary>
        ///     If > 0, the action will time out after this amount of ticks; if
        ///     we're within allowedUndershoot by then, it's a success and we're
        ///     done, otherwise it's a failure state.
        /// </summary>
        public int giveUpAfterTicks = 0;
        /// <summary>
        ///     If > 0, the action will not report failure right away upon the agent
        ///     being stuck further than allowedUndershoot, instead waiting up to this
        ///     number of ticks for the agent to repath.
        /// </summary>
        public int giveUpAfterUnableToReachTicks = 500;

        private bool started = false;
        private bool failed = false;
        private int ticksPassed = 0;
        private int ticksPassedStuck = 0;
        private object precalculatedPath = null;
        public override bool FixedUpdate()
        {
            if (failed) return false;
            if (IsDone) return true;
            var _target = Targets[0];
            if (!started) {
                started = true;
                ticksPassed = 0;
                if (precalculatedPath == null || autoRepath) {
                    if (false == Context.SetPathDestination(Subject, _target, stoppingDistance, autoRepath)) {
                        failed = true;
                        return false;
                    }
                } else {
                    if (false == Context.SetPath(Subject, precalculatedPath, stoppingDistance)) {
                        failed = true;
                        return false;
                    }
                }
            }
            float _shortestDistanceToTarget = -1f; // means undefined
            if (giveUpAfterTicks > 0) {
                ticksPassed += 1;
                if(ticksPassed >= giveUpAfterTicks) {
                    _shortestDistanceToTarget = Context.GetShortestDistance(Subject, _target);
                    return Bail((stoppingDistance + allowedUndershoot) > _shortestDistanceToTarget);
                }
            }
            if (Context.StoppedPathing(Subject)) {
                if (_shortestDistanceToTarget < 0) // if didn't calculate already
                    _shortestDistanceToTarget = Context.GetShortestDistance(Subject, _target);
                if ((stoppingDistance + allowedUndershoot) > _shortestDistanceToTarget) {
                    return Bail(true);
                }
                if (giveUpAfterUnableToReachTicks > 0) {
                    ticksPassedStuck += 1;
                    if (ticksPassedStuck >= giveUpAfterUnableToReachTicks)
                        return Bail(false);
                } else {
                    return Bail(false);
                }
            } else {
                ticksPassedStuck = 0;
            }
            return true;
        }
        protected override double CalculateSuccessProbability()
        {
            if (assumeTargetIsReachable) return 1.0;
            var _target = Targets[0];
            (object _path, float _undershoot) = Context.FindPath(Subject, _target);
            if (_path == null) return 0.0;
            precalculatedPath = _path;
            return _undershoot <= allowedUndershoot ? 1.0 : 0.0;
        }
        private bool Bail(bool success)
        {
            Context.UnsetPath(Subject);
            if (success) {
                IsDone = true;
            } else {
                failed = true;
            }
            return success;
        }
    }
}