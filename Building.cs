using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.ModularBuildings
{
    public enum PartType
    {
        RectFoundation,
        TriFoundation,
        Wall,
        WindowWall
    }

    [Serializable]
    public class Building : MonoBehaviour
    {
        struct PartAndChildIdx
        {
            /// <summary>
            /// The index of the part this Slot belongs to.
            /// </summary>
            public int partIdx;
            public int childIdx;
        }

        struct Part
        {
            public PartType type;
            public Vector3 position;
            public Quaternion rotation;
            public ushort childrenIdx;
        }

        Dictionary<BuildingSlot, PartAndChildIdx> _partAndChildPartIdxForSlot = new Dictionary<BuildingSlot, PartAndChildIdx>();
        Dictionary<BuildingSocket, int> _partIdxForSocket = new Dictionary<BuildingSocket, int>();

        List<Part> _parts = new List<Part>();
        [SerializeField]
        List<int> _partChildren = new List<int>();

        public BuildingSlot GetClosestSlot(Vector3 position, BuildingSlotType slotType, bool forPlacement, out float closestDistance)
        {
            closestDistance = float.MaxValue;
            BuildingSlot closestSlot = null;
            foreach (var slotAndChildPartIdxPair in _partAndChildPartIdxForSlot) {
                var slot = slotAndChildPartIdxPair.Key;
                if (slot.type != slotType)
                    continue;

                if (forPlacement && slot.ignoreForPlacement)
                    continue;

                if (slotType == BuildingSlotType.Wall) {
                    var part = _parts[slotAndChildPartIdxPair.Value.partIdx];
                    var a = Vector3.Dot((part.rotation * slot.transform.localRotation) * Vector3.forward, Camera.main.transform.forward);
                    if (a > -0.5f)
                        continue;

                    DebugDraw.DrawVector(slot.transform.position, transform.rotation * slot.transform.localRotation * Vector3.forward, 0.8f, 0.1f, Color.red);
                }

                var dist = (slot.transform.position - position).sqrMagnitude;
                if (dist < closestDistance) {
                    closestDistance = dist;
                    closestSlot = slot;
                }
            }
            return closestSlot;
        }

        public BuildingSlot[] GetSlotsAtPosition(Vector3 position, BuildingSlotType slotType, int ignorePartIdx = -1)
        {
            const float threshold = 0.05f;

            var slots = new List<BuildingSlot>();
            foreach (var slotChildPartIdxPair in _partAndChildPartIdxForSlot) {
                var slot = slotChildPartIdxPair.Key;
                if (slot.type != slotType)
                    continue;

                if (ignorePartIdx != -1) {
                    if (slotChildPartIdxPair.Value.partIdx == ignorePartIdx)
                        continue;
                }

                var dist = (slot.transform.position - position).sqrMagnitude;
                if (dist < threshold) {
                    slots.Add(slot);
                }
            }
            return slots.ToArray();
        }

        BuildingSocket[] GetSocketsAtPosition(Vector3 position, BuildingSlotType slotType)
        {
            const float threshold = 0.05f;

            var sockets = new List<BuildingSocket>();
            foreach (var socket in _partIdxForSocket.Keys) {
                if (socket.slotType != slotType)
                    continue;

                var dist = (socket.transform.position - position).sqrMagnitude;
                if (dist < threshold) {
                    sockets.Add(socket);
                }
            }
            return sockets.ToArray();
        }

        public void Rebuild()
        {
            Clear();

            for (int partIdx = 0; partIdx < _parts.Count; ++partIdx) {
                var part = _parts[partIdx];

                if (part.type == PartType.Wall) {
                    BuildWall(part, partIdx);
                }
                else {
                    var prefab = BuildingManager.instance.GetPrefabForPartType(part.type);
                    BuildPart(prefab, part, partIdx);
                }
            }
        }

        void BuildWall(Part part, int partIdx)
        {
           

            var prefab = BuildPart(Prefabs.Wall, part, partIdx);

            if (_partChildren[part.childrenIdx + 1] != -1) {
                prefab.GetComponent<MeshFilter>().sharedMesh = Prefabs.Edge_Wall.GetComponent<MeshFilter>().sharedMesh;
            }
            if (_partChildren[part.childrenIdx + 2] != -1) {
                prefab.GetComponent<MeshFilter>().sharedMesh = null;
            }
        }

        GameObject BuildPart(GameObject prefab, Part part, int partIdx)
        {
            var newPrefab = Instantiate(prefab, transform);
            newPrefab.transform.position = part.position;
            newPrefab.transform.rotation = part.rotation;

            var newSlots = newPrefab.GetComponentsInChildren<BuildingSlot>();
            for (int i = 0; i < newSlots.Length; ++i) {
                var partAndChildIdx = new PartAndChildIdx {
                    partIdx = partIdx,
                    childIdx = part.childrenIdx + i
                };

                _partAndChildPartIdxForSlot.Add(newSlots[i], partAndChildIdx);
            }

            var newSockets = newPrefab.GetComponentsInChildren<BuildingSocket>();
            foreach (var newSocket in newSockets) {
                _partIdxForSocket.Add(newSocket, partIdx);
            }

            return newPrefab;
        }

        void Clear()
        {
            _partAndChildPartIdxForSlot.Clear();
            _partIdxForSocket.Clear();
            foreach (Transform child in transform) {
                GameObject.Destroy(child.gameObject);
            }
        }

        public bool IsSlotFree(BuildingSlot slot)
        {
            var indices = _partAndChildPartIdxForSlot[slot];
            return _partChildren[indices.childIdx] == -1;
        }

        public void AddPart(PartType type, BuildingSlot slot)
        {
            Vector3 pos;
            Quaternion rot;
            if (slot != null) {
                pos = slot.transform.position;
                rot = slot.transform.rotation;
            }
            else {
                pos = transform.position;
                rot = transform.rotation;
            }

            var newPart = new Part {
                type = type,
                position = pos,
                rotation = rot,
                childrenIdx = (ushort)_partChildren.Count
            };
            _parts.Add(newPart);

            var newPartIdx = _parts.Count - 1;

            //
            var prefab = BuildingManager.instance.GetPrefabForPartType(type);

            var slots = prefab.GetComponentsInChildren<BuildingSlot>();
            foreach (var mySlot in slots) {
                _partChildren.Add(-1);
            }
                
            // Populate children of this part
            for (int i = 0; i < slots.Length; ++i) {
                var slot2 = slots[i];

                var otherSockets = GetSocketsAtPosition(pos + rot * slot2.transform.localPosition, slot2.type);
                if (otherSockets.Length == 0)
                    continue;

                if (otherSockets.Length > 1) {
                    Debug.LogWarning("Multiple sockets");
                }

                var childPartIdx = _partIdxForSocket[otherSockets[0]];
                _partChildren[newPart.childrenIdx + i] = childPartIdx;
            }

            // Populate other children
            var sockets = prefab.GetComponentsInChildren<BuildingSocket>();
            foreach (var socket in sockets) {
                var otherSlots = GetSlotsAtPosition(pos + rot * socket.transform.localPosition, socket.slotType, newPartIdx);
                foreach (var otherSlot in otherSlots) {
                    if (socket.slotType == BuildingSlotType.Wall) {
                        var part = _parts[_partAndChildPartIdxForSlot[otherSlot].partIdx];
                        var a = Vector3.Dot((part.rotation * slot.transform.localRotation) * Vector3.forward, rot * Vector3.forward);
                        if (a > -0.5f)
                            continue;
                    }

                    var indices = _partAndChildPartIdxForSlot[otherSlot];
                    _partChildren[indices.childIdx] = newPartIdx;
                }
            }
        }

        void Start()
        {
            BuildingManager.instance.building = this;
        }

        void Update()
        {
            for (int partIdx = 0; partIdx < _parts.Count; ++partIdx) {
                var part = _parts[partIdx];

                for (int i = 0; i < BuildingManager.instance.GetNumChildrenForPartType(part.type); ++i) {
                    var childPartIdx = _partChildren[part.childrenIdx + i];
                    if (childPartIdx == -1)
                        continue;

                    var childPart = _parts[childPartIdx];
                    DebugDraw.DrawLine(part.position, Vector3.Lerp(part.position, childPart.position, 0.5f), Color.blue);
                }
            }
        }
    }
}