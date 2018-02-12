using UnityEngine;

[CreateAssetMenu(menuName = "Cube.ModularBuildings/BuildingPartType")]
public class BuildingPartType : NetworkObject
{
    public bool canCreateNewBuilding = false;
}
