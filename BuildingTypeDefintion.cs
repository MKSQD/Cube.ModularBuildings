using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct BuildingPartTypeDefinition
{
    public string name;
    public GameObject prefab;
}

[CreateAssetMenu(menuName = "Core.Building/Type Definition")]
public class BuildingTypeDefintion : ScriptableObject
{
    public string name;
    public List<BuildingPartTypeDefinition> partTypes;
}
