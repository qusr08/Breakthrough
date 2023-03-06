using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGroup : MonoBehaviour {
	[Header("Scene Objects")]
	[SerializeField] private Board board;
	[SerializeField] private GameManager gameManager;
	[Header("Properties")]
	[SerializeField, Tooltip("Whether or not this block group is modified (a block has been added/removed) and needs to be updated.")] public bool IsModified;
	[SerializeField, Tooltip("Whether or not this block group can move downwards.")] public bool CanMove;
	[SerializeField, Tooltip("Whether or not this block group can fall below the bottom of the wall (into the breakthrough area).")] public bool CanFallIntoBreakthroughArea;
	[SerializeField, Tooltip("Whether or not this block group is being controlled by the player.")] public bool IsPlayerControlled;
	[SerializeField, Tooltip("The bounds of this block group.")] public Bounds BlockGroupBounds;

	private float prevFallTime;
	private float prevMoveTime;
	private float prevRotateTime;
	private float placeTimer;

	private Vector3 moveTo;
	private Vector3 moveVelocity;
	private Vector3 rotateTo;
	private Vector3 rotateVelocity;

	public int Size {
		get => transform.childCount;
	}
	public Block this[int index] { get => transform.GetChild(index).GetComponent<Block>( ); }

	private void OnValidate ( ) {
		board = FindObjectOfType<Board>( );
		gameManager = FindObjectOfType<GameManager>( );
	}

	private void Awake ( ) {
		OnValidate( );
	}

	private void Start ( ) {
		CanMove = true;
		CanFallIntoBreakthroughArea = false;
		IsPlayerControlled = false;

		moveTo = transform.position;
	}

	private void Update ( ) {
		transform.position = Vector3.SmoothDamp(transform.position, moveTo, ref moveVelocity, gameManager.BlockAnimationSpeed);
		transform.eulerAngles = Utils.SmoothDampEuler(transform.eulerAngles, rotateTo, ref rotateVelocity, gameManager.BlockAnimationSpeed);

		if (IsPlayerControlled) {
			if (board.BoardUpdateState != BoardUpdateState.PLACING_MINO) {
				return;
			}

			UpdateControlled( );
		} else {
			if (board.BoardUpdateState != BoardUpdateState.UPDATING_BLOCK_GROUPS) {
				return;
			}

			UpdateAutomatic( );
		}
	}

	private void UpdateAutomatic ( ) {
		// If the size of this block group is 0, then destroy it
		if (Size == 0) {
			DestroyImmediate(gameObject);
		}

		// Move the board group down if it is able to
		if (Time.time - prevFallTime > gameManager.FallTimeAccelerated) {
			CanMove = Move(Vector3.down);

			if (CanMove) {
				prevFallTime = Time.time;
			}
		}
	}

	private void UpdateControlled ( ) {
		// Get the horizontal and vertical inputs
		float hori = Input.GetAxisRaw("Horizontal");
		float vert = Input.GetAxisRaw("Vertical");
		placeTimer -= Time.deltaTime;

		// Move the mino left and right
		if (hori == 0) {
			prevMoveTime = 0;
		} else if (Time.time - prevMoveTime > gameManager.MoveTime) {
			if (Move(Vector3.right * hori)) {
				// Make the first horizontal movement of the player be a longer delay in between movements
				// This prevents the player having to quickly tap the left and right buttons to move one board space
				if (prevMoveTime == 0) {
					prevMoveTime = Time.time;
				} else {
					prevMoveTime = Time.time - gameManager.MoveTimeAccelerated;
				}
				placeTimer = gameManager.PlaceTime;
			}
		}

		// Rotate the mino
		if (vert == 0) {
			prevRotateTime = 0;
		} else if (vert > 0 && Time.time - prevRotateTime > gameManager.RotateTime) {
			if (Rotate(gameManager.RotateDirection * 90)) {
				placeTimer = gameManager.PlaceTime;
				prevRotateTime = Time.time;
			}
		}

		// Have the mino automatically fall downwards, or fall faster downwards according to player input
		if (Time.time - prevFallTime > (vert < 0 ? gameManager.FallTimeAccelerated : gameManager.FallTime)) {
			if (Move(Vector3.down)) {
				prevFallTime = Time.time;

				// If the player is fast dropping the mino, make sure to give points and reset the place timer
				if (vert < 0) {
					placeTimer = gameManager.PlaceTime;
					gameManager.BoardPoints += gameManager.PointsPerFastDrop;
					// Debug.Log("Points: Fast drop");
				}
			} else if (placeTimer <= 0) {
				// HasLanded = true;
				// needToUpdateAtDestination = true;
			}
		}
	}

	/// <summary>
	/// Try to move the block group
	/// </summary>
	/// <param name="direction">The direction to move the block group</param>
	/// <returns>Whether or not the move was successful or not</returns>
	private bool Move (Vector3 direction) {
		// Check to see if any of the blocks that are part of this mino collide with other blocks that are part of the board already
		// If they do, then this mino cannot move in that direction
		for (int i = Size - 1; i >= 0; i--) {
			Vector3 currPosition = Utils.Vect3Round(moveTo + this[i].transform.localPosition);
			Vector3 toPosition = currPosition + direction;

			// Check to see if a block that is a part of this block group can't move down
			if (!board.IsPositionValid(toPosition, transform)) {
				// This means that the block group can't move down
				return false;
			}

			// If this block group can't fall below the bottom of the board but is trying to
			if (!CanFallIntoBreakthroughArea && toPosition.y < board.BreakthroughBoardArea.Height) {
				// The block group can't move down then
				return false;
			}

			// If the current position of the block is below the bottom of the board
			if (currPosition.y < board.BreakthroughBoardArea.Height) {
				// Remove the block from the board
				gameManager.BoardPoints += gameManager.PointsPerDroppedBlock;
				// Debug.Log("Points: Dropped block");
				board.RemoveBlockFromBoard(this[i], true);
			}
		}

		// If all of the blocks can move down, then adjust the position
		moveTo += direction;

		// If this block group makes a successful move that is not below the bottom of the board, then it can fall below the board
		if (!CanFallIntoBreakthroughArea && moveTo.y >= board.BreakthroughBoardArea.Height) {
			CanFallIntoBreakthroughArea = true;
		}

		return true;
	}

	private bool Rotate (float degRotation) {
		/// TODO: Move mino to satisfy a rotation
		///		     This could be used for something like t spins, but also just in general is a good thing to have to make the gameplay experience better

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
			block.BlockDirection = (BlockDirection) (((int) block.BlockDirection - gameManager.RotateDirection) % 4);
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
			blockGroups[1].MergeToBlockGroup(blockGroups[0]);
			blockGroups.RemoveAt(1);
		}

		return blockGroups[0];
	}
}

