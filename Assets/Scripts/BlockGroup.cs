using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlockGroup : MonoBehaviour {
	[SerializeField, Tooltip("A reference to the game board.")] private Board board;
	[SerializeField, Tooltip("A list of all the blocks that are a part of this block group.")] private List<Block> _blocks;
	[SerializeField, Tooltip("Whether or not this block group can be controlled by the player.")] private bool _isPlayerControlled;
	[SerializeField, Tooltip("Set to true if any block that is part of this block group is destroyed.")] private bool _isModified;
	[SerializeField, Tooltip("A queue containing the positions that this block group will move to.")] private Queue<Vector2Int> _positionQueue;

	#region Properties
	/// <summary>
	///		A list of all the blocks that are a part of this block group
	/// </summary>
	public List<Block> Blocks { get => _blocks; protected set => _blocks = value; }

	/// <summary>
	///		Whether or not this block group can be controlled by player input
	/// </summary>
	public bool IsPlayerControlled { get => _isPlayerControlled; set => _isPlayerControlled = value; }

	/// <summary>
	///		Whether or not the block group has been modified
	/// </summary>
	public bool IsModified { get => _isModified; set => _isModified = value; }

	/// <summary>
	///		Whether or not this block group has no blocks that are a part of it
	/// </summary>
	public bool IsEmpty => Blocks.Count == 0;

	/// <summary>
	///		A queue containing the positions that this block group will move to
	/// </summary>
	public Queue<Vector2Int> PositionQueue { get => _positionQueue; set => _positionQueue = value; }
	#endregion

	#region Unity Functions
	private void OnValidate ( ) {
		board = FindObjectOfType<Board>( );
		Blocks = GetComponentsInChildren<Block>( ).ToList( );
	}

	private void Awake ( ) {
		OnValidate( );
	}
	#endregion

	/// <summary>
	///		Transfer the specified block from another block group to this block group
	/// </summary>
	/// <param name="block">The block to transfer</param>
	public void TransferBlock (Block block) {
		// Do nothing if the block is already a part of this block group
		if (Blocks.Contains(block)) {
			return;
		}

		// Remove the block from the other block group
		BlockGroup previousBlockGroup = block.BlockGroup;
		if (previousBlockGroup != null) {
			previousBlockGroup.Blocks.Remove(block);
		}

		// Set the blocks transform parent to this block group
		block.transform.SetParent(transform, true);
		Blocks.Add(block);
		block.BlockGroup = this;
	}

	/// <summary>
	///		Merge this block group to another block group
	/// </summary>
	/// <param name="blockGroup">The block group to merge all blocks to</param>
	/// <returns>
	///		<strong>BlockGroup</strong> that is the block group this block group was merged into
	/// </returns>
	public BlockGroup MergeToBlockGroup (BlockGroup blockGroup) {
		// Transfer all of the blocks to the input block group
		for (int i = Blocks.Count - 1; i >= 0; i--) {
			Blocks[i].BlockGroup = blockGroup;
		}

		// Since the block group is now empty, destroy it
		board.BlockGroups.Remove(this);
		Destroy(gameObject);

		return blockGroup;
	}

	/// <summary>
	///		Merge the input block group with this one
	/// </summary>
	/// <param name="blockGroup">The block group to merge with this block group</param>
	/// <returns>
	///		<strong>BlockGroup</strong> that is the two block groups merged together
	/// </returns>
	public BlockGroup MergeBlockGroup (BlockGroup blockGroup) {
		// If the block groups are the same, then just return
		if (blockGroup == this) {
			return blockGroup;
		}

		// If one of the block groups are a player controlled block group, make sure that one is always destroyed
		// This prevents the player from being able to control block groups from minos that have already been placed
		if (blockGroup.IsPlayerControlled) {
			return MergeToBlockGroup(blockGroup);
		}
		if (IsPlayerControlled) {
			return blockGroup.MergeToBlockGroup(this);
		}

		// Merge the smaller block group into the larger block group
		// This saves processing time
		if (blockGroup.Blocks.Count >= Blocks.Count) {
			return MergeToBlockGroup(blockGroup);
		} else {
			return blockGroup.MergeToBlockGroup(this);
		}
	}
}
