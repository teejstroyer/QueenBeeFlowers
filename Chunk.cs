using System.Collections.Generic;
using System.Linq;
using Godot;

public class Chunk : Spatial
{
  public bool ShouldRemove = false;
  private readonly int _Size;
  private readonly int _Subdivisions;
  private readonly OpenSimplexNoise _Noise;
  private readonly int _MaxHeight;
  private readonly Material _Material;
  private readonly Mesh _GrassMesh;


  public Chunk(OpenSimplexNoise noise, Vector3 position, int maxHeight, int chunkSize, int subdivisions, Material material, Mesh grassMesh)
  {
    this.Translation = position;
    _Noise = noise;
    _Size = chunkSize;
    _Subdivisions = subdivisions;
    _MaxHeight = maxHeight;
    _Material = material;
    _GrassMesh = grassMesh;
  }

  public override void _Ready()
  {
    var planeMesh = new PlaneMesh
    {
      Size = new Vector2(_Size, _Size),
      SubdivideDepth = _Subdivisions,
      SubdivideWidth = _Subdivisions,
    };

    var surfaceTool = new SurfaceTool();
    surfaceTool.CreateFrom(planeMesh, 0);

    var arrayPlane = surfaceTool.Commit();
    var dataTool = new MeshDataTool();

    dataTool.CreateFromSurface(arrayPlane, 0);
    for (int i = 0; i < dataTool.GetVertexCount(); i++)
    {
      var vertex = dataTool.GetVertex(i);
      vertex.y = _Noise.GetNoise3d(vertex.x + Translation.x, vertex.y, vertex.z + Translation.z) * _MaxHeight;
      dataTool.SetVertex(i, vertex);
    }

    for (int i = 0; i < arrayPlane.GetSurfaceCount(); i++)
    {
      arrayPlane.SurfaceRemove(i);
    }

    dataTool.CommitToSurface(arrayPlane);
    surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
    surfaceTool.CreateFrom(arrayPlane, 0);
    surfaceTool.GenerateNormals();


    var meshInstance = new MeshInstance()
    {
      Mesh = surfaceTool.Commit()
    };

    meshInstance.SetSurfaceMaterial(0, _Material);
    meshInstance.CreateTrimeshCollision();
    AddChild(meshInstance);

    ArrayMesh a = (ArrayMesh)meshInstance.Mesh;



    //Decorate MULTIMESH
  }

  public void PopulateGrass(MeshInstance targetSurface)
  {
    //https://github.com/godotengine/godot/blob/master/editor/plugins/multimesh_editor_plugin.cpp
    Transform geoXform = GlobalTransform.AffineInverse() * targetSurface.GlobalTransform;
    var faceVertices = targetSurface.Mesh.GetFaces();
    var mesh = targetSurface.Mesh;


    //No faces so exit
    if (faceVertices.Length == 0) return;

    var faces = new Vector3[faceVertices.Length / 3, 3];

    for (int i = 0; i < faceVertices.Length; i++)
    {
      for (int j = 0; j < 3; j++)
      {
        faces[i, j] = faceVertices[i * 3 + j];
      }
    }




    for (int i = 0; i < faces.Length; i++)
    {
      for (int j = 0; j < 3; j++)
      {
        faces[i, j] = geoXform.Xform(faces[i, j]);
      }
    }

    float areaAccumulation = 0;
    Dictionary<float, int> triangleAreaMap = new Dictionary<float, int>();

    for (int i = 0; i < faces.Length; i++)
    {
      float area = CalculateTriangleArea(faces[i, 0], faces[i, 1], faces[i, 2]);

      if (area < Mathf.Epsilon) continue;
      triangleAreaMap.Add(areaAccumulation, i);
      areaAccumulation += area;
    }

    if (triangleAreaMap.Count == 0 || areaAccumulation == 0) return;

    var multimesh = new MultiMesh();
    multimesh.Mesh = mesh;


    int instanceCount = 100;
    float tiltRandom = 25;
    float rotateRandom = 25;
    float scaleRandom = 25;

    int axis = 0;

    bool xAxis = false;

    Transform axisXform = new Transform();
    if (xAxis)
    {
      axisXform.Rotated(new Vector3(1, 0, 0), -Mathf.Pi * 0.5f);
    }
    else
    {
      axisXform.Rotated(new Vector3(0, 0, 1), -Mathf.Pi * 0.5f);
    }

    RandomNumberGenerator rng = new RandomNumberGenerator();

    for (int i = 0; i < instanceCount; i++)
    {
      float areapos = rng.Randf() * areaAccumulation;

      //Find closest value
      var closest = triangleAreaMap.OrderBy(e => Mathf.Abs(e.Key - areapos)).FirstOrDefault();



    }





  }

  // Calculate the area of a triangle give the 3 vertices
  public float CalculateTriangleArea(Vector3 v1, Vector3 v2, Vector3 v3)
  {
    float a = v1.DistanceTo(v2);
    float b = v2.DistanceTo(v3);
    float c = v3.DistanceTo(v1);
    float s = (a + b + c) / 2;
    return Mathf.Sqrt(s * (s - a) * (s - b) * (s - c));
  }


}
