using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marching : MonoBehaviour
{
  public bool smoothTerrain = true;
  List<Vector3> vertices = new List<Vector3>();
  List<int> triangles = new List<int>();
  List<Vector2> uvs = new List<Vector2>();

  MeshFilter meshFilter;
  MeshRenderer meshRenderer;
  MeshCollider meshCollider;

  float terrainSurface = 0.5f;

  TerrainPoint[,,] terrainMap;


  private void Start()
  {
    meshFilter = GetComponent<MeshFilter>();
    meshRenderer = GetComponent<MeshRenderer>();
    meshCollider = GetComponent<MeshCollider>();
    meshRenderer.material.SetTexture("_TexArr", WorldGenerator.Instance.terrainTexArray);
    terrainMap = new TerrainPoint[MarchingData.width + 1, MarchingData.height + 1, MarchingData.width + 1];
    smoothTerrain = WorldGenerator.Instance.smoothTerrain;
    PopulateTerrainMap();
    CreateMeshData();

  }

  void CreateMeshData()
  {
    ClearMesh();

    for (int x = 0; x < MarchingData.width; x++)
      for (int y = 0; y < MarchingData.height; y++)
        for (int z = 0; z < MarchingData.width; z++)
          MarchCube(new Vector3Int(x, y, z));

    BuildMesh();
  }

  void ClearMesh()
  {
    vertices.Clear();
    triangles.Clear();
    uvs.Clear();
  }

  void BuildMesh()
  {
    Mesh mesh = new Mesh();
    mesh.vertices = vertices.ToArray();
    mesh.triangles = triangles.ToArray();
    mesh.uv = uvs.ToArray();
    mesh.RecalculateNormals();
    meshFilter.mesh = mesh;
    meshCollider.sharedMesh = mesh;
  }

  private void Update()
  {

  }

  float Noise(float x, float y, float scale, int octaves, float persistance, float lacunarity)
  {
    if (scale <= 0)
      scale = 0.00001f;

    float noiseHeight = 0;

    float frequency = 1.0f;
    float amplitude = 1.0f;

    for (int i = 0; i < octaves; i++)
    {
      float sampleX = x / scale * frequency;
      float sampleY = y / scale * frequency;

      float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2.0f - 1.0f;
      noiseHeight += perlinValue * amplitude;

      amplitude *= persistance;
      frequency *= lacunarity;
    }

    return noiseHeight;
  }

  void PopulateTerrainMap()
  {
    float px = transform.position.x;
    float py = transform.position.y;
    float pz = transform.position.z;

    float terrainHeight = MarchingData.height;

    for (int x = 0; x < MarchingData.width + 1; x++)
    {

      for (int z = 0; z < MarchingData.width + 1; z++)
      {
        int dx = (int)(px + x);
        int dz = (int)(pz + z);

        float thisHeight = Mathf.Clamp(Noise(dx, dz, 200, 8, 0.5f, 2.0f) / 2.0f + 0.5f, 0, 1);
        int textureID = 0;

        if (thisHeight < 0.25f)
          textureID = 0;
        else if (thisHeight < 0.45f)
          textureID = 1;
        else if (thisHeight < 0.65)
          textureID = 2;
        else
          textureID = 3;

        thisHeight *= terrainHeight;

        for (int y = 0; y < MarchingData.height + 1; y++)
          terrainMap[x, y, z] = new TerrainPoint((float)y - thisHeight, textureID);

      }
    }
  }

  void MarchCube(Vector3Int position)
  {
    float[] cube = new float[8];
    for (int i = 0; i < 8; i++)
      cube[i] = SampleTerrain(position + MarchingData.CornerTable[i]);


    int configIndex = GetCubeConfiguration(cube);

    if (configIndex == 0 || configIndex == 255)
      return;

    int edgeIndex = 0;
    for (int i = 0; i < 5; i++)
    {
      for (int p = 0; p < 3; p++)
      {
        int indice = MarchingData.TriangleTable[configIndex, edgeIndex];

        if (indice == -1)
          return;

        Vector3 vert1 = position + MarchingData.CornerTable[MarchingData.EdgeIndexes[indice, 0]];
        Vector3 vert2 = position + MarchingData.CornerTable[MarchingData.EdgeIndexes[indice, 1]];

        Vector3 vertPosition;

        if (smoothTerrain == true)
        {
          float vert1Sample = cube[MarchingData.EdgeIndexes[indice, 0]];
          float vert2Sample = cube[MarchingData.EdgeIndexes[indice, 1]];
          float difference = vert2Sample - vert1Sample;

          if (difference == 0)
            difference = terrainSurface;
          else
            difference = (terrainSurface - vert1Sample) / difference;

          vertPosition = vert1 + ((vert2 - vert1) * difference);
        }
        else
        {
          vertPosition = (vert1 + vert2) / 2.0f;
        }



        vertices.Add(vertPosition);
        triangles.Add(vertices.Count - 1);

        uvs.Add(new Vector2(terrainMap[(int)vertPosition.x, (int)vertPosition.y, (int)vertPosition.z].textureID, 0));

        edgeIndex++;


      }
    }

  }

  int GetCubeConfiguration(float[] cube)
  {
    int configIndex = 0;

    for (int i = 0; i < 8; i++)
    {
      if (cube[i] > terrainSurface)
        configIndex |= 1 << i;
    }

    return configIndex;
  }

  float SampleTerrain(Vector3Int point)
  {
    return terrainMap[point.x, point.y, point.z].dstSurface;
  }

}
