using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragAndDropHandler : MonoBehaviour{
	[SerializeField] private UIItemSlot cursorSlot = null;
	private ItemSlot cursorItemSlot = null;
	
	[SerializeField] private GraphicRaycaster m_Raycaster = null;
	private PointerEventData m_PointerEventData = null;
	
	[SerializeField] private EventSystem m_EventSystem = null;
	
	private void Start(){
		cursorItemSlot = new ItemSlot(cursorSlot);
	}
	private void Update(){
		if(!World.Instance.inUI) return;
		
		cursorSlot.transform.position = Input.mousePosition;
		
		if(Input.GetMouseButtonDown(0))
			HandleSlotClick(CheckForSlot(), true);
		else if(Input.GetMouseButtonDown(1))
			HandleSlotClick(CheckForSlot(), false);
	}
	private void HandleSlotClick(UIItemSlot clicked, bool leftClk){
		if(clicked == null) return;
		
		ItemSlot clk_slot = clicked.itemSlot;
		ItemStack cursorStack = cursorItemSlot.stack;
		bool cursorHas = cursorSlot.HasItem,
			 clickedHas = clicked.HasItem;
			//  isCreative = clickedHas ? clk_slot.isCreative : false;
		
		if((cursorStack == null || cursorStack.amount == 0) && clickedHas)
			cursorStack = new ItemStack(clk_slot.stack.itemID, 0);
		
		if(cursorHas){
			if(!clickedHas) clk_slot.stack = new ItemStack(cursorStack, true);
			if(leftClk)
				cursorStack = clk_slot.LeaveAll(cursorStack);
			else
				cursorStack = clk_slot.LeaveOne(cursorStack);
		}else{
			if(clickedHas){
				if(leftClk)
					cursorStack = clk_slot.TakeAll(cursorStack);
				else
					cursorStack = clk_slot.TakeHalf(cursorStack);
			} // else return;
		}
		cursorItemSlot.stack = cursorStack;
		cursorSlot.UpdateSlot();
	}
	private UIItemSlot CheckForSlot(){
		m_PointerEventData = new PointerEventData(m_EventSystem);
		m_PointerEventData.position = Input.mousePosition;
		
		List<RaycastResult> results = new List<RaycastResult> ();
		m_Raycaster.Raycast(m_PointerEventData, results);
		
		foreach(RaycastResult result in results){
			if(result.gameObject.tag == "UIItemSlot")
				return result.gameObject.GetComponent<UIItemSlot> ();
		}
		
		return null;
	}
}