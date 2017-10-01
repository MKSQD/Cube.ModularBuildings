using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.ModularBuildings
{
    public class BuildingManager : MonoBehaviourSingleton<BuildingManager>
    {
        public List<Building> buildings = new List<Building>();

        public Building GetBuildingInRange(Vector3 position)
        {
            foreach (var building in buildings) {
                var localPos = building.transform.InverseTransformPoint(position);
                if (localPos.x >= -1.5f && localPos.x <= building.size.x + 1.5f
                    && localPos.z >= -1.5f && localPos.z <= building.size.z + 1.5f)
                    return building;
            }
            return null;
        }

        void Start()
        {
            foreach (var building in buildings) {
                building.Build();
            }
        }

        void OnDrawGizmos()
        {
            foreach (var building in buildings) {
                var tf = building.transform;
                Gizmos.matrix = Matrix4x4.TRS(tf.position + tf.rotation * new Vector3(building.size.x * 0.5f - 0.5f, 0, building.size.z * 0.5f - 0.5f), tf.rotation, Vector3.one);
                Color32 color = Color.yellow;
                color.a = 80;
                Gizmos.color = color;
                Gizmos.DrawCube(Vector3.zero, new Vector3(building.size.x + 2, 0.0001f, building.size.z + 2));

                Gizmos.matrix = Matrix4x4.identity;
                Gizmos.color = Color.white;
            }
        }
    }
}