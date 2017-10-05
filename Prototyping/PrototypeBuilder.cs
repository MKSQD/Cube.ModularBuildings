using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.ModularBuildings;

namespace Core.ModularBuildings
{
    class PrototypeBuilder : MonoBehaviour
    {
        public Material blueprintMaterial, occupiedBlueprintMaterial;

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
            _blueprint.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            _blueprint.GetComponent<Renderer>().sharedMaterial = blueprintMaterial;
        }

        void UpdateFoo()
        {
            var pos = Camera.main.transform.position + Camera.main.transform.rotation * Vector3.forward * 3f;
            var rot = Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y, Vector3.up);
            DebugDraw.DrawMarker(pos, 0.25f, Color.blue);

            _blueprint.transform.position = pos;
            _blueprint.transform.rotation = rot;

            BuildingSlot closestSlot = null;
            var occupied = false;
            var building = BuildingManager.instance.GetBuildingInRange(pos);
            if (building != null) {
                float closestDistance = float.MaxValue;

                var sockets = _blueprint.GetComponentsInChildren<BuildingSocket>();
                foreach (var socket in sockets) {
                    float distance;
                    var slot = building.GetClosestSlot(socket.transform.position, socket.slotType, true, out distance);
                    if (slot == null)
                        continue;

                    if (slot.type == BuildingSlotType.Wall) {
                        var a = Vector3.Dot(slot.transform.forward, Camera.main.transform.forward);
                        if (a > 0)
                            continue;
                    }

                    if (distance < 0.25f && distance < closestDistance) {
                        closestDistance = distance;
                        closestSlot = slot;
                    }
                }

                if (closestSlot != null) {
                    pos = closestSlot.transform.position;
                    rot = closestSlot.transform.rotation;
                    occupied = !building.IsSlotFree(closestSlot);
                }
            }
            _blueprint.transform.position = pos;
            _blueprint.transform.rotation = rot;
            _blueprint.GetComponent<Renderer>().sharedMaterial = !occupied ? blueprintMaterial : occupiedBlueprintMaterial;

            //
            if (Input.GetMouseButtonDown(0)) {
                if (building == null) {
                    var buildingGO = new GameObject("Building");
                    buildingGO.transform.position = _blueprint.transform.position;
                    buildingGO.transform.rotation = _blueprint.transform.rotation;

                    building = buildingGO.AddComponent<Building>();
                }

                building.AddPart(_currentPartType, closestSlot);
                building.Rebuild();
            }
        }
    }
}