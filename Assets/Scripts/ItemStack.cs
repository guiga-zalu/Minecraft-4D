using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStack{
	public byte itemID;
	/* private int _amount;
	public static int max_amount = 64;
	public int amount {
		get { return _amount; }
		set { _amount = Math.Min(value + amount, ItemStack.max_amount); }
	} */
	public int amount;
	
	public ItemStack(byte id, int amo){
		itemID = id;
		amount = amo;
	}
	public ItemStack(ItemStack stack, bool zero = false){
		itemID = stack.itemID;
		amount = zero ? 0 : stack.amount;
	}
	public int item_status {
		get { return itemID; }
	}
}