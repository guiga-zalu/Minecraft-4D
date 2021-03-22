using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour{
	public bool	isGrounded = false,
				isSprinting = false;
	
	private Transform cam;
	public Transform highlightBlock, placeBlock;
	
	[Header("Configurações de Movimento")]
	public float walkSpeed = 3f;
	public float sprintSpeed = 6f;
	public float jumpForce = 4.9035f;
	public float gravity = - 9.807f; // b / s²
	
	[Header("Características do Jogador")]
	public float playerWidth = 0.3f;
	public float playerHeight = 1.8f;
	public float camHeight = 1.62f;
	
	private float	horizontal,
					vertical,
					mouseHorizontal,
					mouseVertical;
	private Vector3 velocity;
	private float verticalMomentum = 0f;
	private bool jumpRequest;
	
	public float	checkIncrement = 0.05f,
					reach = 8f;
	
	int	wsiv_x = VoxelData.WorldSizeInVoxels_x,
		wsiv_z = VoxelData.WorldSizeInVoxels_z,
		wsiv_w = VoxelData.WorldSizeInVoxels_w;
	
	[Header("Bloco Selecionado")]
	public Toolbar toolbar;
	public Text selectedBlockText;
	
	private float pos_w = 0;
	public Vector4 pos {
		get {
			Vector3 p = transform.position;
			return new Vector4(p.x, p.y, p.z, pos_w);
		}
		set {
			pos_w = value.w;
			// Vector3 p = (Vector3) value;
			// transform.position = p;
			transform.position = (Vector3) value;
		}
	}
	private bool is4Dok = VoxelData.IS_4D_OK;
	
	private void Start(){
		cam = GameObject.Find("Camera").transform;
		cam.Rotate(new Vector3(- 90, 0, 0));
		// highlightBlock	= GameObject.Find("HighlightBlock").transform;
		// placeBlock		= GameObject.Find("PlaceHighlightBlock").transform;
		
		World.Instance.inUI = false;
	}
	private void FixedUpdate(){
		CalculateVelocity();
		transform.Translate(velocity, Space.World);
		
		if(inUI) return;
		
		if(jumpRequest) Jump();
		
		float mouseSensitivity = World.Instance.settings.mouseSensitivity;
		transform.Rotate(Vector3.up * mouseHorizontal * mouseSensitivity);
		cam.Rotate(
			Vector3.left * Mathf.Clamp(mouseVertical * mouseSensitivity, - 90, 90)
		);
		
		if(is4Dok){
			if(Input.GetKeyDown(KeyCode.Q))
				move_w(- 1);
			else if(Input.GetKeyDown(KeyCode.E))
				move_w(1);
		}
	}
	private void Update(){
		if(Input.GetKeyDown(KeyCode.R))
			inUI = !inUI;
		
		if(inUI) return;
		GetPlayerInputs();
		placeCursorBlocks();
	}
	bool inUI {
		get { return World.Instance?.inUI ?? false; }
		set { World.Instance.inUI = value; }
	}
	void Jump(){
		verticalMomentum = jumpForce;
		isGrounded = false;
		jumpRequest = false;
	}
	private void CalculateVelocity(){
		float fdt = Time.fixedDeltaTime;
		
		// * Afetar momento vertical com gravidade
		if(verticalMomentum > gravity)
			verticalMomentum += gravity * fdt;
		
		if(inUI) velocity = new Vector3();
		else{
			// * Pegar velocidade
			// *  Se correndo ou andando
			float speed = isSprinting ? sprintSpeed : walkSpeed;
			velocity = (
				(transform.forward * vertical) + (transform.right * horizontal)
			) * speed;
		}
		
		// * Aplicar momento vertical
		velocity += verticalMomentum * Vector3.up;
		
		velocity *= fdt;
		
		float z = velocity.z, x = velocity.x, y = velocity.y;
		if((z > 0 && front) || (z < 0 && back)) velocity.z = 0;
		if((x > 0 && right) || (x < 0 && left)) velocity.x = 0;
		
		if(y < 0) velocity.y = checkDownSpeed(- y);
		else if(y > 0) velocity.y = checkUpSpeed(y);
	}
	private void GetPlayerInputs(){
		if(Input.GetKeyDown(KeyCode.Escape))
			Application.Quit();
		
		horizontal	= Input.GetAxis("Horizontal");
		vertical	= Input.GetAxis("Vertical");
		mouseHorizontal	= Input.GetAxis("Mouse X");
		mouseVertical	= Input.GetAxis("Mouse Y");
		
		if(Input.GetButtonDown("Sprint"))
			isSprinting = true;
		if(Input.GetButtonUp("Sprint"))
			isSprinting = false;
		
		if(isGrounded && Input.GetButtonDown("Jump"))
			jumpRequest = true;
		
		if(highlightBlock.gameObject.activeSelf){
			Vector3Int	destroy = Helper.floorVector3I(highlightBlock.position),
						place = Helper.floorVector3I(placeBlock.position);
			Chunk chunk;
			// * Destrói bloco
			if(Input.GetMouseButtonDown(0))
				World.Instance.GetChunkFromVector3(destroy)?.EditVoxel(destroy, 0);
			// * Coloca bloco
			else if(Input.GetMouseButtonDown(1)){
				if(toolbar.hasAnythingSelected){
					chunk = World.Instance.GetChunkFromVector3(place);
					if(chunk != null){
						chunk.EditVoxel(place, toolbar.selectedBlock);
						toolbar.selectedItemSlot.Take(1);
					}
				}
			}
		}
	}
	private void placeCursorBlocks(){
		// log("placeCursorBlocks");
		float step = checkIncrement;
		Vector3	cam_pos = cam.position,
				cam_dir = cam.forward;
		Vector3Int	lastPos = new Vector3Int(),
					pos;
		bool has = toolbar.hasAnythingSelected;
		
		highlightBlock.gameObject.SetActive(false);
		placeBlock.gameObject.SetActive(false);
		while(step <= reach){
			pos = Helper.floorVector3I( cam_pos + (cam_dir * step) );
			if(World.Instance.CheckForVoxel(pos)){
				highlightBlock.position = (Vector3) pos;
				placeBlock.position = (Vector3) lastPos;
				
				highlightBlock.gameObject.SetActive(true);
				if(has) placeBlock.gameObject.SetActive(true);
				
				return;
			}
			lastPos = pos;
			step += checkIncrement;
		}
	}
	public float checkDownSpeed(float downSpeed){
		float	x = pos.x,
				_y = pos.y,
				y = _y - downSpeed,
				z = pos.z,
				size = playerWidth / 2f;
		// TODO Change boolean to float "solidness", density,
		// TODO  so semisolids can alter players falling speed.
		// TODO  And return downSpeed * (1f - solidness)
		bool _lft = checkVoxel(x - playerWidth, _y, z),
			 _rgt = checkVoxel(x + playerWidth, _y, z),
			 _bck = checkVoxel(x, _y, z - playerWidth),
			 _frt = checkVoxel(x, _y, z + playerWidth);
		isGrounded = (
			(checkVoxel(x - size, y, z - size) && !(_lft || _bck)) ||
			(checkVoxel(x - size, y, z + size) && !(_rgt || _bck)) ||
			(checkVoxel(x + size, y, z + size) && !(_rgt || _frt)) ||
			(checkVoxel(x + size, y, z - size) && !(_lft || _frt))
		);
		float tax = isGrounded || y < 0 ? 0f : 1f;
		return - downSpeed * tax;
	}
	public float checkUpSpeed(float upSpeed){
		float	x = pos.x,
				y = pos.y + upSpeed + playerHeight,
				z = pos.z,
				size = playerWidth / 2f;
		// TODO Change boolean to float "solidness", density,
		// TODO  so semisolids can alter players falling speed.
		// TODO  And return downSpeed * (1f - solidness)
		bool _lft = left, _rgt = right, _bck = back, _frt = front,
			isBlocked = (
			(checkVoxel(x - size, y, z - size) && !(_lft || _bck)) ||
			(checkVoxel(x - size, y, z + size) && !(_rgt || _bck)) ||
			(checkVoxel(x + size, y, z + size) && !(_rgt || _frt)) ||
			(checkVoxel(x + size, y, z - size) && !(_lft || _frt))
		);
		if(isBlocked) verticalMomentum = 0;
		float tax = isBlocked ? 0f : 1f;
		return upSpeed * tax;
	}
	public bool front{
		get {
			float	x = pos.x,
					y = pos.y,
					z = pos.z + playerWidth;
			return checkVoxel(x, y, z) || checkVoxel(x, y + 1f, z);
		}
	}
	public bool back{
		get {
			float	x = pos.x,
					y = pos.y,
					z = pos.z - playerWidth;
			return checkVoxel(x, y, z) || checkVoxel(x, y + 1f, z);
		}
	}
	public bool left{
		get {
			float	x = pos.x - playerWidth,
					y = pos.y,
					z = pos.z;
			return checkVoxel(x, y, z) || checkVoxel(x, y + 1f, z);
		}
	}
	public bool right{
		get {
			float	x = pos.x + playerWidth,
					y = pos.y,
					z = pos.z;
			return checkVoxel(x, y, z) || checkVoxel(x, y + 1f, z);
		}
	}
	public bool w_front{
		get {
			float	x = pos.x,
					y = pos.y,
					z = pos.z,
					w = pos.w + 1f;
			return checkVoxel(x, y, z, w) || checkVoxel(x, y + 1f, z, w);
		}
	}
	public bool w_back{
		get {
			float	x = pos.x,
					y = pos.y,
					z = pos.z,
					w = pos.w - 1f;
			return checkVoxel(x, y, z, w) || checkVoxel(x, y + 1f, z, w);
		}
	}
	bool checkVoxel(float x, float y, float z, float w = 0f){
		if( x < 0 || x >= wsiv_x ||
			z < 0 || z >= wsiv_z ||
			w < 0 || w >= wsiv_w )
			return true;
		if(is4Dok) return World.Instance.CheckForVoxel(new Vector4Int((int) x, (int) y, (int) z, (int) w));
		return World.Instance.CheckForVoxel(new Vector3Int((int) x, (int) y, (int) z));
	}
	void move_w(int amount = 0){
		if(amount < 0){
			amount = - 1;
			if(!w_back) World.Instance.move_w(amount);
		}else if(amount > 0){
			amount = 1;
			if(!w_front) World.Instance.move_w(amount);
		}else return;
	}
	void log(string text){
		Helper.log("Player\t" + text);
	}
}