using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour{
	public GameObject mainPage, settingsPage;
	
	[Header("Main Menu UI Elements")]
	public TextMeshProUGUI seedField;
	
	[Header("Settings Menu UI Elements")]
	public Slider viewDistanceSlider, mouseSlider;
	public TextMeshProUGUI viewDistanceText, mouseText;
	public Toggle chunkAnimation;
	public TMP_Dropdown clouds;
	
	Settings settings;
	
	private void Awake(){
		if(!File.Exists(Application.dataPath + "/settings.json")){
			Helper.log("No settings file found. Going default.");
			settings = new Settings();
		}else{
			string jsonImport = File.ReadAllText(Application.dataPath + "/settings.json");
			settings = JsonUtility.FromJson<Settings> (jsonImport);
		}
	}
	void writeSettings(){
		string jsonExport = JsonUtility.ToJson(settings);
		File.WriteAllText(Application.dataPath + "/settings.json", jsonExport);
	}
	public void StartGame(){
		VoxelData.seed = (int) Mathf.Abs(seedField.text.GetHashCode()) / VoxelData.WorldSizeInChunks_x;
		SceneManager.LoadScene("Game", LoadSceneMode.Single);
	}
	public void GetValues(){
		settings.viewDistance = (int) viewDistanceSlider.value;
		viewDistanceText.text = "View Distance " + settings.viewDistance;
		settings.mouseSensitivity = mouseSlider.value;
		mouseText.text = "Mouse Sensitivity " + settings.mouseSensitivity.ToString("F1");
		settings.animatedGeneration = chunkAnimation.isOn;
		settings.clouds = (CloudStyle) clouds.value;
	}
	public void EnterSettings(){
		viewDistanceSlider.value = settings.viewDistance;
		viewDistanceText.text = "View Distance " + settings.viewDistance;
		mouseSlider.value = settings.mouseSensitivity;
		mouseText.text = "Mouse Sensitivity " + settings.mouseSensitivity.ToString("F1");
		chunkAnimation.isOn = settings.animatedGeneration;
		clouds.value = (int) settings.clouds;
		
		mainPage.SetActive(false);
		settingsPage.SetActive(true);
	}
	public void LeaveSettings(){
		GetValues();
		writeSettings();
		
		settingsPage.SetActive(false);
		mainPage.SetActive(true);
	}
	public void QuitGame(){
		Application.Quit();
	}
}