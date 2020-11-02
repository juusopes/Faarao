using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class FieldOfViewRenderer : MonoBehaviour
{
    Mesh mesh;
    public Character character;
    private Vector3 origin;
    private float startingAngle = 0;
    private int rayCount = 10;
    float angleIncrease;
    Vector3[] vertices;
    Vector2[] uv;
    int[] triangles;
    float angle;
    private Vector3 offset = new Vector3(0, -0.2f, 0);
    private float FOV => character.FOV;
    private float SightRange => character.SightRange;
    private float SightRangeCrouching => character.SightRangeCrouching;

    void Awake()
    {
        Assert.IsNotNull(character, "Character is not set!");
    }

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        origin = Vector3.zero;
        vertices = new Vector3[rayCount + 1 + 1];
        vertices[0] = origin;
        uv = new Vector2[vertices.Length];
        triangles = new int[rayCount * 3];
        //UpdateViewCone();
        //SaveAsset();
    }

    private void LateUpdate()
    {
        UpdateViewCone();
    }

    private void UpdateViewCone()
    {
        SetOrigin(transform.position + offset);
        SetAimDirection(transform.forward, FOV);
        UpdateMesh();
    }

    void UpdateMesh()
    {
        angle = startingAngle;
        vertices[0] = Vector3.zero;
        uv[0] = Vector2.zero;

        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 vertex;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector2.right;
            //Debug.DrawRay(origin, direction * viewDistance, Color.green, 10f);
            RaycastHit raycastHit;
            float uvLenght;

            if (Physics.Raycast(origin, direction, out raycastHit, SightRange, RayCaster.viewConeLayerMask))
            {
                vertex = transform.InverseTransformPoint(origin + direction * raycastHit.distance);
                uvLenght = raycastHit.distance / SightRange;
            }
            else
            {
                vertex = transform.InverseTransformPoint(origin + direction * SightRange);
                uvLenght = 1;
            }


            //Create Cone shaped uvs
            float uvAngle = i * angleIncrease;
            float rad = uvAngle * Mathf.Deg2Rad;
            uv[vertexIndex] = new Vector2(Mathf.Sin(rad) * uvLenght, Mathf.Cos(rad) * uvLenght);


            vertices[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }

            vertexIndex++;
            angle -= angleIncrease;
        }

        //foreach (Vector3 vert in vertices)
        //{
         //   GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
         //   sphere.transform.position = vert + transform.position;
        //}

        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
    }

    public void SetOrigin(Vector3 origin)
    {
        this.origin = origin;
    }

    public void SetAimDirection(Vector3 aimDirection, float fovIn)
    {
        startingAngle = transform.rotation.eulerAngles.y + fovIn / 2f - 90f;
        angleIncrease = fovIn / rayCount;
    }


    void SaveAsset()
    {
        var mf = GetComponent<MeshFilter>();
        if (mf)
        {
            var savePath = "Assets/" + "viewConeMesh.asset";
            Debug.Log("Saved Mesh to:" + savePath);
            AssetDatabase.CreateAsset(mf.mesh, savePath);
        }
    }
}



