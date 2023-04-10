using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

	protected override void Start ( ) {
		base.Start( );

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

	private void Update ( ) {
		UpdateTransform( );

		// Only move the player controlled block group while in this specific board state
		if (board.BoardState != BoardState.PLACING_MINO) {
			return;
		}

		// Fall down based on the fall time of the minos
		if (Time.time - previousFallTime > gameManager.FallTime) {
			CanFall = TryMove(Vector2Int.down);

			if (CanFall) {
				previousFallTime = Time.time;
			} else {
				board.PlaceActiveMino( );
			}
		}
	}
	#endregion

	public void OnMoveHorizontal (InputValue value) {

	}

	public void OnMoveVertical (InputValue value) {

	}

	public void OnRotate (InputValue value) {

	}
}
