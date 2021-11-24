using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static WorldSettings;
using static Constants;

public class Chunk : MonoBehaviour
{
    
    public BlockType[,,] blocks = new BlockType[chunkWidth, chunkHeight, chunkWidth];
    public Vector3 pos;

    private const double freq = 0.02;

    private BlockType defaultBlock = BlockType.Air;

    public MeshFilter meshFilter;
    public MeshCollider MeshCollider;

    private float getNoise(int x, int z)
    {
        var position = transform.position;
        return (float) noise.Evaluate((x + position.x) * 0.02, (z + position.z) * 0.02);
    }

    public bool PopulateChunk()
    {
        bool keep = false;
        
        for (int x = 0; x < chunkWidth; x++)
        {
            for (int z = 0; z < chunkWidth; z++)
            {
                int grasslayer = Mathf.FloorToInt(getNoise(x, z) * 16 + chunkHeight / 1.2f);

                for (int y = 0; y < chunkHeight; y++)
                {
                    if (noise.Evaluate((x + pos.x *chunkWidth) * freq, y * freq, (z + pos.z * chunkWidth) * freq) > 0.5)
                    {
                        blocks[x, y, z] = BlockType.Air;
                    }
                    else if (y > grasslayer)
                    {
                        blocks[x, y, z] = BlockType.Air;
                    }
                    else if (y < grasslayer - 3)
                    {
                        blocks[x, y, z] = BlockType.Stone;
                        keep = true;
                    }
                    else if (y < grasslayer)
                    {
                        blocks[x, y, z] = BlockType.Dirt;
                        keep = true;
                    }
                    else
                    {
                        blocks[x, y, z] = BlockType.Grass;
                        keep = true;
                    }
                }
            }
        }

        return keep;
    }

    public void GenerateMesh()
    {
        Mesh mesh = new Mesh();

        var verts = new List<Vector3>();
        var tris = new List<int>();
        var uvs = new List<Vector2>();

        for(var x = 0; x < chunkWidth; x++)
            for(var z = 0; z < chunkWidth; z++)
                for(var y = 0; y < chunkHeight; y++)
                {
                    if (blocks[x, y, z] == BlockType.Air || blocks[x, y, z] == BlockType.Void) continue;
                    
                    Vector3 blockPos = new Vector3(x, y, z);
                    var numFaces = 0;
                    //no land above, build top face
                    
                    if(GetBlockInChunk(x, y + 1, z, ChunkDirection.Top) == BlockType.Air)
                    {
                        verts.Add(blockPos + new Vector3(0, 1, 0));
                        verts.Add(blockPos + new Vector3(0, 1, 1));
                        verts.Add(blockPos + new Vector3(1, 1, 1));
                        verts.Add(blockPos + new Vector3(1, 1, 0));
                        numFaces++;

                        uvs.AddRange(Block.blocks[blocks[x, y, z]].topPos.GetUVs());
                    }

                    //bottom
                    if(GetBlockInChunk(x, y - 1, z, ChunkDirection.Bottom) == BlockType.Air)
                    {
                        verts.Add(blockPos + new Vector3(0, 0, 0));
                        verts.Add(blockPos + new Vector3(1, 0, 0));
                        verts.Add(blockPos + new Vector3(1, 0, 1));
                        verts.Add(blockPos + new Vector3(0, 0, 1));
                        numFaces++;

                        uvs.AddRange(Block.blocks[blocks[x, y, z]].bottomPos.GetUVs());
                    }

                    //front
                    if(GetBlockInChunk(x, y, z - 1, ChunkDirection.Front) == BlockType.Air)
                    {
                        verts.Add(blockPos + new Vector3(0, 0, 0));
                        verts.Add(blockPos + new Vector3(0, 1, 0));
                        verts.Add(blockPos + new Vector3(1, 1, 0));
                        verts.Add(blockPos + new Vector3(1, 0, 0));
                        numFaces++;

                        uvs.AddRange(Block.blocks[blocks[x, y, z]].sidePos.GetUVs());
                    }

                    //right
                    if(GetBlockInChunk(x + 1, y, z, ChunkDirection.Right) == BlockType.Air)
                    {
                        verts.Add(blockPos + new Vector3(1, 0, 0));
                        verts.Add(blockPos + new Vector3(1, 1, 0));
                        verts.Add(blockPos + new Vector3(1, 1, 1));
                        verts.Add(blockPos + new Vector3(1, 0, 1));
                        numFaces++;

                        uvs.AddRange(Block.blocks[blocks[x, y, z]].sidePos.GetUVs());
                    }

                    //back
                    if(GetBlockInChunk(x, y, z + 1, ChunkDirection.Back) == BlockType.Air)
                    {
                        verts.Add(blockPos + new Vector3(1, 0, 1));
                        verts.Add(blockPos + new Vector3(1, 1, 1));
                        verts.Add(blockPos + new Vector3(0, 1, 1));
                        verts.Add(blockPos + new Vector3(0, 0, 1));
                        numFaces++;

                        uvs.AddRange(Block.blocks[blocks[x, y, z]].sidePos.GetUVs());
                    }

                    //left
                    if(GetBlockInChunk(x - 1, y, z, ChunkDirection.Left) == BlockType.Air)
                    {
                        verts.Add(blockPos + new Vector3(0, 0, 1));
                        verts.Add(blockPos + new Vector3(0, 1, 1));
                        verts.Add(blockPos + new Vector3(0, 1, 0));
                        verts.Add(blockPos + new Vector3(0, 0, 0));
                        numFaces++;

                        uvs.AddRange(Block.blocks[blocks[x, y, z]].sidePos.GetUVs());
                    }


                    int tl = verts.Count - 4 * numFaces;
                    for(var i = 0; i < numFaces; i++)
                    {
                        tris.AddRange(new[] { tl + i * 4, tl + i * 4 + 1, tl + i * 4 + 2, tl + i * 4, tl + i * 4 + 2, tl + i * 4 + 3 });
                    }
                }
        
        
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        MeshCollider.sharedMesh = mesh;
    }

    private BlockType GetBlockInChunk(int x, int y, int z, ChunkDirection direction)
    {
        if (x < chunkWidth && x >= 0 && y < chunkHeight && y >= 0 && z < chunkWidth && z >= 0) return blocks[x, y, z];
        
        switch (direction)
        {
            case ChunkDirection.Top:
            {
                if(loadedChunks.TryGetValue(new Vector3(pos.x, pos.y + 1, pos.z), out Chunk c)) return c.blocks[x, y - chunkHeight, z];
                break;
            }
            case ChunkDirection.Bottom:
            {
                if(loadedChunks.TryGetValue(new Vector3(pos.x, pos.y - 1, pos.z), out Chunk c)) return c.blocks[x, y + chunkHeight, z];
                break;
            }
            case ChunkDirection.Front:
            {
                if(loadedChunks.TryGetValue(new Vector3(pos.x, pos.y, pos.z - 1), out Chunk c)) return c.blocks[x, y, z + chunkWidth];
                break;
            }
            case ChunkDirection.Right:
            {
                if(loadedChunks.TryGetValue(new Vector3(pos.x + 1, pos.y, pos.z), out Chunk c)) return c.blocks[x - chunkWidth, y, z];
                break;
            }
            case ChunkDirection.Back:
            {
                if(loadedChunks.TryGetValue(new Vector3(pos.x, pos.y, pos.z + 1), out Chunk c)) return c.blocks[x, y, z - chunkWidth];
                break;
            }
            case ChunkDirection.Left:
            {
                if(loadedChunks.TryGetValue(new Vector3(pos.x - 1, pos.y, pos.z), out Chunk c)) return c.blocks[x + chunkWidth, y, z];
                break;
            }
        }
        return defaultBlock;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Chunk)), CanEditMultipleObjects]
public class ChunkEditor : Editor
{
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (!GUILayout.Button("regenerate mesh")) return;

        ((Chunk) target).GenerateMesh();
    }
}
#endif
