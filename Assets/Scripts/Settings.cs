using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Settings{
	[Header("Game Data")]
	public byte[] versions = new byte[3] { 0, 0, 1 };
	public string version {
		get{ return versions[0] + "." + versions[1] + "." + versions[2]; }
		set{
			versions[0] = (byte) value[0];
			versions[1] = (byte) value[2];
			versions[2] = (byte) value[4];
		}
	}
	
	[Header("Performance")]
	public int loadDistance = 16;
	public int viewDistance = 8;
	
	[Header("Jogabilidade"), Range(0.1f, 20f)]
	public float mouseSensitivity = 3f;
	
	[Header("Iluminação")]
	[Range(0f, 1f)]
	public float globalLightLevel = 1f;
	public Color day, night;//6FD4E0, 000676
	public Color getBackgroundColor(){
		return Color.Lerp(night, day, globalLightLevel);
	}
	
	[Header("Efeito Visual")]
	public bool animatedGeneration = false;
	public CloudStyle clouds = CloudStyle.Fancy;
}