using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkData{
	int x, z, w;
	public Vector3Int position {
		get { return new Vector3Int(x, z, w); }
		set {
			x = value.x;
			z = value.y;
			w = value.z;
		}
	}
	public Vector4Int globalPosition {
		get { return new Vector4Int(x, 0, z, w); }
	}
	public string name {
		get { return "Chunk " + x + "; " + z + "; " + w; }
	}
	
	[System.NonSerialized]
	public Chunk chunk;
	
	public ChunkData(Vector3Int pos){ position = pos; }
	public ChunkData(int _x, int _z, int _w){
		x = _x; z = _z; w = _w;
	}
	public ChunkData(ChunkData data){
		position = data.position;
		chunk = data.chunk;
		map = data.map;
		
		int x, y, z, w;
		VoxelState voxel;
		Vector4Int pos;
		for(w = 0; w < cDepth; w++){
			for(x = 0; x < cWidth; x++){
				for(y = 0; y < cHeight; y++){
					for(z = 0; z < cLength; z++){
						pos = new Vector4Int(x, y, z, w);
						
						voxel = setVoxel(pos, new VoxelState(getVoxel(pos), this));
						
						for(int p = 0; p < 8; p++){
							neighbor = pos + VoxelData.faceChecks4[p];
							voxel.neighbors[p] = getVoxel(neighbor) ??
								World.Instance.worldData.GetVoxel(global_pos + neighbor);
						}
					}
				}
			}
		}
		
		Post_Populate();
	}
	
	[HideInInspector]
	public VoxelState[,,,] map = new VoxelState[
		VoxelData.ChunkWidth,
		VoxelData.ChunkHeight,
		VoxelData.ChunkLength,
		VoxelData.ChunkDepth
	];
	public VoxelState getVoxel(int x, int y, int z, int w = 0){
		if(x < 0 || x >= cWidth ||
			y < 0 || y >= cHeight ||
			z < 0 || z >= cLength ||
			w < 0 || w >= cDepth)
			return null;
		
		return map[x, y, z, w];
	}
	public VoxelState getVoxel(Vector4Int pos){
		return getVoxel(pos.x, pos.y, pos.z, pos.w);
	}
	public VoxelState setVoxel(int x, int y, int z, int w, VoxelState voxel){
		if(x < 0 || x >= cWidth
		|| y < 0 || y >= cHeight
		|| z < 0 || z >= cLength
		|| w < 0 || w >= cDepth)
			return null;
		return map[x, y, z, w] = voxel;
	}
	public VoxelState setVoxel(Vector4Int pos, VoxelState voxel){
		return setVoxel(pos.x, pos.y, pos.z, pos.w, voxel);
	}
	public VoxelState setVoxel(Vector4Int pos, byte id){
		if(pos.x < 0 || pos.x >= cWidth
		|| pos.y < 0 || pos.y >= cHeight
		|| pos.z < 0 || pos.z >= cLength
		|| pos.w < 0 || pos.w >= cDepth)
			return null;
		VoxelState voxel = map[pos.x, pos.y, pos.z, pos.w] ?? new VoxelState(id, this, pos);
		voxel.blockID = id;
		return map[pos.x, pos.y, pos.z, pos.w] = voxel;
	}
	
	public void Populate(){
		int x, y, z, w;
		int cWidth	= ChunkData.cWidth,
			 cHeight	= ChunkData.cHeight,
			 cLength	= ChunkData.cLength,
			 cDepth	= ChunkData.cDepth;
		Vector4Int	pos,
						neighbor,
						global_pos = globalPosition;
		VoxelState	voxel;
		
		for(w = 0; w < cDepth; w++){
			for(x = 0; x < cWidth; x++){
				for(y = 0; y < cHeight; y++){
					for(z = 0; z < cLength; z++){
						pos = new Vector4Int(x, y, z, w);
						
						voxel = setVoxel(pos, World.Instance.GetVoxel(pos + global_pos));
						
						for(int p = 0; p < 8; p++){
							neighbor = pos + VoxelData.faceChecks4[p];
							/* voxel.neighbors[p] = IsVoxelInChunk(neighbor) ? getVoxel(neighbor) :
								World.Instance.worldData.GetVoxel(neighbor); */
							voxel.neighbors[p] = getVoxel(neighbor) ??
								World.Instance.worldData.GetVoxel(global_pos + neighbor)/*  ?? null */;
						}
					}
				}
			}
		}
		
		Post_Populate();
	}
	private void Post_Populate(){
		Lighting.RecalculateNaturalLight(this);
		World.Instance.worldData.AddToModifieds(this);
	}
	public void ModifyVoxel(Vector4Int pos, byte id){
		VoxelState voxel = getVoxel(pos);
		
		if(voxel.blockID == id)
			return;
		
		BlockType new_voxel = World.Instance.blockTypes[id];
		
		// Cache old opacity
		byte oldOpacity = voxel.properties.opacity;
		
		voxel.blockID = id;
		
		if(voxel.properties.opacity != oldOpacity
		&& ( pos.y == ChunkData.cHeight - 1 || getVoxel(pos + Vector4Int.up).light == 15 )	)
			Lighting.CastNaturalLight(this, pos);
		
		World.Instance.worldData.AddToModifieds(this);
		
		if(chunk != null)
			World.Instance.AddChunkToUpdate(chunk);
	}
	public bool IsVoxelInChunk(Vector4 pos){
		return IsVoxelInChunk(new Vector4Int(pos));
	}
	public bool IsVoxelInChunk(Vector3 pos){
		return IsVoxelInChunk(new Vector4Int(pos));
	}
	public bool IsVoxelInChunk(Vector3Int pos){
		return IsVoxelInChunk(new Vector4Int(pos));
	}
	public bool IsVoxelInChunk(Vector4Int pos){
		return !(
			pos.x < 0 || pos.x >= cWidth ||
			pos.y < 0 || pos.y >= cHeight ||
			pos.z < 0 || pos.z >= cLength ||
			pos.w < 0 || pos.w >= cDepth
		);
	}
	private static int cHeight = VoxelData.ChunkHeight,
							 cWidth = VoxelData.ChunkWidth,
							 cLength = VoxelData.ChunkLength,
							 cDepth	= VoxelData.ChunkDepth;
}