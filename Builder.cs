using Core.Gameplay;
using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Core.ModularBuildings
{
    [CreateAssetMenu(menuName = "Core.ModularBuildings/Builder")]
    public class Builder : EquippableItem
    {
        [Serializable]
        struct KeyPartBinding
        {
            public KeyCode key;
            public BuildingPartType partType;
        }

        public Material blueprintMaterial = null;
        public Material occupiedBlueprintMaterial = null;

        [SerializeField]
        BuildingPartType _currentPartType;
        GameObject _blueprint;
        Building _currentBuildingTheBlueprintIsSnappedTo;
        bool _currentBuildingSlotOccupied;
        BuildingSlot _currentBuildingClosestSlot;
        Vector3 _currentBuildingBuildPosition;
        Quaternion _currentBuildingBuildRotation;
        
        [SerializeField]
        KeyPartBinding[] _bindings;

        BuilderType _type;
        Pawn _pawn;
        
        public override void Equip(EquippableItemType itemType)
        {
            _type = (BuilderType)itemType;

            Debug.Log("Builder Equip isServer=" + isServer);
        }

        public override void Use()
        {
            if (!isClient)
                return;

            var buildingManager = SystemProvider.GetSystem<BuildingManager>(gameObject);

            var canBuild = (_currentPartType.canCreateNewBuilding && _currentBuildingTheBlueprintIsSnappedTo == null || _currentBuildingClosestSlot != null) && !_currentBuildingSlotOccupied;
            if (canBuild && Input.GetMouseButton(0)) {
                if (_currentBuildingTheBlueprintIsSnappedTo == null) {
                    _currentBuildingTheBlueprintIsSnappedTo = buildingManager.CreateBuilding(_type.buildingType, _currentBuildingBuildPosition, _currentBuildingBuildRotation);
                }
                _currentBuildingTheBlueprintIsSnappedTo.AddPart(_currentPartType, _currentBuildingClosestSlot);
                _currentBuildingTheBlueprintIsSnappedTo.Rebuild();
            }

            //
            if (_currentBuildingTheBlueprintIsSnappedTo != null && Input.GetMouseButtonDown(1)) {

                var partIdx = _currentBuildingTheBlueprintIsSnappedTo.GetClosestPartIdx(_currentBuildingBuildPosition);
                if (partIdx != -1) {
                    _currentBuildingTheBlueprintIsSnappedTo.RemovePart(partIdx);
                    _currentBuildingTheBlueprintIsSnappedTo.Rebuild();
                }
            }
        }

        void Start()
        {
            _pawn = GetComponentInParent<Pawn>();
            Assert.IsNotNull(_pawn);

            RebuildBlueprint();
        }

        void Update()
        {
            UpdatePartType();
            UploadBlueprint();
        }

        void OnDestroy()
        {
            DestroyBlueprint();
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
            DestroyBlueprint();

            if (!isClient || _currentPartType == null || _pawn.controller == null)
                return;

            var prefab = _type.buildingType.GetPrefabForPartType(_currentPartType);

            _blueprint = Instantiate(prefab);
            _blueprint.transform.localScale = Vector3.one * 3.025f;
            _blueprint.GetComponent<Renderer>().sharedMaterial = blueprintMaterial;
            foreach (var collider in _blueprint.GetComponents<Collider>()) {
                collider.enabled = false;
            }
        }

        void DestroyBlueprint()
        {
            if (_blueprint == null)
                return;

            Destroy(_blueprint);
            _blueprint = null;
        }

        void UploadBlueprint()
        {
            if (_blueprint == null)
                return;

            var buildingManager = SystemProvider.GetSystem<BuildingManager>(gameObject);

            var buildPosition = Camera.main.transform.position + Camera.main.transform.rotation * Vector3.forward * 3f;
            var buildRotation = Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y, Vector3.up);
            DebugDraw.DrawMarker(buildPosition, 0.25f, Color.blue);

            _blueprint.transform.position = buildPosition;
            _blueprint.transform.rotation = buildRotation;

            BuildingSlot closestSlot = null;
            var occupied = false;
            var building = buildingManager.GetBuildingInRange(buildPosition);
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
            
            _currentBuildingTheBlueprintIsSnappedTo = building;
            _currentBuildingSlotOccupied = occupied;
            _currentBuildingClosestSlot = closestSlot;
            _currentBuildingBuildPosition = buildPosition;
            _currentBuildingBuildRotation = buildRotation;
        }
    }
}