using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "Minecraft 4D / Biome Attribute")]
public class BiomeAttributes : ScriptableObject{
	[Header("Attributes")]
	public string biomeName;
	public int offset;
	public float scale;
	
	[Header("Terrain")]
	// public int solidGroundHeight;
	public int terrainHeight;
	public float terrainScale;
	public byte surfaceBlockID = 5;
	public byte subSurfaceBlockID = 4;
	
	[Header("Major Flora")]
	public float majorFloraZoneScale = 1.3f;
	[Range(0f, 1f)]
	public float majorFloraZoneThreshold = 0.6f;
	public float majorFloraPlacementScale = 15f;
	[Range(0f, 1f)]
	public float majorFloraPlacementThreshold = 0.8f;
	public bool placeMajorFlora = true;
	
	public int	maxHeight = 12,
				minHeight = 5;
	
	public Lode[] lodes;
}

[System.Serializable]
public class Lode{
	public string nodeName;
	public byte blockID;
	public int	minHeight,
					maxHeight;
	public float	scale,
						threshold,
						noiseOffset;
}