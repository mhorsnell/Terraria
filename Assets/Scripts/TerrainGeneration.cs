using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour
{
    [Header("Tile Atlas")]
    public TileAtlas tileAtlas;

    [Header("Trees")]
    public int treeChance = 10;
    public int minTreeHeight = 4;
    public int maxTreeHeight = 6;

    [Header("Generation Settings")]
    public int chunkSize = 16;
    public int worldSize = 100;
    public int dirtLayerHeight = 5;
    public bool generateCaves = true;
    public float surfaceValue = 0.25f;
    public float heightMultiplier = 4f;
    public int heightAddition = 25;

    [Header("Noise Settings")]
    public float terrainFreq = 0.05f;
    public float caveFreq = 0.05f;
    public float seed;
    public Texture2D caveNoiseTexture;

    [Header("Ore Settings")]
    public OreClass[] ores;
    public float coalRarity;
    public float coalSize;
    public float ironRarity, ironSize;
    public float goldRarity, goldSize;
    public float diamondRarity, diamondSize;
    public Texture2D coalSpread;
    public Texture2D ironSpread;
    public Texture2D goldSpread;
    public Texture2D diamondSpread;

    private GameObject[] worldChunks;
    private List<Vector2> worldTiles = new List<Vector2>();

    // private void OnValidate()
    // {
    //     if (caveNoiseTexture == null)
    //     {
    //         caveNoiseTexture = new Texture2D(worldSize, worldSize);
    //         coalSpread = new Texture2D(worldSize, worldSize);
    //         ironSpread = new Texture2D(worldSize, worldSize);
    //         goldSpread = new Texture2D(worldSize, worldSize);
    //         diamondSpread = new Texture2D(worldSize, worldSize);

    //         GenerateNoiseTexture(caveFreq, surfaceValue, caveNoiseTexture);

    //         GenerateNoiseTexture(coalRarity, coalSize, coalSpread);
    //         GenerateNoiseTexture(ironRarity, ironSize, ironSpread);
    //         GenerateNoiseTexture(goldRarity, goldSize, goldSpread);
    //         GenerateNoiseTexture(diamondRarity, diamondSize, diamondSpread);
    //     }
    // }

    private void Start()
    {
        seed = Random.Range(-10000, 10000);
        if (caveNoiseTexture == null)
        {
            caveNoiseTexture = new Texture2D(worldSize, worldSize);
            coalSpread = new Texture2D(worldSize, worldSize);
            ironSpread = new Texture2D(worldSize, worldSize);
            goldSpread = new Texture2D(worldSize, worldSize);
            diamondSpread = new Texture2D(worldSize, worldSize);
        }

        GenerateNoiseTexture(caveFreq, surfaceValue, caveNoiseTexture);

        GenerateNoiseTexture(coalRarity, coalSize, coalSpread);
        GenerateNoiseTexture(ironRarity, ironSize, ironSpread);
        GenerateNoiseTexture(goldRarity, goldSize, goldSpread);
        GenerateNoiseTexture(diamondRarity, diamondSize, diamondSpread);

        CreateChunks();
        GenerateTerrain();
    
    }

    public void CreateChunks()
    {
        int numChunks = worldSize / chunkSize;
        worldChunks = new GameObject[numChunks];

        for (int i =0; i < numChunks; i++)
        {
            GameObject newChunk = new GameObject(name = i.ToString());
            newChunk.name = i.ToString();
            newChunk.transform.parent = this.transform;
            worldChunks[i] = newChunk;
        }
    }

    public void GenerateTerrain()
    {
        for (int x = 0; x < worldSize; x++)
        {
            float height = Mathf.PerlinNoise((x + seed) * terrainFreq, seed * terrainFreq) * heightMultiplier + heightAddition;

            for (int y = 0; y < height; y++)
            {
                Sprite tileSprite;
                if (y < height - dirtLayerHeight)
                {
                    if (coalSpread.GetPixel(x,y).r > 0.5f)
                        tileSprite = tileAtlas.coal.tileSprite;
                    else if (ironSpread.GetPixel(x,y).r > 0.5f)
                        tileSprite = tileAtlas.iron.tileSprite;
                    else if (goldSpread.GetPixel(x,y).r > 0.5f)
                        tileSprite = tileAtlas.gold.tileSprite;
                    else if (diamondSpread.GetPixel(x,y).r > 0.5f)
                        tileSprite = tileAtlas.diamond.tileSprite;
                    else
                        tileSprite = tileAtlas.stone.tileSprite;
                }
                else if (y < height - 1)
                {
                    tileSprite = tileAtlas.dirt.tileSprite;
                }
                else
                {
                    //top layer of the terrain
                    tileSprite = tileAtlas.grass.tileSprite;
                }

                if (generateCaves)
                {
                    if (caveNoiseTexture.GetPixel(x, y).r > 0.5f)
                    {
                        PlaceTile(tileSprite, x, y);
                    }
                }
                else
                {
                    PlaceTile(tileSprite, x, y);
                }

                if (y >= height - 1)
                {
                    int t = Random.Range(0, treeChance);
                    if (t == 1)
                    {
                        //generate a tree
                        if (worldTiles.Contains(new Vector2(x, y)))
                        {
                            GenerateTree(x, y + 1);
                        }
                    }
                }
            }
        }
    }

    public void GenerateNoiseTexture(float frequency, float limit, Texture2D noiseTexture)
    {
        for (int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; x < noiseTexture.width; y++)
            {
                float v = Mathf.PerlinNoise((x + seed) * frequency, (y + seed) * frequency);
                if (v > limit)
                    noiseTexture.SetPixel(x, y, Color.white);
                else
                    noiseTexture.SetPixel(x, y, Color.black);
            }
        }
    }

    void GenerateTree(int x, int y)
    {
        //define our tree

        //generate log
        int treeHeight = Random.Range(minTreeHeight, maxTreeHeight);
        for (int i = 0; i < treeHeight; i++)
        {
            PlaceTile(tileAtlas.log.tileSprite, x, y + i);
        }

        //generate leaves
        PlaceTile(tileAtlas.leaf.tileSprite, x, y + treeHeight);
        PlaceTile(tileAtlas.leaf.tileSprite, x, y + treeHeight + 1);
        PlaceTile(tileAtlas.leaf.tileSprite, x, y + treeHeight + 2);

        PlaceTile(tileAtlas.leaf.tileSprite, x - 1, y + treeHeight);
        PlaceTile(tileAtlas.leaf.tileSprite, x - 1, y + treeHeight + 1);

        PlaceTile(tileAtlas.leaf.tileSprite, x + 1, y + treeHeight);
        PlaceTile(tileAtlas.leaf.tileSprite, x + 1, y + treeHeight + 1);
    }

    public void PlaceTile(Sprite tileSprite, float x, float y)
    {
        GameObject newTile = new GameObject();

        float chunkCoord = (Mathf.Round(x / chunkSize) * chunkSize);
        chunkCoord /= chunkSize;

        newTile.transform.parent = worldChunks[(int)chunkCoord].transform;


        newTile.AddComponent<SpriteRenderer>();
        newTile.GetComponent<SpriteRenderer>().sprite = tileSprite;
        newTile.name = tileSprite.name;
        newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);

        worldTiles.Add(newTile.transform.position - (Vector3.one * 0.5f));
    }
}
