using System.Collections.Generic;
using RPG.Utils.PropertyAttributes;

namespace RPG.Components.AI.BehaviourRunners
{
    /// <summary>
    ///     A simpler behaviour runner that does not rely on problem solving or memory.
    ///     Ordering mode: order - run behaviours in order, continue running last behaviour indefinitely if able (otherwise stop).
    ///     Ordering mode: order looped - run behaviours in order, start again from the top when done.
    ///     Ordering mode: priority - evaluate behaviours in order, run the first one that's promising, then start again from the top.
    /// </summary>
    public class FixedBehaviourRunner : BaseBehaviourRunner
    {
        public enum OrderingMode
        {
            Order,
            OrderLooped,
            Priority
        }
        [UnityEngine.SerializeReference]
        [SelectImplementation]
        private List<BehaviourRunner.Slots.IFixed> slots = new List<BehaviourRunner.Slots.IFixed>();
        [UnityEngine.SerializeField]
        private OrderingMode orderingMode = OrderingMode.Order;
        [UnityEngine.SerializeField]
        private int planningCooldownTicks = 50;
        [UnityEngine.SerializeField]
        private double minBehaviourProbability = 0.5;

        /// <summary>
        ///     -1 means we need to pick a slot to continue;
        ///     slots.Count means we came to an indefinte stop (makes sense for Ordering mode: order)
        /// </summary>
        private int currentBehaviourSlotIdx = -1;
        private int planningCooldownTicksElapsed = 50;
        private Core.Shared.AI.Behaviours.IFixed currentBehaviour = null;

        // TODO custom editor GUI for debugging

        private void Awake()
        {
            // TODO: ideally all this should be a part of the persisted state.
            currentBehaviourSlotIdx = -1;
            planningCooldownTicksElapsed = planningCooldownTicks;
            currentBehaviour = null;
        }

        private void FixedUpdate()
        {
            if (currentBehaviourSlotIdx >= slots.Count) return;
            if (planningCooldownTicksElapsed <= planningCooldownTicks)
                planningCooldownTicksElapsed += 1;
            if (currentBehaviourSlotIdx < 0)
            {
                (currentBehaviourSlotIdx, currentBehaviour) = PickNextSlot();
            }
            else if (currentBehaviour.FixedUpdate() == false)
            {
                (currentBehaviourSlotIdx, currentBehaviour) = PickNextSlot(force:true);
            }
            else if (currentBehaviour.IsDone)
            {
                var (_nextSlot, _nextBehaviour) = PickNextSlot();
                // for Order mode we want to stay on the last behaviour indefinitely
                if (!(orderingMode == OrderingMode.Order && _nextSlot >= slots.Count))
                {
                    currentBehaviourSlotIdx = _nextSlot;
                    currentBehaviour = _nextBehaviour;
                }
            }
        }

        private (int slotIdx, Core.Shared.AI.Behaviours.IFixed behaviour) PickNextSlot(bool force = false)
        {
            if (planningCooldownTicksElapsed <= planningCooldownTicks && force == false)
                return (currentBehaviourSlotIdx, currentBehaviour);
            planningCooldownTicksElapsed = 0;
            var _slotsCount = slots.Count;
            for (
                var _i = (orderingMode == OrderingMode.Priority ? 0 : currentBehaviourSlotIdx + 1);
                _i < _slotsCount;
                ++_i
            ) {
                if (_i < 0) continue;
                if (slots[_i].BehaviourDefinition == null) continue;
                var _behaviour = slots[_i].BehaviourDefinition.InstantiateBehaviour(this, gameObject);
                if (_behaviour == null) continue;
                if (_behaviour.SuccessProbability >= minBehaviourProbability)
                    return (_i, _behaviour);
            }
            return (orderingMode == OrderingMode.Order ? _slotsCount : -1, null);
        }
    }
}