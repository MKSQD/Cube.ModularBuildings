using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.ModularBuildings
{
    public class Building : MonoBehaviour
    {
        public enum PartType : byte
        {
            RectFoundation,
            TriFoundation,
            Wall,
            WindowWall,
            StairFoundation
        }

        [Serializable]
        public struct Part
        {
            public PartType type;
            public Vector3 position;
            public Quaternion rotation;
        }

        [Serializable]
        public struct BuildingData
        {
            public List<Part> parts;
        }

        [SerializeField]
        BuildingData _data;
        public BuildingData data {
            get { return _data; }
            set {
                Clear();
                _data = value;
            }
        }

        List<BuildingSlot> _slots = new List<BuildingSlot>();
        List<BuildingSocket> _sockets = new List<BuildingSocket>();
        List<ushort> _childrenIdxForPart = new List<ushort>();
        List<ushort> _partChildren = new List<ushort>();

        public BuildingSlot GetClosestSlot(Vector3 position, BuildingSlotType slotType, bool forPlacement, out float closestDistance)
        {
            closestDistance = float.MaxValue;
            BuildingSlot closestSlot = null;
            foreach (var slot in _slots) {
                if (slot.type != slotType)
                    continue;

                if (forPlacement && slot.ignoreForPlacement)
                    continue;

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
            foreach (var slot in _slots) {
                if (slot.type != slotType)
                    continue;

                if (ignorePartIdx != -1) {
                    if (slot.partIdx == ignorePartIdx)
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
            foreach (var socket in _sockets) {
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
            BuildParts();
            RebuildChildren();
        }

        void BuildParts()
        {
            for (int partIdx = 0; partIdx < _data.parts.Count; ++partIdx) {
                var part = _data.parts[partIdx];
                var prefab = BuildingManager.instance.GetPrefabForPartType(part.type);
                BuildPart(prefab, part, (ushort)partIdx);
            }
        }

        GameObject BuildPart(GameObject prefab, Part part, ushort partIdx)
        {
            var newPrefab = Instantiate(prefab, transform);
            newPrefab.transform.position = part.position;
            newPrefab.transform.rotation = part.rotation;

            var newSlots = newPrefab.GetComponentsInChildren<BuildingSlot>();
            for (int i = 0; i < newSlots.Length; ++i) {
                var slot = newSlots[i];
                slot.partIdx = partIdx;
                slot.childIdx = (byte)i;

                _slots.Add(slot);
            }

            var newSockets = newPrefab.GetComponentsInChildren<BuildingSocket>();
            foreach (var socket in newSockets) {
                socket.partIdx = partIdx;

                _sockets.Add(socket);
            }

            return newPrefab;
        }

        void RebuildChildren()
        {
            for (int partIdx = 0; partIdx < _data.parts.Count; ++partIdx) {
                var part = _data.parts[partIdx];
                var prefab = BuildingManager.instance.GetPrefabForPartType(part.type);

                _childrenIdxForPart.Add((ushort)_partChildren.Count);

                // Populate children of this part
                var slots = prefab.GetComponentsInChildren<BuildingSlot>();
                for (int i = 0; i < slots.Length; ++i) {
                    var slot2 = slots[i];

                    var child = ushort.MaxValue;

                    var otherSockets = GetSocketsAtPosition(part.position + part.rotation * slot2.transform.localPosition, slot2.type);
                    if (otherSockets.Length > 0) {
                        if (otherSockets.Length > 1) {
                            Debug.LogWarning("Multiple sockets");
                        }

                        child = otherSockets[0].partIdx;
                    }

                    _partChildren.Add(child);
                }
            }
        }

        void Clear()
        {
            _slots.Clear();
            _sockets.Clear();
            _childrenIdxForPart.Clear();
            _partChildren.Clear();
            foreach (Transform child in transform) {
                GameObject.Destroy(child.gameObject);
            }
        }

        public bool IsSlotFree(BuildingSlot slot)
        {
            var childrenIdx = _childrenIdxForPart[slot.partIdx];
            return _partChildren[childrenIdx + slot.childIdx] == ushort.MaxValue;
        }

        public void AddPart(PartType type, BuildingSlot slot)
        {
            var partPosition = slot != null ? slot.transform.position : transform.position;
            var partRotation = slot != null ? slot.transform.rotation : transform.rotation;

            if (_data.parts == null) {
                _data.parts = new List<Part>();
            }

            var newPart = new Part {
                type = type,
                position = partPosition,
                rotation = partRotation
            };
            _data.parts.Add(newPart);
        }

        public int GetClosestPartIdx(Vector3 position)
        {
            float closestDistance = float.MaxValue;
            int closestPartIdx = -1;
            for(int i = 0; i < _data.parts.Count; ++i) {
                var part = _data.parts[i];
                var diff = (part.position - position).sqrMagnitude;
                if(diff < closestDistance) {
                    closestDistance = diff;
                    closestPartIdx = i;
                }
            }
            return closestPartIdx;
        }

        public void RemovePart(int partIdx)
        {
            Clear();
            _data.parts[partIdx] = _data.parts[_data.parts.Count - 1];
            _data.parts.RemoveAt(_data.parts.Count - 1);
        }

        void Update()
        {
            if (_data.parts == null)
                return;

            for (int partIdx = 0; partIdx < _data.parts.Count; ++partIdx) {
                var part = _data.parts[partIdx];
                var childrenIdx = _childrenIdxForPart[partIdx];

                for (int i = 0; i < BuildingManager.instance.GetNumChildrenForPartType(part.type); ++i) {
                    var childPartIdx = _partChildren[childrenIdx + i];
                    if (childPartIdx == ushort.MaxValue)
                        continue;

                    var childPart = _data.parts[childPartIdx];
                    DebugDraw.DrawLine(part.position, Vector3.Lerp(part.position, childPart.position, 0.5f), Color.blue);
                }
            }
        }
    }
}