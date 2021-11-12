using Godot;
public class MeshFactory : Godot.Object
{

  public static ArrayMesh SimpleGrass()
  {
    var verts = new Godot.Collections.Array<Vector3>(){
        new Vector3(-.5f, 0, 0),
        new Vector3(.5f, 0, 1),
        new Vector3(0, 1, 0),
    };
    var uvs = new Godot.Collections.Array<Vector2> {
        new Vector2(0, 0),
        new Vector2(0, 0),
        new Vector2(1, 1),
      };

    var arrays = new Godot.Collections.Array();
    arrays.Resize((int)Mesh.ArrayType.Max);
    arrays[(int)Mesh.ArrayType.Vertex] = verts;
    arrays[(int)Mesh.ArrayType.TexUv2] = uvs;

    var mesh = new ArrayMesh();
    mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
    mesh.CustomAabb = new AABB(new Vector3(-.5f, 0, -.5f), new Vector3(1, 1, 1));

    return mesh;
  }
}
