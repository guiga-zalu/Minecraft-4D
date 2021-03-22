using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkLoadAnimation : MonoBehaviour{
	public static float	speed = 3f;
	Vector3	shown_position,
			hidden_position;
	float	status_range = 0f,
			wait_timer;
	bool reached = false,
		 waiting = true;
	private void Start(){
		wait_timer = Random.Range(0f, 3f);
		shown_position = transform.position;
		hidden_position = (transform.position = shown_position - (Vector3.up * VoxelData.ChunkHeight));
	}
	private void Update(){
		if(waiting){
			wait_timer -= Time.deltaTime;
			if(wait_timer <= 0) waiting = false;
		}else if(!reached) move_up();
		else Destroy(this);
	}
	private void move_up(){
		status_range += Time.deltaTime * ChunkLoadAnimation.speed;
		if(status_range >= 1f){
			status_range = 1f;
			reached = true;
		}
		/* transform.position = Vector3.Lerp(
			transform.position,
			shown_position,
			Time.deltaTime * ChunkLoadAnimation.speed
		);
		if(shown_position.y - transform.position.y <= 0.05f)
			transform.position = shown_position; */
		
		transform.position = Vector3.Lerp(hidden_position, shown_position, status_range);
	}
}