using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGroup : MonoBehaviour {
	[SerializeField] private Board board;
	[Space]
	[SerializeField] public bool IsModified;
	[SerializeField] public bool CanMove;

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
		moveTo = transform.position;
	}

	private void Update ( ) {
		transform.position = Vector3.SmoothDamp(transform.position, moveTo, ref moveVelocity, Constants.MINO_DAMP_SPEED);

		// Move the board group down if it is able to
		if (Time.time - prevFallTime > Constants.MINO_FALL_TIME_ACCELERATED) {
			CanMove = Move(Vector3.down);

			if (CanMove) {
				prevFallTime = Time.time;
			}
		}
	}

	private bool Move (Vector3 direction) {
		// Check to see if any of the blocks that are part of this mino collide with other blocks that are part of the board already
		// If they do, then this mino cannot move in that direction
		foreach (Transform child in transform) {
			Vector3 toPosition = Utils.Vect3Round(moveTo + child.localPosition + direction);

			if (!board.IsPositionValid(toPosition, transform) || toPosition.y < Constants.BOARD_BOTTOM_PADDING) {
				return false;
			}
		}

		moveTo += direction;

		return true;
	}

	public static void MergeToBlockGroup (BlockGroup fromBlockGroup, BlockGroup toBlockGroup) {
		// TODO: Make it so the smaller group merges with the bigger group to save processing time

		while (fromBlockGroup.Size > 0) {
			fromBlockGroup[0].transform.SetParent(toBlockGroup.transform, true);
		}

		DestroyImmediate(fromBlockGroup.gameObject);
	}

	public static BlockGroup MergeAllBlockGroups (List<BlockGroup> blockGroups) {
		// Merge all block groups that are part of the parameter array into one block group
		while (blockGroups.Count > 1) {
			MergeToBlockGroup(blockGroups[1], blockGroups[0]);
			blockGroups.RemoveAt(1);
		}

		return blockGroups[0];
	}
}

