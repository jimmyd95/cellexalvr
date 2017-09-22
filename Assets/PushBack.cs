﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class PushBack : MonoBehaviour {
    public SteamVR_TrackedObject rightController;
    private SteamVR_Controller.Device device;
	private Ray ray;
	private RaycastHit hit;
	private Transform raycastingSource;
	private bool push;
	private bool pull;
    // Use this for initialization
    void Start () {
        rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)rightController.index);
    }
	
	// Update is called once per frame
	void Update () {
		if (push)
		{
			raycastingSource = rightController.transform;
			ray = new Ray (raycastingSource.position, raycastingSource.forward);
			if (Physics.Raycast (ray, out hit) && push) {
				Debug.Log ("PUSH BACK");
				Vector3 dir = hit.transform.position - device.transform.pos;
				dir = dir.normalized;
				transform.position += dir;
				transform.localScale *= 1.2f;
			}
		}
//		if (pull)
//		{
//			raycastingSource = rightController.transform;
//			ray = new Ray (raycastingSource.position, raycastingSource.forward);
//			if (Physics.Raycast (ray, out hit) && pull) {
//				Debug.Log ("PULL BACK");
//				Vector3 dir = hit.transform.position - device.transform.pos;
//				dir = dir.normalized;
//				dir *= -0.5f;
//				transform.position += dir;
//				transform.localScale *= -1.2f;
//			}
//		}
        if (rightController == null)
        {
            //Debug.Log("Find right controller");
            rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();

        }
        if (device == null)
        {
            device = SteamVR_Controller.Input((int)rightController.index);
        }
		 
		if ((GetComponent<GraphInteract>() != null && GetComponent<GraphInteract> ().enabled) || GetComponent<VRTK_InteractableObject>() != null && GetComponent<VRTK_InteractableObject>().enabled)
		{
			if (device.GetPressDown (SteamVR_Controller.ButtonMask.Touchpad))
			{
				Debug.Log ("TOUCHPAD PRESSED");
				Vector2 touchpad = (device.GetAxis (Valve.VR.EVRButtonId.k_EButton_Axis0));
				if (touchpad.y > 0.7f)
				{
					push = true;

				}
				if (touchpad.y < -0.7f)
				{
					transform.position = new Vector3 (0.5f, 0.5f, 0.5f);
					transform.localScale = new Vector3 (1, 1, 1);
				}
			}
		}
		if (device.GetPressUp (SteamVR_Controller.ButtonMask.Touchpad))
		{
			push = false;
		}
    }

}
