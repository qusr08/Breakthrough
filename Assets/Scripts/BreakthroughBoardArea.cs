using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakthroughBoardArea : BoardArea {
	public override void OnDestroyActiveMino ( ) {
		board.BoardState = BoardState.BREAKTHROUGH;
	}

	public override void OnHeightChange ( ) { }

	public override void OnUpdateBlockGroups ( ) {
		// Clear all blocks inside the breakthrough board area
		for (int y = 0; y < Height; y++) {
			for (int x = 0; x < board.Width; x++) {
				board.DamageBlockAt(new Vector2Int(x, y), destroy: true, dropped: true);
			}
		}
	}
}
