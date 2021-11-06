using System.Collections.Generic;
using System.Linq;
using Godot;

public class TerrainGenerator : Spatial
{
  public ChunkOptions ChunkOptions { get; set; } = new ChunkOptions()
  {
    Noise = new OpenSimplexNoise()
    {
      Lacunarity = 0.5f,
      Octaves = 8,
      Persistence = 0.5f,
      Seed = (int)System.DateTime.Now.Ticks,
    },

    Distance = 2,
    Size = 10,
    Subdivisions = 10,
    MaxHeight = 20,
  };

  private Vector3 FollowPosition;
  private readonly Dictionary<string, Chunk> _Chunks = new Dictionary<string, Chunk>();
  public void OnPlayerEmitPosition(Vector3 position)
  {
    FollowPosition = position;
  }

  public override void _Process(float delta) => UpdateChunks();
  private void UpdateChunks()
  {
    _Chunks.ToList().ForEach(c => c.Value.ShouldRemove = true);

    int px = (FollowPosition.x / ChunkOptions.Size).RoundToInt();
    int pz = (FollowPosition.z / ChunkOptions.Size).RoundToInt();

    for (int x = -ChunkOptions.Distance; x <= ChunkOptions.Distance; x++)
    {
      for (int z = -ChunkOptions.Distance; z <= ChunkOptions.Distance; z++)
      {
        AddChunk(new Vector3(x, 0, z) * ChunkOptions.Size);
      }
    }

    //Remove Chunks that need to be removed
    _Chunks.Where(c => c.Value.ShouldRemove).ToList().ForEach(c =>
    {
      _Chunks[c.Key].QueueFree();
      _Chunks.Remove(c.Key);
    });
  }

  private void AddChunk(Vector3 position)
  {
    if (_Chunks.ContainsKey(position.ToKey()))
    {
      _Chunks[position.ToKey()].ShouldRemove = false;
      return;
    }
    var chunk = new Chunk(ChunkOptions.Noise, position, ChunkOptions.MaxHeight, ChunkOptions.Size, ChunkOptions.Subdivisions);
    _Chunks.Add(chunk.Translation.ToKey(), chunk);
    AddChild(chunk);
  }
}

public static class Extensions
{
  public static int RoundToInt(this float f) => Mathf.RoundToInt(f);
  public static string ToKey(this Vector3 v) => $"{v.x}:{v.z}";
}

public class ChunkOptions
{
  public OpenSimplexNoise Noise { get; set; }
  public int Distance { get; set; }
  public int Size { get; set; }
  public int Subdivisions { get; set; }
  public int MaxHeight { get; set; }
}
