using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingSlotType
{
    None,
    Foundation,
    Wall
}

[AddComponentMenu("Core/Building/Slot")]
public class BuildingSlot : MonoBehaviour
{
    public BuildingSlotType type;
    public bool ignoreForPlacement;

    [HideInInspector]
    public ushort partIdx = ushort.MaxValue;
    [HideInInspector]
    public byte childIdx = byte.MaxValue;

    void OnDrawGizmos()
    {
        var c = Color.yellow;
        c.a = 80;
        Gizmos.color = c;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.05f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.2f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.05f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.2f);
    }
}
