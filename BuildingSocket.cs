using UnityEngine;

[AddComponentMenu("Cube.Building/Socket")]
public class BuildingSocket : MonoBehaviour
{
    public BuildingSlotType slotType;

    [HideInInspector]
    public ushort partIdx = ushort.MaxValue;

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
