using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LowPolyTerrain : MonoBehaviour
{
    public enum LowPolyTerrainMode { ModifyHeight, SetHeight, SmoothHeight, Paint, Randomize, Option };
    public LowPolyTerrainMode lowPolyTerrainMode;

    // Modify
    public float brushSize = 1;
    public float brushStrength = 10f;
    public float brushSmoothness = 1f;

    // Set
    public float terrainHeight;

    // Smooth
    public float smoothFactor = 0.5f;

    // Paint
    public int materialIndex = 1;
    public int materialMask = -1;

    // Randomize
    public float perlinScale = 1f;
    public float perlinStrength = 1f;
    public bool perlinPower2 = false;

    // Options
    public int chunksWidth = 1;
    public int chunksHeight = 1;
    public int chunkWidth = 10;
    public int chunkHeight = 10;
    public float quadSize;
    public float diagonalQuadSize;
    public List<Material> materials;

    public float minHeight = -5f;
    public float maxHeight = 5f;
    public bool colliders = true;
    public bool flipNormals = false;

    public void GenerateDefaultMesh()
    {
        if (PrefabUtility.GetPrefabAssetType(gameObject) != PrefabAssetType.NotAPrefab)
        {
            PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        for (int x = 0; x < chunksWidth; x++)
        {
            for (int z = 0; z < chunksHeight; z++)
            {
                GameObject chunk = new GameObject("Terrain Chunk");
                chunk.transform.SetParent(transform, true);
                chunk.transform.localPosition = new Vector3(x * chunkWidth * quadSize, 0, z * chunkHeight * quadSize);
                chunk.AddComponent<LowPolyTerrainChunk>();
                chunk.AddComponent<MeshFilter>();
                chunk.AddComponent<MeshRenderer>();
                chunk.AddComponent<MeshCollider>();
                chunk.hideFlags = HideFlags.NotEditable;
                chunk.GetComponent<LowPolyTerrainChunk>().terrain = this;
                chunk.GetComponent<LowPolyTerrainChunk>().xIndex = x;
                chunk.GetComponent<LowPolyTerrainChunk>().zIndex = z;
                chunk.GetComponent<LowPolyTerrainChunk>().halfDiagonal = Mathf.Sqrt(chunkWidth * quadSize * chunkWidth * quadSize + chunkHeight * quadSize * chunkHeight * quadSize) / 2;
                chunk.GetComponent<LowPolyTerrainChunk>().GenerateDefaultMesh();
            }
        }

        diagonalQuadSize = Mathf.Sqrt(2 * quadSize * quadSize);
    }

    public void GenerateMesh()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<LowPolyTerrainChunk>().GenerateMesh();
        }
    }

    public void RandomizePerlinNoise()
    {
        float perlinSeed = Random.Range(1, 1000000);

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<LowPolyTerrainChunk>().RandomizePerlinNoise(perlinSeed);
        }
    }

    public void FillTerrain()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<LowPolyTerrainChunk>().FillTerrain();
        }
    }

    public void FlattenTerrain()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<LowPolyTerrainChunk>().FlattenTerrain();
        }
    }

    public void ClampVertices()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<LowPolyTerrainChunk>().ClampVertices();
        }
    }

    public void ChangeQuadSize(float previousSize)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<LowPolyTerrainChunk>().ChangeQuadSize(previousSize);
        }

        diagonalQuadSize = Mathf.Sqrt(2 * quadSize * quadSize);
    }

}
