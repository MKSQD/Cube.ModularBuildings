using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Core.Gameplay;

namespace Core.ModularBuildings
{
    [CreateAssetMenu(menuName = "Core.Gameplay/BuilderItemTypeExtension")]
    public class BuilderItemTypeExtension : ItemTypeExtension
    {
        public BuildingType buildingType;
    }
}
