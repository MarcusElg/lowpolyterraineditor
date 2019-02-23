using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LowPolyTerrain))]
public class LowPolyTerrainEditor : Editor
{

    Tool lastTool;
    bool leftButtonDown;

    void OnEnable()
    {
        lastTool = Tools.current;

        Tools.current = Tool.None;

        for (int i = 0; i < targets.Length; i++)
        {
            LowPolyTerrain terrain = (LowPolyTerrain)targets[i];
            if (terrain.transform.childCount == 0)
            {
                terrain.GenerateDefaultMesh();
            }
            else
            {
                for (int j = 0; j < terrain.transform.childCount; j++)
                {
                    if (terrain.transform.GetChild(j).GetComponent<MeshFilter>().sharedMesh != null)
                    {
                        terrain.transform.GetChild(j).GetComponent<LowPolyTerrainChunk>().vertices = new List<Vector3>(terrain.transform.GetChild(j).GetComponent<MeshFilter>().sharedMesh.vertices);
                    }
                }
            }
        }
    }

    private void OnDisable()
    {
        Tools.current = lastTool;
    }

    public override void OnInspectorGUI()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            LowPolyTerrain terrain = (LowPolyTerrain)targets[i];

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Modify"))
            {
                terrain.lowPolyTerrainMode = LowPolyTerrain.LowPolyTerrainMode.ModifyHeight;
            }
            else if (GUILayout.Button("Set"))
            {
                terrain.lowPolyTerrainMode = LowPolyTerrain.LowPolyTerrainMode.SetHeight;
            }
            else if (GUILayout.Button("Smooth"))
            {
                terrain.lowPolyTerrainMode = LowPolyTerrain.LowPolyTerrainMode.SmoothHeight;
            }
            else if (GUILayout.Button("Paint"))
            {
                terrain.lowPolyTerrainMode = LowPolyTerrain.LowPolyTerrainMode.Paint;
            }
            else if (GUILayout.Button("Randomize"))
            {
                terrain.lowPolyTerrainMode = LowPolyTerrain.LowPolyTerrainMode.Randomize;
            }
            else if (GUILayout.Button("Options"))
            {
                terrain.lowPolyTerrainMode = LowPolyTerrain.LowPolyTerrainMode.Option;
            }
            EditorGUILayout.EndHorizontal();
            DrawLine();

            if (terrain.lowPolyTerrainMode == LowPolyTerrain.LowPolyTerrainMode.ModifyHeight)
            {
                EditorGUI.BeginChangeCheck();
                serializedObject.FindProperty("brushSize").floatValue = Mathf.Clamp(EditorGUILayout.FloatField("Brush Size", serializedObject.FindProperty("brushSize").floatValue), 0, Mathf.Max(serializedObject.FindProperty("chunkWidth").intValue * serializedObject.FindProperty("chunksWidth").intValue, serializedObject.FindProperty("chunksHeight").intValue * serializedObject.FindProperty("chunkHeight").intValue) * 0.75f);
                serializedObject.FindProperty("brushStrength").floatValue = Mathf.Max(EditorGUILayout.FloatField("Brush Strength", serializedObject.FindProperty("brushStrength").floatValue), 0);
                serializedObject.FindProperty("brushSmoothness").floatValue = Mathf.Clamp01(EditorGUILayout.FloatField("Brush Smoothness", serializedObject.FindProperty("brushSmoothness").floatValue));

                if (EditorGUI.EndChangeCheck() == true)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
            else if (terrain.lowPolyTerrainMode == LowPolyTerrain.LowPolyTerrainMode.SetHeight)
            {
                EditorGUI.BeginChangeCheck();
                serializedObject.FindProperty("brushSize").floatValue = Mathf.Clamp(EditorGUILayout.FloatField("Brush Size", serializedObject.FindProperty("brushSize").floatValue), 0, Mathf.Max(serializedObject.FindProperty("chunkWidth").intValue * serializedObject.FindProperty("chunksWidth").intValue, serializedObject.FindProperty("chunksHeight").intValue * serializedObject.FindProperty("chunkHeight").intValue) * 0.75f);
                serializedObject.FindProperty("terrainHeight").floatValue = Mathf.Clamp(EditorGUILayout.FloatField("Height", serializedObject.FindProperty("terrainHeight").floatValue), serializedObject.FindProperty("minHeight").floatValue, serializedObject.FindProperty("maxHeight").floatValue);

                if (EditorGUI.EndChangeCheck() == true)
                {
                    serializedObject.ApplyModifiedProperties();
                }

                if (GUILayout.Button("Flatten Terrain"))
                {
                    terrain.FlattenTerrain();
                }
            }
            else if (terrain.lowPolyTerrainMode == LowPolyTerrain.LowPolyTerrainMode.SmoothHeight)
            {
                EditorGUI.BeginChangeCheck();
                serializedObject.FindProperty("brushSize").floatValue = Mathf.Clamp(EditorGUILayout.FloatField("Brush Size", serializedObject.FindProperty("brushSize").floatValue), 0, Mathf.Max(serializedObject.FindProperty("chunkWidth").intValue * serializedObject.FindProperty("chunksWidth").intValue, serializedObject.FindProperty("chunksHeight").intValue * serializedObject.FindProperty("chunkHeight").intValue) * 0.75f);
                serializedObject.FindProperty("smoothFactor").floatValue = Mathf.Clamp(EditorGUILayout.FloatField("Smooth Factor", serializedObject.FindProperty("smoothFactor").floatValue), 1, 10);

                if (EditorGUI.EndChangeCheck() == true)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
            else if (terrain.lowPolyTerrainMode == LowPolyTerrain.LowPolyTerrainMode.Paint)
            {
                EditorGUI.BeginChangeCheck();
                serializedObject.FindProperty("brushSize").floatValue = Mathf.Clamp(EditorGUILayout.FloatField("Brush Size", serializedObject.FindProperty("brushSize").floatValue), 0, Mathf.Max(serializedObject.FindProperty("chunkWidth").intValue * serializedObject.FindProperty("chunksWidth").intValue, serializedObject.FindProperty("chunksHeight").intValue * serializedObject.FindProperty("chunkHeight").intValue) * 0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("materials"), true);
                serializedObject.FindProperty("materialIndex").intValue = Mathf.Clamp(EditorGUILayout.IntField("Material Index", serializedObject.FindProperty("materialIndex").intValue), 0, serializedObject.FindProperty("materials").arraySize - 1);
                serializedObject.FindProperty("materialMask").intValue = Mathf.Clamp(EditorGUILayout.IntField("Material Mask", serializedObject.FindProperty("materialMask").intValue), -1, serializedObject.FindProperty("materials").arraySize - 1);

                if (EditorGUI.EndChangeCheck() == true)
                {
                    serializedObject.ApplyModifiedProperties();
                }

                if (GUILayout.Button("Fill Terrain"))
                {
                    terrain.FillTerrain();
                }
            }
            else if (terrain.lowPolyTerrainMode == LowPolyTerrain.LowPolyTerrainMode.Randomize)
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.Label("Randomize with perlin noise");
                serializedObject.FindProperty("perlinScale").floatValue = Mathf.Min(8f, Mathf.Max(EditorGUILayout.FloatField("Perlin Noise Scale", serializedObject.FindProperty("perlinScale").floatValue), 0));
                serializedObject.FindProperty("perlinStrength").floatValue = EditorGUILayout.FloatField("Perlin Strength", serializedObject.FindProperty("perlinStrength").floatValue);
                serializedObject.FindProperty("perlinPower2").boolValue = GUILayout.Toggle(serializedObject.FindProperty("perlinPower2").boolValue, "Exponential");

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }

                if (GUILayout.Button("Randomize with perlin noise"))
                {
                    terrain.RandomizePerlinNoise();
                }
            }
            else
            {
                GUILayout.Label("Warning: changing these properties will reset the terrain");
                EditorGUI.BeginChangeCheck();
                serializedObject.FindProperty("chunksWidth").intValue = Mathf.Clamp(EditorGUILayout.IntField("Chunks X", serializedObject.FindProperty("chunksWidth").intValue), 1, 50);
                serializedObject.FindProperty("chunksHeight").intValue = Mathf.Clamp(EditorGUILayout.IntField("Chunks Z", serializedObject.FindProperty("chunksHeight").intValue), 1, 50);
                serializedObject.FindProperty("chunkWidth").intValue = Mathf.Clamp(EditorGUILayout.IntField("Chunk Width", serializedObject.FindProperty("chunkWidth").intValue), 1, 20);
                serializedObject.FindProperty("chunkHeight").intValue = Mathf.Clamp(EditorGUILayout.IntField("Chunk Height", serializedObject.FindProperty("chunkHeight").intValue), 1, 20);
                DrawLine();

                if (EditorGUI.EndChangeCheck() == true)
                {
                    serializedObject.ApplyModifiedProperties();
                    terrain.GenerateDefaultMesh();
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("materials"), true);
                serializedObject.FindProperty("minHeight").floatValue = EditorGUILayout.FloatField("Minimum Height", serializedObject.FindProperty("minHeight").floatValue);
                serializedObject.FindProperty("maxHeight").floatValue = EditorGUILayout.FloatField("Maximum Height", serializedObject.FindProperty("maxHeight").floatValue);

                if (serializedObject.FindProperty("minHeight").floatValue >= serializedObject.FindProperty("maxHeight").floatValue)
                {
                    serializedObject.FindProperty("minHeight").floatValue = serializedObject.FindProperty("maxHeight").floatValue;
                }

                terrain.ClampVertices();

                if (EditorGUI.EndChangeCheck() == true)
                {
                    serializedObject.ApplyModifiedProperties();
                    terrain.GenerateMesh();
                }

                float quadSize = serializedObject.FindProperty("quadSize").floatValue;
                EditorGUI.BeginChangeCheck();
                serializedObject.FindProperty("quadSize").floatValue = Mathf.Max(EditorGUILayout.FloatField("Quad Size", serializedObject.FindProperty("quadSize").floatValue), 0.0001f);

                if (EditorGUI.EndChangeCheck() == true)
                {
                    if (quadSize != serializedObject.FindProperty("quadSize").floatValue)
                    {
                        serializedObject.ApplyModifiedPropertiesWithoutUndo();
                        terrain.ChangeQuadSize(quadSize);
                        terrain.GenerateMesh();
                    }
                }

                EditorGUI.BeginChangeCheck();
                serializedObject.FindProperty("colliders").boolValue = GUILayout.Toggle(serializedObject.FindProperty("colliders").boolValue, "Generate Colliders");

                if (EditorGUI.EndChangeCheck() == true)
                {
                    for (int j = 0; j < terrain.transform.childCount; j++)
                    {
                        serializedObject.ApplyModifiedProperties();
                        terrain.transform.GetChild(j).GetComponent<MeshCollider>().enabled = serializedObject.FindProperty("colliders").boolValue;
                    }
                }
            }

            GUILayout.Space(20);
            DrawLine();
            if (GUILayout.Button("Generate Terrain"))
            {
                terrain.GenerateMesh();
            }
            if (GUILayout.Button("Reset Terrain"))
            {
                terrain.GenerateDefaultMesh();
            }
        }
    }

    private void DrawLine()
    {
        Rect rect = EditorGUILayout.BeginHorizontal();
        Handles.color = Color.black;
        Handles.DrawLine(new Vector2(rect.x, rect.y), new Vector2(rect.width, rect.y));
        EditorGUILayout.EndHorizontal();
    }

    private void OnSceneGUI()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            LowPolyTerrain terrain = (LowPolyTerrain)targets[i];
            terrain.transform.rotation = Quaternion.identity;
            terrain.transform.localScale = Vector3.one;

            HandleUtility.nearestControl = GUIUtility.GetControlID(FocusType.Passive);
            Event currentEvent = Event.current;

            if (terrain.lowPolyTerrainMode == LowPolyTerrain.LowPolyTerrainMode.ModifyHeight || terrain.lowPolyTerrainMode == LowPolyTerrain.LowPolyTerrainMode.SetHeight || terrain.lowPolyTerrainMode == LowPolyTerrain.LowPolyTerrainMode.SmoothHeight || terrain.lowPolyTerrainMode == LowPolyTerrain.LowPolyTerrainMode.Paint)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
                RaycastHit raycastHit;

                if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
                {
                    leftButtonDown = true;
                }
                else if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0)
                {
                    leftButtonDown = false;
                }

                if (Physics.Raycast(ray, out raycastHit))
                {
                    if (raycastHit.transform.parent == ((LowPolyTerrain)target).transform)
                    {
                        Handles.color = Color.white;
                        Handles.DrawWireDisc(raycastHit.point, Vector3.up, terrain.brushSize);

                        if (leftButtonDown == true)
                        {
                            Undo.RegisterFullObjectHierarchyUndo(terrain, "Modify terrain");

                            if (terrain.lowPolyTerrainMode == LowPolyTerrain.LowPolyTerrainMode.Paint)
                            {
                                for (int j = 0; j < terrain.transform.childCount; j++)
                                {
                                    LowPolyTerrainChunk terrainChunk = terrain.transform.GetChild(j).GetComponent<LowPolyTerrainChunk>();

                                    // Chunk could be inside brush area
                                    if (terrainChunk.GetDistance(raycastHit.point) < terrainChunk.halfDiagonal + terrain.brushSize)
                                    {
                                        for (int k = 0; k < terrainChunk.triangles.Count; k += 3)
                                        {
                                            Vector3 centerPosition = GetCenter(terrainChunk.vertices[terrainChunk.triangles[k]], terrainChunk.vertices[terrainChunk.triangles[k + 1]], terrainChunk.vertices[terrainChunk.triangles[k + 2]]);
                                            float distance = Vector2.Distance(new Vector2(raycastHit.point.x, raycastHit.point.z) - new Vector2(terrainChunk.transform.localPosition.x, terrainChunk.transform.localPosition.z), new Vector2(centerPosition.x, centerPosition.z));

                                            if (distance < terrain.brushSize)
                                            {
                                                if (terrain.materialMask == -1 || terrainChunk.materialIndexes[k / 3] == terrain.materialMask)
                                                {
                                                    if (currentEvent.shift == true)
                                                    {
                                                        terrainChunk.materialIndexes[k / 3] = 0;
                                                    }
                                                    else
                                                    {
                                                        terrainChunk.materialIndexes[k / 3] = terrain.materialIndex;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if (terrain.lowPolyTerrainMode == LowPolyTerrain.LowPolyTerrainMode.SmoothHeight)
                            {
                                // Add vertices in near chunks
                                List<Vector3> vertices = new List<Vector3>();
                                List<int> indexes = new List<int>();
                                List<LowPolyTerrainChunk> terrainChunks = new List<LowPolyTerrainChunk>();

                                for (int j = 0; j < terrain.transform.childCount; j++)
                                {
                                    LowPolyTerrainChunk terrainChunk = terrain.transform.GetChild(j).GetComponent<LowPolyTerrainChunk>();

                                    // Chunk could be inside brush area
                                    if (terrainChunk.GetDistance(raycastHit.point) < terrainChunk.halfDiagonal + terrain.brushSize)
                                    {
                                        for (int k = 0; k < terrainChunk.vertices.Count; k += 4)
                                        {
                                            vertices.Add(terrainChunk.vertices[k] + terrainChunk.transform.position);
                                            indexes.Add(k);
                                            terrainChunks.Add(terrainChunk);
                                        }
                                    }
                                }

                                // Add vertices inside brush
                                List<int> verticeIndexes = new List<int>();
                                List<float> verticeHeights = new List<float>();
                                List<LowPolyTerrainChunk> verticeChunks = new List<LowPolyTerrainChunk>();

                                for (int k = 0; k < vertices.Count; k++)
                                {
                                    float distance = Vector2.Distance(new Vector2(raycastHit.point.x, raycastHit.point.z), new Vector2(vertices[k].x, vertices[k].z));

                                    if (distance < terrain.brushSize)
                                    {
                                        float totalVertexHight = 0;
                                        float amount = 0;
                                        for (int l = 0; l < vertices.Count; l++)
                                        {
                                            if (Vector2.Distance(new Vector2(vertices[l].x, vertices[l].z), new Vector2(vertices[k].x, vertices[k].z)) < terrain.diagonalQuadSize)
                                            {
                                                totalVertexHight += vertices[l].y;
                                                amount += 1;
                                            }
                                        }

                                        verticeIndexes.Add(indexes[k]);
                                        verticeHeights.Add(totalVertexHight / amount);
                                        verticeChunks.Add(terrainChunks[k]);
                                    }
                                }

                                // Modify vertices
                                for (int k = 0; k < verticeIndexes.Count; k++)
                                {
                                    Vector3 vertex = verticeChunks[k].vertices[verticeIndexes[k]];
                                    float height = Mathf.Lerp(vertex.y, verticeHeights[k], terrain.smoothFactor / 10f);
                                    Vector3 newPosition = new Vector3(vertex.x, height, vertex.z);
                                    verticeChunks[k].vertices[verticeIndexes[k]] = newPosition;
                                    verticeChunks[k].vertices[verticeIndexes[k] + 1] = newPosition;
                                    verticeChunks[k].vertices[verticeIndexes[k] + 2] = newPosition;
                                    verticeChunks[k].vertices[verticeIndexes[k] + 3] = newPosition;
                                }
                            }
                            else
                            {
                                for (int j = 0; j < terrain.transform.childCount; j++)
                                {
                                    LowPolyTerrainChunk terrainChunk = terrain.transform.GetChild(j).GetComponent<LowPolyTerrainChunk>();

                                    // Chunk could be inside brush area
                                    if (terrainChunk.GetDistance(raycastHit.point) < terrainChunk.halfDiagonal + terrain.brushSize)
                                    {
                                        for (int k = 0; k < terrainChunk.vertices.Count; k += 4)
                                        {
                                            float distance = Vector2.Distance(new Vector2(raycastHit.point.x, raycastHit.point.z) - new Vector2(terrainChunk.transform.localPosition.x, terrainChunk.transform.localPosition.z), new Vector2(terrainChunk.vertices[k].x, terrainChunk.vertices[k].z));

                                            if (distance < terrain.brushSize)
                                            {
                                                distance = Mathf.Min(terrain.brushSize, terrain.brushSize - distance + ((1 - terrain.brushSmoothness) * terrain.brushSize));

                                                if (terrain.lowPolyTerrainMode == LowPolyTerrain.LowPolyTerrainMode.ModifyHeight)
                                                {
                                                    if (currentEvent.shift == true)
                                                    {
                                                        Vector3 position = new Vector3(terrainChunk.vertices[k].x, Mathf.Clamp(terrainChunk.vertices[k].y - (0.01f * terrain.brushStrength * distance), terrain.minHeight, terrain.maxHeight), terrainChunk.vertices[k].z);
                                                        terrainChunk.vertices[k] = position;
                                                        terrainChunk.vertices[k + 1] = position;
                                                        terrainChunk.vertices[k + 2] = position;
                                                        terrainChunk.vertices[k + 3] = position;
                                                    }
                                                    else
                                                    {
                                                        Vector3 position = new Vector3(terrainChunk.vertices[k].x, Mathf.Clamp(terrainChunk.vertices[k].y + (0.01f * terrain.brushStrength * distance), terrain.minHeight, terrain.maxHeight), terrainChunk.vertices[k].z);
                                                        terrainChunk.vertices[k] = position;
                                                        terrainChunk.vertices[k + 1] = position;
                                                        terrainChunk.vertices[k + 2] = position;
                                                        terrainChunk.vertices[k + 3] = position;
                                                    }
                                                }
                                                else
                                                {
                                                    Vector3 position = new Vector3(terrainChunk.vertices[k].x, terrain.terrainHeight, terrainChunk.vertices[k].z);
                                                    terrainChunk.vertices[k] = position;
                                                    terrainChunk.vertices[k + 1] = position;
                                                    terrainChunk.vertices[k + 2] = position;
                                                    terrainChunk.vertices[k + 3] = position;
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            terrain.GenerateMesh();
                        }
                    }
                }
            }
        }

        SceneView.RepaintAll();
    }

    public Vector3 GetCenter(Vector3 one, Vector3 two, Vector3 three)
    {
        return (one + two + three) / 3;
    }

}
