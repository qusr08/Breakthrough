using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakthroughBoardArea : BoardArea {
	public override void OnDestroyActiveMino ( ) {
		board.BoardState = BoardState.BREAKTHROUGH;
	}

	public override void OnHeightChange ( ) { }

	public override void OnUpdateBlockGroups ( ) {
		 
	}
}
