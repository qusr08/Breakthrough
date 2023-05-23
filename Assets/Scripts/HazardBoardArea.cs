using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardBoardArea : BoardArea {
	public override void OnDestroyActiveMino ( ) { }

	public override void OnHeightChange ( ) {
		CheckForGameOver( );
	}

	public override void OnUpdateBlockGroups ( ) {
		CheckForGameOver( );
	}

	private void CheckForGameOver ( ) {
		// If the state is already in game over, do not try and set it again
		// This can happen if the game over board area changes in height after the block groups are updated
		if (board.BoardState == BoardState.GAMEOVER) {
			return;
		}

		if (board.GetPercentageClear(0, Mathf.RoundToInt(board.HazardBoardArea.Height), board.Width, 1) < 1f) {
			board.BoardState = BoardState.GAMEOVER;
		}
	}
}
