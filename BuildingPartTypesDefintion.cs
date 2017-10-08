using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct BuildingPartTypeDefinition
{
    public string name;
    public GameObject prefab;
}

[CreateAssetMenu(menuName = "Core/BuildingPartTypes")]
public class BuildingPartTypesDefintion : ScriptableObject
{
    public List<BuildingPartTypeDefinition> partTypes;
}
