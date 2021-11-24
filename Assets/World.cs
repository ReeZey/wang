using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;

public class World : MonoBehaviour
{
    public Transform chunkPrefab;
    public Transform Player;

    private static List<Vector3> chunksToLoad = new List<Vector3>();
    
    private const int horizontalRenderDistance = 3;
    private const int verticalRenderDistance = 1;

    private const bool updatingChunks = true;

    private void Start()
    {
        
        StartCoroutine(CreateChunk(new Vector3Int(0, 0, 0)));
        
        for (int x = -horizontalRenderDistance; x < horizontalRenderDistance; x++)
        {
            for (int y = -verticalRenderDistance; y < verticalRenderDistance; y++)
            {
                for (int z = -horizontalRenderDistance; z < horizontalRenderDistance; z++)
                {
                    chunksToLoad.Add(new Vector3(x,y,z));
                }
            }
        }
        
        
        StartCoroutine(GoThrough());
    }

    private void FixedUpdate()
    {
        Vector3Int playerPosition = Vector3Int.FloorToInt(Player.position);

        int xoff = (playerPosition.x / WorldSettings.chunkWidth);
        int yoff = (playerPosition.y / WorldSettings.chunkHeight);
        int zoff = (playerPosition.z / WorldSettings.chunkWidth);
        
        for (int x = -horizontalRenderDistance + xoff; x < horizontalRenderDistance + xoff; x++)
        {
            for (int y = -verticalRenderDistance + yoff; y < verticalRenderDistance + yoff; y++)
            {
                for (int z = -horizontalRenderDistance + zoff; z < horizontalRenderDistance + zoff; z++)
                {
                    var pang = new Vector3(x, y, z);
                    if(!WorldSettings.allChunks.Keys.Contains(pang))
                        chunksToLoad.Add(pang);
                }
            }
        }
    }

    public void wang()
    {
        StartCoroutine(GoThrough());
    }

    private IEnumerator GoThrough()
    {
        while(true){
            if(chunksToLoad.Count == 0) continue;

            foreach (var chunk in chunksToLoad.ToArray())
            {
                chunksToLoad.Remove(chunk);
                if (WorldSettings.allChunks.Keys.Contains(chunk)) continue;
                
                StartCoroutine(CreateChunk(chunk));
                yield return new WaitForEndOfFrame();
            }
            
            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator CreateChunk(Vector3 pos)
    {
        if (WorldSettings.allChunks.Keys.Contains(pos)) yield break;
        
        Chunk go = Instantiate(chunkPrefab, new Vector3(pos.x * WorldSettings.chunkWidth, pos.y * WorldSettings.chunkHeight, pos.z * WorldSettings.chunkWidth), Quaternion.identity, transform).GetComponent<Chunk>();
        WorldSettings.allChunks.Add(pos, go);
        
        go.pos = pos;
        
        go.PopulateChunk();
        go.GenerateMesh();

        foreach (var direction in Constants.directions)
        {
            if (WorldSettings.allChunks.TryGetValue(go.pos + direction, out Chunk c)) c.GenerateMesh();
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(World)), CanEditMultipleObjects]
public class PinInfoEditor : Editor
{
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (!GUILayout.Button("Your ButtonText")) return;

        ((World) target).wang();
    }
}
#endif