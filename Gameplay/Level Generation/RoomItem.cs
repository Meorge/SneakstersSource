using UnityEngine;
using UnityEngine.AI;

namespace Sneaksters.Gameplay
{
    public class RoomItem : MonoBehaviour
    {
        [Header("Walls")]
        public GameObject topWall;
        public GameObject leftWall;
        public GameObject bottomWall;
        public GameObject rightWall;

        [Header("Doors")]
        public GameObject doorContainer;
        public GameObject topDoor;
        public GameObject leftDoor;
        public GameObject bottomDoor;
        public GameObject rightDoor;

        [Header("Other")]
        public GameObject floor;
        public NavMeshSurface navMeshSurface;

        public uint wallFlags;

        public void Awake() {
            navMeshSurface = floor.GetComponent<NavMeshSurface>();
        }

        public void SetWallFlags(uint flags) {
            wallFlags = flags;

            // order is bottom, left, top, right

            bool topWallExists = ((int)flags & 1) != 0;
            bool rightWallExists = ((int)flags & 2) != 0;
            bool bottomWallExists = ((int) flags & 4) != 0;
            bool leftWallExists = ((int) flags & 8) != 0;

            bool topDoorExists = ((int)flags & 16) != 0;
            bool rightDoorExists = ((int)flags & 32) != 0;
            bool bottomDoorExists = ((int)flags & 64) != 0;
            bool leftDoorExists = ((int)flags & 128) != 0;

            topWall.SetActive(topWallExists);
            leftWall.SetActive(leftWallExists);
            bottomWall.SetActive(bottomWallExists);
            rightWall.SetActive(rightWallExists);

            // topDoor.SetActive(topDoorExists);
            // leftDoor.SetActive(leftDoorExists);
            // bottomDoor.SetActive(bottomDoorExists);
            // rightDoor.SetActive(rightDoorExists);

            if (!topDoorExists) {
                Destroy(topDoor);
            }

            if (!leftDoorExists) {
                Destroy(leftDoor);
            }

            if (!bottomDoorExists) {
                Destroy(bottomDoor);
            }

            if (!rightDoorExists) {
                Destroy(rightDoor);
            }

        }

        void SetDoorsInactive() {
            doorContainer.SetActive(false);
        }

        public void SetDoorsActive() {
            doorContainer.SetActive(true);
        }

        public void RemoveMeshData() {
            navMeshSurface.RemoveData();
            SetDoorsInactive();
        }

        [ContextMenu("Update NavMesh data")]
        public void UpdateMeshData() {
            navMeshSurface.BuildNavMesh();
            SetDoorsActive();
        }
    }
}