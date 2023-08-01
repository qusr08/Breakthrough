using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakthroughBoardArea : BoardArea {
	#region Unity Functions
	protected override void OnChildValidate ( ) {
		Color breakthroughColor = themeManager.ActiveTheme.BreakthroughColor;
		areaSpriteRenderer.color = breakthroughColor;
		lineSpriteRenderer.color = new Color(breakthroughColor.r, breakthroughColor.g, breakthroughColor.b, 1f);
	}
	#endregion

	public override void OnDestroyActiveMino ( ) {
		gameManager.BoardPoints += Constants.POINT_BRKTH;
		gameManager.Breakthroughs++;
		board.BoardState = BoardState.BREAKTHROUGH;
	}

	public override void OnHeightChange ( ) { }

	public override void OnMergeBlockGroups ( ) {
		// Clear all blocks inside the breakthrough board area
		for (int y = 0; y < Height; y++) {
			for (int x = 0; x < gameManager.GameSettings.BoardWidth; x++) {
				board.DamageBlockAt(new Vector2Int(x, y), destroy: true, dropped: true);
			}
		}
	}
}
