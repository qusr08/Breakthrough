using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mino : MonoBehaviour {
	[SerializeField] private Board board;

	private bool canMove;

	private float prevFallTime;
	private float prevMoveTime;
	private float prevRotateTime;
	private float placeTime;

	private Vector3 moveTo;
	private Vector3 moveVelocity;
	private Vector3 rotateTo;
	private Vector3 rotateVelocity;

	private void Awake ( ) {
		board = FindObjectOfType<Board>( );
	}

	private void Start ( ) {
		// Generate a random number between 0 and 1, and if it is less than the percentage of a boom block being on a mino, then place a random one
		if (Random.Range(0f, 1f) < Constants.MINO_PERCENT_BOOM) {
			// Get a random child block of the mino
			Block block = GetComponentsInChildren<Block>( )[Random.Range(0, transform.childCount)];

			// Set that block to be a random type of boom block
			switch (Random.Range(0, 3)) {
				case 0:
					block.BlockType = BlockType.BOOM_DIRECTION;
					break;
				case 1:
					block.BlockType = BlockType.BOOM_SURROUND;
					break;
				case 2:
					block.BlockType = BlockType.BOOM_LINE;
					break;
			}
		}

		canMove = true;
		moveTo = transform.position;
	}

	private void Update ( ) {
		// TODO: Make it so a mino is not placed if it is moving/rotating
		//		     Probably have to alter prevTime or something to prevent that from happening

		// Smoothly transition the position and rotation of the mino
		transform.position = Vector3.SmoothDamp(transform.position, moveTo, ref moveVelocity, Constants.MINO_DAMP_SPEED);
		transform.eulerAngles = Utils.SmoothDampEuler(transform.eulerAngles, rotateTo, ref rotateVelocity, Constants.MINO_DAMP_SPEED);

		// If the mino is placed, wait for the position and rotation of it to be not transitioning anymore to spawn a new mino
		if (!canMove) {
			// Debug.Log($"position: {moveTo} == {transform.position} | rotation: {rotateTo} == {transform.eulerAngles}");
			// Debug.Log($"position {Utils.CompareVectors(moveTo, transform.position)} | rotation: {Utils.CompareAngleVectors(rotateTo, transform.eulerAngles)}");

			if (Utils.CompareVectors(moveTo, transform.position) && Utils.CompareAngleVectors(rotateTo, transform.eulerAngles)) {
				// The position and rotation of the mino might be a little off, so make sure it is exact before the mino is deactivated
				transform.eulerAngles = rotateTo;
				transform.position = moveTo;

				// Add the blocks to the board
				board.AddMinoToBoard(this);
			}

			return;
		}

		// Get the horizontal and vertical inputs
		float hori = Input.GetAxisRaw("Horizontal");
		float vert = Input.GetAxisRaw("Vertical");
		placeTime -= Time.deltaTime;

		// Move the mino left and right
		if (hori == 0) {
			prevMoveTime = 0;
		} else if (Time.time - prevMoveTime > Constants.MINO_MOVE_TIME) {
			if (Move(Vector3.right * hori)) {
				// Make the first horizontal movement of the player be a longer delay in between movements
				// This prevents the player having to quickly tap the left and right buttons to move one board space
				if (prevMoveTime == 0) {
					prevMoveTime = Time.time;
				} else {
					prevMoveTime = Time.time - Constants.MINO_MOVE_TIME_ACCELERATED;
				}
				placeTime = Constants.MINO_PLACE_TIME;
			}
		}

		// Rotate the mino
		if (vert == 0) {
			prevRotateTime = 0;
		} else if (vert > 0 && Time.time - prevRotateTime > Constants.MINO_ROTATE_TIME) {
			if (Rotate(Constants.MINO_ROTATE_DIRECTION * 90)) {
				placeTime = Constants.MINO_PLACE_TIME;
				prevRotateTime = Time.time;
			}
		}

		// Have the mino automatically fall downwards, or fall faster downwards according to player input
		if (Time.time - prevFallTime > (vert < 0 ? Constants.MINO_FALL_TIME_ACCELERATED : Constants.MINO_FALL_TIME)) {
			if (Move(Vector3.down)) {
				prevFallTime = Time.time;

				if (vert < 0) {
					placeTime = Constants.MINO_PLACE_TIME;
				}
			} else if (placeTime <= 0) {
				canMove = false;
			}
		}
	}

	private bool Move (Vector3 direction) {
		// Check to see if any of the blocks that are part of this mino collide with other blocks that are part of the board already
		// If they do, then this mino cannot move in that direction
		foreach (Transform child in transform) {
			Vector3 toPosition = Utils.Vect3Round(Utils.RotatePositionAroundPivot(moveTo + child.localPosition, moveTo, rotateTo.z) + direction);

			if (!board.IsPositionValid(toPosition, transform)) {
				return false;
			}
		}

		moveTo += direction;

		return true;
	}

	private bool Rotate (float degRotation) {
		// TODO: Move mino to satisfy a rotation
		//		     This could be used for something like t spins, but also just in general is a good thing to have to make the gameplay experience better

		// Check to see if any of the blocks that are part of this mino collide with other blocks that are part of the board already
		// If they do, then this mino cannot rotate in that direction
		foreach (Transform child in transform) {
			Vector3 toPosition = Utils.Vect3Round(Utils.RotatePositionAroundPivot(moveTo + child.localPosition, moveTo, rotateTo.z + degRotation));

			if (!board.IsPositionValid(toPosition, transform)) {
				return false;
			}
		}

		rotateTo += Vector3.forward * degRotation;
		// Make sure to alter the direction of each of the blocks so boom blocks still explode in the right direction
		foreach (Block block in GetComponentsInChildren<Block>( )) {
			block.BlockDirection = (BlockDirection) (((int) block.BlockDirection - Constants.MINO_ROTATE_DIRECTION) % 4);
		}

		return true;
	}
}
