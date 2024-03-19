using System.Collections.Generic;

namespace RPG.Core.Shared.AI
{
    public interface IAction
    {
        /// <returns>
        ///     False if unable to perform the action, true otherwise (even if already done).
        /// </returns>
        bool FixedUpdate();
        /// <summary>
        ///     True if the action is considered done (and e.g. the subject can move on to performing further actions).
        /// </summary>
        bool IsDone { get; }
        IAction Copy();
        /// <summary>
        ///     Same as copy, but also re-allocates objects that are not supposed
        ///     to be normally modified (if they could affect action execution in
        ///     a qualitative manner) in case Unity Editor intervenes.
        /// </summary>
        IAction SafeCopy();
        IActionContext Context { get; set; }
        object Subject { get; set; }
        List<object> Targets { get; set; }
        double SuccessProbability { get; }
    }
}