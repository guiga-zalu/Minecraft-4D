using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clouds : MonoBehaviour{
	public int	cloudHeight = 100,
					cloudDepth = 4;
	
	[SerializeField]
	private Texture2D cloudPattern = null;
	[SerializeField]
	private Material cloudMaterial = null;
	
	bool[,] cloudData;
	
	int cloudTexWidth,
		 cloudTileSize;
	Vector3Int offset;
	
	Dictionary<Vector2Int, GameObject> clouds = new Dictionary<Vector2Int, GameObject> ();
	
	private CloudStyle style {
		get { return World.Instance?.settings.clouds ?? CloudStyle.Off; }
	}
	private void Start(){
		cloudTexWidth	= cloudPattern.width;
		cloudTileSize	= VoxelData.ChunkWidth;
		offset = new Vector3Int(- cloudTexWidth / 2, 0, - cloudTexWidth / 2);
		
		Vector3 pos = (Vector3) VoxelData.WorldCenter;
		pos.y = cloudHeight;
		transform.position = pos;
		
		LoadCloudData();
		CreateClouds();
	}
	private void LoadCloudData(){
		cloudData = new bool[cloudTexWidth, cloudTexWidth];
		Color[] cloudTex = cloudPattern.GetPixels();
		
		for(int x = 0; x < cloudTexWidth; x++){
			for(int z = 0; z < cloudTexWidth; z++){
				cloudData[x, z] = cloudTex[cloudTexWidth * z + x].a > 0;
			}
		}
	}
	private void CreateClouds(){
		int x, z;
		float half_ctw = cloudTexWidth / 2f;
		Vector3 pos, times = new Vector3(1f, 0f, 1f);
		
		switch(style){
			case CloudStyle.Fancy:
				for(x = 0; x < cloudTexWidth; x += cloudTileSize){
					for(z = 0; z < cloudTexWidth; z += cloudTileSize){
						pos = new Vector3(x - half_ctw, 0, z - half_ctw) + transform.position;
						pos.y = cloudHeight;
						clouds.Add(
							CloudTilePosFromV3(pos),
							CreateCloudTile(
								CreateFancyCloudMesh(x, z),
								pos
							)
						);
					}
				}
				break;
			case CloudStyle.Fast:
				for(x = 0; x < cloudTexWidth; x += cloudTileSize){
					for(z = 0; z < cloudTexWidth; z += cloudTileSize){
						pos = new Vector3(x - half_ctw, 0, z - half_ctw) + transform.position;
						pos.y = cloudHeight;
						clouds.Add(
							CloudTilePosFromV3(pos),
							CreateCloudTile(
								CreateFastCloudMesh(x, z),
								pos
							)
						);
					}
				}
				break;
			case CloudStyle.Off:
			default: break;
		}
	}
	public void UpdateClouds(){
		if(style == CloudStyle.Off) return;
		
		int x, z;
		Vector3 pos;
		Vector2Int cloudPosition;
		for(x = 0; x < cloudTexWidth; x += cloudTileSize){
			for(z = 0; z < cloudTexWidth; z += cloudTileSize){
				pos = new Vector3(x, 0, z) + offset + (Vector3) World.Instance.player.pos;
				pos.x = RoundToCloud(pos.x);
				pos.y = cloudHeight;
				pos.z = RoundToCloud(pos.z);
				cloudPosition = CloudTilePosFromV3(pos);
				clouds[cloudPosition].transform.position = pos;
			}
		}
	}
	private int RoundToCloud(float f){
		return Helper.floor(f / cloudTileSize) * cloudTileSize;
	}
	private Mesh CreateFastCloudMesh(int x, int z){
		List<Vector3>	vertices	= new List<Vector3> (),
							normals	= new List<Vector3> ();
		List<int> triangles = new List<int> ();
		int vertCount = 0;
		Vector3 pos;
		
		int _x, _z, __x, __z, i;
		for(_x = 0; _x < cloudTileSize; _x++){
			for(_z = 0; _z < cloudTileSize; _z++){
				__x = x + _x;
				__z = z + _z;
				
				if(!cloudData[__x, __z]) continue;
				
				pos = new Vector3(_x, 0, _z);
				
				for(i = 0; i < 4; i++){
					vertices.Add(pos + Clouds.sides[i]);
					normals.Add(Vector3.down);
				}
				
				triangles.Add(vertCount + 1);
				triangles.Add(vertCount);
				triangles.Add(vertCount + 2);
				
				triangles.Add(vertCount + 2);
				triangles.Add(vertCount);
				triangles.Add(vertCount + 3);
				
				vertCount += 4;
			}
		}
		
		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.normals = normals.ToArray();
		
		return mesh;
	}
	private Mesh CreateFancyCloudMesh(int x, int z){
		List<Vector3>	vertices	= new List<Vector3> (),
							normals	= new List<Vector3> ();
		List<int> triangles = new List<int> ();
		int vertCount = 0;
		Vector3Int pos, vert1;
		Vector3 vert2;
		
		int _x, _z, __x, __z, p, i;
		for(_x = 0; _x < cloudTileSize; _x++){
			for(_z = 0; _z < cloudTileSize; _z++){
				__x = x + _x;
				__z = z + _z;
				
				if(!cloudData[__x, __z]) continue;
				
				vert1 = new Vector3Int(_x, 0, _z);
				pos = new Vector3Int(__x, 0, __z);
				
				for(p = 0; p < 6; p++){
					if(CheckCloudData(VoxelData.faceChecks[p] + pos)) continue;
					
					for(i = 0; i < 4; i++){
						vert2 = vert1 + VoxelData.voxelVerts[ VoxelData.voxelTris[p, i] ];
						vert2.y *= cloudDepth;
						vertices.Add(vert2);
						normals.Add(VoxelData.faceChecks[p]);
					}
					triangles.Add(  vertCount  );
					triangles.Add(vertCount + 1);
					triangles.Add(vertCount + 2);
					triangles.Add(vertCount + 2);
					triangles.Add(vertCount + 1);
					triangles.Add(vertCount + 3);
					
					vertCount += 4;
				}
			}
		}
		
		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.normals = normals.ToArray();
		
		return mesh;
	}
	private bool CheckCloudData(Vector3Int point){
		int x = point.x,
			 y = point.y,
			 z = point.z;
		if(y != 0) return false;
		
		// if(x < 0 || z < 0){
			// return CheckCloudData(point + new Vector3Int(cloudTexWidth, 0, cloudTexWidth));
			// x += cloudTexWidth;
			// z += cloudTexWidth;
		// }
		
		if(x < 0) x = cloudTexWidth - 1;
		if(z < 0) z = cloudTexWidth - 1;
		
		// if(x > cloudTexWidth || z > cloudTexWidth){
			// return CheckCloudData(new Vector3Int(x % cloudTexWidth, 0, z % cloudTexWidth));
			// x %= cloudTexWidth;
			// z %= cloudTexWidth;
		// }
		
		if(x > cloudTexWidth - 1) x = 0;
		if(z > cloudTexWidth - 1) z = 0;
		
		return cloudData[x, z];
	}
	private GameObject CreateCloudTile(Mesh mesh, Vector3 pos){
		GameObject cloudTile = new GameObject();
		cloudTile.transform.position = pos;
		cloudTile.transform.parent = transform;
		cloudTile.name = "Cloud " + pos.ToString();
		
		MeshFilter mf = cloudTile.AddComponent<MeshFilter> ();
		MeshRenderer mr = cloudTile.AddComponent<MeshRenderer> ();
		
		mr.material = cloudMaterial;
		mf.mesh = mesh;
		
		return cloudTile;
	}
	private Vector2Int CloudTilePosFromV3(Vector3 pos){
		return new Vector2Int(
			CloudTileCoordFromFloat(pos.x),
			CloudTileCoordFromFloat(pos.z)
		);
	}
	private int CloudTileCoordFromFloat(float value){
		float a = value / (float) cloudTexWidth;
		a -= Helper.floor(a);
		
		return Helper.floor(a * (float) cloudTexWidth);
	}
	private static Vector3Int[] sides = new Vector3Int[4] {
		new Vector3Int(0, 0, 0),
		new Vector3Int(0, 0, 1),
		new Vector3Int(1, 0, 1),
		new Vector3Int(1, 0, 0)
	};
}

public enum CloudStyle {
	Off,
	Fast,
	Fancy
}