﻿using CellexalVR.Interaction;
using Valve.VR.InteractionSystem;

namespace CellexalVR.Menu.Buttons.Heatmap
{
    /// <summary>
    /// Represents the button used for creating a new heatmap from a selection on the heatmap.
    /// </summary>
    public class CreateNewHeatmapButton : CellexalButton
    {
        private HeatmapRaycast heatmapRaycast;

        protected override string Description
        {
            get { return "Create New Heatmap From Selection"; }
        }

        private void Start()
        {
            heatmapRaycast = GetComponentInParent<HeatmapRaycast>();

        }

        public override void Click()
        {
            heatmapRaycast.CreateNewHeatmapFromSelection();
            Player.instance.rightHand.TriggerHapticPulse(2000);
        }
    }
}