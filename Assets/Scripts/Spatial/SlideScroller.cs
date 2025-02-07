using CellexalVR.General;
using CellexalVR.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CellexalVR.Spatial
{
    /// <summary>
    /// This class handles the scrolling of the geo mx images. In the future could be used for different type of image data.
    /// The images are placed in a circle around the user and six images are shown at a time. 
    /// </summary>
    public class SlideScroller : MonoBehaviour
    {
        private GeoMXImageHandler imageHandler;
        [SerializeField] private InputActionAsset inputActionAsset;
        [SerializeField] private InputActionReference touchPadClick;
        [SerializeField] private InputActionReference touchPadPos;


        [HideInInspector] public Dictionary<int, int> currentSlide = new Dictionary<int, int>();
        public string currentScanID;
        [HideInInspector] public string[] currentScanIDs;
        [HideInInspector] public string[] currentROIIDs;
        [HideInInspector] public string[] currentAOIIDs;
        [HideInInspector] public string[] currentIDs;
        [HideInInspector] public Dictionary<string, GeoMXSlide> currentSlides;
        [HideInInspector] public int currentType; // 0: scan, 1: roi, 2: aoi
        private bool _requireToggleToClick;
        public bool RequireToggleToClick
        {
            get { return _requireToggleToClick; }
            set
            {
                if (_requireToggleToClick == value)
                {
                    return;
                }
                _requireToggleToClick = value;
                if (value)
                {
                    touchPadClick.action.performed += OnTouchPadClick;
                    touchPadPos.action.performed -= OnTouchPadClick;
                }
                else
                {
                    touchPadClick.action.performed -= OnTouchPadClick;
                    touchPadPos.action.performed += OnTouchPadClick;
                }
            }
        }

        private void Awake()
        {
            _requireToggleToClick = false;
            touchPadPos.action.performed += OnTouchPadClick;
            imageHandler = GetComponent<GeoMXImageHandler>();
            CellexalEvents.ConfigLoaded.AddListener(() => RequireToggleToClick = CellexalConfig.Config.RequireTouchpadClickToInteract);
        }

        /// <summary>
        /// Clicking right or left on touchpad/thumbstick triggers the scrolling. 
        /// Only scroll if the controller is being pointed towards one of the images.
        /// </summary>
        /// <param name="context"></param>
        private void OnTouchPadClick(InputAction.CallbackContext context)
        {
            if (ReferenceManager.instance.controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.SelectionTool)
                return;

            Vector2 pos = touchPadPos.action.ReadValue<Vector2>();
            Transform rLaser = ReferenceManager.instance.rightLaser.transform;
            Physics.Raycast(rLaser.position, rLaser.forward, out RaycastHit hit, 1 << LayerMask.NameToLayer("EnvironmentButtonLayer"));
            if (!hit.collider || !hit.collider.GetComponent<GeoMXSlide>())
                return;
            GeoMXSlideStack stack = hit.collider.transform.GetComponentInParent<GeoMXSlideStack>();
            if (pos.x > 0.5f)
            {
                if (stack)
                {
                    ScrollStack(1, stack.Group);
                    ReferenceManager.instance.multiuserMessageSender.SendMessageScrollStack(1, stack.Group);
                    return;
                }
                Scroll(1);
                ReferenceManager.instance.multiuserMessageSender.SendMessageScroll(1);
            }
            else if (pos.x < -0.5f)
            {
                if (stack)
                {
                    ScrollStack(-1, stack.Group);
                    ReferenceManager.instance.multiuserMessageSender.SendMessageScrollStack(-1, stack.Group);
                    return;
                }
                Scroll(-1);
                ReferenceManager.instance.multiuserMessageSender.SendMessageScroll(-1);
            }
        }

        /// <summary>
        /// Scroll to a specific image slice.
        /// </summary>
        /// <param name="toSlice"></param>
        public void ScrollTo(int toSlice)
        {
            int diff = toSlice - currentSlide[currentType];
            Scroll(diff);
        }

        /// <summary>
        /// Scroll right or left through the image slices.
        /// </summary>
        /// <param name="val"></param>
        public void Scroll(int val)
        {
            if (currentSlides.Count <= imageHandler.nrOfPositions)
                return;
            if (val > 0)
            {
                for (int i = 0; i < val; i++)
                {
                    int index = mod(currentSlide[currentType], currentIDs.Length);
                    GeoMXSlide sliceToToggleOff = currentSlides[currentIDs[index]].GetComponent<GeoMXSlide>();
                    Vector3 inactivePos = new Vector3(imageHandler.inactivePosLeft.x, sliceToToggleOff.transform.localPosition.y, imageHandler.inactivePosLeft.z);
                    if (!sliceToToggleOff.detached)
                    {
                        sliceToToggleOff.Move(inactivePos);
                        sliceToToggleOff.Fade(false);
                    }
                    currentSlide[currentType] = mod(++currentSlide[currentType], currentIDs.Length);
                    Vector3 targetPos;
                    for (int j = 0; j < imageHandler.nrOfPositions; j++)
                    {
                        GeoMXSlide s = currentSlides[currentIDs[mod(currentSlide[currentType] + j, currentIDs.Length)]].GetComponent<GeoMXSlide>();
                        s.gameObject.SetActive(true);
                        if (j == imageHandler.nrOfPositions - 1)
                        {
                            targetPos = new Vector3(imageHandler.inactivePosRight.x, s.transform.localPosition.y, imageHandler.inactivePosRight.z);
                            if (!s.detached)
                            {
                                s.transform.localPosition = targetPos;
                                s.Fade(true);
                            }
                        }
                        if (!s.detached)
                        {
                            targetPos = new Vector3(imageHandler.sliceCirclePositions[j].x, s.transform.localPosition.y, imageHandler.sliceCirclePositions[j].z);
                            s.Move(targetPos);
                        }
                    }
                }
            }
            else if (val < 0)
            {
                for (int i = val; i < 0; i++)
                {
                    int index = mod(currentSlide[currentType] + imageHandler.nrOfPositions - 1, currentIDs.Length);
                    GeoMXSlide sliceToToggleOff = currentSlides[currentIDs[index]].GetComponent<GeoMXSlide>();
                    Vector3 inactivePos = new Vector3(imageHandler.inactivePosRight.x, sliceToToggleOff.transform.localPosition.y, imageHandler.inactivePosRight.z);
                    if (!sliceToToggleOff.detached)
                    {
                        sliceToToggleOff.Move(inactivePos);
                        sliceToToggleOff.Fade(false);
                    }
                    currentSlide[currentType] = mod(--currentSlide[currentType], currentIDs.Length);
                    Vector3 targetPos;
                    for (int j = imageHandler.nrOfPositions; j >= 0; j--)
                    {
                        GeoMXSlide s = currentSlides[currentIDs[mod(currentSlide[currentType] + j, currentIDs.Length)]].GetComponent<GeoMXSlide>();
                        s.gameObject.SetActive(true);
                        if (j == 0)
                        {
                            if (!s.detached)
                            {
                                targetPos = new Vector3(imageHandler.inactivePosLeft.x, s.transform.localPosition.y, imageHandler.inactivePosLeft.z);
                                s.transform.localPosition = targetPos;
                                s.Fade(true);
                            }
                        }
                        if (!s.detached)
                        {
                            targetPos = new Vector3(imageHandler.sliceCirclePositions[j].x, s.transform.localPosition.y, imageHandler.sliceCirclePositions[j].z);
                            s.Move(targetPos);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Scroll through a stack of images works differently so it calls the function in <see cref="GeoMXSlideStack"/>.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="group"></param>
        public void ScrollStack(int val, int group)
        {
            GeoMXSlideStack stack = imageHandler.stacks[group];
            stack.Scroll(val);
        }

        /// <summary>
        /// Helper function used among other things in the scrolling so that you go back to the first one after scrolling through the last.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static int mod(int x, int m)
        {
            return (x % m + m) % m;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SlideScroller))]
    public class SlideScrollerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SlideScroller myTarget = (SlideScroller)target;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Left"))
            {
                myTarget.Scroll(-1);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Right"))
            {
                myTarget.Scroll(1);
            }
            GUILayout.EndHorizontal();

            DrawDefaultInspector();
        }
    }
#endif
}


