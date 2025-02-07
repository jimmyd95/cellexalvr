﻿using UnityEngine;
using TMPro;
using CellexalVR.DesktopUI;

namespace CellexalVR.General
{
    /// <summary>
    /// Handles the spawning of notifications. These can be spawned from anywhere e.g. when a script has finished and you want to notify the user.
    /// </summary>
    public class NotificationManager : MonoBehaviour
    {

        public ReferenceManager referenceManager;
        public GameObject notificationPrefab;
        public bool active = true;


        private Animator notificationAnimator;
        private bool notificationShown;
        private GameObject notification;
        private int notificationCounter;
        private AudioSource audioSource;
        
        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }


        void Start()
        {
            GetComponent<Canvas>().worldCamera = referenceManager.headset.GetComponent<Camera>();
            audioSource = GetComponent<AudioSource>();
            if (CrossSceneInformation.Tutorial)
            {
                active = false;
            }
        }


        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// Waits until the close notification animation has finished and then deactivates the notification game object
        /// </summary>
        public void DeactivateNotification()
        {
            notificationCounter--;
        }


        /// <summary>
        /// Spawn notification that will appear in front of the user.
        /// If several notification messages are displayed at the same time they will appear on top of each other.
        /// </summary>
        /// <param name="message">The notification message to display.</param>
        [ConsoleCommand("notificationManager", aliases: "sn")]
        public void SpawnNotification(string message)
        {
            if (active)
            {

                notification = Instantiate(notificationPrefab, this.transform);
                notification.transform.localPosition += new Vector3(0, 100 * notificationCounter, 0);
                notification.GetComponent<Notification>().notificationManager = this;
                notification.GetComponentInChildren<TextMeshProUGUI>().text = message;
                notificationCounter++;
                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }
            }
            CellexalEvents.CommandFinished.Invoke(true);
        }

    }
}
