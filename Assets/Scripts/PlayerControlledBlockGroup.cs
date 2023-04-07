using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlledBlockGroup : BlockGroup {
	[Header("Properties - Player Controlled Block Group")]
	[SerializeField] private bool _hasBoomBlock = false;

	#region Properties
	public bool HasBoomBlock { get => _hasBoomBlock; private set => _hasBoomBlock = value; }
	#endregion

	#region Unity
	protected override void OnValidate ( ) {
		base.OnValidate( );
	}

	protected override void Awake ( ) {
		base.Awake( );
	}

	protected void Start ( ) {
		if (Random.Range(0f, 1f) < gameManager.BoomBlockSpawnChance) {
			Block block = this[Random.Range(0, Count)];

			switch (Random.Range(0, 3)) {
				case 0:
					block.BlockType = BlockType.BOOM_LINE;
					break;
				case 1:
					block.BlockType = BlockType.BOOM_AREA;
					break;
				case 2:
					block.BlockType = BlockType.BOOM_PYRA;
					break;
			}

			HasBoomBlock = true;
		}
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
