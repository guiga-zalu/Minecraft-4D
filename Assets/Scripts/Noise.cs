using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise{
	static int cHeight	= VoxelData.ChunkHeight;
	static int cWidth	= VoxelData.ChunkWidth;
	static int cLength	= VoxelData.ChunkLength;
	// public static float Get2DPerlin(Vector2 position, float offset, float scale){
	static float noise(float x, float y){
		return Mathf.PerlinNoise(x, y);
	}
	public static float Get2DPerlin(float _x, float _z, float offset, float scale){
		offset += (float) VoxelData.seed;
		float	x = (_x + 0.1f + offset) * scale / cWidth,
				z = (_z + 0.1f + offset) * scale / cLength;
		return noise(x, z);
	}
	public static float Get2DPerlin(Vector2 pos, float offset, float scale){
		return Get2DPerlin(pos.x, pos.y, offset, scale);
	}
	public static float Get2DPerlin(Vector3 pos, float offset, float scale){
		return Get2DPerlin(pos.x, pos.z, offset, scale);
	}
	public static float Get2DPerlin(Vector3Int pos, float offset, float scale){
		return Get2DPerlin((float) pos.x, (float) pos.z, offset, scale);
	}
	static float fn(float x){
		return x;
	}
	static float nf(float x){
		return x;
	}
	public static float Get3DPerlin(Vector3 position, float offset, float scale){
		offset += (float) VoxelData.seed;
		// https://www.youtube.com/watch?v=Aga0TBJkchM Carpilot on YouTube
		float	x = (position.x + 0.1f + offset) * scale,
				y = (position.y + 0.1f + offset) * scale,
				z = (position.z + 0.1f + offset) * scale,
				n = fn(noise(x, y)) + fn(noise(y, z)) + fn(noise(z, x)) +
					fn(noise(x, z)) + fn(noise(z, y)) + fn(noise(y, x));
		n /= 6f;
		return nf(n);
	}
	public static float Get3DPerlin(Vector3Int position, float offset, float scale){
		// https://www.youtube.com/watch?v=Aga0TBJkchM Carpilot on YouTube
		float	x = (position.x + 0.1f + offset) * scale,
				y = (position.y + 0.1f + offset) * scale,
				z = (position.z + 0.1f + offset) * scale,
				n = fn(noise(x, y)) + fn(noise(y, z)) + fn(noise(z, x)) +
					fn(noise(x, z)) + fn(noise(z, y)) + fn(noise(y, x));
		n /= 6f;
		return nf(n);
	}
}