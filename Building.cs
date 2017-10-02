using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.ModularBuildings
{
    enum PartType
    {
        RectFoundation,
        TriFoundation,
        Floor,
        Wall
    }
    
//     class AstNode
//     { }
// 
//     class AstRectFoundation : AstNode
//     {
//         public AstNode[] foundations = new AstNode[4];
//         public AstNode[] walls = new AstNode[4];
//     }
// 
//     class AstTriFoundation : AstNode
//     {
//         public AstNode[] walls;
//     }
// 
//     class AstWall : AstNode
//     { }
// 

    [Serializable]
    public class Building : MonoBehaviour
    {
        List<BuildingSlot> _slots = new List<BuildingSlot>();

        public BuildingSlot FindSlot(Vector3 position, BuildingSlotType slotType, out float closestDistance)
        {
            closestDistance = float.MaxValue;
            BuildingSlot closestSlot = null;
            foreach (var slot in _slots) {
                if (slot.type != slotType)
                    continue;

                var dist = (slot.transform.position - position).sqrMagnitude;
                if (dist < closestDistance) {
                    closestDistance = dist;
                    closestSlot = slot;
                }
            }
            return closestSlot;
        }

        public void AddPart(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var newPrefab = Instantiate(prefab, transform);
            newPrefab.transform.position = position;
            newPrefab.transform.rotation = rotation;

            var newSlots = newPrefab.GetComponentsInChildren<BuildingSlot>();
            _slots.AddRange(newSlots);
        }

        void Start()
        {
            BuildingManager.instance.building = this;
        }
    }
}