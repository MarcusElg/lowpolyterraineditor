using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LowPolyTerrainChunk : MonoBehaviour
{

    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();
    public List<int> materialIndexes = new List<int>();
    public LowPolyTerrain terrain;
    public int xIndex, zIndex;
    public float halfDiagonal;

    public void GenerateDefaultMesh()
    {
        Undo.RegisterFullObjectHierarchyUndo(this, "Reset Terrain");
        vertices.Clear();
        materialIndexes.Clear();

        for (int x = 0; x < terrain.chunkWidth + 1; x++)
        {
            for (int y = 0; y < terrain.chunkHeight + 1; y++)
            {
                for (int i = 0; i < 4; i++)
                {
                    vertices.Add(new Vector3(x * terrain.quadSize, 0, y * terrain.quadSize));
                }

                if (x < terrain.chunkWidth && y < terrain.chunkHeight)
                {
                    materialIndexes.Add(0);
                    materialIndexes.Add(0);
                }
            }
        }

        GenerateMesh();
    }

    public void GenerateMesh()
    {
        if (terrain.materials == null || terrain.materials.Count == 0 || terrain.materials[0] == null)
        {
            terrain.materials = new List<Material> { Resources.Load("Materials/Grass") as Material };
        }

        List<List<int>> meshTriangles = new List<List<int>>();
        for (int i = 0; i < terrain.materials.Count; i++)
        {
            meshTriangles.Add(new List<int>());
        }

        triangles.Clear();

        for (int x = 0; x < terrain.chunkWidth; x++)
        {
            for (int y = 0; y < terrain.chunkHeight; y++)
            {
                triangles.Add(y * 4 + x * (terrain.chunkHeight + 1) * 4);
                triangles.Add(y * 4 + 4 + x * (terrain.chunkHeight + 1) * 4);
                triangles.Add(y * 4 + 4 + terrain.chunkHeight * 4 + x * (terrain.chunkHeight + 1) * 4);

                triangles.Add(y * 4 + 5 + x * (terrain.chunkHeight + 1) * 4);
                triangles.Add(y * 4 + 9 + 4 * terrain.chunkHeight + x * (terrain.chunkHeight + 1) * 4);
                triangles.Add(y * 4 + 5 + 4 * terrain.chunkHeight + x * (terrain.chunkHeight + 1) * 4);
            }
        }

        if (terrain.flipNormals == true)
        {
            triangles.Reverse();
        }

        for (int i = 0; i < materialIndexes.Count; i++)
        {
            meshTriangles[materialIndexes[i]].Add(triangles[i * 3]);
            meshTriangles[materialIndexes[i]].Add(triangles[i * 3 + 1]);
            meshTriangles[materialIndexes[i]].Add(triangles[i * 3 + 2]);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.subMeshCount = terrain.materials.Count;

        for (int i = 0; i < terrain.materials.Count; i++)
        {
            mesh.SetTriangles(meshTriangles[i].ToArray(), i);
        }

        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshRenderer>().sharedMaterials = terrain.materials.ToArray();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public void RandomizePerlinNoise(float perlinSeed)
    {
        Undo.RegisterFullObjectHierarchyUndo(this, "Randomize terrain (perlin noise)");
        for (int i = 0; i < vertices.Count; i++)
        {
            float xValue = terrain.perlinScale * 0.1f * (vertices[i].x + xIndex * terrain.chunkWidth) + perlinSeed;
            float zValue = terrain.perlinScale * 0.1f * (vertices[i].z + zIndex * terrain.chunkHeight) + perlinSeed;

            if (terrain.perlinPower2 == true)
            {
                vertices[i] = new Vector3(vertices[i].x, vertices[i].y + Mathf.PerlinNoise(xValue, zValue) * terrain.perlinStrength * Mathf.PerlinNoise(xValue, zValue) * terrain.perlinStrength, vertices[i].z);
            }
            else
            {
                vertices[i] = new Vector3(vertices[i].x, vertices[i].y + Mathf.PerlinNoise(xValue, zValue) * terrain.perlinStrength, vertices[i].z);
            }
        }

        GenerateMesh();
    }

    public void FillTerrain()
    {
        for (int i = 0; i < materialIndexes.Count; i++)
        {
            if (terrain.materialMask == -1 || materialIndexes[i] == terrain.materialMask)
            {
                materialIndexes[i] = terrain.materialIndex;
            }
        }

        GenerateMesh();
    }

    public void FlattenTerrain()
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = new Vector3(vertices[i].x, terrain.terrainHeight, vertices[i].z);
        }

        GenerateMesh();
    }

    public void ClampVertices()
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = new Vector3(vertices[i].x, Mathf.Clamp(vertices[i].y, terrain.minHeight, terrain.maxHeight), vertices[i].z);
        }
    }

    public void ChangeQuadSize(float previousSize)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = new Vector3((vertices[i].x / previousSize) * terrain.quadSize, vertices[i].y, (vertices[i].z / previousSize) * terrain.quadSize);
        }

        transform.localPosition = new Vector3(xIndex * terrain.chunkWidth * terrain.quadSize, 0, zIndex * terrain.chunkHeight * terrain.quadSize);
        halfDiagonal = Mathf.Sqrt(terrain.chunkWidth * terrain.quadSize * terrain.chunkWidth * terrain.quadSize + terrain.chunkHeight * terrain.quadSize * terrain.chunkHeight * terrain.quadSize) / 2;
    }

    public float GetDistance(Vector3 point)
    {
        Vector3 center = transform.localPosition + new Vector3(terrain.chunkWidth * terrain.quadSize, 0, terrain.chunkHeight * terrain.quadSize) / 2;
        return Vector2.Distance(new Vector2(point.x, point.z), new Vector2(center.x, center.z));
    }

}
