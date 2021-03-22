using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

public static class SaveSystem{
	public static void SaveWorld(WorldData world){
		string savePath = World.Instance.appPath + "/saves/" + world.worldName + "/";
		
		if(!Directory.Exists(savePath))
			Directory.CreateDirectory(savePath);
		
		Helper.log("Saving {" + world.worldName + "}");
		
		BinaryFormatter formatter = new BinaryFormatter();
		FileStream stream = new FileStream(savePath + "world." + WORLD_EXT, FileMode.Create);
		
		formatter.Serialize(stream, world);
		
		stream.Close();
		
		Thread thread = new Thread(() => SaveChunks(world));
		thread.Start();
	}
	public static void SaveChunks(WorldData world){
		List<ChunkData> modifiedChunks = new List<ChunkData> (world.modifiedChunks);
		world.modifiedChunks.Clear();
		
		int count = 0;
		foreach(ChunkData chunk in modifiedChunks){
			SaveChunk(chunk, world.worldName); count++;
		}
		if(count != 0) Helper.log(count + " chunks saved.");
		else Helper.log("No chunk saved.");
	}
	public static void SaveChunk(ChunkData chunk, string worldName){
		string savePath = World.Instance.appPath + "/saves/" + worldName + "/chunks/";
		
		if(!Directory.Exists(savePath))
			Directory.CreateDirectory(savePath);
		
		// Helper.log("Saving {" + chunk.name + "}");
		
		BinaryFormatter formatter = new BinaryFormatter();
		FileStream stream = new FileStream(savePath + chunk.name + "." + CHUNK_EXT, FileMode.Create);
		
		formatter.Serialize(stream, chunk);
		
		stream.Close();
	}
	public static WorldData LoadWorld(string worldName, int seed = 0){
		string loadPath = World.Instance.appPath + "/saves/" + worldName + "/";
		
		WorldData world;
		if(File.Exists(loadPath + "world.world")){
			Helper.log("{" + worldName + "} found. Loading...");
			
			BinaryFormatter formatter = new BinaryFormatter();
			FileStream stream = new FileStream(loadPath + "world." + WORLD_EXT, FileMode.Open);
			
			world = formatter.Deserialize(stream) as WorldData;
			stream.Close();
			return new WorldData(world);
		}
		Helper.log("{" + worldName + "} not found.");
		world = new WorldData(worldName, seed);
		SaveWorld(world);
		
		return world;
	}
	public static ChunkData LoadChunk(string worldName, Vector3Int pos){
		string chunkName = "Chunk " + pos.x + "; " + pos.y + "; " + pos.z + "." + CHUNK_EXT;
		
		string loadPath = World.Instance.appPath + "/saves/" + worldName + "/chunks/" + chunkName;
		
		ChunkData chunk;
		if(File.Exists(loadPath)){
			// Helper.log("{" + worldName + "} found. Loading...");
			
			BinaryFormatter formatter = new BinaryFormatter();
			FileStream stream = new FileStream(loadPath, FileMode.Open);
			
			chunk = formatter.Deserialize(stream) as ChunkData;
			stream.Close();
			return new ChunkData(chunk);
		}
		return null;
	}
	private static string WORLD_EXT = "world", CHUNK_EXT = "chunk";
}