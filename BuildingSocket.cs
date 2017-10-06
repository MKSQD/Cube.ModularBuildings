using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Core/Building/Socket")]
public class BuildingSocket : MonoBehaviour
{
    public BuildingSlotType slotType;

    void OnDrawGizmos()
    {
        var c = Color.blue;
        c.a = 80;
        Gizmos.color = c;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.05f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.05f);
    }
}
