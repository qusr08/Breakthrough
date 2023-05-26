using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UIElements;

public class BlockGroup : MonoBehaviour {
    [Header("Components - Block Group")]
    [SerializeField] protected Board board;
    [SerializeField] protected GameManager gameManager;
    [Header("Properties - Block Group")]
    [SerializeField] private int _id = -1;
    [SerializeField] private bool _isModified = false;
    [Space]
    [SerializeField] private bool _canFall;
    [SerializeField] private bool _canFallBelow;

    protected Vector3 toPosition;
    private Vector3 toPositionVelocity;
    protected Vector3 toRotation;
    private Vector3 toRotationVelocity;
    protected bool isDoneTweening;

    protected float previousFallTime;
    protected float previousMoveTime;
    protected float previousRotateTime;

	#region Properties
	public int ID { get => _id; set => _id = value; }
    public bool IsModified { get => _isModified; set => _isModified = value; }
    public bool CanFall { get => _canFall; set => _canFall = value; }
    public bool CanFallBelow { get => _canFallBelow; protected set => _canFallBelow = value; }
    public int Count => transform.childCount;
    public bool IsPlayerControlled => (this is PlayerControlledBlockGroup);
    #endregion

    #region Unity 
    protected virtual void OnValidate ( ) {
        board = FindObjectOfType<Board>( );
        gameManager = FindObjectOfType<GameManager>( );
    }

    protected virtual void Awake ( ) {
        OnValidate( );

        toPosition = transform.position;
        toRotation = transform.eulerAngles;
        isDoneTweening = true;

        previousFallTime = Time.time;
		previousMoveTime = Time.time;
		previousRotateTime = Time.time;
	}

    protected virtual void Start ( ) {

    }

    private void Update ( ) {
        UpdateTransform( );

        if (board.BoardState != BoardState.UPDATING_BLOCKGROUPS) {
            return;
        }

        if (Time.time - previousFallTime > gameManager.FallTimeAccelerated) {
            CanFall = TryMove(Vector2Int.down);

            if (CanFall) {
                previousFallTime = Time.time;
            }
        }
    }
    #endregion

    protected virtual void UpdateTransform ( ) {
        transform.position = Vector3.SmoothDamp(transform.position, toPosition, ref toPositionVelocity, gameManager.BlockGroupAnimationSpeed);
        transform.eulerAngles = Utils.SmoothDampEuler(transform.eulerAngles, toRotation, ref toRotationVelocity, gameManager.BlockGroupAnimationSpeed);
        isDoneTweening = (Utils.CompareVectors(transform.position, toPosition) && Utils.CompareDegreeAngleVectors(transform.eulerAngles, toRotation));
    }

    /// <summary>
    /// Try to move this block group in the input direction
    /// </summary>
    /// <param name="deltaPosition">The direction to move the block group in</param>
    /// <returns>Returns true if the move was successful, false otherwise</returns>
    protected bool TryMove (Vector2Int deltaPosition) {
        // Check to see if every block can move in the direction specified
        // If one block cannot, the entire block group cannot
        for (int i = Count - 1; i >= 0; i--) {
            if (!IsValidBlockPosition(GetBlock(i), deltaPosition, 0f)) {
                return false;
            }
        }

        // Add the new delta position to the position this block group needs to move towards
        toPosition += (Vector3Int) deltaPosition;

        // If the block group has fallen at least one block above the breakthrough area, then it can fall into the breakthrough area
        // if (!CanFallBelow && toPosition.y >= board.BreakthroughBoardArea.Height) {
        if (!CanFallBelow && toPosition.y >= 2) {
            CanFallBelow = true;
        }

        return true;
    }

    protected bool TryRotate (float deltaRotation) {
		// Check to see if every block can move in the direction specified
		// If one block cannot, the entire block group cannot
		for (int i = Count - 1; i >= 0; i--) {
			if (!IsValidBlockPosition(GetBlock(i), Vector2Int.zero, deltaRotation)) {
				return false;
			}
		}

        // Add the new delta rotation to the rotation this block group needs to rotate towards
		toRotation += Vector3.forward * deltaRotation;

		// Make sure to alter the direction of each of the blocks so boom blocks still explode in the right direction
		foreach (Block block in GetComponentsInChildren<Block>( )) {
			block.BlockDirection = (BlockDirection) (((int) block.BlockDirection - gameManager.RotateDirection) % 4);
		}

		return true;
	}

    /// <summary>
    /// Check to see if the input position is valid for the input block to move to
    /// </summary>
    /// <param name="block">The block to be moved</param>
    /// <param name="newPosition">The position to move the block to</param>
    /// <returns>Returns true if the block can validly be moved to the input position, false otherwise</returns>
    private bool IsValidBlockPosition (Block block, Vector2Int deltaPosition, float deltaRotation) {
        // Get the current position of the block and the position that the block will move towards
        // Doing some fancy stuff here to make sure it accounts for the position that the block group will move towards
		Vector2Int currBlockPosition = Utils.Vect2Round(Utils.RotatePositionAroundPivot(toPosition + block.transform.localPosition, toPosition, toRotation.z));
		Vector2Int toBlockPosition = Utils.Vect2Round(Utils.RotatePositionAroundPivot(currBlockPosition, toPosition, deltaRotation) + deltaPosition);

		// If the block group cannot fall below the breakthrough line but the current block is trying to, return false
		// if (!CanFallBelow && toBlockPosition.y < board.BreakthroughBoardArea.Height) {
		if (!CanFallBelow && toBlockPosition.y < 2) {
            return false;
        }

        // If there is a block at the position this block is trying to move towards, make sure the block group can't move down
        if (board.IsBlockAt(toBlockPosition, blockGroupID: ID)) {
            // If the y position of the block is going to be below 0, as in below the bottom of the board, then that block has been dropped
            // This block can just be destroyed in that case
            if (toBlockPosition.y < 0) {
				board.DamageBlock(block, destroy: true, dropped: true);
            } else {
                return false;
            }
        }

        return true;
	}

	/// <summary>
	/// Get a block at a specified index inside the block group
	/// </summary>
	/// <param name="index">The index of the block to get</param>
	/// <returns>Returns the block at the specified index.</returns>
	public Block GetBlock (int index) {
		return transform.GetChild(index).GetComponent<Block>( );
	}

	/// <summary>
	/// Get a list of all the blocks that are part of this block group. Use sparingly as this can be taxing if you have a big block group.
	/// </summary>
	/// <returns>Returns a list of all the blocks that are in this block group</returns>
	public List<Block> GetBlocks ( ) {
		return transform.GetComponentsInChildren<Block>( ).ToList( );
	}

	private BlockGroup MergeToBlockGroup (BlockGroup blockGroup) {
        while (Count > 0) {
            GetBlock(0).BlockGroup = blockGroup;
        }

        return blockGroup;
    }

    /// <summary>
    /// Merge two block groups together
    /// </summary>
    /// <param name="blockGroup1">A block group to merge</param>
    /// <param name="blockGroup2">A block group to merge</param>
    /// <returns>Returns a block group that contains all the blocks from the two input block groups</returns>
    public static BlockGroup MergeBlockGroups (BlockGroup blockGroup1, BlockGroup blockGroup2) {
        // If the block groups are the same, then just return
        if (blockGroup1.ID == blockGroup2.ID) {
            return blockGroup1;
        }

        // If one of the block groups are a player controlled block group, make sure that one is always destroyed (as in all the blocks move out of it)
        if (blockGroup2.GetType( ) == typeof(PlayerControlledBlockGroup)) {
            return blockGroup2.MergeToBlockGroup(blockGroup1);
        }
        if (blockGroup1.GetType( ) == typeof(PlayerControlledBlockGroup)) {
            return blockGroup1.MergeToBlockGroup(blockGroup2);
        }

        // Merge the smaller block group into the larger block group to improve performance
        if (blockGroup1.Count >= blockGroup2.Count) {
            return blockGroup2.MergeToBlockGroup(blockGroup1);
        } else {
            return blockGroup1.MergeToBlockGroup(blockGroup2);
        }
    }

    /// <summary>
    /// Merge all block groups in the list together
    /// </summary>
    /// <param name="blockGroups">All the block groups that should be merged together</param>
    /// <returns>Returns a block group that contains all the blocks from the list of block groups</returns>
    public static BlockGroup MergeAllBlockGroups (List<BlockGroup> blockGroups) {
        // This is the block group that all of the other block groups will be merged into
        // The block group referenced by this object may change as the block groups are merged
        BlockGroup mergedBlockGroup = blockGroups[0];
        blockGroups.RemoveAt(0);

        while (blockGroups.Count > 0) {
            mergedBlockGroup = MergeBlockGroups(mergedBlockGroup, blockGroups[0]);
            blockGroups.RemoveAt(0);
        }

        return mergedBlockGroup;
    }
}
