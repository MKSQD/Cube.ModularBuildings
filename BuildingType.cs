using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Core.ModularBuildings/Building Type")]
public class BuildingType : NetworkObject
{
    [Serializable]
    struct Entry
    {
        public BuildingPartType partType;
        public GameObject prefab;
    }

    public GameObject serverBuildingPrefab;

    [SerializeField]
    Entry[] _partTypes;

    public GameObject GetPrefabForPartType(BuildingPartType partType)
    {
        foreach (var partT in _partTypes) {
            if (partT.partType == partType)
                return partT.prefab;
        }
        return null;
    }
}
