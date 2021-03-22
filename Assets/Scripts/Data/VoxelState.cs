using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VoxelState{
	public byte blockID;
	
	[System.NonSerialized]
	public ChunkData chunkData;
	
	[System.NonSerialized]
	public VoxelNeighbors neighbors;
	
	[System.NonSerialized]
	public Vector4Int position;
	
	[System.NonSerialized]
	private byte _light;
	
	public byte light {
		get { return _light; }
		set {
			/* if(value != _light){
				_light = value;
				if(_light > 1)
					// chunkData.AddForLightForPropagation(this);
					PropagateLight();
			} */
			if(value != _light){
				byte oldLight = _light, cast = castLight;
				
				_light = value;
				
				if(_light < oldLight){
					List<int> neighborsToDarken = new List<int> ();
					VoxelState neighbor;
					for(int p = 0; p < 8; p++){
						neighbor = neighbors[p];
						if(neighbor == null) continue;
						
						if(neighbor.light <= cast)
							neighborsToDarken.Add(p);
						else
							neighbor.PropagateLight();
					}
					
					foreach(int i in neighborsToDarken)
						neighbors[i].light = 0;;
					
					if(chunkData.chunk != null)
						World.Instance.AddChunkToUpdate(chunkData.chunk);
				}else if(_light > 1)
					PropagateLight();
			}
		}
	}
	/* public byte lightLevel {
		get { return (byte) Mathf.Round(globalLightPercent * (float) VoxelData.lightLevels); }
	} */
	
	public VoxelState(byte id, ChunkData chunk, Vector4Int pos){
		blockID = id;
		position = pos;
		neighbors = new VoxelNeighbors(this);
		chunkData = chunk;
		light = 0;
	}
	public VoxelState(VoxelState voxel, ChunkData chunk){
		blockID = voxel.blockID;
		position = voxel.position;
		neighbors = new VoxelNeighbors(this);
		chunkData = chunk;
		light = 0;
	}
	public Vector4Int globalPosition {
		get { return position + chunkData.globalPosition; }
	}
	public float lightAsFloat {
		get { return VoxelData.unityOfLight * (float) light; }
	}
	public byte castLight {
		get {
			int lightLevel = _light - properties.opacity - 1;
			if(lightLevel < 0) lightLevel = 0;
			return (byte) lightLevel;
		}
	}
	public void PropagateLight(){
		if(light < 2) return;
		
		VoxelState neighbor;
		byte cast = castLight;
		for(int p = 0; p < 8; p++){
			neighbor = neighbors[p];
			if(neighbor == null) continue;
			
			if(neighbor.light < cast)
				neighbor.light = cast;
			
			if(chunkData.chunk != null)
				World.Instance.AddChunkToUpdate(chunkData.chunk);
		}
	}
	public BlockType properties {
		get { return World.Instance.blockTypes[blockID]; }
	}
}

public class VoxelNeighbors {
	public readonly VoxelState parent;
	public VoxelNeighbors(VoxelState _parent){ parent = _parent; }
	
	private VoxelState[] _neighbors = new VoxelState[8];
	//public int Length { get { return _neighbors.Length; } }
	public int Length { get { return 6; } }
	
	public VoxelState this[int index] {
		get {
			if(_neighbors[index] == null){
				_neighbors[index] = World.Instance.worldData.GetVoxel(
					parent.globalPosition + VoxelData.faceChecks4[index]
				);
				ReturnNeighbor(index);
			}
			
			return _neighbors[index];
		}
		set {
			_neighbors[index] = value;
			ReturnNeighbor(index);
		}
	}
	void ReturnNeighbor(int index){
		VoxelState neighbor = _neighbors[index];
		if(neighbor == null) return;
		
		int counter_index = VoxelData.revFaceChecks4[index];
		if(neighbor.neighbors[counter_index] != parent)
			neighbor.neighbors[counter_index] = parent;
	}
}