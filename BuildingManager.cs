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
        
    }
}