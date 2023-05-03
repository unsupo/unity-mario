using UnityEngine;
using System.Collections;
using static PlayerController;

public class Mushroom : AbstractItem {
	public override void Touch(PlayerController player) {
		Debug.Log("Touched: " + this);
		player.ChangeForm(Form.big);
	}
}

