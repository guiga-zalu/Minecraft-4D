using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AtlasPacker : EditorWindow{
	// Block size in pixels
	int blockSize = 16;
	int atlasSizeInBlocks = 16;
	int atlasSize;
	
	Object[] rawTextures = new Object[256];
	List<Texture2D> sortedTextures = new List<Texture2D> ();
	Texture2D atlas;
	
	[MenuItem("V4/Atlas Packer")]
	public static void ShowWindow(){
		EditorWindow.GetWindow(typeof(AtlasPacker));
	}
	private void OnGUI(){
		atlasSize = blockSize * atlasSizeInBlocks;
		GUILayout.Label("V4 Texture Atlas Packer", EditorStyles.boldLabel);
		
		blockSize = EditorGUILayout.IntField("Block Size", blockSize);
		atlasSizeInBlocks = EditorGUILayout.IntField("Atlas Size in Blocks", atlasSizeInBlocks);
		
		GUILayout.Label(atlas);
		
		if(GUILayout.Button("Load Textures")){
			LoadTextures();
			PackAtlas();
			log("Textures Loaded");
		}
		if(GUILayout.Button("Clear Textures")){
			atlas = new Texture2D(atlasSize, atlasSize);
			log("Textures Cleared");
		}
		if(GUILayout.Button("Save Atlas")){
			byte[] bytes = atlas.EncodeToPNG();
			
			try{
				File.WriteAllBytes(Application.dataPath + "/Textures/BlockAtlas.png", bytes);
			}catch{
				log("Coundn't save atlas to file");
			}
		}
	}
	void LoadTextures(){
		if(sortedTextures.Count > 0)
			sortedTextures.Clear();
		rawTextures = Resources.LoadAll("AtlasPacker", typeof(Texture2D));
		
		int index = 0;
		foreach(Object tex in rawTextures){
			Texture2D t = (Texture2D) tex;
			if(t.width == blockSize && t.height == blockSize){
				sortedTextures.Add(t);
				index++;
			}else
				log(tex.name + " hasn't the correct dimensions");
		}
		
		log(index + " successfully loaded.");
	}
	void PackAtlas(){
		atlas = new Texture2D(atlasSize, atlasSize);
		
		int i = atlasSize * atlasSize;
		Color[] pixels = new Color[i];
		
		// for(i--; i >= 0; i--){}
		int x, y, j,
			 currentBlockX, currentBlockY,
			 index;
			//  currentPixelX, currentPixelY;
		
		for(x = 0; x < atlasSize; x++){
			for(y = 0; y < atlasSize; y++){
				currentBlockX = x / blockSize;
				currentBlockY = y / blockSize;
				
				index = currentBlockY * atlasSizeInBlocks + currentBlockX;
				// index = (y / blockSize) * atlasSizeInBlocks + (x / blockSize);
				
				// currentPixelX = x - (currentBlockX * blockSize);
				// currentPixelY = y - (currentBlockY * blockSize);
				
				j = (atlasSize - y - 1) * atlasSize + x;
				if(index < sortedTextures.Count)
					pixels[j] = sortedTextures[index].GetPixel(x, blockSize - y - 1);
				else
					pixels[j] = new Color(0f, 0f, 0f, 0f);
			}
		}
		atlas.SetPixels(pixels);
		atlas.Apply();
	}
	private void log(string txt = ""){
		Debug.Log("Atlas Packer: " + txt);
	}
}