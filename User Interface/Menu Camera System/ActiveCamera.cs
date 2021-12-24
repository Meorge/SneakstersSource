using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace Sneaksters
{
    public class ActiveCamera : MonoBehaviour
    {
        public string id = "";
        public CinemachineVirtualCamera cam;

        public static List<ActiveCamera> cameras = new List<ActiveCamera>();

        void Awake() {
            cameras.Add(this);
            cam = GetComponent<CinemachineVirtualCamera>();
        }

        public static void SetActiveCamera(string cameraID) {
            if (cameraID == "") return;
            foreach (ActiveCamera a in cameras) {
                if (a.id == cameraID) {
                    a.cam.Priority = 1;
                } else {
                    a.cam.Priority = 0;
                }
            }
        }
    }
}