using UnityEngine;

namespace Core.ModularBuildings
{
    public interface IBuildingSystem
    {
        Building CreateBuilding(BuildingType type, Vector3 position, Quaternion rotation);
        void RegisterBuilding(Building newBuilding);
        Building GetBuildingInRange(Vector3 position);
        int GetNumChildrenForPartType(BuildingType type, BuildingPartType partType);
    }
}