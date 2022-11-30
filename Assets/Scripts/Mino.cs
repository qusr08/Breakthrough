using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mino : MonoBehaviour {
	public const float PERCENT_BOOM = 0.6f; // Might vary with level
	public const float FALL_TIME = 1.0f; // Varies with level
	public const float FALL_TIME_ACCELERATED = FALL_TIME / 20f;
	public const float MOVE_TIME = 0.15f;
	public const float MOVE_TIME_ACCELERATED = MOVE_TIME / 2f;
	public const float ROTATE_TIME = 0.25f;
	public const float PLACE_TIME = 0.75f;
	public const float ROTATE_DIRECTION = -1; // Clockwise
	public const float TILE_SCALE = 0.95f;
	public const float DAMP_SPEED = 0.04f;

	[Header("Scene GameObjects")]
	[SerializeField] private Board board;
	[SerializeField] private GameManager gameManager;
	[Header("Properties")]
	[Tooltip("Whether or not the Mino can move.")]
	[SerializeField] public bool HasLanded;

	// The previous time that this Mino moved downwards on the game board
	private float prevFallTime;
	// The previous time that this Mino moved left or right on the game board
	private float prevMoveTime;
	// The previous time that this Mino rotated on the game board
	private float prevRotateTime;
	// A timer to determine when the Mino should be placed
	// When it reaches 0 the Mino gets placed on the board
	// It can be reset by either moving or rotating the Mino
	private float placeTimer;

	// Vectors to move or rotate the Mino to
	private Vector3 moveTo;
	private Vector3 moveVelocity;
	private Vector3 rotateTo;
	private Vector3 rotateVelocity;

	private void OnValidate ( ) {
		board = FindObjectOfType<Board>( );
		gameManager = FindObjectOfType<GameManager>( );
	}

	private void Awake ( ) {
		OnValidate( );
	}

	private void Start ( ) {
		/// TODO: Make multiple boom blocks able to spawn
		/// TODO: Make percentage chance to spawn with a boom block change over time to make it fair
		///			Guarentee the player will have a boom block in X minos

		// Generate a random number between 0 and 1, and if it is less than the percentage of a boom block being on a mino, then place a random one
		if (Random.Range(0f, 1f) < PERCENT_BOOM) {
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

		HasLanded = true;
		moveTo = transform.position;
	}

	private void Update ( ) {
		// Smoothly transition the position and rotation of the mino
		transform.position = Vector3.SmoothDamp(transform.position, moveTo, ref moveVelocity, DAMP_SPEED);
		transform.eulerAngles = Utils.SmoothDampEuler(transform.eulerAngles, rotateTo, ref rotateVelocity, DAMP_SPEED);

		// If the mino landed, wait for the position and rotation of it to be not transitioning anymore to spawn a new mino
		if (!HasLanded) {
			// Debug.Log($"position: {moveTo} == {transform.position} | rotation: {rotateTo} == {transform.eulerAngles}");
			// Debug.Log($"position {Utils.CompareVectors(moveTo, transform.position)} | rotation: {Utils.CompareAngleVectors(rotateTo, transform.eulerAngles)}");

			if (Utils.CompareVectors(moveTo, transform.position) && Utils.CompareDegreeAngleVectors(rotateTo, transform.eulerAngles)) {
				// The position and rotation of the mino might be a little off, so make sure it is exact before the mino is deactivated
				transform.eulerAngles = rotateTo;
				transform.position = moveTo;

				// Add the Mino to the game board
				board.AddLandedMinoToBoard(this);
			}

			return;
		}

		// Get the horizontal and vertical inputs
		float hori = Input.GetAxisRaw("Horizontal");
		float vert = Input.GetAxisRaw("Vertical");
		placeTimer -= Time.deltaTime;

		// Move the mino left and right
		if (hori == 0) {
			prevMoveTime = 0;
		} else if (Time.time - prevMoveTime > MOVE_TIME) {
			if (Move(Vector3.right * hori)) {
				// Make the first horizontal movement of the player be a longer delay in between movements
				// This prevents the player having to quickly tap the left and right buttons to move one board space
				if (prevMoveTime == 0) {
					prevMoveTime = Time.time;
				} else {
					prevMoveTime = Time.time - MOVE_TIME_ACCELERATED;
				}
				placeTimer = PLACE_TIME;
			}
		}

		// Rotate the mino
		if (vert == 0) {
			prevRotateTime = 0;
		} else if (vert > 0 && Time.time - prevRotateTime > ROTATE_TIME) {
			if (Rotate(ROTATE_DIRECTION * 90)) {
				placeTimer = PLACE_TIME;
				prevRotateTime = Time.time;
			}
		}

		// Have the mino automatically fall downwards, or fall faster downwards according to player input
		if (Time.time - prevFallTime > (vert < 0 ? FALL_TIME_ACCELERATED : FALL_TIME)) {
			if (Move(Vector3.down)) {
				prevFallTime = Time.time;

				// If the player is fast dropping the mino, make sure to give points and reset the place timer
				if (vert < 0) {
					placeTimer = PLACE_TIME;
					// gameManager.TriggerPointsEvent(PointsEventType.FAST_DROP);
				}
			} else if (placeTimer <= 0) {
				HasLanded = false;
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
			block.BlockDirection = (BlockDirection) (((int) block.BlockDirection - ROTATE_DIRECTION) % 4);
		}

		return true;
	}
}
