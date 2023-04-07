using System.Collections;
using System.Collections.Generic;
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

	protected Vector2Int toPosition;
	protected Quaternion toRotation;
	protected float previousFallTime = 0;

	#region Properties
	public int ID { get => _id; set => _id = value; }
	public bool IsModified { get => _isModified; set => _isModified = value; }

	public bool CanFall { get => _canFall; private set => _canFall = value; }
	public bool CanFallBelow { get => _canFallBelow; private set => _canFallBelow = value; }

	public Block this[int i] {
		get => blocks[i];
		set => blocks[i] = value;
	}
	public int Count => blocks.Count;
	#endregion

	#region Unity 
	protected virtual void OnValidate ( ) {
		board = FindObjectOfType<Board>( );
		gameManager = FindObjectOfType<GameManager>( );
	}

	protected virtual void Awake ( ) {
		OnValidate( );
	}

	protected virtual void Update ( ) {
		/// TODO: Update transforms over time (tween)

		UpdateSelf( );
	}
	#endregion

	protected virtual void UpdateSelf ( ) {
		if (board.BoardState != BoardState.UPDATING_BLOCKGROUPS) {
			return;
		}

		if (Time.time - previousFallTime > gameManager.FallTimeAccelerated) {
			if (TryMove(Vector2Int.down)) {
				previousFallTime = Time.time;
			}
		}
	}

	protected bool TryMove (Vector2Int deltaPosition) {
		for (int i = Count - 1; i >= 0; i--) {
			if (!IsValidBlockPosition(blocks[i], blocks[i].Position + deltaPosition)) {
				return false;
			}
		}

		toPosition += deltaPosition;

		return true;
	}

	protected bool TryRotate ( ) {
		return true;
	}

	private bool IsValidBlockPosition (Block block, Vector2Int newPosition) {
		// If the block group cannot fall below the breakthrough line but the current block is trying to, return false
		if (!CanFallBelow && newPosition.y < board.BreakthroughBoardAreaHeight) {
			return false;
		}

		if (board.IsBlockAt(newPosition, ID)) {
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
			Destroy(this);
		}
	}

	private BlockGroup MergeToBlockGroup (BlockGroup blockGroup) {
		while (Count > 0) {
			this[0].BlockGroup = blockGroup;
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
		while (blockGroups.Count > 1) {
			// If either the first or second index points to a null block group, remove it from the list and continue to the next iteration
			if (blockGroups[0] == null) {
				blockGroups.RemoveAt(0);
				continue;
			}
			if (blockGroups[1] == null) {
				blockGroups.RemoveAt(1);
				continue;
			}

			// Merge the first and second block groups together and save that into the first index
			blockGroups[0] = MergeBlockGroups(blockGroups[0], blockGroups[1]);
		}

		return blockGroups[0];
	}
}
