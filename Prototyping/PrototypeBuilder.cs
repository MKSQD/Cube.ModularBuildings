using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.ModularBuildings;

namespace Core.ModularBuildings
{
    class PrototypeBuilder : MonoBehaviour
    {
        public Material blueprintMaterial, occupiedBlueprintMaterial;

        PrototypingBuildingPartType _currentPartType = PrototypingBuildingPartType.RectFoundation;
        GameObject _blueprint;

        void Start()
        {
            RebuildBlueprint();
        }

        void Update()
        {
            UpdatePartType();
            UploadBlueprint();
        }

        void UpdatePartType()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                _currentPartType = PrototypingBuildingPartType.RectFoundation;
                RebuildBlueprint();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2)) {
                _currentPartType = PrototypingBuildingPartType.TriFoundation;
                RebuildBlueprint();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3)) {
                _currentPartType = PrototypingBuildingPartType.Wall;
                RebuildBlueprint();
            }
            if (Input.GetKeyDown(KeyCode.Alpha4)) {
                _currentPartType = PrototypingBuildingPartType.StairFoundation;
                RebuildBlueprint();
            }
            if (Input.GetKeyDown(KeyCode.Alpha5)) {
                _currentPartType = PrototypingBuildingPartType.WindowWall;
                RebuildBlueprint();
            }
        }

        void RebuildBlueprint()
        {
            if (_blueprint != null) {
                Destroy(_blueprint);
                _blueprint = null;
            }

            var prefab = BuildingPartTypes.GetPrefab(BuildingType.Prototyping, (byte)_currentPartType);

            _blueprint = Instantiate(prefab);
            _blueprint.transform.localScale = new Vector3(1.05f, 1.05f, 1.05f);
            _blueprint.GetComponent<Renderer>().sharedMaterial = blueprintMaterial;
        }

        void UploadBlueprint()
        {
            var buildPosition = Camera.main.transform.position + Camera.main.transform.rotation * Vector3.forward * 3f;
            var buildRotation = Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y, Vector3.up);
            DebugDraw.DrawMarker(buildPosition, 0.25f, Color.blue);

            _blueprint.transform.position = buildPosition;
            _blueprint.transform.rotation = buildRotation;

            BuildingSlot closestSlot = null;
            var occupied = false;
            var building = BuildingManager.instance.GetBuildingInRange(buildPosition);
            if (building != null) {
                float closestDistance = float.MaxValue;

                var sockets = _blueprint.GetComponentsInChildren<BuildingSocket>();
                foreach (var socket in sockets) {
                    float distance;
                    var slot = building.GetClosestSlot(socket.transform.position, socket.slotType, true, out distance);
                    if (slot == null)
                        continue;

                    if (distance < 0.25f && distance < closestDistance) {
                        closestDistance = distance;
                        closestSlot = slot;
                    }
                }

                if (closestSlot != null) {
                    buildPosition = closestSlot.transform.position;
                    buildRotation = closestSlot.transform.rotation;
                    occupied = !building.IsSlotFree(closestSlot);
                }
            }
            _blueprint.transform.position = buildPosition + Vector3.up * 0.025f;
            _blueprint.transform.rotation = buildRotation;
            _blueprint.GetComponent<Renderer>().sharedMaterial = !occupied ? blueprintMaterial : occupiedBlueprintMaterial;

            //
            var canBuild = ((_currentPartType == PrototypingBuildingPartType.RectFoundation || _currentPartType == PrototypingBuildingPartType.TriFoundation) && building == null || closestSlot != null) && !occupied;
            if (canBuild && Input.GetMouseButton(0)) {
                if (building == null) {
                    building = BuildingManager.instance.CreateBuilding(BuildingType.Prototyping, buildPosition, buildRotation);
                }
                building.AddPart((byte)_currentPartType, closestSlot);
                building.Rebuild();
            }

            //
            if (building != null && Input.GetMouseButtonDown(1)) {

                var partIdx = building.GetClosestPartIdx(buildPosition);
                if (partIdx != -1) {
                    building.RemovePart(partIdx);
                    building.Rebuild();
                }
            }
        }
    }
}