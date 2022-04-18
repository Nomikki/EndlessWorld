using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "Endless World/BiomeAttribute")]
public class BiomeAttributes : ScriptableObject
{
    [Header("Biome settings")]
    public string biomeName;

    public int offset;
    public float scale;
    public int octaces;

    public int terrainHeight;
    public float terrainScale;

    public byte surfaceBlock;
    public byte subsurfaceBlock;

}
