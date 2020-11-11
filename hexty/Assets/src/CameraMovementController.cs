// Copyright (c) 2020 Rafael Alcaraz Mercado. All rights reserved.
// Licensed under the MIT license <LICENSE-MIT or http://opensource.org/licenses/MIT>.
// All files in the project carrying such notice may not be copied, modified, or distributed
// except according to those terms.
// THE SOURCE CODE IS AVAILABLE UNDER THE ABOVE CHOSEN LICENSE "AS IS", WITH NO WARRANTIES.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovementController : MonoBehaviour
{
    private Camera _camera;
    private GameObject _hookedObject;

    public float Step = 0.5f;
    public bool ControlRotationWithMouse = false;
    public float HorizontalSpeed = 2.0f;
    public float VerticalSpeed = 2.0f;
    public float Offset = 50f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    void Start()
    {
    }

    public void ResetCamera()
    {
        _camera = Camera.main;
        transform.position = new Vector3(0, 20, 0);
        transform.rotation = Quaternion.Euler(45, 0, 0);
        _camera.orthographic = false;
    }

    public void Hook(GameObject obj)
    {
        _hookedObject = obj;
        transform.position = new Vector3(
            transform.position.x,
            Offset,
            transform.position.z
        );
        _camera.fieldOfView = Offset;
    }

    // Update is called once per frame
    void Update()
    {
        if (_hookedObject == null)
        {
            transform.position += new Vector3(
                Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)
                    ? -Step
                    : Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)
                        ? Step
                        : 0,
                Input.GetKey(KeyCode.J) || Input.mouseScrollDelta.y > 0
                    ? -Step
                    : Input.GetKey(KeyCode.K) || Input.mouseScrollDelta.y < 0
                        ? +Step
                        : 0,
                Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)
                    ? -Step
                    : Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)
                        ? Step
                        : 0
            );

            var euler = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(euler + new Vector3(
                Input.GetKey(KeyCode.U)
                    ? -Step
                    : Input.GetKey(KeyCode.I)
                        ? +Step
                        : 0,
                0,
                Input.GetKey(KeyCode.O)
                    ? -Step
                    : Input.GetKey(KeyCode.L)
                        ? +Step
                        : 0
            ));

            if (ControlRotationWithMouse)
            {
                yaw += HorizontalSpeed * Input.GetAxis("Mouse X");
                pitch -= VerticalSpeed * Input.GetAxis("Mouse Y");
                transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
            }

            if (Input.GetKey(KeyCode.R))
            {
                ResetCamera();
            }
        }
        else
        {
            var degrees = _hookedObject.transform.rotation.eulerAngles.y;
            var radians = degrees * (Mathf.PI / 180f);

            transform.position = new Vector3(
                _hookedObject.transform.position.x + (Offset * Mathf.Cos(radians)), // X coordinate based on angle and the parametric equation of a circle
                transform.position.y,
                _hookedObject.transform.position.z + (Offset * Mathf.Sin(radians)) // Z coordinate based on angle and the parametric equation of a circle
            );

            transform.LookAt(_hookedObject.transform.position);
        }
    }
}
