using System.Collections.Generic;
using RPG.Utils.AI;

// This was left unfinished since the project went in a different direction. Bummer.

namespace RPG.Managers.PersistentManagers.ClientSequences
{
    public partial class CampaignSequence
    {
        private Dictionary<Components.Actors.Sentient, SentientActorState> sentientActors = new Dictionary<Components.Actors.Sentient, SentientActorState>();

        public class SentientActorState : ISentient
        {
            private List<BehaviourDefinition> behaviours = new List<BehaviourDefinition> {
                // hardcode while WIP
                new BehaviourDefinition {
                    trigger = new List<Statement>
                    {
                        new Statement
                        {
                            category=Statement.Category.HURT_BY,
                            match=Statement.Match.ANY
                        },
                        new Statement
                        {
                            category=Statement.Category.LOS_TO,
                            match=Statement.Match.MATCH
                        }
                    },
                    goal = new List<Statement>
                    {
                        new Statement
                        {
                            category=Statement.Category.EVADING,
                            match=Statement.Match.MATCH
                        }
                    }
                }
            };

            private Dictionary<Statement.Category, List<UnityEngine.GameObject>> memory = null;

            public List<List<object>> MatchStatementListAgainstMemory(
                List<Statement> statements,
                List<List<object>> givenMatches = null,
                List<List<object>> triggerMatches = null
            ) {
                // FIXME impl + aggregate matches
                return null;
                // @neuro: make sure inner lists are non-null (unless we can actually come up with semantics for a null match list)
            }

            private (
                BehaviourDefinition behaviour,
                List<List<object>> givenMatches,
                List<List<object>> triggerMatches,
                List<IAction> plan,
                uint actionIdx
            ) activeBehaviour = (null, null, null, null, 0);

            private void changeActiveBehaviour(
                BehaviourDefinition next,
                List<List<object>> givenMatches = null,
                List<List<object>> triggerMatches = null
            ) {
                // FIXME stop current action
                activeBehaviour = (next, givenMatches, triggerMatches);
                // FIXME if we're in a behaviour, we MUST BE in an action
            }

            private int nextReplanIn = 0;

            public void FixedUpdate()
            {
                if (memory == null)
                {  // @neuro: init memory only when actually need to
                    memory = new Dictionary<Statement.Category, List<UnityEngine.GameObject>>();
                    // FIXME sensors should poll here, and rely on push afterwards
                }
                // FIXME if we're in a behaviour, we MUST BE in an action - do it
                // FIXME need to check if action arrived to its goal and switch to the next one
                if (nextReplanIn > 0)
                { // @neuro: don't replan every tick
                    --nextReplanIn;
                    return;
                }
                if (activeBehaviour.behaviour != null)
                { // @neuro: "given"s of a behaviour must be valid all the while it's active
                    var _givenMatches = MatchStatementListAgainstMemory(activeBehaviour.behaviour.given);
                    if (_givenMatches != null)
                    { // @neuro: exact matched objects have to be there as well, since actions might reference them
                        for (int _i = activeBehaviour.givenMatches.Count - 1; _i >= 0; --_i)
                        {
                            var _initialMatchesForThisGiven = activeBehaviour.givenMatches[_i];
                            if (_initialMatchesForThisGiven.Count == 0)
                                continue;
                            var _newMatchesForThisGiven = _givenMatches[_i];
                            if (
                                _newMatchesForThisGiven.Count == 0
                                || ! _initialMatchesForThisGiven.TrueForAll(o => _newMatchesForThisGiven.Contains(o))
                            )
                            {
                                _givenMatches = null;
                            }
                        }
                    }
                    if ( _givenMatches == null)
                    {
                        changeActiveBehaviour(null);
                    }
                }
                if (activeBehaviour.behaviour == null || activeBehaviour.behaviour.isInterruptible /* FIXME and current action is interruptible */)
                { // @neuro: if no behaviour is currently set, or a more high-priority one should become active:
                    foreach (var _behaviour in behaviours)
                    {
                        if (activeBehaviour.behaviour == _behaviour)
                        { // @neuro: only lower-priority behaviours left, bail
                            break;
                        }
                        var _givenMatches = MatchStatementListAgainstMemory(_behaviour.given);
                        if (_givenMatches == null)
                            continue;
                        var _triggerMatches = MatchStatementListAgainstMemory(_behaviour.trigger, _givenMatches);
                        if (_triggerMatches == null)
                            continue;
                        if (MatchStatementListAgainstMemory(_behaviour.goal, _givenMatches, _triggerMatches) != null)
                            continue;
                        changeActiveBehaviour(_behaviour, _givenMatches, _triggerMatches);
                        break;
                    }
                }
                // FIXME if unable to complete action, mark action + match as invalid somehow?

                if (activeBehaviour.behaviour != null)
                {
                    // FIXME if no current action, plan actions
                    // FIXME execute current action
                    // FIXME if current action signals inability to complete - replan
                    // FIXME if unable to replan:
                    changeActiveBehaviour(null);
                }
                nextReplanIn = 60; // TODO use spatial data to be smarter about which actors need updating?
                // FIXME some actions/behaviours should stay active if nothing else comes up
            }
        }

        public void RegisterSentientActor(Components.Actors.Sentient o)
        {
            if (sentientActors.ContainsKey(o)) return;
            sentientActors.Add(o, new SentientActorState());
        }
    }
}