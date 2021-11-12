using System.Collections.Generic;
using System.Linq;
using Godot;

public class TerrainGenerator : Spatial
{
  [Export]
  public OpenSimplexNoise Noise = new OpenSimplexNoise()
  {
    Lacunarity = 0.5f,
    Octaves = 8,
    Persistence = 0.5f,
    Seed = (int)System.DateTime.Now.Ticks,
  };

  [Export] public int ChunkDistance = 2;
  [Export] public int ChunkMaxHeight = 20;
  [Export] public int ChunkSize = 4;
  [Export] public int ChunkSubdivisions = 1;
  [Export] public Material GrassMaterial;
  [Export] public Mesh BladeOfGrass;

  private Vector3 FollowPosition;
  private readonly Dictionary<string, Chunk> _Chunks = new Dictionary<string, Chunk>();
  public void OnPlayerEmitPosition(Vector3 position) => FollowPosition = position;

  public override void _Process(float delta) => UpdateChunks();
  private void UpdateChunks()
  {
    foreach (var chunk in _Chunks)
    {
      chunk.Value.ShouldRemove = true;
    }

    int px = (FollowPosition.x / ChunkSize).RoundToInt();
    int pz = (FollowPosition.z / ChunkSize).RoundToInt();

    for (int x = px - ChunkDistance; x <= px + ChunkDistance; x++)
    {
      for (int z = pz - ChunkDistance; z <= pz + ChunkDistance; z++)
      {
        AddChunk(new Vector3(x, 0, z) * ChunkSize);
      }
    }

    foreach (var it in _Chunks.Where(i => i.Value.ShouldRemove).ToList())
    {
      _Chunks[it.Key].QueueFree();
      _Chunks.Remove(it.Key);
    }
  }

  private void AddChunk(Vector3 position)
  {
    if (_Chunks.ContainsKey(position.ToKey()))
    {
      _Chunks[position.ToKey()].ShouldRemove = false;
      return;
    }
    var chunk = new Chunk(Noise, position, ChunkMaxHeight, ChunkSize, ChunkSubdivisions, GrassMaterial, BladeOfGrass)
    {
      ShouldRemove = false
    };

    _Chunks.Add(chunk.Translation.ToKey(), chunk);
    AddChild(chunk);
  }
}