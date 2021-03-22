using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Lighting{
	public static void RecalculateNaturalLight(ChunkData chunk){
		int x, z, w;
		for(w = 0; w < VoxelData.ChunkDepth; w++){
			for(x = 0; x < VoxelData.ChunkWidth; x++){
				for(z = 0; z < VoxelData.ChunkLength; z++){
					CastNaturalLight(chunk, x, z, w, VoxelData.ChunkHeight - 1);
				}
			}
		}
	}
	public static void CastNaturalLight(ChunkData chunk, Vector4Int pos){
		CastNaturalLight(chunk, pos.x, pos.z, pos.w, pos.y);
	}
	public static void CastNaturalLight(ChunkData chunk, int x, int z, int w, int startY){
		startY = startY > VoxelData.ChunkHeight - 1 ? VoxelData.ChunkHeight - 1 : startY;
		
		bool obstructed = false;
		VoxelState voxel;
		for(int y = startY; y >= 0; y--){
			voxel = chunk.map[x, y, z, w];
			
			if(!obstructed && voxel.properties.opacity > 0)
				obstructed = true;
			
			voxel.light = (byte) (obstructed ? 0 : 15);
		}
	}
}