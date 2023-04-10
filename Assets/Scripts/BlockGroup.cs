using System.Collections;
using System.Collections.Generic;
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

	protected List<Block> blocks = new List<Block>( );

	protected Vector3 toPosition;
	private Vector3 toPositionVelocity;
	protected Vector3 toRotation;
	private Vector3 toRotationVelocity;

	protected float previousFallTime;

	#region Properties
	public int ID { get => _id; set => _id = value; }
	public bool IsModified { get => _isModified; set => _isModified = value; }
	public bool CanFall { get => _canFall; protected set => _canFall = value; }
	public bool CanFallBelow { get => _canFallBelow; protected set => _canFallBelow = value; }
	public int Count => blocks.Count;
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
	}

	protected virtual void Start ( ) {
		foreach (Block block in GetComponentsInChildren<Block>( )) {
			AddBlock(block);

			// Update the position of the block
			Vector2Int _ = block.Position;
		}

        previousFallTime = Time.time;
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
	}

	protected bool TryMove (Vector2Int deltaPosition) {
        for (int i = Count - 1; i >= 0; i--) {
            Debug.Log("Try Move Block Group - " + i);
            if (!IsValidBlockPosition(blocks[i], blocks[i].Position + deltaPosition)) {
				return false;
			}
		}

		toPosition += (Vector3Int) deltaPosition;

		// if (!CanFallBelow && toPosition.y >= board.BreakthroughBoardArea.Height) {
		if (!CanFallBelow && toPosition.y >= 2) {
			CanFallBelow = true;
		}

		return true;
	}

	protected bool TryRotate ( ) {
		return true;
	}

	private bool IsValidBlockPosition (Block block, Vector2Int newPosition) {
		// If the block group cannot fall below the breakthrough line but the current block is trying to, return false
		// if (!CanFallBelow && newPosition.y < board.BreakthroughBoardArea.Height) {
		if (!CanFallBelow && newPosition.y < 2) {
			Debug.Log("Cant fall below. " + newPosition);
			return false;
		}

		if (board.IsBlockAt(newPosition, blockGroupID: ID)) {
			Debug.Log("is block at : " + newPosition);
			if (newPosition.y < 0) {
				board.DamageBlock(block, destroy: true);
			} else {
				return false;
			}
		}

		return true;
	}

	public void AddBlock (Block block) {
		blocks.Add(block);
	}

	public void RemoveBlock (Block block) {
		blocks.Remove(block);

		if (Count == 0) {
			Destroy(gameObject);
		}
	}

	private BlockGroup MergeToBlockGroup (BlockGroup blockGroup) {
		while (Count > 0) {
			blocks[0].BlockGroup = blockGroup;
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
			Debug.Log("MergeAllBlockGroups Loop");
			mergedBlockGroup = MergeBlockGroups(mergedBlockGroup, blockGroups[0]);
			blockGroups.RemoveAt(0);
		}

		Debug.Log("Done");

		return mergedBlockGroup;
	}
}
