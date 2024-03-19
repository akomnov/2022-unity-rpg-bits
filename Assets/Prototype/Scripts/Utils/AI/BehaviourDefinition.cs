using System.Collections.Generic;

namespace RPG.Utils.AI
{
    // @neuro: this is a DTO and a static method namespace, move it into shared
    [System.Serializable]
    public class BehaviourDefinition
    {
        /// <summary>
        ///     Every "given" statement must be true while the behaviour is active.
        ///     If a "given" statement is no longer true, behaviour will stop even if it's not interruptible.
        /// </summary>
        public List<Statement> given = new List<Statement>();
        /// <summary>
        ///     Every "trigger" statement must be true for the behaviour to become active.
        /// </summary>
        public List<Statement> trigger = new List<Statement>();
        /// <summary>
        ///     Every "goal" statement must be true for the behaviour to complete.
        /// </summary>
        public List<Statement> goal = new List<Statement>();
        /// <summary>
        ///     If false, this behaviour will be run to completion (or failure) even if more important behaviours become available.
        /// </summary>
        public bool isInterruptible = true;
    }
}