using Core.Gameplay;
using UnityEngine;

namespace Core.ModularBuildings
{
    [CreateAssetMenu(menuName = "Core.ModularBuildings/BuilderType")]
    public class BuilderType : EquippableItemType
    {
        public BuildingType buildingType;
    }
}