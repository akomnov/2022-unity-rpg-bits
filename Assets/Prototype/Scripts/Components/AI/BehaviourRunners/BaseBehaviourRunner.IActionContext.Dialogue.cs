using System.Collections.Generic;

namespace RPG.Components.AI.BehaviourRunners
{
    public partial class BaseBehaviourRunner : RPG.Core.Shared.AI.IActionContext
    {
        public void Bark(object subject, string message)
        {
            if (sequenceManager == null)
                return;
            var _cas = sequenceManager.CampaignSequence.GetCurrentAreaSequence();
            if (_cas == null)
                return;
            var _bc = _cas.GetBarkContainer((UnityEngine.GameObject)subject);
            if (_bc == null)
                return;
            _bc.SetBark(message);
        }
    }
}