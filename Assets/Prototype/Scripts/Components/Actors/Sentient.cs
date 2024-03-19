namespace RPG.Components.Actors
{
    /// <summary>
    ///     A Sentient actor has a model of its surroundings (memory state), an
    ///     ordered list of behaviours it exhibits, and a set of possible actions
    ///     it can take in order to achieve goals dictated by said behaviours.
    ///     Memory persistence and planning are done by CampaignSequence to enforce
    ///     persistence strategy and perform planning on a cadence that is globally
    ///     decided.
    /// </summary>
    public class Sentient : UnityEngine.MonoBehaviour
    {
        private Managers.PersistentManagers.ClientSequences.CampaignSequence campaignSequence = null;

        private void StartInterop()
        {
            if (campaignSequence != null) return;
            // @neuro: here we specifically return null from a getter when we mean
            // to, and if that's a null-like Unity object, we want this to error out.
#pragma warning disable UNT0008 // Null propagation on Unity objects
            campaignSequence = Managers.PersistentManagers.ClientSequenceManager.Instance?.CampaignSequence;
#pragma warning restore UNT0008 // Null propagation on Unity objects
            if (campaignSequence == null) return;
            campaignSequence.RegisterSentientActor(this);
        }

        private void StopInterop()
        {
            if (campaignSequence == null) return;
            campaignSequence = null;
        }

        private void OnEnable()
        {
            StartInterop();
        }

        private void Start()
        {
            StartInterop();
        }

        private void OnDisable()
        {
            StopInterop();
        }

        private void OnDestroy()
        {
            StopInterop();
        }
    }

}