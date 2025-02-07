﻿using System;
using CellexalVR.General;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// General slider script that can be used to change a value that has a max and min value.
    /// One or more function(s) being called when the slider handle is grabbed and one or more when the handle is released;
    /// </summary>
    public class VRSlider : MonoBehaviour
    {
        //        public GameObject handle;
        //        public GameObject fillArea;
        //        public TextMeshPro sliderValueText;
        //        public TextMeshPro header;
        //        public string headerText;
        //        public float minValue;
        //        public float maxValue;
        //        public float startValue;
        //        public ReferenceManager referenceManager;

        public enum SliderType
        {
            VelocityParticleSize,
            PDFCurvature,
            PDFRadius,
            PDFWidth,
            PDFHeight
        };

        //        public SliderType sliderType;

        public float Value
        {
            get => value;
            set => this.value = value;
        }

        //        public UnityEvent OnHandleRelease;

        private float value;

        //        [Serializable]
        //        public class OnHandleGrabbedEvent : UnityEvent<float>
        //        {
        //        };

        //        public OnHandleGrabbedEvent OnHandleGrabbed = new OnHandleGrabbedEvent();

        //        private Vector3 handleStartPosition;
        //        private Vector3 handleStartScale;
        //        private Vector3 handleStartRotation;
        //        private Transform handleParent;
        //        private float xMaxPos = 100;
        //        private float xMinPos = 0;
        //        private InteractableObjectBasic handleInteractable;

        //        // Start is called before the first frame update
        //        private void Start()
        //        {
        //            handleParent = handle.transform.parent;
        //            handleStartPosition = handle.transform.localPosition;
        //            handleStartScale = handle.transform.localScale;
        //            handleStartRotation = handle.transform.eulerAngles;
        //            float relativeVal = startValue / maxValue;
        //            float xValue = xMaxPos * relativeVal;
        //            handleStartPosition.x = xValue;

        //            xValue /= xMaxPos;

        //            handle.transform.localPosition = handleStartPosition;
        //            handleInteractable = handle.GetComponent<InteractableObjectBasic>();
        //            handleInteractable.InteractableObjectUnGrabbed += OnRelease;
        //            header.text = headerText;
        //            sliderValueText.text = $"{((int) (xValue * 100)).ToString()}%";
        //            Value = minValue + xValue * (maxValue - minValue);
        //        }

        //        private void OnValidate()
        //        {
        //            handleStartPosition = handle.transform.localPosition;
        //            float relativeVal = startValue / maxValue;
        //            float xValue = xMaxPos * relativeVal;
        //            handleStartPosition.x = xValue;
        //            xValue /= xMaxPos;
        //            Value = minValue + xValue * (maxValue - minValue);
        //            handle.transform.localPosition = handleStartPosition;
        //            Vector3 fillAreaScale = fillArea.transform.localScale;
        //            fillAreaScale.x = 0.001f * xValue;
        //            fillArea.transform.localScale = fillAreaScale;
        //            sliderValueText.text = $"{((int) (xValue * 100)).ToString()}%";
        //            if (gameObject.scene.IsValid())
        //            {
        //                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        //            }
        //        }

        //        private void Update()
        //        {
        //            float xValue = handleParent.InverseTransformPoint(handle.transform.position).x;
        //            if (xValue >= 100)
        //            {
        //                xValue = 100;
        //            }
        //            if (xValue <= 0)
        //            {
        //                xValue = 0;
        //            }
        //            if (handleInteractable.isGrabbed)
        //            {
        //                OnHandleGrabbed.Invoke(Value);
        //            }
        //            handle.transform.position = handleParent.TransformPoint(new Vector3(xValue, handleStartPosition.y, 0));
        //        }

        /// <summary>
        /// The position of the handle goes from 0 - 100.
        /// This is used to determine the value which depends on the min and max set for the slider.
        /// </summary>
        public void UpdateSliderValue()
        {
            //float xValue = handleParent.InverseTransformPoint(handle.transform.position).x;
            //if (xValue >= 100)
            //{
            //    xValue = 100;
            //}

            //if (xValue < 0)
            //{
            //    xValue = 0;
            //}

            //handle.transform.position = handleParent.TransformPoint(new Vector3(xValue, 5, 0));
            //handle.transform.rotation = Quaternion.Euler(handleStartRotation);
            //xValue /= xMaxPos;
            //Value = minValue + xValue * (maxValue - minValue);
            //sliderValueText.text = $"{((int)(xValue * 100)).ToString()}%";
            //Vector3 fillAreaScale = fillArea.transform.localScale;
            //fillAreaScale.x = 0.001f * xValue;
            //fillArea.transform.localScale = fillAreaScale;
        }

        /// <summary>
        /// If slider value is to be updated from outside (e.g. multi user) then update handler first then value.
        /// </summary>
        /// <param name="value"></param>
        public void UpdateSliderValue(float value)
        {
            //handle.transform.position = handleParent.TransformPoint(new Vector3(value, 5, 0));
            UpdateSliderValue();
        }

        //        /// <summary>
        //        /// Inform other users in session to also update slider value. 
        //        /// </summary>
        //        public void UpdateSliderValueMultiUser()
        //        {
        //            referenceManager.multiuserMessageSender.SendMessageUpdateSliderValue(sliderType, handle.transform.localPosition.x);
        //        }

        //        private void OnRelease(object sender, Hand hand)
        //        {
        //            OnHandleRelease.Invoke();
        //            handle.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        //            handle.transform.localScale = handleStartScale;
        //        }
        //    }
    }
}