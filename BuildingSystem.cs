using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Core.Networking;
using Core.Networking.Server;
using UnityEngine.Assertions;

namespace Core.ModularBuildings
{
    public class BuildingSystem : MonoBehaviour, IBuildingSystem
    {
        [SerializeField]
        List<Building> _buildings;

        public Building CreateBuilding(BuildingType type, Vector3 position, Quaternion rotation)
        {
            Assert.IsNotNull(type.serverBuildingPrefab);

            var server = SystemProvider.GetSystem<UnityServer>(gameObject);
            var buildingGo = server.replicaManager.InstantiateReplica(type.serverBuildingPrefab);

            buildingGo.transform.position = position;
            buildingGo.transform.rotation = rotation;

            var building = buildingGo.GetComponent<Building>();
            Assert.IsNotNull(building);

            var data = building.data;
            data.type = type;
            building.data = data;

            return building;
        }

        public void RegisterBuilding(Building building) {
            if (_buildings == null)
                _buildings = new List<Building>();

            _buildings.Add(building);
        }

        public Building GetBuildingInRange(Vector3 position, float maxDistance)
        {
            float closestDistance = float.MaxValue;
            Building closestBuilding = null;

            foreach (var building in _buildings) {
                float distance;
                var slot = building.GetClosestSlot(position, BuildingSlotType.All, true, out distance);

                if(distance <= maxDistance && distance < closestDistance) {
                    closestDistance = distance;
                    closestBuilding = building;
                }
            }

            return closestBuilding;
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
            SystemProvider.SetSystem<IBuildingSystem>(gameObject, this);
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

//         void OnApplicationQuit()
//         {
//             if (_building == null || _building.data.parts.Count == 0)
//                 return;
// 
//             var str = JsonUtility.ToJson(_building.data);
//             File.WriteAllText("test.building", str);
//         }
    }
}