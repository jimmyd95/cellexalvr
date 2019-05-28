﻿using CellexalVR.General;
using UnityEngine;
using VRTK;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Handles what happens when a network center is interacted with.
    /// </summary>
    class NetworkCenterInteract : VRTK_InteractableObject
    {
        public ReferenceManager referenceManager;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        }

        public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
        {
            referenceManager.gameManager.InformDisableColliders(gameObject.name);
            if (grabbingObjects.Count == 1)
            {
                // moving many triggers really pushes what unity is capable of
                foreach (Collider c in GetComponentsInChildren<Collider>())
                {
                    if (c.gameObject.name != "Ring" && !c.gameObject.name.Contains("Enlarged_Network"))
                    {
                        c.enabled = false;
                    }
                    //else if (c.gameObject.name == "Ring")
                    //{
                    //    ((MeshCollider)c).convex = true;
                    //}
                }
            }
            base.OnInteractableObjectGrabbed(e);
        }

        public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
        {
            referenceManager.gameManager.InformEnableColliders(gameObject.name);
            if (grabbingObjects.Count == 0)
            {
                foreach (Collider c in GetComponentsInChildren<Collider>())
                {
                    if (c.gameObject.name != "Ring" && !c.gameObject.name.Contains("Enlarged_Network"))
                    {
                        c.enabled = true;
                    }
                    //else if (c.gameObject.name == "Ring")
                    //{
                    //    ((MeshCollider)c).convex = false;
                    //}

                }
            }
            base.OnInteractableObjectUngrabbed(e);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (referenceManager.controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.TwoLasers
                || referenceManager.controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.Keyboard)
            {
                if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
                {
                    CellexalEvents.ObjectGrabbed.Invoke();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (referenceManager.controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.TwoLasers
                || referenceManager.controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.Keyboard)
            {
                if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
                {
                    CellexalEvents.ObjectUngrabbed.Invoke();
                }
            }
        }
    }
}