using System.Collections.Generic;

namespace RPG.Core.Shared.AI.Actions
{
    public abstract class BaseAction : IAction
    {
        protected IActionContext context = null;
        public IActionContext Context {
            get => context;
            set
            {
                context = value;
                successProbability = -1.0;
            }
        }
        private object subject = null;
        public object Subject
        {
            get => subject;
            set
            {
                subject = value;
                successProbability = -1.0;
            }
        }
        private List<object> targets = null;
        public List<object> Targets
        {
            get => targets;
            set
            {
                targets = value;
                successProbability = -1.0;
            }
        }
        protected double successProbability = -1.0; // success_probability < 0 means "need to recalculate"
        public double SuccessProbability {
            get
            {
                if (successProbability < 0.0)
                    successProbability = CalculateSuccessProbability();
                return successProbability;
            }
        }
        protected virtual double CalculateSuccessProbability()
        {
            return 1.0;
        }
        public bool IsDone { get; protected set; } = false;
        public virtual IAction Copy()
        {
            return (IAction) MemberwiseClone();
        }
        public virtual IAction SafeCopy()
        {
            return Copy();
        }
        public abstract bool FixedUpdate();
    }
}