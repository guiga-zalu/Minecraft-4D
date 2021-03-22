using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIItemSlot : MonoBehaviour{
	public bool isLinked = false;
	public ItemSlot itemSlot;
	public Image slotImage,
				 slotIcon;
	public Text slotAmount;
	
	public bool HasItem {
		get {
			return itemSlot?.HasItem ?? false;
		}
	}
	public void Link(ItemSlot slot){
		itemSlot = slot;
		itemSlot.LinkUIItemSlot(this);
		isLinked = true;
		
		UpdateSlot();
	}
	public void Unlink(){
		itemSlot.UnlinkUIItemSlot();
		itemSlot = null;
		isLinked = false;
		
		UpdateSlot();
	}
	public void UpdateSlot(){
		if(HasItem){
			slotIcon.sprite = World.Instance.blockTypes[ itemSlot.stack.itemID ].icon;
			slotIcon.enabled = true;
			
			int amount = itemSlot.stack.amount;
			slotAmount.text = amount == 1 ? "" : amount.ToString();
			slotAmount.enabled = true;
		}else Clear();
	}
	public void Clear(){
			slotIcon.sprite = null;
			slotIcon.enabled = false;
			
			slotAmount.text = "";
			slotAmount.enabled = false;
	}
	private void OnDestroy(){
		if(isLinked)
			itemSlot?.UnlinkUIItemSlot();
	}
}

public class ItemSlot{
	public ItemStack stack = null;
	// ui item slot = ui
	private UIItemSlot ui = null;
	
	public bool isCreative = false;
	
	public ItemSlot(UIItemSlot _ui){
		_ui.Link(this);
	}
	public ItemSlot(UIItemSlot _ui, ItemStack _stack){
		stack = _stack;
		_ui.Link(this);
	}
	public void LinkUIItemSlot(UIItemSlot _ui){
		ui = _ui;
	}
	public void UnlinkUIItemSlot(){
		ui = null;
	}
	public void EmptySlot(){
		stack = null;
		
		ui?.UpdateSlot();
	}
	public int Take(int amount){
		if(amount >= stack.amount){
			amount = stack.amount;
			EmptySlot();
		}else if(amount < stack.amount){
			stack.amount -= amount;
			ui.UpdateSlot();
		}
		return amount;
	}
	public ItemStack TakeAll(ItemStack _stack){
		if(isCreative) return new ItemStack(stack);
		
		if(WillSwap(_stack)) return Swap(_stack);
		
		// ItemStack handOver = stack; EmptySlot();
		ItemStack handOver = new ItemStack(stack);
		EmptySlot();
		return handOver;
	}
	public ItemStack TakeHalf(ItemStack _stack){
		if(isCreative) return new ItemStack(stack.itemID, stack.amount / 2);
		
		if(WillSwap(_stack)) return Swap(_stack);
		
		int initial = stack.amount;
		ItemStack handOver = new ItemStack(stack);
		handOver.amount /= 2;
		stack.amount = initial - handOver.amount;
		
		ui.UpdateSlot();
		return handOver;
	}
	public ItemStack LeaveAll(ItemStack _stack){
		if(isCreative) return null;
		
		if(WillSwap(_stack)) return Swap(_stack);
		
		stack.amount += _stack.amount;
		_stack.amount = 0;
		
		ui.UpdateSlot();
		return _stack;
	}
	public ItemStack LeaveOne(ItemStack _stack){
		if(isCreative){
			if(_stack.amount > 0) _stack.amount -= 1;
			return _stack;
		}
		
		if(WillSwap(_stack)) return Swap(_stack);
		
		if(_stack.amount > 0){
			stack.amount += 1;
			_stack.amount -= 1;
			ui.UpdateSlot();
		}
		
		return _stack;
	}
	public ItemStack Swap(ItemStack _stack){
		ItemStack temp = stack;
		stack = _stack;
		
		ui.UpdateSlot();
		return temp;
	}
	public bool HasItem {
		get {
			return (stack?.amount ?? 0) != 0;
		}
	}
	private bool WillSwap(ItemStack _stack){
		return stack.item_status != _stack.item_status;
	}
}