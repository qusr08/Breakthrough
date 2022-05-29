using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGroup : MonoBehaviour {
	[SerializeField] private Board board;

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
		// If the block group has no blocks inside of it, get ridda da bitch
		if (Size == 0) {
			Destroy(gameObject);

			return;
		}

		transform.position = Vector3.SmoothDamp(transform.position, moveTo, ref moveVelocity, Constants.MINO_DAMP_SPEED);

		if (Time.time - prevFallTime > Constants.MINO_FALL_TIME_ACCELERATED) {
			if (Move(Vector3.down)) {
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

	public void MergeToBlockGroup (BlockGroup blockGroup) {
		// TODO: Make it so the smaller group merges with the bigger group to save processing time

		while (Size > 0) {
			this[0].transform.SetParent(blockGroup.transform, true);
		}
	}
}

