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
  private readonly int _GrassCount;

  public Chunk(OpenSimplexNoise noise, Vector3 position, int maxHeight, int chunkSize, int subdivisions, Material material, Mesh grassMesh, int grassCount)
  {
    this.Translation = position;
    _Noise = noise;
    _Size = chunkSize;
    _Subdivisions = subdivisions;
    _MaxHeight = maxHeight;
    _Material = material;
    _GrassMesh = grassMesh;
    _GrassCount = grassCount;
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
    GenerateGrass(meshInstance);
  }

  public void GenerateGrass(MeshInstance targetSurface)
  {
    var vertices = targetSurface.Mesh.GetFaces();
    var faces = vertices.ToTriangleArray();
    int instanceCount = (_GrassCount / faces.Length) * faces.Length;

    var multimesh = new MultiMesh
    {
      Mesh = _GrassMesh,
      TransformFormat = MultiMesh.TransformFormatEnum.Transform3d,
      InstanceCount = instanceCount,
    };

    var blade = 0;
    foreach (var face in faces)
    {
      for (int i = 0; i < (instanceCount / faces.Length); i++)
      {
        var pos = face.GetRandomPoint();
        var opAxis = (face[0] - face[1]).Normalized();
        Transform xform = new Transform();
        xform.SetLookAt(pos, pos + opAxis, Vector3.Up);
        xform.basis.Scale = new Vector3(1, 1, 1);
        multimesh.SetInstanceTransform(blade, xform);
        blade++;
      }
    }

    //Get Strays not caught by previous loop
    while (blade != instanceCount)
    {
      var face = faces[0];
      var pos = face.GetRandomPoint();
      var opAxis = (face[0] - face[1]).Normalized();
      Transform xform = new Transform();
      xform.SetLookAt(pos, pos + opAxis, Vector3.Up);
      xform.basis.Scale = new Vector3(1, 1, 1);
      multimesh.SetInstanceTransform(blade++, xform);
    }

    var mmi = new MultiMeshInstance() { Multimesh = multimesh };
    AddChild(mmi);
  }

}
