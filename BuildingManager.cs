using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Core.ModularBuildings
{
    public class BuildingManager : MonoBehaviourSingleton<BuildingManager>
    {
        public Building building;

        public Building CreateBuilding(Vector3 position, Quaternion rotation)
        {
            var buildingGO = new GameObject("Building");
            buildingGO.transform.position = position;
            buildingGO.transform.rotation = rotation;

            building = buildingGO.AddComponent<Building>();
            return building;
        }

        public Building GetBuildingInRange(Vector3 position)
        {
            return building;
        }

        public GameObject GetPrefabForPartType(Building.PartType type)
        {
            GameObject prefab = null;
            switch (type) {
                case Building.PartType.RectFoundation:
                    prefab = Prefabs.RectFoundation;
                    break;

                case Building.PartType.TriFoundation:
                    prefab = Prefabs.TriFoundation;
                    break;

                case Building.PartType.Wall:
                    prefab = Prefabs.Wall;
                    break;

                case Building.PartType.WindowWall:
                    prefab = Prefabs.WindowWall;
                    break;
            }
            return prefab;
        }

        public int GetNumChildrenForPartType(Building.PartType type)
        {
            var prefab = GetPrefabForPartType(type);
            if (prefab == null)
                return 0;

            var slots = prefab.GetComponentsInChildren<BuildingSlot>();
            return slots.Length;
        }

        void Start()
        {
            try {
                if (building == null) {
                    CreateBuilding(Vector3.zero, Quaternion.identity);
                }

                var str = File.ReadAllText("test.building");
                building.data = JsonUtility.FromJson<Building.BuildingData>(str);
                building.Rebuild();
            }
            catch(Exception e) {
                Debug.LogError("Failed to load building: " + e);
                Destroy(building);
            }
        }

        void OnApplicationQuit()
        {
            if (building == null || building.data.parts.Count == 0)
                return;
            
            var str = JsonUtility.ToJson(building.data);
            File.WriteAllText("test.building", str);
        }
    }
}