using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPoint 
{
    public float dstSurface = 1.0f;
    public int textureID = 0;

    public TerrainPoint(float dst, int tex)
    {
        dstSurface = dst;
        textureID = tex;
    }
}
