using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour 
{
    [SerializeField] int seed;
    [SerializeField] Transform player;
    [SerializeField] BiomeAttributes biome;
    Vector3 spawnPosition;

    public Material material;
    public BlockType[] blocktypes;

    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];
    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;

    private void Start()
    {
        Random.InitState(seed);
        spawnPosition = new Vector3((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f, VoxelData.ChunkHeight -50f, (VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f);
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);

    }

    private void Update() 
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);
        //if (!playerChunkCoord.Equals(playerLastChunkCoord))
          //  CheckViewDistance();

    }

    ChunkCoord GetChunkCoordFromVector3 (Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, z);

    }

    private void GenerateWorld () 
    {

        for (int x = VoxelData.WorldSizeInChunks / 2 - VoxelData.ViewDistanceInChunks; x < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; x++) 
        {
            for (int z = VoxelData.WorldSizeInChunks / 2 - VoxelData.ViewDistanceInChunks; z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; z++)
            {

                CreateNewChunk(x,z);

            }
        }
        player.position = spawnPosition;

    }

    private void CheckViewDistance () 
    {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);


        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);


        // Loop through all chunks currently within view distance of the player.
        for (int x = coord.x - VoxelData.ViewDistanceInChunks; x < coord.x + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = coord.z - VoxelData.ViewDistanceInChunks; z < coord.z + VoxelData.ViewDistanceInChunks; z++)
            {

                // If the current chunk is in the world...
                if (IsChunkInWorld(new ChunkCoord(x, z)))
                {

                    // Check if it active, if not, activate it.
                    if (chunks[x, z] == null)
                        CreateNewChunk(x, z);
                    else if (!chunks[x, z].isActive)
                    {
                        chunks[x, z].isActive = true;
                        activeChunks.Add(new ChunkCoord(x, z));
                    }

                }

                // Check through previously active chunks to see if this chunk is there. If it is, remove it from the list.
                for (int i = 0; i < previouslyActiveChunks.Count; i++)
                {

                    if (previouslyActiveChunks[i].Equals(new ChunkCoord(x, z)))
                        previouslyActiveChunks.RemoveAt(i);

                }

            }
        }

        foreach (ChunkCoord c in previouslyActiveChunks)
            chunks[c.x, c.z].isActive = false;

    }

    bool IsChunkInWorld(ChunkCoord coord)
    {

        if (coord.x > 0 && coord.x < VoxelData.WorldSizeInChunks - 1 && coord.z > 0 && coord.z < VoxelData.WorldSizeInChunks - 1)
            return true;
        else
            return
                false;

    }

    void CreateNewChunk(int x, int z)
    {

        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this);
        activeChunks.Add(new ChunkCoord(x, z));

    }
    public bool CheckForVoxel(float x, float y, float z)
    {
        int xCheck = Mathf.FloorToInt(x);
        int yCheck = Mathf.FloorToInt(y);
        int zCheck = Mathf.FloorToInt(z);

        int xChunk = xCheck / VoxelData.ChunkWidth;
        int zChunk = zCheck / VoxelData.ChunkWidth;

        xCheck -= (xChunk * VoxelData.ChunkWidth);
        zCheck -= (zChunk * VoxelData.ChunkWidth);

        return blocktypes[chunks[xChunk, zChunk].voxelMap[xCheck, yCheck, zCheck]].isSolid;
    }
    public byte GetVoxel (Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);
        // IMMUTABLE PASS
        if (!IsVoxelInWorld(pos))
            return (byte)BlockTypeName.Air;
        if (yPos == 0)
            return (byte)BlockTypeName.Grass;

        // Basic terrain pass

        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.terrainScale)) + biome.solidGroundHeight;
        byte voxelValue = 0;
        if (yPos == terrainHeight)
            voxelValue = (byte)BlockTypeName.Grass;
        else if (yPos < terrainHeight && yPos > terrainHeight - 4f)
            voxelValue = (byte)BlockTypeName.Dirt;
        else if (yPos > terrainHeight)
            return (byte)BlockTypeName.Air;
        else
            voxelValue = (byte)BlockTypeName.Stone;

        // SECOND PASS

        if(voxelValue == 2)
        {
            foreach(var lode in biome.lodes)
            {
                if(yPos > lode.minHeight && yPos < lode.maxHeight)
                {
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                        voxelValue = lode.blockID;
                }
            }
        }

        return voxelValue;
        
    }

    private bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInBlocks && pos.y >= 0 && pos.y < VoxelData.ChunkHeight && pos.z >= 0 && pos.z < VoxelData.WorldSizeInBlocks)
            return true;
        else
            return false;
    }
}

public struct ChunkCoord 
{

    public int x;
    public int z;

    public ChunkCoord (int _x, int _z) 
    {

        x = _x;
        z = _z;

    }

    public bool Equals(ChunkCoord other) 
    {
        if (other.x == x && other.z == z)
            return true;
        else
            return false;

    }

}

[System.Serializable]
public class BlockType
{

    public string blockName;
    public bool isSolid;
    [Header("Texture Values")]
    [SerializeField] int backFaceTexture;
    [SerializeField] int frontFaceTexture;
    [SerializeField] int topFaceTexture;
    [SerializeField] int bottomFaceTexture;
    [SerializeField] int leftFaceTexture;
    [SerializeField] int rightFaceTexture;

    // Back, Front, Top, Bottom, Left, Right

    public int GetTextureID (int faceIndex) 
    {

        switch (faceIndex) {

            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;


        }

    }

}
public enum BlockTypeName
{
    Air,
    BedRock,
    Stone,
    Grass,
    Sand,
    Dirt

}
