using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{

  private static WorldGenerator _instance;
  public static WorldGenerator Instance { get { return _instance; } }


  public GameObject MarchingCubePrefab;
  public int sizeOfMap = 256;

  public Texture2DArray terrainTexArray;
  public Texture2D[] terrainTextures;

  List<Vector2> buildList = new List<Vector2>();


  private void Awake()
  {
    if (_instance != null && _instance != this)
    {
      Destroy(this.gameObject);
    }
    else
    {
      _instance = this;
    }

    PopulateTextureArray();
  }



  // Start is called before the first frame update
  void Start()
  {


    for (int x = 0; x < sizeOfMap; x += MarchingData.width)
    {
      for (int z = 0; z < sizeOfMap; z += MarchingData.width)
      {
        buildList.Add(new Vector2(x, z));
      }
    }
    StartCoroutine(PopulateChunks());


  }

  // Update is called once per frame
  void Update()
  {

  }

  IEnumerator PopulateChunks()
  {
    while (buildList.Count > 0)
    {
      int x = (int)buildList[0].x;
      int z = (int)buildList[0].y;
      GameObject gob = Instantiate(MarchingCubePrefab, new Vector3(x, 0, z), Quaternion.identity);
      gob.name = "chunk " + x + ", " + z;
      buildList.RemoveAt(0);
      yield return null;
    }
  }

  void PopulateTextureArray()
  {
    terrainTexArray = new Texture2DArray(32, 32, terrainTextures.Length, TextureFormat.ARGB32, false);

    for (int i = 0; i < terrainTextures.Length; i++)
    {
      terrainTexArray.SetPixels(terrainTextures[i].GetPixels(0), i, 0);
    }
    terrainTexArray.Apply();

  }
}
