using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk{
	public ChunkCoord coord;
	
	GameObject chunkObject;
	MeshRenderer meshRenderer;
	MeshFilter meshFilter;
	
	int vertexIndex = 0;
	List<Vector3>	vertices	= new List<Vector3> (),
						normals	= new List<Vector3> ();
	List<Vector2> uvs		= new List<Vector2> ();
	List<int> triangles	= new List<int> (),
				 transparentTriangles = new List<int> ();
	Material[] materials	= new Material[2];
	List<Color> colors	= new List<Color> ();
	
	public Vector3 position;
	
	int cHeight	= VoxelData.ChunkHeight,
		 cWidth	= VoxelData.ChunkWidth,
		 cLength	= VoxelData.ChunkLength,
		 cDepth	= VoxelData.ChunkDepth;
	
	private bool _isActive;
	
	ChunkData chunkData;
	
	public Chunk(ChunkCoord _coord){
		coord = _coord;
		
		chunkData = World.Instance.worldData.RequestChunk(
			new Vector3Int(
				coord.x * VoxelData.ChunkWidth,
				coord.z * VoxelData.ChunkLength,
				coord.w * VoxelData.ChunkDepth
			), true
		);
		chunkData.chunk = this;
		
		chunkObject	 = new GameObject();
		meshFilter	 = chunkObject.AddComponent<MeshFilter> ();
		meshRenderer = chunkObject.AddComponent<MeshRenderer> ();
		
		materials[0] = World.Instance.material;
		materials[1] = World.Instance.transparentMaterial;
		meshRenderer.materials = materials;
		
		chunkObject.transform.SetParent(World.Instance.transform);
		position = chunkObject.transform.position = new Vector3(coord.x * cWidth, 0f, coord.z * cLength);
		chunkObject.name = "Chunk " + coord.x + ", " + coord.z;
		
		World.Instance.AddChunkToUpdate(this);
		
		if(World.Instance.settings.animatedGeneration)
			chunkObject.AddComponent<ChunkLoadAnimation> ();
	}
	public void UpdateChunk(){
		ClearMeshData();
		
		int x, y, z, w;
		for(w = 0; w < cDepth; w++){
			for(x = 0; x < cWidth; x++){
				for(y = 0; y < cHeight; y++){
					for(z = 0; z < cLength; z++){
						if(getVoxel(x, y, z).properties.isSolid)
							UpdateMeshData(new Vector3Int(x, y, z));
					}
				}
			}
		}
		
		World.Instance.chunksToDraw.Enqueue(this);
	}
	
	void ClearMeshData(bool tolog = false){
		if(tolog) log("ClearMeshData");
		
		vertexIndex = 0;
		vertices.Clear();
		normals.Clear();
		triangles.Clear();
		transparentTriangles.Clear();
		uvs.Clear();
		colors.Clear();
	}
	public bool isActive {
		get { return _isActive; }
		set {
			_isActive = value;
			chunkObject?.SetActive(value);
		}
	}
	public void EditVoxel(Vector3Int pos, byte newID){
		// Coordenada do bloco
		Vector4Int pos1 = Helper.floorVector4I(pos);
		// Coordenada do bloco na chunk
		pos1 -= Helper.floorVector4I(position);
		
		chunkData.ModifyVoxel(pos1, newID);

		UpdateSurroundingVoxels(pos1);
	}
	void UpdateSurroundingVoxels(Vector4Int voxel){
		Vector4Int	current;
		Chunk chunk;
		
		for(int p = 0; p < 8; p++){
			current = voxel + VoxelData.faceChecks4[p];
			
			if(!World.Instance.IsVoxelInWorld(current)
			|| chunkData.IsVoxelInChunk(current)) continue;
			
			chunk = World.Instance.GetChunkFromVector4(
				current + Helper.floorVector4I(position)
			);
			if(chunk != null) World.Instance.AddChunkToUpdate(chunk, true);
			else{
				log("UpdateSurroundingVoxels: chunk null!\n" + current.ToString());
			}
		}
	}
	public VoxelState GetVoxelFromGlobalVector4(Vector4Int pos){
		// World.Instance.log("" + pos.x + "\t" + pos.y + "\t" + pos.z);
		pos -= Helper.floorVector4I(position);
		
		return getVoxel(pos);
	}
	void UpdateMeshData(Vector3Int pos, bool tolog = false){
		if(tolog) log("UpdateMeshData\t" + pos.ToString());
		
		VoxelState neighbor, voxel = getVoxel(pos);
		BlockType blockType = voxel.properties;
		bool renderNeighborFaces = blockType.renderNeighborFaces;
		float darkLevel;
		Vector3 posf = (Vector3) pos;
		Color color;
		
		/* if(voxel == null){
			log("Voxelless!\n" + pos.ToString());
			return;
		}else if(voxel.neighbors == null){
			log("Neighborless!\n" + pos.ToString());
			return;
		} */
		
		for(int p = 0, i; p < 6; p++){
			neighbor = voxel?.neighbors?[p];
			if(neighbor != null
			&& neighbor.properties.renderNeighborFaces){
				darkLevel = neighbor.lightAsFloat;
				color = new Color(0, 0, 0, darkLevel);
				
				for(i = 0; i < 4; i++){
					vertices.Add(posf + VoxelData.voxelVerts[ VoxelData.voxelTris[p, i] ]);
					normals.Add(VoxelData.faceChecks[p]);
					colors.Add(color);
				}
				
				AddTexture( blockType.GetTextureID(p) );
				
				if(renderNeighborFaces){
					transparentTriangles.Add(vertexIndex + 0);
					transparentTriangles.Add(vertexIndex + 1);
					transparentTriangles.Add(vertexIndex + 2);
					transparentTriangles.Add(vertexIndex + 2);
					transparentTriangles.Add(vertexIndex + 1);
					transparentTriangles.Add(vertexIndex + 3);
				}else{
					triangles.Add(vertexIndex + 0);
					triangles.Add(vertexIndex + 1);
					triangles.Add(vertexIndex + 2);
					triangles.Add(vertexIndex + 2);
					triangles.Add(vertexIndex + 1);
					triangles.Add(vertexIndex + 3);
				}
				
				vertexIndex += 4;
			}
		}
	}
	public void CreateMesh(bool tolog = false){
		if(tolog) log("CreateMesh");
		
		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		
		mesh.subMeshCount = 2;
		mesh.SetTriangles(triangles.ToArray(), 0);
		mesh.SetTriangles(transparentTriangles.ToArray(), 1);
		// mesh.triangles = triangles.ToArray();
		
		mesh.uv = uvs.ToArray();
		mesh.colors = colors.ToArray();
		
		// mesh.RecalculateNormals();
		mesh.normals = normals.ToArray();
		meshFilter.mesh = mesh;
	}
	void AddTexture(int textureID){
		float nbts = VoxelData.NormalizedBlockTextureSize;
		int tasib = VoxelData.TextureAtlasSizeInBlocks;
		
		float	y = textureID / tasib,
				x = textureID - (y * tasib);
		
		x *= nbts;
		y *= nbts;
		
		y = 1f - y - nbts;
		
		uvs.Add(new Vector2(x, y));
		uvs.Add(new Vector2(x, y + nbts));
		uvs.Add(new Vector2(x + nbts, y));
		uvs.Add(new Vector2(x + nbts, y + nbts));
	}
	public void log(string text){
		Helper.log(name() + ":\t" + text);
	}
	public string name(){
		return "Chunk " + coord.name();
	}
	public VoxelState getVoxel(int x, int y, int z, int w = 0){
		return chunkData.getVoxel(x, y, z, w);
	}
	public VoxelState getVoxel(Vector3Int pos){
		return getVoxel(new Vector4Int(pos));
	}
	public VoxelState getVoxel(Vector4Int pos){
		return chunkData.getVoxel(pos);
	}
	public VoxelState setVoxel(Vector3Int pos, byte id){
		return setVoxel(new Vector4Int(pos), id);
	}
	public VoxelState setVoxel(Vector4Int pos, byte id){
		return setVoxel(pos, id);
	}
	public VoxelState setVoxel(int x, int y, int z, int w, VoxelState voxel){
		return chunkData.setVoxel(x, y, z, w, voxel);
	}
	public VoxelState setVoxel(Vector4Int pos, VoxelState voxel){
		return chunkData.setVoxel(pos, voxel);
	}
	public VoxelState setVoxel(Vector3Int pos, VoxelState voxel){
		return setVoxel(new Vector4Int(pos), voxel);
	}
}

public class ChunkCoord{
	public int x, z, w = 0;
	
	public ChunkCoord(){
		x = 0; z = 0;
	}
	public ChunkCoord(int _x, int _z, int _w = 0){
		x = _x; z = _z; w = _w;
	}
	public ChunkCoord(Vector3 pos){
		x = (int) pos.x / VoxelData.ChunkWidth;
		z = (int) pos.z / VoxelData.ChunkLength;
	}
	public ChunkCoord(Vector3Int pos){
		x = pos.x / VoxelData.ChunkWidth;
		z = pos.z / VoxelData.ChunkLength;
	}
	public ChunkCoord(Vector4 pos){
		x = (int) pos.x / VoxelData.ChunkWidth;
		z = (int) pos.z / VoxelData.ChunkLength;
		w = (int) pos.w / VoxelData.ChunkDepth;
	}
	public ChunkCoord(Vector4Int pos){
		x = pos.x / VoxelData.ChunkWidth;
		z = pos.z / VoxelData.ChunkLength;
		w = pos.w / VoxelData.ChunkDepth;
	}
	public bool Equals(ChunkCoord other){
		if(other == null) return false;
		return (other.x == x && other.z == z && other.w == w);
	}
	public string name(){
		return "[" + x + "; " + z + "; " + w + "]";
	}
	public bool isInWorld(){
		return (
			x >= 0 && x < VoxelData.WorldSizeInChunks_x &&
			z >= 0 && z < VoxelData.WorldSizeInChunks_z &&
			w >= 0 && w < VoxelData.WorldSizeInChunks_w
		);
	}
	public static explicit operator Vector4(ChunkCoord c){
		return new Vector4(c.x, 0, c.z, c.w);
	}
	public static explicit operator Vector3(ChunkCoord c){
		return new Vector3(c.x, 0, c.z);
	}
}