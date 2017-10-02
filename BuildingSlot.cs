using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingSlotType
{
    Foundation,
    Wall
}

[AddComponentMenu("Core/Building/Slot")]
public class BuildingSlot : MonoBehaviour
{
    public BuildingSlotType type;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.05f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.2f);
    }
}
