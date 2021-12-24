using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TakeScreenshot : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame) {
            print("take a screenshot");
            ScreenCapture.CaptureScreenshot($"Sneaksters Screenshot at {System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}.png");
        }
    }
}
