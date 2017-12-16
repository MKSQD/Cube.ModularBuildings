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
        //Pawn _pawn;

        float _nextShotTime;

        public override void Equip(EquippableItemType itemType) {
            _type = (BuilderType)itemType;

            //_pawn = GetComponentInParent<Pawn>();
            //Assert.IsNotNull(_pawn);

            RebuildBlueprint();
        }

        public override void Use() {
            if (!isClient)
                return;

            if (Time.time < _nextShotTime)
                return;

            _nextShotTime = Time.time + 1f;


            if (_currentBuildingTheBlueprintIsSnappedTo == null && _currentPartType.canCreateNewBuilding)
            {
                Debug.Log("RpcBuildNew");
                RpcBuildNew(_currentBuildingBuildPosition, _currentBuildingBuildRotation, _currentPartType);

            }
//             else if (_currentBuildingClosestSlot != null && !_currentBuildingSlotOccupied)
//                 {
//                 var buildingReplica = _currentBuildingTheBlueprintIsSnappedTo.GetComponent<Replica>();
//                 RpcBuild(buildingReplica, _currentPartType, _currentBuildingClosestSlot);
//             }
        }

        [ReplicaRpc(RpcTarget.Server)]
        void RpcBuildNew(Vector3 position, Quaternion rotation, BuildingPartType partType) {
            var buildingManager = SystemProvider.GetSystem<IBuildingSystem>(gameObject);

            var newBuilding = buildingManager.CreateBuilding(_type.buildingType, _currentBuildingBuildPosition, _currentBuildingBuildRotation);
            newBuilding.AddPart(partType, _currentBuildingClosestSlot);
            newBuilding.Rebuild();
        }

        [ReplicaRpc(RpcTarget.Server)]
        void RpcBuild(Replica buildingReplica, BuildingPartType partType, BuildingSlot slot) {
            var building = buildingReplica.GetComponent<Building>();
            building.AddPart(partType, slot);
            building.Rebuild();
        }

        void Update() {
            UpdatePartType();
            UploadBlueprint();
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

            if (!isClient || _currentPartType == null /* || _pawn.controller == null*/)
                return;

            var prefab = _type.buildingType.GetPrefabForPartType(_currentPartType);

            _blueprint = Instantiate(prefab);
            _blueprint.transform.localScale = Vector3.one * 3.025f;
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

        void UploadBlueprint() {
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