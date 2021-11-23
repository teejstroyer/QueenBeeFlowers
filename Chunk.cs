using Godot;

public class Chunk : Spatial
{

  [Export] public Vector2 sway_yaw { get; set; } = new Vector2(0.0f, 10.0f);
  [Export] public Vector2 sway_pitch { get; set; } = new Vector2(0.0f, 10.0f);
  public bool ShouldRemove = false;
  private readonly int _Size;
  private readonly int _Subdivisions;
  private readonly OpenSimplexNoise _Noise;
  private readonly int _MaxHeight;
  private readonly Material _GroundMaterial;
  private readonly Material _GrassMaterial;
  private readonly Mesh _GrassMesh;
  private readonly int _GrassCountPerTriangle;

  //Mesh
  private MultiMeshInstance _MultiMeshInstance;
  private MultiMesh _MultiMesh;

  public Chunk(OpenSimplexNoise noise, Vector3 position, int maxHeight, int chunkSize, int subdivisions, Material groundMaterial, Material grassMaterial, Mesh grassMesh, int grassCountPerTriangle)
  {
    this.Translation = position;
    _Noise = noise;
    _Size = chunkSize;
    _Subdivisions = subdivisions;
    _MaxHeight = maxHeight;
    _GrassMaterial = grassMaterial;
    _GroundMaterial = groundMaterial;
    _GrassMesh = grassMesh;
    _GrassCountPerTriangle = grassCountPerTriangle;
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

    meshInstance.SetSurfaceMaterial(0, _GroundMaterial);
    meshInstance.CreateTrimeshCollision();
    AddChild(meshInstance);

    //Decorate MULTIMESH
    GenerateGrass(meshInstance);
  }

  public void GenerateGrass(MeshInstance targetSurface)
  {
    var faces = targetSurface.Mesh.GetFaces().ToTriangleArray();
    int instanceCount = faces.Length * _GrassCountPerTriangle;
    _MultiMesh = new MultiMesh()
    {
      Mesh = _GrassMesh,
      CustomDataFormat = MultiMesh.CustomDataFormatEnum.Float,
      TransformFormat = MultiMesh.TransformFormatEnum.Transform3d,
      InstanceCount = instanceCount,
    };

    int blade = 0;
    var rng = new RandomNumberGenerator();
    foreach (var face in faces)
    {
      for (int i = 0; i < _GrassCountPerTriangle; i++)
      {
        var pos = face.GetRandomPoint();
        var basis = new Basis(Vector3.Up, Mathf.Deg2Rad(rng.RandfRange(0, 359)));
        _MultiMesh.SetInstanceTransform(blade, new Transform(basis, pos));

        _MultiMesh.SetInstanceCustomData(blade, new Color(
              (float)GD.RandRange(0.01f, .02f),
              (float)GD.RandRange(0.04f, .08f),
          Mathf.Deg2Rad((float)GD.RandRange(sway_pitch.y, sway_pitch.x)),
          Mathf.Deg2Rad((float)GD.RandRange(sway_yaw.y, sway_yaw.x))
        ));
        blade++;
      }
    }

    _MultiMeshInstance = new MultiMeshInstance() { Multimesh = _MultiMesh, MaterialOverride = _GrassMaterial };
    AddChild(_MultiMeshInstance);
  }

  public void SetPlayerPosition(Vector3 position)
  {
    ((ShaderMaterial)_MultiMeshInstance.MaterialOverride).SetShaderParam("character_position", position);
  }

}
