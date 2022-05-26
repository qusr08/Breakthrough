using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour {
	[SerializeField] private Board board;

	private bool canMove;

	private float prevFallTime;
	private float prevMoveTime;
	private float prevRotateTime;

	private Vector3 moveTo;
	private Vector3 moveVelocity;
	private Vector3 rotateTo;
	private Vector3 rotateVelocity;

	private void Awake ( ) {
		board = FindObjectOfType<Board>( );
	}

	private void Start ( ) {
		if (Random.Range(0f, 1f) < Constants.BLOCK_PERCENT_BOMB) {
			BlockTile blockTile = GetComponentsInChildren<BlockTile>( )[Random.Range(0, transform.childCount)];

			switch (Random.Range(0, 3)) {
				case 0:
					blockTile.SetTileType(TileType.BOMB_DIRECTION);
					break;
				case 1:
					blockTile.SetTileType(TileType.BOMB_SURROUND);
					break;
				case 2:
					blockTile.SetTileType(TileType.BOMB_LINE);
					break;
			}
		}

		canMove = true;
		moveTo = transform.position;
	}

	private void Update ( ) {
		// TODO: Make inputs more expandable and better functioning
		// TODO: Make it so a block is not placed if it is moving/rotating
		//		     Probably have to alter prevTime or something to prevent that from happening

		transform.position = Vector3.SmoothDamp(transform.position, moveTo, ref moveVelocity, Constants.BLOCK_DAMP_SPEED);
		transform.eulerAngles = Utils.SmoothDampEuler(transform.eulerAngles, rotateTo, ref rotateVelocity, Constants.BLOCK_DAMP_SPEED);

		if (!canMove) {
			// Debug.Log($"position: {moveTo} == {transform.position} | rotation: {rotateTo} == {transform.eulerAngles}");
			// Debug.Log($"position {Utils.CompareVectors(moveTo, transform.position)} | rotation: {Utils.CompareAngleVectors(rotateTo, transform.eulerAngles)}");

			if (Utils.CompareVectors(moveTo, transform.position) && Utils.CompareAngleVectors(rotateTo, transform.eulerAngles)) {
				// The position and rotation of the block might be a little off, so make sure it is exact before the block pieces deactivated
				transform.eulerAngles = rotateTo;
				transform.position = moveTo;

				// Add the tiles to the board and spawn another block
				board.AddTilesToBoard(this);
				enabled = false;
				board.SpawnRandomBlock( );
			}

			return;
		}

		float hori = Input.GetAxisRaw("Horizontal");
		float vert = Input.GetAxisRaw("Vertical");

		if (hori == 0) {
			prevMoveTime = 0;
		} else if (Time.time - prevMoveTime > Constants.BLOCK_MOVE_TIME) {
			Move(Vector3.right * hori);
			prevMoveTime = Time.time;
		}

		// TODO: Move block to satisfy a rotation
		//		     This could be used for something like t spins, but also just in general is a good thing to have to make the gameplay experience better

		if (vert == 0) {
			prevRotateTime = 0;
		} else if (vert > 0 && Time.time - prevRotateTime > Constants.BLOCK_ROTATE_TIME) {
			Rotate(Constants.BLOCK_ROTATE_DIRECTION * 90);
			prevRotateTime = Time.time;
		}

		if (Time.time - prevFallTime > (vert < 0 ? Constants.BLOCK_FALL_TIME / 20 : Constants.BLOCK_FALL_TIME)) {
			if (Move(Vector3.down)) {
				prevFallTime = Time.time;
			} else {
				canMove = false;
			}
		}
	}

	public bool Move (Vector3 direction) {
		foreach (Transform child in transform) {
			Vector2Int position = Vector2Int.RoundToInt(Utils.RotatePositionAroundPivot(moveTo + child.localPosition, moveTo, rotateTo.z) + direction);

			if (!board.IsInBounds(position) || !board.IsBoardTileFree(position)) {
				return false;
			}
		}

		moveTo += direction;

		return true;
	}

	public bool Rotate (float degRotation) {
		foreach (Transform child in transform) {
			Vector2Int position = Vector2Int.RoundToInt(Utils.RotatePositionAroundPivot(moveTo + child.localPosition, moveTo, rotateTo.z + degRotation));

			if (!board.IsInBounds(position) || !board.IsBoardTileFree(position)) {
				return false;
			}
		}

		rotateTo += Vector3.forward * degRotation;
		foreach (BlockTile blockTile in GetComponentsInChildren<BlockTile>( )) {
			blockTile.SetTileDirection((TileDirection) (((int) blockTile.Direction - Constants.BLOCK_ROTATE_DIRECTION) % 4));
		}

		return true;
	}
}
