using System.Collections.Generic;

namespace RPG.Core.Shared.AI
{
    public interface IBehaviour
    {
        /// <returns>
        ///     False if unable to continue performing the behaviour, true otherwise (even if already done).
        /// </returns>
        bool FixedUpdate();
        /// <summary>
        ///     True if the behaviour can be considered done (and e.g. the subject can pick another).
        /// </summary>
        bool IsDone { get; }
        double SuccessProbability { get; }
    }
}