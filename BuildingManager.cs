using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.ModularBuildings
{
    public class BuildingManager : MonoBehaviourSingleton<BuildingManager>
    {
        public Building building;

        public Building GetBuildingInRange(Vector3 position)
        {
            return building;
        }

        public int GetNumChildrenForPartType(PartType type)
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
            return slots.Length;
        }
    }
}