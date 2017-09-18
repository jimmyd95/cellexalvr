﻿using UnityEngine;
/// <summary>
/// This class is repsonsible for turning on and off the laser pointers.
/// </summary>
public class LasersButton : StationaryButton
{

    private ControllerModelSwitcher controllerModelSwitcher;

    protected override string Description
    {
        get { return "Toggle Lasers"; }
    }

    protected override void Awake()
    {
        base.Awake();
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            // turn both off only if both are on, otherwise turn both on.
            bool enabled = controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.TwoLasers;
            if (enabled)
            {
                controllerModelSwitcher.TurnOffActiveTool(true);
            }
            else
            {
                controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.TwoLasers;
                controllerModelSwitcher.ActivateDesiredTool();
            }
        }

    }
}
