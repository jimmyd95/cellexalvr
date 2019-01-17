﻿using System.Windows.Input;
using System.Collections;
using UnityEngine;

public class SpecatorController : MonoBehaviour
{
    public float speed = 1f;
    public GameObject CtrlsCanvas;
    public GameObject TextCanvas;


    void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            CtrlsCanvas.SetActive(true);
            TextCanvas.SetActive(false);
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            CtrlsCanvas.SetActive(false);
            TextCanvas.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            speed /= 4;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            speed = 1;
        }

        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * Time.deltaTime * speed);
        }

        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back * Time.deltaTime * speed);
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * Time.deltaTime * speed);
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * Time.deltaTime * speed);
        }

        if (Input.GetKey(KeyCode.E))
        {
            transform.Translate(Vector3.up * Time.deltaTime * speed);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            transform.Translate(Vector3.down * Time.deltaTime * speed);
        }
    }

}

