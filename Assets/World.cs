using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;

public class World : MonoBehaviour
{
    public Transform chunkPrefab;

    private Queue<Vector3> chunksToLoad = new Queue<Vector3>();

    private async void Start()
    {
        await CreateChunk(new Vector3(0, 0, 0));
    }

    [ContextMenu("DANGEROUS")]
    private async Task DANGEROUS()
    {
        int yep = 10;
        while (yep-- > 0)
        {
            await AnothaOne();
            await Master();
        }
    }
    
    [ContextMenu("load next chunk...")]
    private async Task Master()
    {
        while (chunksToLoad.Count != 0)
        {
            await getChunk();
        }
    }
    
    private async Task getChunk()
    {
        await CreateChunk(chunksToLoad.Dequeue());
    }
    

    [ContextMenu("Test Generate All Terrain")]
    private async Task AnothaOne()
    {
        foreach (var direction in Constants.directions)
        {
            foreach (var pair in WorldSettings.loadedChunks)
            {
                var chunkPos = pair.Key + direction;
                
                if (WorldSettings.loadedChunks.ContainsKey(chunkPos) || chunksToLoad.Contains(chunkPos)) continue;
                
                chunksToLoad.Enqueue(chunkPos);
            }
        }

        await Task.Yield();
    }
    
    private async Task CreateChunk(Vector3 pos)
    {
        float chunkX = pos.x * WorldSettings.chunkWidth;
        float chunkY = pos.y * WorldSettings.chunkHeight;
        float chunkZ = pos.z * WorldSettings.chunkWidth;

        Chunk chunk = Instantiate(chunkPrefab, new Vector3(chunkX, chunkY, chunkZ), Quaternion.identity, transform).GetComponent<Chunk>();

        chunk.pos = pos;
        
        bool keep = chunk.PopulateChunk();

        if (!keep)
        {
            Destroy(chunk.gameObject);
            return;
        }
        
        chunk.GenerateMesh();

        WorldSettings.loadedChunks.Add(pos, chunk);
        
        foreach (var direction in Constants.directions)
        {
            if (WorldSettings.loadedChunks.TryGetValue(chunk.pos + direction, out Chunk c)) c.GenerateMesh();
        }

        await Task.Yield();
    }
}