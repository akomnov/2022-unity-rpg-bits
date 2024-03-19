using RPG.Utils;

namespace RPG.Managers.PersistentManagers
{
    using RPG.Managers.PersistentManagers.ClientSequences;

    /// <summary>
    ///     The main sequence manager in the client app. "Client" meaning, as
    ///     opposed to (eventual/potential) server app.
    ///
    ///     Supposed to be used with IClientSequence implementations.
    /// </summary>
    public interface IClientSequenceManager : IGameSequenceManager {
        void NewGame(object sender);
        IntroSequence IntroSequence { get; }
        MainMenuSequence MainMenuSequence { get; }
        LoadingSequence LoadingSequence { get; }
        CampaignSequence CampaignSequence { get; }
        CurtainSequence CurtainSequence { get; }
        UnityEngine.GameObject PlayerPrefab { get; }
        void HandleUnrecoverableException(System.Exception e, object current);
        bool HandleUnrecoverableException(System.Exception e);
    }
}