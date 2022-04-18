using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Jobs;

public class Marching : MonoBehaviour
{
  public bool smoothTerrain = true;
  List<Vector3> vertices = new List<Vector3>();
  List<int> triangles = new List<int>();
  List<Vector2> uvs = new List<Vector2>();

  //NativeArray<Vector3> vertices = new NativeArray<Vector3>();
  //NativeArray<int> triangles = new NativeArray<int>();
  //NativeArray<Vector2> uvs = new NativeArray<Vector2>();

  MeshFilter meshFilter;
  MeshRenderer meshRenderer;
  MeshCollider meshCollider;

  float terrainSurface = 0.5f;

  TerrainPoint[,,] terrainMap;

  private void Start()
  {
    Build();
  }

  public void Build()
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
    
    for (int x = 0; x < MarchingData.width; x++)
      for (int y = 0; y < MarchingData.height; y++)
        for (int z = 0; z < MarchingData.width; z++)
          MarchCube(new Vector3Int(x, y, z));
          

    BuildMesh();
  }


  void BuildMesh()
  {
    Mesh mesh = new Mesh();
    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    mesh.vertices = vertices.ToArray();
    mesh.MarkDynamic();
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

  float Get2DPerlin(Vector2 position, float offset, float scale)
  {
    position.x += (offset + 0.1f);
    position.y += (offset + 0.1f);

    return Mathf.PerlinNoise(position.x / MarchingData.width * scale, position.y / MarchingData.width * scale);
  }

  void PopulateTerrainMap()
  {
    float px = transform.position.x;
    float py = transform.position.y;
    float pz = transform.position.z;

    float terrainHeight = MarchingData.height;

    float dx = 0;
    float dz = 0;
    float thisHeight = 0;
    int textureID = 0;

    int solidGroundHeight = 20;

    for (int x = 0; x < MarchingData.width + 1; x++)
    {

      for (int z = 0; z < MarchingData.width + 1; z++)
      {
        dx = (int)(px + x + WorldGenerator.Instance.randomOffset.x);
        dz = (int)(pz + z + WorldGenerator.Instance.randomOffset.y);


        //biome pass
        float strongestWeight = 0;
        int strongestBiomeIndex = 0;
        int count = 0;
        float sumOfHeight = 0;

        for (int i = 0; i < WorldGenerator.Instance.biomes.Length; i++)
        {
          BiomeAttributes tempBiome = WorldGenerator.Instance.biomes[i];
          float weight = Get2DPerlin(new Vector2(dx, dz), tempBiome.offset, tempBiome.scale);
          if (weight > strongestWeight)
          {
            strongestWeight = weight;
            strongestBiomeIndex = i;
          }

          float height = tempBiome.terrainHeight * Get2DPerlin(new Vector2(dx, dz), 0, tempBiome.terrainScale) * weight;

          if (height > 0)
          {
            sumOfHeight += height;
            count++;
          }
          //Debug.Log(i + ": " + height);
        }

        BiomeAttributes biome = WorldGenerator.Instance.biomes[strongestBiomeIndex];
        if (sumOfHeight > 0)
          sumOfHeight /= count;

        //Debug.Log(sumOfHeight);


        //height pass

        //thisHeight = Mathf.Clamp(Noise(dx, dz, 200, 8, 0.5f, 2.0f) / 2.0f + 0.5f, 0, 1);
        thisHeight = Mathf.Clamp(solidGroundHeight + sumOfHeight, 0, MarchingData.height);
        
        
        float hight = (Mathf.Clamp(Noise(dx, dz, 200, 8, 0.5f, 2.0f), 0, 1) * 128);
        float low = (Mathf.Clamp(Noise(dx, dz, 1000, 10, 0.5f, 2.0f), 0, 1) * 128);



        thisHeight = Mathf.Clamp(thisHeight + (hight > low ? hight : low), 0, 128);


        for (int y = 0; y < MarchingData.height + 1; y++)
        {
          float yPos = (float)y - thisHeight;
          
          if (y == (int)thisHeight)
          {
            /*
            if (biomeVariation < 0.25f)
              textureID = MarchingData.BiomeTypes.sand;
            else if (biomeVariation < 0.45f)
              textureID = MarchingData.BiomeTypes.grass;
            else if (biomeVariation < 0.65)
              textureID = MarchingData.BiomeTypes.rock;
            else
              textureID = MarchingData.BiomeTypes.snow;
              */
              textureID = biome.surfaceBlock;
          } else {
            textureID = biome.subsurfaceBlock;
          }
          
          terrainMap[x, y, z] = new TerrainPoint(yPos, (int)textureID);
        }

        //generate trees
        /* 
        if (thisHeight > WorldGenerator.Instance.seaHeight)
        {
          int tree = Random.Range(0, 4);

          if (textureID == 1)
          {
            
            if (Random.Range(0, 100) > 85)
            {
              GameObject gob = Instantiate(WorldGenerator.Instance.TreePrefabs[tree], new Vector3(x + px, thisHeight, z + pz), Quaternion.Euler(-90 + Random.Range(-15, 15),  Random.Range(0, 360), 0));
              float r = Random.Range(80f, 160f);
              gob.transform.localScale = new Vector3(r, r, r);
              gob.transform.SetParent(this.transform);
            }
          } else if (textureID == 2)
          {
            if (Random.Range(0, 100) > 98)
            {
              float r = Random.Range(80f, 160f);
              GameObject gob = Instantiate(WorldGenerator.Instance.TreePrefabs[tree], new Vector3(x + px, thisHeight, z + pz), Quaternion.Euler(-90 + Random.Range(-15, 15),  Random.Range(0, 360), 0));
              gob.transform.localScale = new Vector3(r, r, r);
              gob.transform.SetParent(this.transform);
            }
          }
        }
         */
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

        Vector3 v1 = (MarchingData.CornerTable[MarchingData.EdgeIndexes[indice, 0]]);
        Vector3 v2 = (MarchingData.CornerTable[MarchingData.EdgeIndexes[indice, 1]]);

        Vector3 vert1 = position + v1;
        Vector3 vert2 = position + v2;


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
