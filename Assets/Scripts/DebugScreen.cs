using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour{
	Text text;
	
	string baseText = "";
	float	frameRate,
			timer = 0f;
	
		// half world size in voxels
	int hwsiv_x	= VoxelData.WorldSizeInVoxels_x / 2,
		hwsiv_z	= VoxelData.WorldSizeInVoxels_z / 2,
		hwsiv_w	= VoxelData.WorldSizeInVoxels_w / 2,
		// half world size in chunks
		hwsic_x	= VoxelData.WorldSizeInChunks_x / 2,
		hwsic_z	= VoxelData.WorldSizeInChunks_z / 2,
		hwsic_w	= VoxelData.WorldSizeInChunks_w / 2,
		cWidth	= VoxelData.ChunkWidth,
		cLength	= VoxelData.ChunkLength,
		cDepth	= VoxelData.ChunkDepth;
	
	void Start(){
		text = GetComponent<Text> ();
	}
	void Update(){
		Vector4 pos = World.Instance.player.pos;
		float	x = pos.x - hwsiv_x,
				y = pos.y,
				z = pos.z - hwsiv_z,
				w = pos.w;
		ChunkCoord p_coord = World.Instance.playerChunkCoord;
		int cx = p_coord.x - hwsic_x,
			 cz = p_coord.z - hwsic_z,
			 cw = p_coord.w - hwsic_w;
		
		string debugText = "Minecraft 4D\t(" + frameRate + "fps)\n";
		debugText += "XYZ coords:  " +
			Helper.round(x, 2) + "; " +
			Helper.round(y, 2) + "; " +
			Helper.round(z, 2) + "\n";
		debugText += "Chunk coords:" + cx + "; " + cz + "\n";
		
		x %= cWidth; z %= cLength; w %= cDepth;
		
		debugText += "In-Chunk XYZ:" +
			Helper.round(x, 2) + "; " +
			Helper.round(y, 2) + "; " +
			Helper.round(z, 2) + "\n";
		
		text.text = debugText + baseText;
		
		if(timer > 1f){
			frameRate = (int) (1f / Time.unscaledDeltaTime);
			timer = 0f;
		}else
			timer += Time.deltaTime;
	}
	public void write(string _text){
		baseText += _text;
	}
}