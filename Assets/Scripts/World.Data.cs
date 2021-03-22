using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;

public partial class World : MonoBehaviour{
	public BlockType[] blockTypes;
	
	int wsic_x	= VoxelData.WorldSizeInChunks_x,
		 wsic_z	= VoxelData.WorldSizeInChunks_z,
		 wsic_w	= VoxelData.WorldSizeInChunks_w,
		 wsiv_x	= VoxelData.WorldSizeInVoxels_x,
		 wsiv_z	= VoxelData.WorldSizeInVoxels_z,
		 wsiv_w	= VoxelData.WorldSizeInVoxels_w,
		 cHeight	= VoxelData.ChunkHeight,
		 cWidth	= VoxelData.ChunkWidth,
		 cLength	= VoxelData.ChunkLength,
		 cDepth	= VoxelData.ChunkDepth;
	Chunk[,,] chunks = new Chunk[
		VoxelData.WorldSizeInChunks_x,
		VoxelData.WorldSizeInChunks_z,
		VoxelData.WorldSizeInChunks_w
	];
	Vector4Int WorldCenter = VoxelData.WorldCenter;
	
	// List<ChunkCoord> chunksToCreate = new List<ChunkCoord> ();
	private List<Chunk> chunksToUpdate = new List<Chunk> ();
	public Queue<Chunk> chunksToDraw = new Queue<Chunk> ();
	bool applyingModifications = false;
	
	Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>> ();
	
	public WorldData worldData;
	
	public Chunk getChunk(int x, int z, int w = 0){
		if(x >= 0 && x < wsic_x && z >= 0 && z < wsic_z && w >= 0 && w < wsic_w)
			return chunks[x, z, w];
		return null;
	}
	public Chunk getChunk(ChunkCoord coord){
		if(coord.isInWorld())
			return chunks[coord.x, coord.z, coord.w];
		return null;
	}
	public Chunk setChunk(int x, int z, Chunk chunk){
		return setChunk(x, z, 0, chunk);
	}
	public Chunk setChunk(int x, int z, int w, Chunk chunk){
		if(x >= 0 && x < wsic_x && z >= 0 && z < wsic_z && w >= 0 && w < wsic_w)
			return chunks[x, z, w] = chunk;
		return null;
	}
	public Chunk setChunk(ChunkCoord coord, Chunk chunk){
		if(coord.isInWorld())
			return chunks[coord.x, coord.z, coord.w] = chunk;
		return null;
	}
	void LoadWorld(){
		// log("LoadWorld");
		int vdic = settings.loadDistance;
		int center_x = wsic_x / 2,
			 center_z = wsic_z / 2,
			 center_w = wsic_w / 2,
			 min_x = center_x - vdic,
			 max_x = center_x + vdic,
			 min_z = center_z - vdic,
			 max_z = center_z + vdic,
			 min_w = center_w - vdic,
			 max_w = center_w + vdic;
		
		if(min_x < 0) min_x = 0;
		if(min_z < 0) min_z = 0;
		if(min_w < 0) min_w = 0;
		if(max_x >= wsic_x) max_x = wsic_x - 1;
		if(max_z >= wsic_z) max_z = wsic_z - 1;
		if(max_w >= wsic_w) max_w = wsic_w - 1;
		
		for(int w = min_w; w <= max_w; w++){
			for(int x = min_x; x <= max_x; x++){
				for(int z = min_z; z <= max_z; z++){
					worldData.LoadChunk(x, z, w);
				}
			}
		}
	}
	public void AddChunkToUpdate(Chunk chunk){
		AddChunkToUpdate(chunk, false);
	}
	public void AddChunkToUpdate(Chunk chunk, bool inTop){
		lock(ChunkUpdateThreadLock){
			if(!chunksToUpdate.Contains(chunk)){
				if(inTop)
					chunksToUpdate.Insert(0, chunk);
				else
					chunksToUpdate.Add(chunk);
			}
		}
	}
	void UpdateChunks(){
		ChunkCoord coord;
		Chunk chunk;
		
		lock(ChunkUpdateThreadLock){
			chunk = chunksToUpdate[0];
			
			chunk.UpdateChunk();
			coord = chunk.coord;
			if(!activeChunks.Contains(coord))
				activeChunks.Add(coord);
			chunksToUpdate.RemoveAt(0);
		}
	}
	void ApplyModifications(){
		applyingModifications = true;
		// int count = 0;
		Queue<VoxelMod> queue;
		VoxelMod mod;
		
		while(modifications.Count > 0){
			queue = modifications.Dequeue();
			if(queue == null) continue;
			while(queue.Count > 0){
				mod = queue.Dequeue();
				
				worldData.SetVoxel(mod.position, mod.id);
			}
		}
		
		applyingModifications = false;
	}
	ChunkCoord GetChunkCoordFromVector3(Vector3Int pos){
		int x = Helper.floor(pos.x / cWidth),
			 z = Helper.floor(pos.z / cLength);
		if(x < 0 || x >= wsic_x ||
			z < 0 || z >= wsic_z)
			log("Vector " + pos.ToString() + " tried to create ChunkCoord at " + x + "; " + z );
		return new ChunkCoord(x, z);
	}
	ChunkCoord GetChunkCoordFromVector4(Vector4Int pos){
		int x = pos.x / cWidth,
			 z = pos.z / cLength,
			 w = pos.w / cDepth;
		if(x < 0 || x >= wsic_x ||
			z < 0 || z >= wsic_z ||
			w < 0 || w >= wsic_w)
			log("Vector " + pos.ToString() + " tried to create ChunkCoord at " + x + "; " + z + "; " + w);
		return new ChunkCoord(x, z, w);
	}
	ChunkCoord GetChunkCoordFromVector3(Vector3 pos){
		return GetChunkCoordFromVector3(Helper.floorVector3I(pos));
	}
	ChunkCoord GetChunkCoordFromVector4(Vector4 pos){
		return GetChunkCoordFromVector4(Helper.floorVector4I(pos));
	}
	public Chunk GetChunkFromVector3(Vector3Int pos){
		if(pos.y < 0 || pos.y >= cHeight)
			return null;
		
		int x = Helper.floor(pos.x / cWidth),
			 z = Helper.floor(pos.z / cLength);
		return getChunk(x, z);
	}
	public Chunk GetChunkFromVector4(Vector4Int pos){
		if(pos.y < 0 || pos.y >= cHeight)
			return null;
		
		int x = Helper.floor(pos.x / cWidth),
			 z = Helper.floor(pos.z / cLength),
			 w = Helper.floor(pos.w / cDepth);
		return getChunk(x, z, w);
	}
	public bool CheckForVoxel(Vector3Int pos){
		return CheckForVoxel(new Vector4Int(pos));
	}
	public bool CheckForVoxel(Vector4Int pos){
		VoxelState voxel = worldData.GetVoxel(pos);
		return voxel?.properties?.isSolid ?? false;
	}
	public byte GetVoxel(Vector4Int pos){
		return GetVoxel((Vector3Int) pos);
	}
	public byte GetVoxel(Vector3Int pos){
		int y = pos.y, i;
		
		// ! Passos Imutáveis
		// * Se fora do mundo, então ar
		if(!IsVoxelInWorld(pos)) return 0;
		// * Base vertical de bedrock
		if(y == 0) return 2;
		
		// ! Passo de Seleção de Bioma
		int solidGroundHeight = 42,
			 count = 0,
			 strongestWeightIndex = 0;
		float	sumOfHeights = 0f,
				strongestWeight = 0f,
				weight, height;
		BiomeAttributes biome;
		for(i = biomes.Length - 1; i >= 0; i--){
			biome = biomes[i];
			weight = noise2d(pos, biome.offset, biome.scale);
			/* weight *= weight + 0.5f;
			weight = weight < 0f ? 0f : (weight > 1f ? 1f : weight); */
			if(weight > strongestWeight){
				strongestWeight = weight;
				strongestWeightIndex = i;
			}
			height = biome.terrainHeight * noise2d(pos, 0, biome.terrainScale) * weight;
			if(height > 0){
				sumOfHeights += height;
				count++;
			}
		}
		biome = biomes[ strongestWeightIndex ];
		int biome_index = strongestWeightIndex,
			 terrainHeight = solidGroundHeight + Helper.floor(sumOfHeights / (float) count);
		// int biome_index = Helper.floor(Noise.Get2DPerlin(pos, 123445, 0.5f) * (biomes.Length - 1));
		// BiomeAttributes biome = biomes[biome_index];
		
		// ! Passos Básicos de Terreno
		/* int terrainHeight = Helper.floor(
			noise2d(x, z, 0, biome.terrainScale) * biome.terrainHeight
		) + solidGroundHeight; */
		
		byte voxelValue = 0;
		// * Se topo do mundo, grama
		if(y == terrainHeight) voxelValue = biome.surfaceBlockID;
		// * Se perto do topo, terra
		else if(y < terrainHeight && y > terrainHeight - 4) voxelValue = biome.subSurfaceBlockID;
		// * Se acima do topo, ar
		else if(y > terrainHeight) return 0;
		// * Senão, pedra
		else voxelValue = 1;
		
		// ! Terceiro Passo
		float noise;
		if(voxelValue == 1){
			foreach(Lode lode in biome.lodes){
				if(y > lode.minHeight && y < lode.maxHeight){
					noise = Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale);
					if(noise > lode.threshold) voxelValue = lode.blockID;
				}
			}
		}
		
		// ! Quarto Passo
		// * Flora Principal
		if(y == terrainHeight){
			if(biome.placeMajorFlora){
				noise = noise2d(pos, 0, biome.majorFloraZoneScale);
				if(noise > biome.majorFloraZoneThreshold){
					// voxelValue = 2;
					noise = noise2d(pos, 0, biome.majorFloraPlacementScale);
					if(noise > biome.majorFloraPlacementThreshold){
						// voxelValue = 8;
						modifications.Enqueue(
							Structure.GenerateMajorFlora(
								biome_index, pos, biome.minHeight, biome.maxHeight
							)
						);
					}
				}
			}
		}
		
		return voxelValue;
	}
	public bool IsVoxelInWorld(Vector3 pos){
		return (
			pos.x >= 0 && pos.x < wsiv_x &&
			pos.y >= 0 && pos.y < cHeight &&
			pos.z >= 0 && pos.z < wsiv_z
		);
	}
	public bool IsVoxelInWorld(Vector3Int pos){
		return (
			pos.x >= 0 && pos.x < wsiv_x &&
			pos.y >= 0 && pos.y < cHeight &&
			pos.z >= 0 && pos.z < wsiv_z
		);
	}
	public bool IsVoxelInWorld(Vector4 pos){
		return (
			pos.x >= 0 && pos.x < wsiv_x &&
			pos.y >= 0 && pos.y < cHeight &&
			pos.z >= 0 && pos.z < wsiv_z &&
			pos.w >= 0 && pos.w < wsiv_w
		);
	}
	public bool IsVoxelInWorld(Vector4Int pos){
		return (
			pos.x >= 0 && pos.x < wsiv_x &&
			pos.y >= 0 && pos.y < cHeight &&
			pos.z >= 0 && pos.z < wsiv_z &&
			pos.w >= 0 && pos.w < wsiv_w
		);
	}
}