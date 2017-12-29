using UnityEngine;

[CreateAssetMenu(menuName = "Core.ModularBuildings/BuildingPartType")]
public class BuildingPartType : NetworkObject
{
    public bool canCreateNewBuilding = false;
}
