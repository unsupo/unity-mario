using UnityEngine;
using System.Collections;

public class BreakableBlock : AbstractItemBlock {
	public override bool Hit(PlayerController player, Collision2D collision) {
		// if it has an item then treat it like a question block
		if(base.Hit(player, collision))
			return true;
		// else if big 
		Debug.Log("hit breakable");
		if(player.IsBig()) {
			DestroyImmediate(this);
			return true;
		}
		return false;
	}
}

