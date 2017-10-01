using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.ModularBuildings
{
    [Serializable]
    public class Building : MonoBehaviour
    {
        enum RoomType
        {
            None,
            Foundation
        }

        struct Room
        {
            public RoomType type;
            public GameObject gameObject;
        }

        public IntVector3 size = new IntVector3(1, 1, 1);

        Room[,,] _rooms = new Room[1, 1, 1];
        
        void Start()
        {
            _rooms[0, 0, 0] = new Room();

            BuildingManager.instance.buildings.Add(this);
        }

        public void Build()
        {
            Clear();

            for (int x = 0; x < size.x; ++x) {
                for (int y = 0; y < size.y; ++y) {
                    for (int z = 0; z < size.z; ++z) {
                        var room = _rooms[x, y, z];
                        if (room.type == RoomType.Foundation) {

                            var go = GameObject.Instantiate(Prefabs.Foundation, transform);
                            go.transform.localPosition = new Vector3(x, y, z);
                            go.transform.localRotation = Quaternion.identity;
                            _rooms[x, y, z].gameObject = go;
                        }
                    }
                }
            }
        }

        void Clear()
        {
            for (int x = 0; x < size.x; ++x) {
                for (int y = 0; y < size.y; ++y) {
                    for (int z = 0; z < size.z; ++z) {
                        if (_rooms[x, y, z].gameObject == null)
                            continue;

                        Destroy(_rooms[x, y, z].gameObject);
                        _rooms[x, y, z].gameObject = null;
                    }
                }
            }
        }

        public void Place(Vector3 localPosition)
        {
            Debug.Log(localPosition);

            var oldSize = size;
            var newSize = size;
            var offset = IntVector3.zero;
            if (localPosition.x < 0) {
                newSize.x += 1;
                offset.x += 1;
            }
            if (localPosition.z < 0) {
                newSize.z += 1;
                offset.z += 1;
            }
            if (localPosition.x >= size.x) {
                newSize.x += 1;
            }
            if (localPosition.z >= size.z) {
                newSize.z += 1;
            }
            transform.position -= transform.rotation * offset;
            localPosition += offset;

            var newRooms = new Room[newSize.x, newSize.y, newSize.z];
            for (int x = 0; x < oldSize.x; ++x) {
                for (int y = 0; y < oldSize.y; ++y) {
                    for (int z = 0; z < oldSize.z; ++z) {
                       // newRooms[x + offset.x, y, z + offset.z] = _rooms[x, y, z];
                    }
                }
            }
            Clear();
            
            newRooms[(int)localPosition.x, (int)localPosition.y, (int)localPosition.z].type = RoomType.Foundation;
            size = newSize;
            _rooms = newRooms;
        }
    }
}