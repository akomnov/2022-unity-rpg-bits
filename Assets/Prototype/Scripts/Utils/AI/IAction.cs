using System.Collections.Generic;

namespace RPG.Utils.AI
{
    public interface IAction
    {
        bool FixedUpdate(ISentient actor);

        List<Statement> From { get; }

        List<Statement> To { get; }
    }
}