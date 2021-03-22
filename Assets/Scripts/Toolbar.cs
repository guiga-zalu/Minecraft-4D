using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Toolbar : MonoBehaviour{
	public UIItemSlot[] slots;
	
	public Player player;
	
	public RectTransform highlight;
	
	int slotIndex = 0;
	
	private void Start(){
		byte index = 1;
		foreach(UIItemSlot ui_slot in slots){
			ItemStack stack = new ItemStack(index++, Random.Range(2, 65));
			ItemSlot slot = new ItemSlot(ui_slot, stack);
		}
	}
	private void Update(){
		float scroll = Input.GetAxis("Mouse ScrollWheel");
		
		if(scroll != 0){
			if(scroll > 0) slotIndex--;
			else slotIndex++;
			
			int max = slots.Length - 1;
			if(slotIndex > max) slotIndex = 0;
			else if(slotIndex < 0) slotIndex = max;
			// slotIndex = (slotIndex + slots.Length) % slots.Length;
			
			UIItemSlot slot = slots[slotIndex];
			highlight.position = slot.slotIcon.transform.position;
			
			if(hasAnythingSelected){
				string name = World.Instance.blockTypes[ selectedBlock ].blockName;
				player.selectedBlockText.text = name + " block selected";
			}
		}
	}
	public bool hasAnythingSelected {
		get {
			UIItemSlot slot = slots[slotIndex];
			return slot?.HasItem ?? false;
		}
	}
	public byte selectedBlock {
		get { return selectedItemSlot.stack.itemID; }
	}
	public ItemSlot selectedItemSlot {
		get { return slots[slotIndex].itemSlot; }
	}
}

/* [System.Serializable]
public class ItemSlot{
	public byte itemID;
	public Image icon;
} */