using Godot;

[Tool]
public class Planter : MultiMeshInstance
{
  private float span = 5;
  private int count = 10000;
  private Vector2 width = new Vector2(.01f, .02f);
  private Vector2 height = new Vector2(.04f, .08f);
  private Vector2 swayYaw = new Vector2(0, 10);
  private Vector2 swayPitch = new Vector2(0, 10);

  [Export]
  public float Span { get => span; set { span = value; Rebuild(); } }
  [Export]
  public int Count { get => count; set { count = value; Rebuild(); } }
  [Export]
  public Vector2 Width { get => width; set { width = value; Rebuild(); } }
  [Export]
  public Vector2 Height { get => height; set { height = value; Rebuild(); } }
  [Export]
  public Vector2 SwayYaw { get => swayYaw; set { swayYaw = value; Rebuild(); } }
  [Export]
  public Vector2 SwayPitch { get => swayPitch; set { swayPitch = value; Rebuild(); } }
  public Planter() => Rebuild();
  public override void _Ready() => Rebuild();

  public void Rebuild()
  {
    if (Multimesh == null) Multimesh = new MultiMesh();

    Multimesh.InstanceCount = 0;
    Multimesh.Mesh = MeshFactory.SimpleGrass();
    Multimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3d;
    Multimesh.CustomDataFormat = MultiMesh.CustomDataFormatEnum.Float;
    Multimesh.ColorFormat = MultiMesh.ColorFormatEnum.None;
    Multimesh.InstanceCount = Count;
    var rnd = new RandomNumberGenerator();
    for (int i = 0; i < Multimesh.InstanceCount; i++)
    {
      var pos = new Vector3(rnd.RandfRange(-Span, Span), 0, rnd.RandfRange(-Span, Span));
      var basis = new Basis(Vector3.Up, Mathf.Deg2Rad(rnd.RandfRange(-Span, Span)));
      Multimesh.SetInstanceTransform(i, new Transform(basis, pos));
      Multimesh.SetInstanceCustomData(i,
              new Color(
                  rnd.RandfRange(Width.x, Width.y),
                  rnd.RandfRange(Height.x, Height.y),
                  Mathf.Deg2Rad(rnd.RandfRange(SwayPitch.x, SwayPitch.y)),
                  Mathf.Deg2Rad(rnd.RandfRange(SwayYaw.x, SwayYaw.y))
              ));

    }

  }

}

