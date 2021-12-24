using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;

namespace Sneaksters.Gameplay
{
    public class LevelBuilder : MonoBehaviour
    {
        public static LevelBuilder levelBuilder;

        public TextAsset levelAsset;

        public GameObject roomItemPrefab;
        public GameObject gemPrefab;
        public GameObject sackPrefab;
        public GameObject patrolPrefab;
        public GameObject exitManholePrefab;
        public GameObject beaconPrefab;

        public BinaryReader reader;

        public GameObject roomsContainer;

        public bool isMaster = false;

        private float sizeMult = 5f;

        [SerializeField]
        public List<List<Vector3>> nodeSets = new List<List<Vector3>>();

        List<RoomItem> roomItems = new List<RoomItem>();

        public LayerMask allLayers;

        public Vector2 minRoomPosition = Vector2.zero, maxRoomPosition = Vector2.one;

        public enum Actor {
            SpawnPoint = 0,
            ExitPoint = 1,
            VisibilityBeacon = 2,
            Gemstone = 3,
            GemstoneSack = 4,
            Guard = 5,
            SecurityCamera = 6,
            TrapTile = 7
        }

        SpawnActorMethod[] spawnActorMethods;

        delegate void SpawnActorMethod(uint x, uint y, byte[] flags);

        public void Awake() {
            if (levelBuilder != null) {
                Destroy(gameObject);
                return;
            }

            levelBuilder = this;
        }

        public void Start() {
            // if (PhotonGameManager.gameManager != null) {
            //     BuildLevelV2(PhotonGameManager.gameManager.CurrentLevelFileName(), PhotonGameManager.gameManager.IsMasterClient());
            // } else {
            //     print("game manager is null :(");
            //     BuildLevelV2("new_level1-1", true);
            // }
        }


        public void BuildLevelV2(string levelID, bool iM) {
            isMaster = iM;

            spawnActorMethods = new SpawnActorMethod[] {
                CreateSpawnPoint,
                CreateExitPoint,
                CreateVisibilityBeacon,
                CreateGemstone,
                CreateGemstoneSack,
                CreateGuard
            };
            
            foreach (Transform t in roomsContainer.transform) {
                Destroy(t.gameObject);
            }

            levelAsset = Resources.Load("Levels/Level Files/" + levelID + ".snk") as TextAsset;
            Stream stream = new MemoryStream(levelAsset.bytes);
            reader = new BinaryReader(stream);

            char[] levlMarker = reader.ReadChars(4);

            string levlString = new string(levlMarker);

            char fileVersion = reader.ReadChar();

            reader.BaseStream.Position += 35;

            uint offsetTILE = reader.ReadUInt32();
            uint lengthTILE = reader.ReadUInt32();

            uint offsetACTR = reader.ReadUInt32();
            uint lengthACTR = reader.ReadUInt32();

            uint offsetNODE = reader.ReadUInt32();
            uint lengthNODE = reader.ReadUInt32();

            UnpackTILE(offsetTILE, lengthTILE);

            UnpackNODE(offsetNODE, lengthNODE);

            UnpackACTR(offsetACTR, lengthACTR);
        }

        void UnpackACTR(uint offset, uint length) {
            reader.BaseStream.Position = offset;

            char[] actrMarker = reader.ReadChars(4);
            uint noActors = reader.ReadUInt32();

            for (int i = 0; i < noActors; i++) {
                uint id = reader.ReadUInt16();
                uint xPos = reader.ReadUInt16();
                uint yPos = reader.ReadUInt16();
                byte[] flags = reader.ReadBytes(8);

                // run the actor creation code for this actor
                spawnActorMethods[id](xPos, yPos, flags);
            }
            return;
        }

        void CreateSpawnPoint(uint x, uint y, byte[] flags) {
            SpawnPoint spawnPt = new GameObject("SpawnPoint").AddComponent<SpawnPoint>();
            spawnPt.transform.SetParent(roomsContainer.transform, false);
            spawnPt.transform.localPosition = new Vector3((float)x * sizeMult, 1.5f, (float)y * sizeMult * -1f);
            return;
        }

        void CreateExitPoint(uint x, uint y, byte[] flags) {
            ExitManhole man = Instantiate(exitManholePrefab).GetComponent<ExitManhole>();
            man.transform.SetParent(roomsContainer.transform, false);
            man.transform.localPosition = new Vector3((float)x * sizeMult, 0f, (float)y * sizeMult * -1f);
            return;
        }

        void CreateVisibilityBeacon(uint x, uint y, byte[] flags) {
            VisibilityBeacon beac = Instantiate(beaconPrefab).GetComponent<VisibilityBeacon>();
            beac.transform.SetParent(roomsContainer.transform, false);

            Stream flagStream = new MemoryStream(flags);
            BinaryReader flagReader = new BinaryReader(flagStream);

            uint radius = flagReader.ReadUInt16();
            bool invert = flagReader.ReadBoolean();

            beac.SetRadius((int)radius * (int)(sizeMult / 2));
            beac.transform.localPosition = new Vector3((float)x * sizeMult, 0f, (float)y * sizeMult * -1f);
            return;
        }

        void CreateGemstone(uint x, uint y, byte[] flags) {
            if (!isMaster) return;

            Vector3 pos = new Vector3((float)x * sizeMult, 1.5f, (float)y * sizeMult * -1f);

            PhotonGemstone gem;
            
            if (PhotonNetwork.IsConnected) {
                //print("CreateGemstone() - instantiate gemstone as a room object");
                gem = PhotonNetwork.InstantiateRoomObject(gemPrefab.name, pos, Quaternion.identity, 0).GetComponent<PhotonGemstone>();
            } else {
                //print("CreateGemstone() - not online, so instantiate gemstone normally");
                gem = Instantiate(gemPrefab, pos, Quaternion.identity).GetComponent<PhotonGemstone>();
            }

            //print($"CreateGemstone() - set the gemstone's local position to {pos}");
            gem.transform.localPosition = pos;

            return;
        }

        void CreateGemstoneSack(uint x, uint y, byte[] flags) {
            if (!isMaster) return;

            Vector3 pos = new Vector3((float)x * sizeMult, 0f, (float)y * sizeMult * -1f);

            PhotonGemSack sack = PhotonNetwork.IsConnected ?
                PhotonNetwork.InstantiateRoomObject(sackPrefab.name, pos, Quaternion.identity, 0).GetComponent<PhotonGemSack>() :
                Instantiate(sackPrefab, pos, Quaternion.identity).GetComponent<PhotonGemSack>();
                
            sack.transform.localPosition = pos;
            return;
        }

        void CreateGuard(uint x, uint y, byte[] flags) {
            if (!isMaster) return;
            Vector3 pos = new Vector3((float)x * sizeMult, 0f, (float)y * sizeMult * -1f);

            PhotonPatrolGuard guard = PhotonNetwork.IsConnected ?
                PhotonNetwork.Instantiate(patrolPrefab.name, pos, Quaternion.identity, 0).GetComponent<PhotonPatrolGuard>() :
                Instantiate(patrolPrefab, pos, Quaternion.identity).GetComponent<PhotonPatrolGuard>();

            guard.transform.localPosition = pos;

            Stream flagStream = new MemoryStream(flags);
            BinaryReader flagReader = new BinaryReader(flagStream);

            uint nodeSetID = flagReader.ReadUInt16();

            List<Vector3> nodeSet = nodeSets[(int)nodeSetID];

            foreach (Vector3 nodePos in nodeSet) {
                GameObject nodeGO = new GameObject("Patrol Node");
                nodeGO.transform.SetParent(roomsContainer.transform, false);
                nodeGO.transform.localPosition = nodePos;

                guard.stations.Add(nodeGO.transform);
            }

            guard.chaseSpeed = 5f;

            return;
        }

        void UnpackNODE(uint offset, uint length) {
            nodeSets = new List<List<Vector3>>();

            reader.BaseStream.Position = offset;

            char[] nodeMarker = reader.ReadChars(4);
            uint noNodes = reader.ReadUInt32();

            for (int i = 0; i < noNodes; i++) {
                
                uint lengthOfNodeSet = reader.ReadUInt16();

                List<Vector3> newNodeSet = new List<Vector3>();

                for (int g = 0; g < lengthOfNodeSet; g++) {
                    uint x = reader.ReadUInt16();
                    uint y = reader.ReadUInt16();
                    uint flags = reader.ReadUInt16();

                    Vector3 newNodeItem = new Vector3((float)x * sizeMult, 0f, (float)y * sizeMult * -1f);
                    newNodeSet.Add(newNodeItem);
                }
                nodeSets.Add(newNodeSet);
            }

            return;
        }


        void UnpackTILE(uint offset, uint length) {
            reader.BaseStream.Position = offset;

            char[] tileMarker = reader.ReadChars(4);
            uint noTiles = reader.ReadUInt32();


            for (int i = 0; i < noTiles; i++) {
                uint xPos = reader.ReadUInt16();
                uint yPos = reader.ReadUInt16();
                uint flags = reader.ReadUInt16();

                GameObject newRoomObj = Instantiate(roomItemPrefab);
                newRoomObj.transform.SetParent(roomsContainer.transform, false);

                float roomX = (float)xPos * sizeMult;
                float roomY = (float)yPos * -sizeMult;
                newRoomObj.transform.localPosition = new Vector3(roomX, 0f, roomY);

                if (minRoomPosition.x == 0 || roomX < minRoomPosition.x) minRoomPosition.x = roomX;
                if (minRoomPosition.y == 0 || roomY < minRoomPosition.y) minRoomPosition.y = roomY;

                if (maxRoomPosition.y == 1 || roomX > maxRoomPosition.x) maxRoomPosition.x = roomX;
                if (maxRoomPosition.y == 1 || roomY > maxRoomPosition.y) maxRoomPosition.y = roomY;

                RoomItem r = newRoomObj.GetComponent<RoomItem>();
                r.SetWallFlags(flags);
                roomItems.Add(r);
            }

            foreach (RoomItem r in roomItems) r.RemoveMeshData();
            roomItems[0].UpdateMeshData();
            foreach (RoomItem r in roomItems) r.SetDoorsActive();
        }

        void UnpackSPWN(uint offset) {
            reader.BaseStream.Position = offset;

            char[] ptrlMarker = reader.ReadChars(4);
            uint xPosSpawn = reader.ReadUInt16();
            uint yPosSpawn = reader.ReadUInt16();
            uint xPosExit = reader.ReadUInt16();
            uint yPosExit = reader.ReadUInt16();

            // Debug.Log("SPWN: " + new string(ptrlMarker) + " " + xPosSpawn.ToString() + " " + yPosSpawn.ToString());

            SpawnPoint spawnPt = new GameObject("SpawnPoint").AddComponent<SpawnPoint>();
            spawnPt.transform.SetParent(roomsContainer.transform, false);
            spawnPt.transform.localPosition = new Vector3((float)xPosSpawn * sizeMult, 1.5f, (float)yPosSpawn * sizeMult * -1f);


            ExitManhole man = Instantiate(exitManholePrefab).GetComponent<ExitManhole>();
            man.transform.SetParent(roomsContainer.transform, false);
            man.transform.localPosition = new Vector3((float)xPosExit * sizeMult, 0f, (float)yPosExit * sizeMult * -1f);
        }
    }
}