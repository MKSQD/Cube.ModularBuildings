using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.ModularBuildings;

namespace Core.ModularBuilding
{
    public class PrototypeBuilder : MonoBehaviour
    {
        PartType _currentPartType = PartType.RectFoundation;
        GameObject _blueprint;

        void Start()
        {
            RebuildBlueprint();
        }

        void Update()
        {
            UpdatePartType();
            UpdateFoo();
        }

        void UpdatePartType()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                _currentPartType = PartType.RectFoundation;
                RebuildBlueprint();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2)) {
                _currentPartType = PartType.TriFoundation;
                RebuildBlueprint();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3)) {
                _currentPartType = PartType.Wall;
                RebuildBlueprint();
            }
            if (Input.GetKeyDown(KeyCode.Alpha4)) {
                _currentPartType = PartType.Floor;
                RebuildBlueprint();
            }
        }

        void RebuildBlueprint()
        {
            if (_blueprint != null) {
                Destroy(_blueprint);
                _blueprint = null;
            }

            GameObject prefab = null;
            switch (_currentPartType) {
                case PartType.RectFoundation:
                    prefab = Prefabs.RectFoundation;
                    break;

                case PartType.TriFoundation:
                    prefab = Prefabs.TriFoundation;
                    break;

                case PartType.Wall:
                    prefab = Prefabs.Wall;
                    break;
            }

            _blueprint = Instantiate(prefab);
            _blueprint.transform.localScale = new Vector3(1.05f, 1.05f, 1.05f);
        }

        void UpdateFoo()
        {
            var pos = Camera.main.transform.position + Camera.main.transform.rotation * Vector3.forward * 3f;
            var rot = Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y, Vector3.up);
            DebugDraw.DrawMarker(pos, 0.25f, Color.blue);

            _blueprint.transform.position = pos;
            _blueprint.transform.rotation = rot;

            var building = BuildingManager.instance.GetBuildingInRange(pos);
            if (building != null) {
                float closestDistance = float.MaxValue;
                BuildingSocket closestSocket = null;
                BuildingSlot closestSlot = null;

                var sockets = _blueprint.GetComponentsInChildren<BuildingSocket>();
                foreach (var socket in sockets) {
                    float distance;
                    var slot = building.FindSlot(socket.transform.position, socket.slotType, out distance);
                    if (slot == null)
                        continue;

                    if (distance < 0.25f && distance < closestDistance) {
                        DebugDraw.DrawMarker(slot.transform.position, 0.25f, Color.green);
                        closestDistance = distance;
                        closestSocket = socket;
                        closestSlot = slot;
                    }
                }

                if (closestSocket != null) {
                    pos = closestSlot.transform.position;
                    rot = closestSlot.transform.rotation;

                    if (_currentPartType == PartType.Wall) {
                        var a = Vector3.Dot(closestSlot.transform.forward, Camera.main.transform.forward);
                        if (a > 0) {
                            rot *= Quaternion.AngleAxis(180, Vector3.up);
                        }
                    }
                }
            }
            _blueprint.transform.position = pos;
            _blueprint.transform.rotation = rot;

            //
            if (Input.GetMouseButtonDown(0)) {
                if (building == null) {
                    var buildingGO = new GameObject("Building");
                    buildingGO.transform.position = _blueprint.transform.position;
                    buildingGO.transform.rotation = _blueprint.transform.rotation;

                    building = buildingGO.AddComponent<Building>();
                }

                GameObject prefab = null;
                switch (_currentPartType) {
                    case PartType.RectFoundation:
                        prefab = Prefabs.RectFoundation;
                        break;

                    case PartType.TriFoundation:
                        prefab = Prefabs.TriFoundation;
                        break;

                    case PartType.Wall:
                        prefab = Prefabs.Wall;
                        break;
                }

                building.AddPart(prefab, _blueprint.transform.position, _blueprint.transform.rotation);
            }
        }
    }
}