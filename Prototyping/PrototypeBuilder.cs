﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.ModularBuildings;

namespace Core.ModularBuildings
{
    class PrototypeBuilder : MonoBehaviour
    {
        public Material blueprintMaterial, occupiedBlueprintMaterial;

        Building.PartType _currentPartType = Building.PartType.RectFoundation;
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
                _currentPartType = Building.PartType.RectFoundation;
                RebuildBlueprint();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2)) {
                _currentPartType = Building.PartType.TriFoundation;
                RebuildBlueprint();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3)) {
                _currentPartType = Building.PartType.Wall;
                RebuildBlueprint();
            }
            if (Input.GetKeyDown(KeyCode.Alpha4)) {
                _currentPartType = Building.PartType.StairFoundation;
                RebuildBlueprint();
            }
            if (Input.GetKeyDown(KeyCode.Alpha5)) {
                _currentPartType = Building.PartType.WindowWall;
                RebuildBlueprint();
            }
        }

        void RebuildBlueprint()
        {
            if (_blueprint != null) {
                Destroy(_blueprint);
                _blueprint = null;
            }

            var prefab = BuildingManager.instance.GetPrefabForPartType(_currentPartType);

            _blueprint = Instantiate(prefab);
            _blueprint.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            _blueprint.GetComponent<Renderer>().sharedMaterial = blueprintMaterial;
        }

        void UploadBlueprint()
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
            var canBuild = ((_currentPartType == Building.PartType.RectFoundation || _currentPartType == Building.PartType.TriFoundation) && building == null || closestSlot != null) && !occupied;
            if (canBuild && Input.GetMouseButton(0)) {
                if (building == null) {
                    building = BuildingManager.instance.CreateBuilding(_blueprint.transform.position, _blueprint.transform.rotation);
                }
                building.AddPart(_currentPartType, closestSlot);
                building.Rebuild();
            }

            //
            if (building != null && Input.GetMouseButtonDown(1)) {

                var partIdx = building.GetClosestPartIdx(pos);
                if(partIdx != -1) {
                    building.RemovePart(partIdx);
                    building.Rebuild();
                }
            }
        }
    }
}