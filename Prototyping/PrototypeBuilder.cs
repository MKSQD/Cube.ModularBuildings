using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.ModularBuildings;
using System;

namespace Core.ModularBuildings
{
    class PrototypeBuilder : MonoBehaviour
    {
        public Material blueprintMaterial = null;
        public Material occupiedBlueprintMaterial = null;

        [SerializeField]
        BuildingType _buildingType;

        BuildingPartType _currentPartType;
        GameObject _blueprint;

        [Serializable]
        struct KeyPartBinding
        {
            public KeyCode key;
            public BuildingPartType partType;
        }

        [SerializeField]
        KeyPartBinding[] _bindings;

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
            foreach (var binding in _bindings) {
                if (Input.GetKeyDown(binding.key)) {
                    _currentPartType = binding.partType;
                    RebuildBlueprint();
                }
            }
        }

        void RebuildBlueprint()
        {
            if (_blueprint != null) {
                Destroy(_blueprint);
                _blueprint = null;
            }

            if (_currentPartType == null)
                return;

            var prefab = _buildingType.GetPrefabForPartType(_currentPartType);

            _blueprint = Instantiate(prefab);
            _blueprint.transform.localScale = Vector3.one * 3.025f;
            _blueprint.GetComponent<Renderer>().sharedMaterial = blueprintMaterial;
            foreach (var collider in _blueprint.GetComponents<Collider>()) {
                collider.enabled = false;
            }
        }

        void UploadBlueprint()
        {
            if (_blueprint == null)
                return;

            var buildingManager = gameObject.GetSystem<IBuildingSystem>();

            var buildPosition = Camera.main.transform.position + Camera.main.transform.rotation * Vector3.forward * 3f;
            var buildRotation = Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y, Vector3.up);
            DebugDraw.DrawMarker(buildPosition, 0.25f, Color.blue);

            _blueprint.transform.position = buildPosition;
            _blueprint.transform.rotation = buildRotation;
            
            BuildingSlot closestSlot = null;
            var occupied = false;
            var building = buildingManager.GetBuildingInRange(buildPosition, 3f);
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
            var canBuild = (_currentPartType.canCreateNewBuilding && building == null || closestSlot != null) && !occupied;
            if (canBuild && Input.GetMouseButton(0)) {
                if (building == null) {
                    //building = buildingManager.CreateBuilding(_buildingType, buildPosition, buildRotation);
                }
                building.AddPart(_currentPartType, closestSlot);
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