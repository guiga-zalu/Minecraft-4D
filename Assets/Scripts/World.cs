using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;

public partial class World : MonoBehaviour{
	public Settings settings;
	
	[Header("World Generation Values")]
	public BiomeAttributes[] biomes;
	
	public Player player;
	public Vector4 spawnPosition;
	public Clouds clouds;
	
	public Material material, transparentMaterial;
	
	public BlockType[] blockTypes;
	
	List<ChunkCoord> activeChunks = new List<ChunkCoord> ();
	public ChunkCoord playerChunkCoord = new ChunkCoord();
	ChunkCoord playerLastChunkCoord = new ChunkCoord();
	
	private bool _inUI = false;
	public GameObject creativeInventoryWindow, cursorSlot, debugScreen;
	
	Thread ChunkUpdateThread = null;
	public object	ChunkUpdateThreadLock = new object(),
						ChunkListThreadLock = new object();
	
	private static World _instance;
	public static World Instance { get{ return _instance; } }
	
	public string appPath;
	
	private void Awake(){
		if(_instance != null && _instance != this)
			Destroy(this.gameObject);
		else _instance = this;
		
		appPath = Application.persistentDataPath;
	}
	private void Start(){
		worldData = SaveSystem.LoadWorld("none");
		
		string jsonImport = File.ReadAllText(Application.dataPath + "/settings.json");
		settings = JsonUtility.FromJson<Settings> (jsonImport);
		
		int seed = VoxelData.seed;
		log("Generating world using the seed " + seed);
		
		Random.InitState(seed);
		
		Shader.SetGlobalFloat("min_ll", VoxelData.minLightLevel);
		Shader.SetGlobalFloat("max_ll", VoxelData.maxLightLevel);
		Shader.SetGlobalFloat("lightFalloff", VoxelData.lightFalloff);
		
		inUI = false;
		
		LoadWorld();
		
		SetGlobalLightValue();
		
		// GenerateWorld();
		
		spawnPosition = (Vector4) WorldCenter;
		spawnPosition.y = cHeight - 50f;
		log("Spawn position:\t" + spawnPosition.ToString());
		player.pos = spawnPosition;
		
		CheckViewDistance();
		
		ChunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
		ChunkUpdateThread.Start();
	}
	public void SetGlobalLightValue(){
		float gll = settings.globalLightLevel;
		Shader.SetGlobalFloat("GlobalLightLevel", gll);
		Camera.main.backgroundColor = settings.getBackgroundColor();
	}
	private void Update(){
		CheckViewDistance();
		
		/* if(chunksToCreate.Count > 0)
			CreateChunk(); */
		
		if(chunksToDraw.Count > 0)
			chunksToDraw.Dequeue().CreateMesh();
		
		if(Input.GetKeyDown(KeyCode.F)) // F3
			debugScreen.SetActive(!debugScreen.activeSelf);
		
		if(Input.GetKeyDown(KeyCode.U))
			SaveSystem.SaveWorld(worldData);
	}
	void ThreadedUpdate(){
		do{
			
			if(!applyingModifications)
				ApplyModifications();
			
			if(chunksToUpdate.Count > 0)
				UpdateChunks();
			
		}while(true);
	}
	private void OnDisable(){
		// if(ChunkUpdateThread != null) ChunkUpdateThread.Abort();
		ChunkUpdateThread?.Abort();
	}
	void CheckViewDistance(){
		playerChunkCoord = GetChunkCoordFromVector4(player.pos);
		if(playerChunkCoord.Equals(playerLastChunkCoord)) return;
		
		// log("CheckViewDistance " + playerChunkCoord.name());
		
		ChunkCoord	coord = playerChunkCoord,
						_coord;
		// pac = previously active chunks
		List<ChunkCoord> pac = new List<ChunkCoord> (activeChunks);
		activeChunks.Clear();
		Chunk chunk;
		
		int vdic = settings.viewDistance,
			 min_x = coord.x - (vdic / 2),
			 max_x = coord.x + (vdic / 2),
			 min_z = coord.z - (vdic / 2),
			 max_z = coord.z + (vdic / 2),
			 min_w = coord.w - (vdic / 2),
			 max_w = coord.w + (vdic / 2),
			 x, z, w;
		
		min_x = min_x < 0 ? 0 : min_x;
		min_z = min_z < 0 ? 0 : min_z;
		min_w = min_w < 0 ? 0 : min_w;
		max_x = max_x >= wsic_x ? 0 : max_x;
		max_z = max_z >= wsic_z ? 0 : max_z;
		max_w = max_w >= wsic_w ? 0 : max_w;
		/* log(
			"\n\t" +
			"[" + min_x + "," + min_z + "," + min_w + "]" +
			"\n\t" +
			"[" + max_x + "," + max_z + "," + max_w + "]"
		); */
		for(w = min_w; w <= max_w; w++){
			for(x = min_x; x < max_x; x++){
				for(z = min_z; z < max_z; z++){
					_coord = new ChunkCoord(x, z, w);
					if(_coord.isInWorld()){
						chunk = getChunk(_coord);
						
						if(chunk == null)
							chunk = setChunk(_coord, new Chunk(_coord));
							// chunksToCreate.Add(_coord);
						// }else if(!chunk.isActive)
						chunk.isActive = true;
						
						activeChunks.Add(_coord);
					}
					for(int i = pac.Count - 1; i >= 0; i--){
						if(pac[i].Equals(_coord))
							pac.RemoveAt(i);
					}
				}
			}
		}
		foreach(ChunkCoord __coord in pac){
			chunk = getChunk(__coord);
			if(chunk != null) chunk.isActive = false;
		}
		
		playerLastChunkCoord = playerChunkCoord;
		clouds.UpdateClouds();
	}
	float noise2d(Vector3 pos, float offset, float scale){
		return Noise.Get2DPerlin(pos, offset, scale);
	}
	float noise2d(Vector3Int pos, float offset, float scale){
		return Noise.Get2DPerlin(pos, offset, scale);
	}
	public bool inUI {
		get { return _inUI; }
		set {
			_inUI = value;
			if(_inUI){
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				creativeInventoryWindow.SetActive(true);
				cursorSlot.SetActive(true);
			}else{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
				creativeInventoryWindow.SetActive(false);
				cursorSlot.SetActive(false);
			}
		}
	}
	public void log(string text){
		Helper.log("world:\t" + text);
	}
	public void move_w(int w){}
}

[System.Serializable]
public class BlockType{
	public string blockName;
	public bool isSolid;
	public bool isComplete = true;
	public bool renderNeighborFaces;
	[Range(0, 15)]
	public byte opacity = 15;
	public Sprite icon;

	[Header("Texture Values")]
	public int	backFaceTexture,
				frontFaceTexture,
				topFaceTexture,
				bottomFaceTexture,
				leftFaceTexture,
				rightFaceTexture;
	
	public int GetTextureID(int faceIndex){
		switch(faceIndex){
			case 5: return rightFaceTexture;
			case 4: return leftFaceTexture;
			case 3: return bottomFaceTexture;
			case 2: return topFaceTexture;
			case 1: return frontFaceTexture;
			case 0: return backFaceTexture;
			default:
				Helper.log("Error in GetTextureID; invalid face index");
				return 0;
		}
	}
}

/* [System.Serializable]
public class LiquidType{
	public string liquidName;
	public bool antigravity = false;
	public byte flow_range = 8;
	public Sprite icon;
	public float density = 1.0f;
	public int	texture_top_still,
					texture_top_flowing,
					texture_side_still,
					texture_side_flowing;
} */

public class VoxelMod{
	public Vector4Int position;
	public byte id;
	
	public VoxelMod(){
		position = new Vector4Int();
		id = 0;
	}
	public VoxelMod(Vector4Int pos, byte _id){
		position = pos;
		id = _id;
	}
	public VoxelMod(Vector3Int pos, byte _id){
		position = new Vector4Int(pos);
		id = _id;
	}
}