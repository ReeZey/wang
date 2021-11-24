using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    Vector3 pos;

    public Transform camera;

    private void Start()
    {
        pos = Input.mousePosition;
        Cursor.lockState = CursorLockMode.Confined;
    }

    void LateUpdate()
    {
        var temp = Input.mousePosition;
        if (temp == pos) return;
        
        var direction = temp - pos;
        direction /= 8f;
        
        var eulerAngles = camera.eulerAngles;
            
        eulerAngles += new Vector3(-direction.y, direction.x, 0);
        camera.eulerAngles = eulerAngles;
        
        pos = temp;
    }
}
