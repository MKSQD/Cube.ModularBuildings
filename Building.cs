using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.ModularBuildings
{
    public enum PartType
    {
        RectFoundation,
        TriFoundation,
        Wall
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

        List<Part> _parts = new List<Part>();
        List<int> _partChildren = new List<int>();

        public BuildingSlot GetClosestSlot(Vector3 position, BuildingSlotType slotType, bool forPlacement, out float closestDistance)
        {
            closestDistance = float.MaxValue;
            BuildingSlot closestSlot = null;
            foreach (var slot in _partAndChildPartIdxForSlot.Keys) {
                if (slot.type != slotType)
                    continue;

                if (forPlacement && slot.ignoreForPlacement)
                    continue;

                DebugDraw.DrawMarker(slot.transform.position, 0.25f, Color.green);

                var dist = (slot.transform.position - position).sqrMagnitude;
                if (dist < closestDistance) {
                    closestDistance = dist;
                    closestSlot = slot;
                }
            }
            return closestSlot;
        }

        public BuildingSlot[] GetSlotsAtPosition(Vector3 position, BuildingSlotType slotType)
        {
            const float threshold = 0.05f;

            var slots = new List<BuildingSlot>();
            foreach (var slot in _partAndChildPartIdxForSlot.Keys) {
                if (slot.type != slotType)
                    continue;

                var dist = (slot.transform.position - position).sqrMagnitude;
                if (dist < threshold) {
                    slots.Add(slot);
                }
            }
            return slots.ToArray();
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
                    GameObject prefab = null;
                    switch (part.type) {
                        case PartType.RectFoundation:
                            prefab = Prefabs.RectFoundation;
                            break;

                        case PartType.TriFoundation:
                            prefab = Prefabs.TriFoundation;
                            break;
                    }
                    BuildPart(prefab, part, partIdx);
                }
            }
        }

        void BuildWall(Part part, int partIdx)
        {
            //if (_partChildren[part.childrenIdx + 2] != -1)
            //    return;

            var prefab = BuildPart(Prefabs.Wall, part, partIdx);

            //             if (_partChildren[part.childrenIdx + 1] != -1) {
            //                 prefab.GetComponent<MeshFilter>().sharedMesh = Prefabs.Edge_Wall.GetComponent<MeshFilter>().sharedMesh;
            //             }
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
            return newPrefab;
        }

        void Clear()
        {
            _partAndChildPartIdxForSlot.Clear();
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

            //
            {
                GameObject prefab = null;
                switch (type) {
                    case PartType.RectFoundation:
                        prefab = Prefabs.RectFoundation;
                        break;

                    case PartType.TriFoundation:
                        prefab = Prefabs.TriFoundation;
                        break;

                    case PartType.Wall:
                        prefab = Prefabs.Wall;
                        break;
                }

                var slots = prefab.GetComponentsInChildren<BuildingSlot>();
                foreach (var mySlot in slots) {
                    _partChildren.Add(-1);
                }

                if (slot != null) {
                    var indices = _partAndChildPartIdxForSlot[slot];
                    _partChildren[indices.childIdx] = _parts.Count - 1;
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