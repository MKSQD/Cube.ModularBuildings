using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Core/Building/Socket")]
public class BuildingSocket : MonoBehaviour
{
    public BuildingSlotType slotType;

    void OnDrawGizmos()
    {
        var c = Color.green;
        c.a = 80;
        Gizmos.color = c;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.04f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.04f);
    }
}
