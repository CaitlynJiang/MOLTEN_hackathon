using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Reefkeeper
{
    // A persistent hand slot that uses XRI selection, including for laptop input.
    public sealed class ReefPickupInteractor : XRBaseInteractor
    {
        public XRGrabInteractable Requested { get; set; }
        public override bool isSelectActive => isPerformingManualInteraction;
        public override bool CanSelect(IXRSelectInteractable target) => ReferenceEquals(target, Requested);
        public override void GetValidTargets(List<IXRInteractable> targets)
        {
            targets.Clear();
            if (Requested) targets.Add(Requested);
        }
    }
}
