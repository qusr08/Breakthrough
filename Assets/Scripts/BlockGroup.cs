using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGroup : MonoBehaviour {
	[Header("Scene Objects")]
	[SerializeField] protected Board board;
	[SerializeField] protected GameManager gameManager;
	[SerializeField] protected AudioManager audioManager;
	[Header("Properties")]
	[SerializeField, Tooltip("Whether or not this block group is modified (a block has been added/removed) and needs to be updated.")] public bool IsModified;
	[SerializeField, Tooltip("Whether or not this block group can move downwards.")] public bool CanMoveDownwards;
	[SerializeField, Tooltip("Whether or not this block group can fall below the bottom of the wall (into the breakthrough area).")] public bool CanFallIntoBreakthroughArea;

	protected float prevFallTime;
	protected float prevMoveTime;
	protected float prevRotateTime;

	private Vector3 moveTo;
	private Vector3 moveVelocity;
	private Vector3 rotateTo;
	private Vector3 rotateVelocity;

	protected bool needToUpdateTransform;

	protected bool IsAtDestination {
		get => Utils.CompareVectors(moveTo, transform.position) && Utils.CompareDegreeAngleVectors(rotateTo, transform.eulerAngles);
	}
	public int Size {
		get => transform.childCount;
	}
	public Block this[int index] { get => transform.GetChild(index).GetComponent<Block>( ); }

	protected virtual void OnValidate ( ) {
		board = FindObjectOfType<Board>( );
		gameManager = FindObjectOfType<GameManager>( );
		audioManager = FindObjectOfType<AudioManager>( );
	}

	protected virtual void Awake ( ) {
		OnValidate( );
	}

	protected virtual void Start ( ) {
		CanMoveDownwards = true;
		CanFallIntoBreakthroughArea = false;
		moveTo = transform.position;
		rotateTo = transform.eulerAngles;
	}

	private void Update ( ) {
		// If the size of this block group is 0, then destroy it
		if (Size == 0) {
			DestroyImmediate(gameObject);

			return;
		}

		UpdateTransform( );

		// Make sure that its the right board update state before updating the position
		if (board.BoardState != BoardState.UPDATING_BLOCK_GROUPS) {
			return;
		}

		// Move the board group down if it is able to
		if (Time.time - prevFallTime > gameManager.FallTimeAccelerated) {
			CanMoveDownwards = Move(Vector3.down);

			if (CanMoveDownwards) {
				prevFallTime = Time.time;
				needToUpdateTransform = true;
			}
		}
	}

	protected void UpdateTransform ( ) {
		transform.position = Vector3.SmoothDamp(transform.position, moveTo, ref moveVelocity, gameManager.AnimationSpeed);
		transform.eulerAngles = Utils.SmoothDampEuler(transform.eulerAngles, rotateTo, ref rotateVelocity, gameManager.AnimationSpeed);

		if (IsAtDestination && needToUpdateTransform) {
			needToUpdateTransform = false;

			transform.position = moveTo;
			transform.eulerAngles = rotateTo;
		}
	}

	/// <summary>
	/// Try to move the block group
	/// </summary>
	/// <param name="direction">The direction to move the block group</param>
	/// <returns>Whether or not the move was successful or not</returns>
	protected bool Move (Vector3 direction) {
		// Check to see if any of the blocks that are part of this mino collide with other blocks that are part of the board already
		// If they do, then this mino cannot move in that direction
		for (int i = Size - 1; i >= 0; i--) {
			if (!CheckBlockForValidMove(this[i], direction, 0f)) {
				return false;
			}
		}

		// If all of the blocks can move down, then adjust the position
		moveTo += direction;

		// If this block group makes a successful move that is not below the bottom of the board, then it can fall below the board
		if (!CanFallIntoBreakthroughArea && moveTo.y >= gameManager.BreakthroughBoardArea.DefaultHeight) {
			CanFallIntoBreakthroughArea = true;
		}

		audioManager.PlaySoundEffect(SoundEffectClipType.MOVE_BLOCK_GROUP);

		return true;
	}

	protected bool Rotate (float degRotation) {
		/// TODO: Move mino to satisfy a rotation
		///		     This could be used for something like t spins, but also just in general is a good thing to have to make the gameplay experience better
		// Check to see if any of the blocks that are part of this mino collide with other blocks that are part of the board already
		// If they do, then this mino cannot rotate in that direction
		for (int i = Size - 1; i >= 0; i--) {
			if (!CheckBlockForValidMove(this[i], Vector3.zero, degRotation)) {
				return false;
			}
		}

		rotateTo += Vector3.forward * degRotation;

		// Make sure to alter the direction of each of the blocks so boom blocks still explode in the right direction
		foreach (Block block in GetComponentsInChildren<Block>( )) {
			block.BlockDirection = (BlockDirection) (((int) block.BlockDirection - gameManager.MinoRotateDirection) % 4);
		}

		audioManager.PlaySoundEffect(SoundEffectClipType.ROTATE_BLOCK_GROUP);

		return true;
	}

	private bool CheckBlockForValidMove (Block block, Vector3 positionChange, float rotationChange) {
        Vector3Int currPosition = Utils.Vect3Round(Utils.RotatePositionAroundPivot(moveTo + block.transform.localPosition, moveTo, rotateTo.z));
        Vector3Int toPosition = Utils.Vect3Round(Utils.RotatePositionAroundPivot(currPosition, moveTo, rotationChange) + positionChange);

		// If this block group can't fall below the bottom of the board but is trying to
		if (!CanFallIntoBreakthroughArea && toPosition.y < gameManager.BreakthroughBoardArea.DefaultHeight) {
			// The block group can't move down then
			return false;
		}

		// Check to see if a block that is a part of this block group can't move down
		if (!board.IsPositionValid(toPosition, transform)) {
			// If the current position of the block is below the bottom of the board
			if (toPosition.y < 0f) {
				// Remove the block from the board
				gameManager.BoardPoints += gameManager.PointsPerDroppedBlock;
				// Debug.Log("Points: Dropped block");
				board.RemoveBlockFromBoard(block, true);
			} else {
				// This means that the block group can't move down
				return false;
			}
		}

		// If this block group is a mino and it has been completely destroyed, then update the board area
		// This happens when the mino is moving and destroys itself at the bottom of the board
		if (Size == 0 && GetComponent<MinoBlockGroup>( )) {
			gameManager.BreakthroughBoardArea.OnDestroyMino( );
			gameManager.HazardBoardArea.OnDestroyMino( );
		}

		return true;
	}

	/// <summary>
	/// Merge this block group with another block group
	/// </summary>
	/// <param name="blockGroup">The block group to merge to</param>
	public void MergeToBlockGroup (BlockGroup blockGroup) {
		/// TODO: Make it so the smaller group merges with the bigger group to save processing time

		while (Size > 0) {
			this[0].transform.SetParent(blockGroup.transform, true);
		}

		blockGroup.CanFallIntoBreakthroughArea = false;
		DestroyImmediate(gameObject);
	}

	/// <summary>
	/// Merge all specified block groups into one block group
	/// </summary>
	/// <param name="blockGroups">The block groups to merge together.</param>
	/// <returns>A BlockGroup object that is the combination of all the specified block groups.</returns>
	public static BlockGroup MergeAllBlockGroups (List<BlockGroup> blockGroups) {
		// Merge all block groups that are part of the parameter array into one block group
		while (blockGroups.Count > 1) {
			// Make sure that no block groups are merged into a mino, all minos should merge into other block groups
			if (blockGroups[0].GetComponent<MinoBlockGroup>( ) != null) {
				blockGroups[0].MergeToBlockGroup(blockGroups[1]);
				blockGroups.RemoveAt(0);
			} else {
				blockGroups[1].MergeToBlockGroup(blockGroups[0]);
				blockGroups.RemoveAt(1);
			}
		}

		return blockGroups[0];
	}
}

