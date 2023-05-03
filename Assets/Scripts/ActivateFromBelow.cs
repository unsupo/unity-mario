using System;
using UnityEngine;

public interface IActivateFromBelow {
	bool Hit(PlayerController player, Collision2D collision);
}

