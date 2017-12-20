using Core.Gameplay;
using Core.Networking;
using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Core.ModularBuildings
{
    [AddComponentMenu("Core.ModularBuildings/Builder")]
    public class Builder : EquippableItem
    {
        const float PART_SNAP_DISTANCE = 0.75f;

        [Serializable]
        struct KeyPartBinding {
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

        float _nextShotTime;

        public override void Equip(EquippableItemType itemType, Replica owner) {
            _type = (BuilderType)itemType;
            _pawn = GetComponentInParent<Pawn>();

            RebuildBlueprint();
        }

        public override void Use() {
            if (!isClient)
                return;

            if (Time.time < _nextShotTime)
                return;

            _nextShotTime = Time.time + 0.2f;
            
            if (_currentBuildingTheBlueprintIsSnappedTo == null && _currentPartType.canCreateNewBuilding)
                BuildNew();
            else if (_currentBuildingClosestSlot != null && !_currentBuildingSlotOccupied)
                Build();
        }

        void BuildNew() {
            RpcBuildNew(_currentBuildingBuildPosition, _currentBuildingBuildRotation, _currentPartType);
        }

        void Build() {
            var buildingReplica = _currentBuildingTheBlueprintIsSnappedTo.GetComponent<Replica>();
            RpcBuild(buildingReplica, _currentPartType, _currentBuildingClosestSlot.type, _currentBuildingBuildPosition);

            _currentBuildingTheBlueprintIsSnappedTo.AddPart(_currentPartType, _currentBuildingClosestSlot);
            _currentBuildingTheBlueprintIsSnappedTo.Rebuild();
        }

        [ReplicaRpc(RpcTarget.Server)]
        void RpcBuildNew(Vector3 position, Quaternion rotation, BuildingPartType partType) {
            var buildingManager = SystemProvider.GetSystem<IBuildingSystem>(gameObject);

            var newBuilding = buildingManager.CreateBuilding(_type.buildingType, position, rotation);
            newBuilding.AddPart(partType, null);
            newBuilding.Rebuild();
        }

        [ReplicaRpc(RpcTarget.Server)]
        void RpcBuild(Replica buildingReplica, BuildingPartType partType, BuildingSlotType buildingSlotType, Vector3 slotPosition) {
            var building = buildingReplica.GetComponent<Building>();
            
            float distance = 0;
            var slot = building.GetClosestSlot(slotPosition, buildingSlotType, true, out distance);
            //#TODO check distance

            Assert.IsNotNull(slot);

            building.AddPart(partType, slot);
            building.Rebuild();
        }

        void Update() {
            
            UpdatePartType();
            UpdateBlueprint();
        }

        void OnDestroy() {
            DestroyBlueprint();
        }

        void UpdatePartType() {
            foreach (var binding in _bindings) {
                if (Input.GetKeyDown(binding.key)) {
                    _currentPartType = binding.partType;
                    RebuildBlueprint();
                }
            }
        }

        void RebuildBlueprint() {
            DestroyBlueprint();

            if (!isClient || _currentPartType == null || _pawn.controller == null)
                return;

            var prefab = _type.buildingType.GetPrefabForPartType(_currentPartType);

            _blueprint = Instantiate(prefab);
            _blueprint.GetComponent<Renderer>().sharedMaterial = blueprintMaterial;
            foreach (var collider in _blueprint.GetComponents<Collider>()) {
                collider.enabled = false;
            }
        }

        void DestroyBlueprint() {
            if (_blueprint == null)
                return;

            Destroy(_blueprint);
            _blueprint = null;
        }

        void UpdateBlueprint() {
            if (_blueprint == null)
                return;

            var buildingManager = SystemProvider.GetSystem<IBuildingSystem>(gameObject);

            var buildPosition = Camera.main.transform.position + Camera.main.transform.rotation * Vector3.forward * 3f;
            var buildRotation = Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y, Vector3.up);
            DebugDraw.DrawMarker(buildPosition, 0.25f, Color.blue);

            _blueprint.transform.position = buildPosition;
            _blueprint.transform.rotation = buildRotation;

            BuildingSlot closestSlot = null;
            var occupied = false;
            var building = buildingManager.GetBuildingInRange(buildPosition, PART_SNAP_DISTANCE * 2f);
            if (building != null) {

                float closestDistance = float.MaxValue;

                var sockets = _blueprint.GetComponentsInChildren<BuildingSocket>();
                foreach (var socket in sockets) {
                    float distance;
                    var slot = building.GetClosestSlot(socket.transform.position, socket.slotType, true, out distance);
                    if (slot == null)
                        continue;

                    if (distance <= PART_SNAP_DISTANCE && distance < closestDistance) {
                        closestDistance = distance;
                        closestSlot = slot;
                    }
                }

                //Debug.Log(closestSlot);

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