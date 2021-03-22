using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldData{
	public string worldName = "none.";
	public int seed;
	
	[System.NonSerialized]
	public Dictionary<Vector3Int, ChunkData> chunks = new Dictionary<Vector3Int, ChunkData> ();
	
	[System.NonSerialized]
	public List<ChunkData> modifiedChunks = new List<ChunkData> ();
	
	public void AddToModifieds(ChunkData chunk){
		if(!modifiedChunks.Contains(chunk))
			modifiedChunks.Add(chunk);
	}
	
	public WorldData(string _worldName, int _seed){
		worldName = _worldName;
		seed = _seed;
	}
	public WorldData(WorldData data){
		worldName = data.worldName;
		seed = data.seed;
	}
	
	public ChunkData RequestChunk(Vector3Int coord, bool create){
		ChunkData c;
		
		lock(World.Instance.ChunkListThreadLock){
			if(chunks.ContainsKey(coord))
				c = chunks[coord];
			else if(create){
				LoadChunk(coord);
				c = chunks[coord];
			}else
				c = null;
		}
		
		return c;
	}
	
	public void LoadChunk(int x, int z, int w){
		LoadChunk(new Vector3Int(x, z, w));
	}
	public void LoadChunk(Vector3Int coord){
		if(chunks.ContainsKey(coord))
			return;
		
		ChunkData chunk = SaveSystem.LoadChunk(worldName, coord);
		if(chunk != null){
			chunks.Add(coord, chunk);
			return;
		}
		
		chunks.Add(coord, new ChunkData(coord));
		chunks[coord].Populate();
	}
	bool IsVoxelInWorld(Vector3 pos){
		return (
			pos.x >= 0 && pos.x < wsiv_x &&
			pos.y >= 0 && pos.y < cHeight &&
			pos.z >= 0 && pos.z < wsiv_z
		);
	}
	bool IsVoxelInWorld(Vector3Int pos){
		return (
			pos.x >= 0 && pos.x < wsiv_x &&
			pos.y >= 0 && pos.y < cHeight &&
			pos.z >= 0 && pos.z < wsiv_z
		);
	}
	bool IsVoxelInWorld(Vector4 pos){
		return (
			pos.x >= 0 && pos.x < wsiv_x &&
			pos.y >= 0 && pos.y < cHeight &&
			pos.z >= 0 && pos.z < wsiv_z &&
			pos.w >= 0 && pos.w < wsiv_w
		);
	}
	bool IsVoxelInWorld(Vector4Int pos){
		return (
			pos.x >= 0 && pos.x < wsiv_x &&
			pos.y >= 0 && pos.y < cHeight &&
			pos.z >= 0 && pos.z < wsiv_z &&
			pos.w >= 0 && pos.w < wsiv_w
		);
	}
	public void SetVoxel(Vector4 pos, byte value){
		if(!IsVoxelInWorld(pos))
			return;
		
		int x = Helper.floor(pos.x / cWidth) * cWidth,
			 z = Helper.floor(pos.z / cLength) * cLength,
			 w = Helper.floor(pos.w / cDepth) * cDepth;
		
		ChunkData chunk = RequestChunk(new Vector3Int(x, z, w), true);
		
		Vector4Int voxel_pos = new Vector4Int(pos.x - x, pos.y, pos.z - z, pos.w - w);
		
		chunk.ModifyVoxel(voxel_pos, value);
	}
	public VoxelState GetVoxel(Vector4 pos){
		if(!IsVoxelInWorld(pos))
			return null;
		
		int x = Helper.floor(pos.x / cWidth) * cWidth,
			 z = Helper.floor(pos.z / cLength) * cLength,
			 w = Helper.floor(pos.w / cDepth) * cDepth;
		
		ChunkData chunk = RequestChunk(new Vector3Int(x, z, w), false);
		
		if(chunk == null) return null;
		
		Vector4Int voxel = new Vector4Int(pos.x - x, pos.y, pos.z - z, pos.w - w);
		
		return chunk.getVoxel(voxel);
		// return chunk?.getVoxel(voxel);
	}
	public VoxelState GetVoxel(Vector3Int pos){
		return GetVoxel(new Vector4(pos.x, pos.y, pos.z, 0));
	}
	private static int wsiv_x = VoxelData.WorldSizeInVoxels_x,
							 wsiv_z = VoxelData.WorldSizeInVoxels_z,
							 wsiv_w = VoxelData.WorldSizeInVoxels_w,
							 cHeight = VoxelData.ChunkHeight,
							 cWidth = VoxelData.ChunkWidth,
							 cLength = VoxelData.ChunkLength,
							 cDepth	= VoxelData.ChunkDepth;
}