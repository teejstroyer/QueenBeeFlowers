using Godot;
using System;

public class CameraOrbit : Spatial
{
  private float LookSensitivity = 15.0f;
  private float MinLookAngle = -20.0f;
  private float MaxLookAngle = 75.0f;
  private Vector2 MouseDelta = new Vector2();
  private KinematicBody Player;
  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    Player = (KinematicBody)GetParent();
    Input.SetMouseMode(Input.MouseMode.Captured);
  }

  public override void _Process(float delta)
  {
    var rot = new Vector3(MouseDelta.y, MouseDelta.x, 0) * LookSensitivity * delta;
    RotationDegrees = new Vector3(
            Mathf.Clamp(RotationDegrees.x + rot.x, MinLookAngle, MaxLookAngle),
            RotationDegrees.y - rot.y,
            RotationDegrees.z);
    MouseDelta = Vector2.Zero;
  }
  public override void _Input(InputEvent @event)
  {
    if (@event is InputEventMouseMotion mouseEvent)
    {
      MouseDelta = mouseEvent.Relative;
    }
  }

}
