using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlledBlockGroup : BlockGroup {
	#region Unity
	protected override void OnValidate ( ) {
		base.OnValidate( );
	}

	protected override void Awake ( ) {
		base.Awake( );
	}
	#endregion

	protected override void UpdateSelf ( ) {
		if (board.BoardState != BoardState.PLACING_MINO) {
			return;
		}

		previousFallTime += Time.deltaTime;
		if (previousFallTime > gameManager.FallTime) {
			TryMove(Vector2Int.down);
		}
	}

	/*public void OnMoveHorizontal (InputValue value) {

	}

	public void OnMoveVertical (InputValue value) {

	}

	public void OnRotate (InputValue value) {

	}*/
}
