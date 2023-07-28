using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HazardBoardArea : BoardArea {
	public override void OnDestroyActiveMino ( ) { }

	public override void OnHeightChange ( ) {
		board.UpdatePercentageCleared( );
		CheckForGameOver( );
	}

	public override void OnMergeBlockGroups ( ) {
		CheckForGameOver( );
	}

	private void CheckForGameOver ( ) {
		// If the state is already in game over, do not try and set it again
		// This can happen if the game over board area changes in height after the block groups are updated
		if (gameManager.GameState == GameState.GAMEOVER) {
			return;
		}

		if (board.GetPercentageClear(0, gameManager.GameSettings.BoardHeight - Height, gameManager.GameSettings.BoardWidth, 1) < 1f) {
			gameManager.GameState = GameState.GAMEOVER;
		}
	}
}
