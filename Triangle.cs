using Godot;

public class Triangle
{
  private readonly Vector3[] _Vertices = new Vector3[3];
  private readonly Vector3 ABC = new Vector3();
  private readonly float D = 0;
  public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
  {
    _Vertices[0] = v1;
    _Vertices[1] = v2;
    _Vertices[2] = v3;
    ABC = new Vector3(v1.y * v2.z - v1.z * v2.y, v1.z * v2.x - v1.x * v2.z, v1.x * v2.y - v1.y * v2.x);
    D = ABC.x * v3.x + ABC.y * v3.y + ABC.z * v3.z;
  }

  public Vector3 this[int index]
  {
    get => _Vertices[index];
    set => _Vertices[index] = value;
  }

  public float Area
  {
    get
    {
      float a = _Vertices[0].DistanceTo(_Vertices[1]);
      float b = _Vertices[1].DistanceTo(_Vertices[2]);
      float c = _Vertices[2].DistanceTo(_Vertices[0]);
      float s = (a + b + c) / 2;
      return Mathf.Sqrt(s * (s - a) * (s - b) * (s - c));
    }
  }

  public float CalculateY(float x, float z) => (ABC.x * x + ABC.y * z + D) / ABC.z;

  public Vector3 GetRandomPoint()
  {
    float r1 = (float)GD.RandRange(0, 1);
    float r2 = (float)GD.RandRange(0, 1);
    if (r1 + r2 > 1)
    {
      r1 = 1 - r1;
      r2 = 1 - r2;
    }
    return _Vertices[0] * r1 + _Vertices[1] * r2 + _Vertices[2] * (1 - r1 - r2);
  }

  public Vector3 Normal
  {
    get
    {
      Vector3 normal = new Vector3
      {
        x = ABC.y * _Vertices[2].z - ABC.z * _Vertices[2].y,
        y = ABC.z * _Vertices[2].x - ABC.x * _Vertices[2].z,
        z = ABC.x * _Vertices[2].y - ABC.y * _Vertices[2].x
      };
      return normal.Normalized();
    }
  }
}