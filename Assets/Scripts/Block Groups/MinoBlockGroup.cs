using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinoBlockGroup : BlockGroup {
	[SerializeField, Tooltip("Whether or not the height of this block group needs to be updated.")] private bool _isDirty;
	[SerializeField, Tooltip("The height of this block group above the ground. This is used to position the ghost.")] private int _ghostHeight;

	#region Properties
	/// <summary>
	///		Whether or not the height of this block group needs to be updated
	/// </summary>
	public bool IsDirty { get => _isDirty; set => _isDirty = value; }

	/// <summary>
	///		The height of this block group above the ground. This is also the number of blocks that this block group can fall (and will fall in the future)
	/// </summary>
	public int GhostHeight {
		get {
			if (IsDirty) {
				// If the block group movement height is greater than the height of the board, then the block group must have fallen into the breakthrough board area
				// In this case, do not move the block group down any further
				while (_ghostHeight < GameSettingsManager.BoardHeight * 2) {
					// If all of the blocks that are a part of the block group can move downwards, then increment the movement height of the block group
					if (CheckMove(Vector2Int.down)) {
						_ghostHeight++;
						continue;
					}

					// If the block group cannot move downwards, then quit out of the loop
					break;
				}

				IsDirty = false;
			}

			return _ghostHeight;
		}
		set => _ghostHeight = value;
	}
	#endregion

	#region Unity Functions

	#endregion

	/// <summary>
	///		Update this block group. This function is different than the Unity update function because this class and subclasses behave differently
	/// </summary>
	public override void UpdateBlockGroup ( ) {
		// If the board state is not currently updating the mino, then do not process any of the code in this function to move the mino
		if (gameManager.Board.BoardState != BoardState.UPDATING_MINO) {
			return;
		}

		// If the fall timer has reached the fall time, move this block group downwards
		if (fallTimer >= gameManager.MinoFallTime) {
			if (TryMove(Vector2Int.down)) {
				fallTimer -= gameManager.MinoFallTime;
			}
		} else {
			// Update the fall timer
			fallTimer += Time.deltaTime;
		}
	}
}