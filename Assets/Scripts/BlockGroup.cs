using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGroup : MonoBehaviour {
	[SerializeField] private Board board;
	[Space]
	[SerializeField] public bool IsModified;
	[SerializeField] public bool CanMove;
	[SerializeField] public bool CanFallBelow;

	private float prevFallTime;

	private Vector3 moveTo;
	private Vector3 moveVelocity;

	public int Size {
		get {
			return transform.childCount;
		}
	}

	public Block this[int index] {
		get {
			return transform.GetChild(index).GetComponent<Block>( );
		}
	}

	private void Awake ( ) {
		board = FindObjectOfType<Board>( );
	}

	private void Start ( ) {
		CanMove = true;
		CanFallBelow = false;
		moveTo = transform.position;
	}

	private void Update ( ) {
		transform.position = Vector3.SmoothDamp(transform.position, moveTo, ref moveVelocity, Constants.MINO_DAMP_SPEED);

		if (board.BoardUpdateState != BoardUpdateState.UPDATING_BLOCK_GROUPS) {
			return;
		}

		// If the size of this block group is 0, then destroy it
		if (Size == 0) {
			DestroyImmediate(gameObject);
		}

		// Move the board group down if it is able to
		if (Time.time - prevFallTime > Constants.MINO_FALL_TIME_ACCELERATED) {
			CanMove = Move(Vector3.down);

			if (CanMove) {
				prevFallTime = Time.time;
			}
		}
	}

	private bool Move (Vector3 direction) {
		bool isValidMove = true;

		// Check to see if any of the blocks that are part of this mino collide with other blocks that are part of the board already
		// If they do, then this mino cannot move in that direction
		for (int i = Size - 1; i >= 0; i--) {
			Vector3 currPosition = Utils.Vect3Round(moveTo + this[i].transform.localPosition);
			Vector3 toPosition = currPosition + direction;

			// Check to see if a block that is a part of this block group can't move down
			if (isValidMove && !board.IsPositionValid(toPosition, transform)) {
				// This means that the block group can't move down
				isValidMove = false;
			}

			// If this block group can't fall below the bottom of the board but is trying to
			if (!CanFallBelow && toPosition.y < Constants.BOARD_BOTTOM_PADDING) {
				// The block group can't move down then
				isValidMove = false;
			}

			// If the current position of the block is below the bottom of the board
			if (currPosition.y < Constants.BOARD_BOTTOM_PADDING) {
				// Remove the block from the board
				board.RemoveBlockFromBoard(this[i], true);
			}
		}

		// If all of the blocks can move down, then adjust the position
		if (isValidMove) {
			moveTo += direction;

			// If this block group makes a successful move that is not below the bottom of the board, then it can fall below the board
			if (!CanFallBelow && moveTo.y >= Constants.BOARD_BOTTOM_PADDING) {
				CanFallBelow = true;
			}
		}

		return isValidMove;
	}

	public void MergeToBlockGroup (BlockGroup blockGroup) {
		// TODO: Make it so the smaller group merges with the bigger group to save processing time

		while (Size > 0) {
			this[0].transform.SetParent(blockGroup.transform, true);
		}

		blockGroup.CanFallBelow = false;
		DestroyImmediate(gameObject);
	}

	public static BlockGroup MergeAllBlockGroups (List<BlockGroup> blockGroups) {
		// Merge all block groups that are part of the parameter array into one block group
		while (blockGroups.Count > 1) {
			blockGroups[1].MergeToBlockGroup(blockGroups[0]);
			blockGroups.RemoveAt(1);
		}

		return blockGroups[0];
	}
}

