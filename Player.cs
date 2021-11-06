using Godot;

public class Player : KinematicBody
{
  [Signal]
  public delegate Vector3 EmitPosition();
  public float SensitivityX = 0.005f;
  public float SensitivityY = 0.0005f;
  public float MaximumYLook = 45;
  public float Accelaration = 5f;
  public float MaximumWalkSpeed = 10f;
  public float MaximumRunSpeed = 20f;
  public float JumpSpeed = 20f;
  public Vector3 Gravity = new Vector3(0, .98f, 0);
  public Vector3 Velocity = Vector3.Zero;
  public float ForwardVelocity = 0;
  public float WalkSpeed = 0;
  public float Lerp(float one, float two, float by) => one * (1 - by) + two * by;

  public override void _Ready()
  {
    base._Ready();
    Input.SetMouseMode(Input.MouseMode.Captured);
    ForwardVelocity = WalkSpeed;
    SetProcess(true);
  }

  public override void _Process(float delta)
  {
    base._Process(delta);
    if (Input.IsKeyPressed((int)KeyList.Escape)) GetTree().Quit();
    EmitSignal(nameof(EmitPosition), Translation);
  }

  public override void _PhysicsProcess(float delta)
  {
    base._PhysicsProcess(delta);
    Velocity -= Gravity;
    bool isMoving = false;
    bool isRunning = Input.IsKeyPressed((int)KeyList.Shift);
    float currentMaxSpeed = isRunning ? MaximumRunSpeed : MaximumWalkSpeed;

    if (Input.IsActionPressed("move_forward"))
    {
      isMoving = true;
      WalkSpeed += Accelaration;
      if (WalkSpeed > currentMaxSpeed) WalkSpeed = currentMaxSpeed;
      Velocity.x = -GlobalTransform.basis.z.x * WalkSpeed;
      Velocity.z = -GlobalTransform.basis.z.z * WalkSpeed;
    }
    if (Input.IsActionPressed("move_backward"))
    {
      isMoving = true;
      WalkSpeed += Accelaration;
      if (WalkSpeed > currentMaxSpeed) WalkSpeed = currentMaxSpeed;
      Velocity.x = GlobalTransform.basis.z.x * WalkSpeed;
      Velocity.z = GlobalTransform.basis.z.z * WalkSpeed;
    }
    if (Input.IsActionPressed("move_left"))
    {
      isMoving = true;
      WalkSpeed += Accelaration;
      if (WalkSpeed > currentMaxSpeed) WalkSpeed = currentMaxSpeed;
      Velocity.x = -GlobalTransform.basis.x.x * WalkSpeed;
      Velocity.z = -GlobalTransform.basis.x.z * WalkSpeed;
    }
    if (Input.IsActionPressed("move_right"))
    {
      isMoving = true;
      WalkSpeed += Accelaration;
      if (WalkSpeed > currentMaxSpeed) WalkSpeed = currentMaxSpeed;
      Velocity.x = GlobalTransform.basis.x.x * WalkSpeed;
      Velocity.z = GlobalTransform.basis.x.z * WalkSpeed;
    }

    if (IsOnFloor() && Input.IsActionJustPressed("jump"))
    {
      Velocity.y = JumpSpeed;
    }

    if (!isMoving)
    {
      Velocity.x = 0;
      Velocity.z = 0;
    }

    Velocity = MoveAndSlide(Velocity, new Vector3(0, 1, 0));
  }

  public override void _Input(InputEvent @event)
  {
    base._Input(@event);
    if (@event is InputEventMouseMotion motion)
      RotateY(-SensitivityX * motion.Relative.x);
  }

}