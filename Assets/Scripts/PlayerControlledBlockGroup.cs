using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControlledBlockGroup : BlockGroup {
	[Header("Properties - Player Controlled Block Group")]
	[SerializeField] private bool _hasBoomBlock = false;
	[SerializeField] private bool _hasLanded = false;

	private int horizontalValue = 0;
	private int verticalValue = 0;
	private int rotateValue = 0;

	private float placeTimer;

	#region Properties
	public bool HasBoomBlock {
		get => _hasBoomBlock; private set => _hasBoomBlock = value;
	}
	public bool HasLanded {
		get => _hasLanded; private set => _hasLanded = value;
	}
	#endregion

	#region Unity

	protected override void Start ( ) {
		base.Start( );

		if (Random.Range(0f, 1f) < gameManager.BoomBlockSpawnChance) {
			Block block = GetBlock(Random.Range(0, Count));

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

		// If the mino was generated with a boom block, then the boom block drought is over
		// If the mino was not generated with a boom block, then the boom block drought continues and increases the chance of a boom block to spawn for the next mino
		if (HasBoomBlock) {
			gameManager.BoomBlockDrought = 0;
		} else {
			gameManager.BoomBlockDrought++;
		}
	}

	private void Update ( ) {
		UpdateTransform( );

		// Only move the player controlled block group while in this specific board state
		if (board.BoardState != BoardState.PLACING_MINO) {
			return;
		}

		// If the mino has landed, then do not update any more movement
		if (HasLanded) {
			return;
		}

		placeTimer -= Time.deltaTime;

		// Fall down based on the fall time of the minos
		if (Time.time - previousFallTime > (verticalValue > 0 ? gameManager.FallTimeAccelerated : gameManager.FallTime)) {
			CanFall = TryMove(Vector2Int.down);

			if (CanFall) {
				previousFallTime = Time.time;

				if (verticalValue > 0) {
					gameManager.BoardPoints += gameManager.PointsPerFastDrop;
					placeTimer = gameManager.PlaceTime;
				}
			} else if (isDoneTweening && placeTimer <= 0) {
				board.PlaceActiveMino( );
				HasLanded = true;
			}
		}

		// Try to move the mino horizontally
		// If the mino is not moving horizontally, then reset the last time it moved
		if (horizontalValue == 0) {
			previousMoveTime = 0;
		} else if (Time.time - previousMoveTime > gameManager.MoveTime) {
			// If the mino can move horizontally, reset the place timer and move time
			if (TryMove(new Vector2Int(horizontalValue, 0))) {
				// If the player holds down the button, the mino should move slightly faster
				if (previousMoveTime == 0) {
					previousMoveTime = Time.time;
				} else {
					previousMoveTime = Time.time - gameManager.MoveTimeAccelerated;
				}

				placeTimer = gameManager.PlaceTime;
			}
		}

		// Try and rotate the mino
		if (rotateValue == 0) {
			previousRotateTime = 0;
		} else if (Time.time - previousRotateTime > gameManager.RotateTime) {
			// If the mino was successfully rotated, then reset the place timer and rotate time
			if (TryRotate(gameManager.RotateDirection * 90f)) {
				// If the player holds down the button, the mino should rotate slightly faster
				if (previousRotateTime == 0) {
					previousRotateTime = Time.time;
				} else {
					previousRotateTime = Time.time - gameManager.RotateTimeAccelerated;
				}

				placeTimer = gameManager.PlaceTime;
			}
		}
	}
	#endregion

	public void OnMoveHorizontal (InputValue value) {
		horizontalValue = Mathf.RoundToInt(value.Get<float>( ));
	}

	public void OnMoveVertical (InputValue value) {
		verticalValue = Mathf.RoundToInt(value.Get<float>( ));
	}

	public void OnRotate (InputValue value) {
		rotateValue = Mathf.RoundToInt(value.Get<float>( ));
	}
}
