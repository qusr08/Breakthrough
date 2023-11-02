using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallBlockGroup : BlockGroup {
	#region Properties

	#endregion

	#region Unity Functions

	#endregion

	/// <summary>
	///		Update this block group. This function is different than the Unity update function because this class and subclasses behave differently
	/// </summary>
	public override void UpdateBlockGroup ( ) {
		// If the board state is not currently updating block groups, then do not process any of the code in this function to move the mino
		if (gameManager.Board.BoardState == BoardState.UPDATING_BLOCKGROUPS) {
			return;
		}

		// If the fall timer has reached the fall time, move this block group downwards
		if (fallTimer >= gameManager.MinMinoFallTime) {
			if (TryMove(Vector2Int.down)) {
				fallTimer -= gameManager.MinMinoFallTime;
			}
		} else {
			// Update the fall timer
			fallTimer += Time.deltaTime;
		}
	}

}
