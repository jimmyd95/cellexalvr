﻿/// <summary>
/// This class represents the button that toggles all graph 
/// </summary>

public class RemoveExpressedCellsButton : StationaryButton
{
    private CellManager cellManager;

    protected override string Description
    {
        get { return "Toggle cells with some expression"; }
    }

    private void Start()
    {
        cellManager = referenceManager.cellManager;
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            cellManager.ToggleExpressedCells();
        }
    }

}
