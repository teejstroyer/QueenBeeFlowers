using Godot;

public static class Extensions
{
  public static int RoundToInt(this float f) => Mathf.RoundToInt(f);
  public static string ToKey(this Vector3 v) => $"{v.x}:{v.z}";

  public static Triangle[] ToTriangleArray(this Vector3[] v)
  {
    var triangles = new Triangle[v.Length / 3];
    for (int i = 0; i < v.Length; i += 3)
    {
      triangles[i / 3] = new Triangle(v[i], v[i + 1], v[i + 2]);
    }
    return triangles;
  }
}