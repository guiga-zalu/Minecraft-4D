using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData{
	public static readonly int	ChunkWidth	= 16,
										ChunkHeight	= 128,
										ChunkLength	= 16,
										ChunkDepth	= 1,
										WorldSizeInChunks_x	= 8,
										WorldSizeInChunks_z	= 8,
										WorldSizeInChunks_w	= 1;
	
	public static int WorldSizeInVoxels_x {
		get { return WorldSizeInChunks_x * ChunkWidth; }
	}
	public static int WorldSizeInVoxels_z {
		get { return WorldSizeInChunks_z * ChunkLength; }
	}
	public static int WorldSizeInVoxels_w {
		get { return WorldSizeInChunks_w * ChunkDepth; }
	}
	
	public static readonly int ViewDistanceInChunks = 4;
	
	public static Vector4Int WorldCenter {
		get {
			return new Vector4Int(
				ChunkWidth * WorldSizeInChunks_x / 2,
				ChunkHeight,
				ChunkLength * WorldSizeInChunks_z / 2,
				ChunkDepth * WorldSizeInChunks_w / 2
			);
		}
	}
	
	public static float minLightLevel = 0.25f, maxLightLevel = 0.8f;
	// public static byte lightLevels = 8;
	/* public static float lightFalloff {
		get { return (maxLightLevel - minLightLevel) / (float) lightLevels; }
	} */
	public static float lightFalloff = 0.08f;
	public static float unityOfLight = 0.0625f; //1f / 16f;
	
	public static int seed = 123;
	
	public static readonly int TextureAtlasSizeInBlocks = 16;
	public static float NormalizedBlockTextureSize {
		get { return 1f / (float) TextureAtlasSizeInBlocks; }
	}
	
	public static readonly Vector3Int[] voxelVerts = new Vector3Int[8] {
		new Vector3Int(0, 0, 0),
		new Vector3Int(1, 0, 0),
		new Vector3Int(1, 1, 0),
		new Vector3Int(0, 1, 0),
		new Vector3Int(0, 0, 1),
		new Vector3Int(1, 0, 1),
		new Vector3Int(1, 1, 1),
		new Vector3Int(0, 1, 1)
	};
	public static readonly Vector3Int[] faceChecks = new Vector3Int[6] {
		new Vector3Int(0,	0,	-1),// Trás
		new Vector3Int(0,	0,	1),	// Frente
		new Vector3Int(0,	1,	0),	// Cima
		new Vector3Int(0,	-1,	0),	// Baixo
		new Vector3Int(-1,	0,	0),	// Esquerda
		new Vector3Int(1,	0,	0)	// Direita
	};
	public static readonly Vector4Int[] faceChecks4 = new Vector4Int[8] {
		new Vector4Int(0,	0,	-1,	0),	// Trás
		new Vector4Int(0,	0,	1,	0),	// Frente
		new Vector4Int(0,	1,	0,	0),	// Cima
		new Vector4Int(0,	-1,	0,	0),	// Baixo
		new Vector4Int(-1,	0,	0,	0),	// Esquerda
		new Vector4Int(1,	0,	0,	0),	// Direita
		new Vector4Int(0,	0,	0,	-1),// W-
		new Vector4Int(0,	0,	0,	1)	// W+
	};
	public static readonly int[] revFaceChecks	= new int[6] { 1, 0, 3, 2, 5, 4 },
								 revFaceChecks4	= new int[8] { 1, 0, 3, 2, 5, 4, 7, 6 };
	public static readonly int[,] voxelTris = new int[6, 4] {
		// 0 1 2 2 1 3
		{0, 3, 1, 2},	// Trás
		{5, 6, 4, 7},	// Frente
		{3, 7, 2, 6},	// Topo
		{1, 5, 0, 4},	// Base
		{4, 7, 0, 3},	// Esquerda
		{1, 2, 5, 6}	// Direita
	};
	public static readonly Vector2[] voxelUvs = new Vector2[4] {
		new Vector2(0.0f, 0.0f),
		new Vector2(0.0f, 1.0f),
		new Vector2(1.0f, 0.0f),
		new Vector2(1.0f, 1.0f)
	};
	
	public static readonly bool IS_4D_OK = false;
}

public static class Helper{
	public static int floor(float f){
		return Mathf.FloorToInt(f);
	}
	public static Vector3 floorVector3(Vector3 v){
		return new Vector3(
			Helper.floor(v.x),
			Helper.floor(v.y),
			Helper.floor(v.z)
		);
	}
	public static Vector3Int floorVector3I(Vector3 v){
		return new Vector3Int(
			Helper.floor(v.x),
			Helper.floor(v.y),
			Helper.floor(v.z)
		);
	}
	public static Vector3Int floorVector3I(Vector4 v){
		return new Vector3Int(
			Helper.floor(v.x),
			Helper.floor(v.y),
			Helper.floor(v.z)
		);
	}
	public static Vector4Int floorVector4I(Vector3 v){
		return new Vector4Int(
			Helper.floor(v.x),
			Helper.floor(v.y),
			Helper.floor(v.z),
			0
		);
	}
	public static Vector4Int floorVector4I(Vector4 v){
		return new Vector4Int(
			Helper.floor(v.x),
			Helper.floor(v.y),
			Helper.floor(v.z),
			Helper.floor(v.w)
		);
	}
	public static float round(float x, int casas = 0){
		float c = Mathf.Pow(10, casas);
		return Helper.floor(x * c) / c;
	}
	public static void log(string text){
		Debug.Log(text);
	}
}