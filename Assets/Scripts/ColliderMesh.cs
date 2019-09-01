using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class ColliderMesh : MonoBehaviour {

    PolygonCollider2D polygonCollider;

    void Start()
    {
        polygonCollider = gameObject.GetComponent<PolygonCollider2D>();
        //Render thing
        int pointCount = 0;
        pointCount = polygonCollider.GetTotalPointCount();
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        Vector2[] points = polygonCollider.points;
        Vector3[] vertices = new Vector3[pointCount];
        Vector2[] uv = new Vector2[pointCount];
        for (int j = 0; j < pointCount; j++)
        {
            Vector2 actual = points[j];
            vertices[j] = new Vector3(actual.x, actual.y, 0);
            uv[j] = actual;
        }
        Triangulator tr = new Triangulator(points);
        int[] triangles = tr.Triangulate();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.name = "ColliderMesh";
        mf.mesh = mesh;
        //Render thing
    }
}