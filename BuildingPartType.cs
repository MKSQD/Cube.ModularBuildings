using Cube.Networking;
using UnityEngine;

namespace Cube.ModularBuildings
{
    [CreateAssetMenu(menuName = "Cube.ModularBuildings/BuildingPartType")]
    public class BuildingPartType : NetworkObject
    {
        public bool canCreateNewBuilding = false;
    }
}