using UnityEngine;

namespace Core.ModularBuildings
{
    public interface IBuildingSystem
    {
#if SERVER
        Building CreateBuilding(BuildingType type, Vector3 position, Quaternion rotation);
#endif
        void RegisterBuilding(Building newBuilding);
        Building GetBuildingInRange(Vector3 position, float maxDistance);
        int GetNumChildrenForPartType(BuildingType type, BuildingPartType partType);
    }
}