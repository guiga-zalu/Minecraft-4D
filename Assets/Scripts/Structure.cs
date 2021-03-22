using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure{
	public static Queue<VoxelMod> GenerateMajorFlora(
		int index, Vector3Int pos, int min, int max
	){
		switch(index){
			case 0: return MakeTree(pos, min, max);
			case 1: return MakeCacti(pos, min, max);
		}
		return new Queue<VoxelMod> ();
	}
	public static Queue<VoxelMod> MakeTree(Vector3Int pos, int minTrunkHeight, int maxTrunkHeight){
		Queue<VoxelMod> queue = new Queue<VoxelMod> ();
		
		float noise = Noise.Get2DPerlin(pos, 250f, 3f);
		// int height = minTrunkHeight + (int) ((maxTrunkHeight - minTrunkHeight) * noise),
		int height = (int) (maxTrunkHeight * noise),
			 i, x, z;
		
		if(height < minTrunkHeight) height = minTrunkHeight;
		
		for(i = 1; i < height; i++)
			queue.Enqueue(
				new VoxelMod(pos + new Vector3Int(0, i, 0), 6)
			);
		
		for(x = -3; x < 4; x++)
			for(z = -3; z < 4; z++)
				// for(i = height - 3; i <= height; i++)
				for(i = height + 7 - 1; i >= height; i--)
					queue.Enqueue(
						new VoxelMod(pos + new Vector3Int(x, i, z), 11)
					);
		
		return queue;
	}
	public static Queue<VoxelMod> MakeCacti(Vector3Int pos, int minHeight, int maxHeight){
		Queue<VoxelMod> queue = new Queue<VoxelMod> ();
		
		float noise = Noise.Get2DPerlin(pos, 2358f, 2f);
		int height = minHeight + (int) ((maxHeight - minHeight) * noise);
		// int height = (int) (maxHeight * noise);
		
		// if(height < minHeight) height = minHeight;
		
		for(int i = 1; i <= height; i++)
			queue.Enqueue(
				new VoxelMod(pos + new Vector3Int(0, i, 0), 12)
			);
		
		return queue;
	}
}