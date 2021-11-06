using Godot;

public class Chunk : Spatial
{
  public bool ShouldRemove = false;
  private int _Size;
  private int _Subdivisions;
  private OpenSimplexNoise _Noise;
  private int _MaxHeight;
  public Chunk(OpenSimplexNoise noise, Vector3 position, int maxHeight, int chunkSize, int subdivisions)
  {
    this.Translation = position;
    _Noise = noise;
    _Size = chunkSize;
    _Subdivisions = subdivisions;
    _MaxHeight = maxHeight;
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
      Mesh = surfaceTool.Commit(),
    };

    //To be replaced with shader 3d grass material.
    var material = new SpatialMaterial()
    {
      FlagsTransparent = false,
      AlbedoColor = new Color(0.2f, .8f, .2f),
    };

    meshInstance.SetSurfaceMaterial(0, material);
    meshInstance.CreateTrimeshCollision();
    AddChild(meshInstance);
  }
}
