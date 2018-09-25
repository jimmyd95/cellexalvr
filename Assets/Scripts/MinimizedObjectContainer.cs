﻿using UnityEngine;

/// <summary>
/// Represents an object that temporarily holds another object while it is minimized.
/// </summary>
public class MinimizedObjectContainer : MonoBehaviour
{

    private SteamVR_TrackedObject rightController;
    private MinimizeTool minimizeTool;
    private Color orgColor;
    public GameObject MinimizedObject { get; set; }
    public MinimizedObjectHandler Handler { get; set; }
    /// <summary>
    /// The x-coordinate in the grid that this container is in.
    /// Has a range of [0, 4]
    /// </summary>
    public int SpaceX { get; set; }
    /// <summary>
    /// The y-coordinate in the grid that this container is in.
    /// Has a range of [0, 4]
    /// </summary>
    public int SpaceY { get; set; }

    private bool controllerInside = false;
    private string laserColliderName = "[RightController]BasePointerRenderer_ObjectInteractor_Collider";
    private int frameCount;
    private int layerMask;


    private void Start()
    {
        rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();
        this.name = "Jail_" + MinimizedObject.name;
        minimizeTool = Handler.referenceManager.minimizeTool;
        orgColor = GetComponent<Renderer>().material.color;
        frameCount = 0;
        layerMask = 1 << LayerMask.NameToLayer("Ignore Raycast");
    }

    private void Update()
    {
        var device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            if (MinimizedObject.CompareTag("Graph"))
            {
                MinimizedObject.GetComponent<Graph>().ShowGraph();
                //minimizeTool.MaximizeObject(MinimizedObject, this, "Graph");
                Handler.referenceManager.gameManager.InformShowGraph(MinimizedObject.name, this.name);
            }
            if (MinimizedObject.CompareTag("Network"))
            {
                MinimizedObject.GetComponent<NetworkHandler>().ShowNetworks();
                //minimizeTool.MaximizeObject(MinimizedObject, this, "Network");
                Handler.referenceManager.gameManager.InformShowNetwork(MinimizedObject.name, this.name);
            }
            if (MinimizedObject.CompareTag("HeatBoard"))
            {
                MinimizedObject.GetComponent<Heatmap>().ShowHeatmap();
                //minimizeTool.MaximizeObject(MinimizedObject, this, "Network");
                //Handler.referenceManager.gameManager.InformShowNetwork(MinimizedObject.name, this.name);
            }
            Handler.ContainerRemoved(this);
            Destroy(gameObject);
        }

        frameCount++;
        // Button sometimes stays active even though ontriggerexit should have been called.
        // To deactivate button again check every 10th frame if laser pointer collider is colliding.
        if (frameCount % 30 == 0)
        {
            bool inside = false;
            Collider[] collidesWith = Physics.OverlapBox(transform.position, transform.localScale, Quaternion.identity, layerMask);

            foreach (Collider col in collidesWith)
            {
                if (col.gameObject.name == laserColliderName)
                {
                    inside = true;
                    return;
                }
            }

            controllerInside = inside;
            GetComponent<Renderer>().material.color = orgColor;
            frameCount = 0;
        }
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "[RightController]BasePointerRenderer_ObjectInteractor_Collider")
        {
            controllerInside = true;
            GetComponent<Renderer>().material.color = Color.cyan;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "[RightController]BasePointerRenderer_ObjectInteractor_Collider")
        {
            controllerInside = false;
            GetComponent<Renderer>().material.color = orgColor;
        }
    }
}