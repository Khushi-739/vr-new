using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PalmMesh : MonoBehaviour
{
    [Tooltip("Assign in this order: Wrist, IndexBase, MiddleBase, RingBase, PinkyBase")]
    public Transform[] palmPoints; // size = 5

    Mesh mesh;

    void Awake()
    {
        mesh = new Mesh();
        mesh.name = "PalmMesh";
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void LateUpdate()
    {
        if (palmPoints == null || palmPoints.Length != 5)
            return;

        Vector3 wrist  = ToLocal(palmPoints[0]);
        Vector3 index  = ToLocal(palmPoints[1]);
        Vector3 middle = ToLocal(palmPoints[2]);
        Vector3 ring   = ToLocal(palmPoints[3]);
        Vector3 pinky  = ToLocal(palmPoints[4]);

        Vector3 center =
            (wrist + index + middle + ring + pinky) / 5f;

        Vector3[] vertices = new Vector3[]
        {
            center, // 0
            index,  // 1
            middle, // 2
            ring,   // 3
            pinky,  // 4
            wrist   // 5
        };

        int[] triangles = new int[]
        {
            0,1,2,
            0,2,3,
            0,3,4,
            0,4,5,
            0,5,1
        };

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    Vector3 ToLocal(Transform t)
    {
        return transform.InverseTransformPoint(t.position);
    }
}