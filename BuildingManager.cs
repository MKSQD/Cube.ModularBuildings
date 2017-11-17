using System.IO;
using UnityEngine;

namespace Core.ModularBuildings
{
    public class BuildingManager : MonoBehaviour
    {
        [HideInInspector]
        public Building building;

        public Building CreateBuilding(BuildingType type, Vector3 position, Quaternion rotation)
        {
            var buildingGO = new GameObject("Building");

            var buildingTransform = buildingGO.transform;
            buildingTransform.parent = transform.parent;
            buildingTransform.position = position;
            buildingTransform.rotation = rotation;
            buildingTransform.localScale = Vector3.one * 3;

            building = buildingGO.AddComponent<Building>();

            var data = building.data;
            data.type = type;
            building.data = data;

            return building;
        }

        public Building GetBuildingInRange(Vector3 position)
        {
            return building;
        }
        
        public int GetNumChildrenForPartType(BuildingType type, BuildingPartType partType)
        {
            var prefab = type.GetPrefabForPartType(partType);
            if (prefab == null)
                return 0;

            var slots = prefab.GetComponentsInChildren<BuildingSlot>();
            return slots.Length;
        }

        void Awake()
        {
            SystemProvider.SetSystem(gameObject, this);
        }

        //         void Start()
        //         {
        //             try {
        //                 if (building == null) {
        //                     CreateBuilding(BuildingType.Prototyping, Vector3.zero, Quaternion.identity);
        //                 }
        // 
        //                 var str = File.ReadAllText("test.building");
        //                 building.data = JsonUtility.FromJson<Building.BuildingData>(str);
        //                 building.Rebuild();
        //             }
        //             catch (Exception e) {
        //                 Debug.LogError("Failed to load building: " + e);
        //                 Destroy(building);
        //             }
        //         }

        void OnApplicationQuit()
        {
            if (building == null || building.data.parts.Count == 0)
                return;

            var str = JsonUtility.ToJson(building.data);
            File.WriteAllText("test.building", str);
        }
    }
}