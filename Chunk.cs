using System.Collections;
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

    //Decorate MULTIMESH
    PopulateGrass(meshInstance);
  }

  public void PopulateGrass(MeshInstance targetSurface)
  {
    //https://github.com/godotengine/godot/blob/master/editor/plugins/multimesh_editor_plugin.cpp
    Transform geoXform = GlobalTransform.AffineInverse() * targetSurface.GlobalTransform;
    var faceVertices = targetSurface.Mesh.GetFaces();

    var mesh = _GrassMesh;
    if (faceVertices.Length == 0) return;
    var faces = new Triangle[faceVertices.Length / 3];

    for (int i = 0; i < faces.Length; i++)
    {
      faces[i] = new Triangle(faceVertices[3 * i], faceVertices[3 * i + 1], faceVertices[3 * i + 2]);
    }

    foreach (var face in faces)
    {
      face[0] = geoXform.Xform(face[0]);
      face[1] = geoXform.Xform(face[1]);
      face[2] = geoXform.Xform(face[2]);
    }

    float areaAccumulation = 0;
    Dictionary<float, int> triangleAreaMap = new Dictionary<float, int>();

    for (int i = 0; i < faces.Length; i++)
    {
      float area = faces[i].Area;
      if (area < Mathf.Epsilon) continue;
      triangleAreaMap.Add(areaAccumulation, i);
      areaAccumulation += area;
    }

    if (triangleAreaMap.Count == 0 || areaAccumulation == 0) return;


    int instanceCount = 1000;
    float tiltRandom = 25;
    float rotateRandom = 25;
    float scaleRandom = 25;
    bool xAxis = false;

    var multimesh = new MultiMesh
    {
      Mesh = mesh,
      TransformFormat = MultiMesh.TransformFormatEnum.Transform3d,
      InstanceCount = instanceCount,
      //ColorFormat = MultiMesh.ColorFormatEnum.None
    };

    Transform axisXform = new Transform();
    if (xAxis)
      axisXform.Rotated(new Vector3(1, 0, 0), -Mathf.Pi * 0.5f);
    else
      axisXform.Rotated(new Vector3(0, 0, 1), -Mathf.Pi * 0.5f);

    RandomNumberGenerator rng = new RandomNumberGenerator();

    for (int i = 0; i < instanceCount; i++)
    {
      float areapos = rng.Randf() * areaAccumulation;
      //Find closest value
      var closest = triangleAreaMap.OrderBy(e => Mathf.Abs(e.Key - areapos)).FirstOrDefault();
      var face = faces[closest.Value];
      var pos = face.GetRandomPoint();
      var normal = face.Normal;
      var opAxis = (face[0] - face[1]).Normalized();

      Transform xform = new Transform();
      xform.SetLookAt(pos, pos + opAxis, normal);
      xform *= axisXform;

      var postXform = new Basis();
      postXform.Rotated(xform.basis[1], rng.Randf() * rotateRandom * Mathf.Pi);
      postXform.Rotated(xform.basis[2], rng.Randf() * tiltRandom * Mathf.Pi);
      postXform.Rotated(xform.basis[0], rng.Randf() * tiltRandom * Mathf.Pi);
      xform.basis = postXform * xform.basis;
      xform.basis.Scale = new Vector3(1, 1, 1) * (rng.Randf() * scaleRandom + 100);
      multimesh.SetInstanceTransform(i, xform);
    }
    var mmi = new MultiMeshInstance()
    {
      Multimesh = multimesh,
    };
    AddChild(mmi);
  }

}
