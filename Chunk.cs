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

    //Next Need to take multimesh and for each item in mesh align to ground of chunk
    var mm = new MultiMesh
    {
      Mesh = _GrassMesh
    };

  }
}
