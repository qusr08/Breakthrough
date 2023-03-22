using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinoBlockGroup : BlockGroup {
	[Header("Properties")]
	[SerializeField, Tooltip("The bounds of the Mino.")] public Bounds MinoBounds;
	[SerializeField, Tooltip("Whether or not this Mino has a boom block on it or not.")] public bool HasBoomBlock;

	private float placeTimer;

	public bool HasLanded {
		get => !CanMoveDownwards && placeTimer <= 0;
	}

	/*// Variables to get the bounds max and mins
    // This can be used to check if a mino has entered or landed in a board area
    public float BoundsMinY {
        get {
            // If the mino is rotated either 0 or 180 degrees, use the y value of the bounds
            // If it is rotated 90 or 270 degrees, use the x values of the bounds
            // For this we only want the lowest part of the bounds which could be either the x or y extents
            // Monitor the z rotation as a mino is rotated in the inspector for more information

            bool isNegative = transform.localEulerAngles.z < 0;
            bool is180 = Mathf.RoundToInt(transform.localEulerAngles.z) % 180 == 0;

            if (is180) {
                if (isNegative) {
                    return transform.position.y + MinoBounds.max.y;
                } else {
                    return transform.position.y + MinoBounds.min.y;
                }
            } else {
                if (isNegative) {
                    return transform.position.y + MinoBounds.max.x;
                } else {
                    return transform.position.y + MinoBounds.min.x;
                }
            }
        }
    }
    public float BoundsMaxY {
        get {
            // If the mino is rotated either 0 or 180 degrees, use the y value of the bounds
            // If it is rotated 90 or 270 degrees, use the x values of the bounds
            // For this we only want the lowest part of the bounds which could be either the x or y extents
            // Monitor the z rotation as a mino is rotated in the inspector for more information

            bool isNegative = transform.localEulerAngles.z < 0;
            bool is180 = Mathf.RoundToInt(transform.localEulerAngles.z) % 180 == 0;

            if (is180) {
                if (isNegative) {
                    return transform.position.y + MinoBounds.min.y;
                } else {
                    return transform.position.y + MinoBounds.max.y;
                }
            } else {
                if (isNegative) {
                    return transform.position.y + MinoBounds.min.x;
                } else {
                    return transform.position.y + MinoBounds.max.x;
                }
            }
        }
    }*/

	private new void OnValidate ( ) {
		base.OnValidate( );

		// Update the bounds of this mino based on the colliders of the child block objects
		MinoBounds = new Bounds(transform.position, Vector3.zero);
		foreach (Collider2D blockCollider2D in GetComponentsInChildren<Collider2D>( )) {
			MinoBounds.Encapsulate(blockCollider2D.bounds);
		}

		// Keep the center at a relative position so we can use it to calculate the min and max of the mino bounds as it moves
		MinoBounds.center -= transform.position;
	}

	private new void Awake ( ) {
		OnValidate( );
	}

	private new void Start ( ) {
		base.Start( );

		/// TODO: Make multiple boom blocks able to spawn
		// Generate a random number between 0 and 1, and if it is less than the percentage of a boom block being on a mino, then place a random one
		if (Random.Range(0f, 1f) < gameManager.CurrentBoomBlockSpawnPercentage) {
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

			HasBoomBlock = true;
		}
	}

	private void Update ( ) {
		// If the size of this block group is 0, then destroy it
		if (Size == 0) {
			DestroyImmediate(gameObject);

			return;
		}

		// If the mino has landed and is at its destination that it was moving to, add it to the board
		if (HasLanded && IsAtDestination) {
			board.AddMinoToBoard(this);

			return;
		}

		UpdateTransform( );

		// Make sure that its the right board update state before updating the position
		if (board.BoardState != BoardState.PLACING_MINO) {
			return;
		}

		// Get the horizontal and vertical inputs
		float hori = Input.GetAxisRaw("Horizontal");
		float vert = Input.GetAxisRaw("Vertical");
		placeTimer -= Time.deltaTime;

		// Move the mino left and right
		if (hori == 0) {
			prevMoveTime = 0;
		} else if (Time.time - prevMoveTime > gameManager.MinoMoveTime) {
			if (Move(Vector3.right * hori)) {
				// Make the first horizontal movement of the player be a longer delay in between movements
				// This prevents the player having to quickly tap the left and right buttons to move one board space
				if (prevMoveTime == 0) {
					prevMoveTime = Time.time;
				} else {
					prevMoveTime = Time.time - gameManager.MoveTimeAccelerated;
				}

				placeTimer = gameManager.MinoPlaceTime;
				needToUpdateTransform = true;
			}
		}

		// Rotate the mino
		if (vert == 0) {
			prevRotateTime = 0;
		} else if (vert > 0 && Time.time - prevRotateTime > gameManager.MinoRotateTime) {
			if (Rotate(gameManager.MinoRotateDirection * 90)) {
				placeTimer = gameManager.MinoPlaceTime;
				prevRotateTime = Time.time;
				needToUpdateTransform = true;
			}
		}

		// Have the mino automatically fall downwards, or fall faster downwards according to player input
		if (Time.time - prevFallTime > (vert < 0 ? gameManager.FallTimeAccelerated : gameManager.MinoFallTime)) {
			CanMoveDownwards = Move(Vector3.down);

			if (CanMoveDownwards) {
				// If the player is fast dropping the mino, make sure to give points and reset the place timer
				if (vert < 0) {
					placeTimer = gameManager.MinoPlaceTime;
					gameManager.BoardPoints += gameManager.PointsPerFastDrop;
					// Debug.Log("Points: Fast drop");
				}

				prevFallTime = Time.time;
				needToUpdateTransform = true;
			}
		}
	}
}
