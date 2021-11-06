using Godot;

public static class Extensions
{
  public static int RoundToInt(this float f) => Mathf.RoundToInt(f);
  public static string ToKey(this Vector3 v) => $"{v.x}:{v.z}";
}