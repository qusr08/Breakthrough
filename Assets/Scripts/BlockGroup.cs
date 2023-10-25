using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlockGroup : MonoBehaviour {
	[SerializeField, Tooltip("A reference to the game board.")] private Board board;
	[SerializeField, Tooltip("A list of all the blocks that are a part of this block group.")] private List<Block> _blocks;
	[SerializeField, Tooltip("Whether or not this block group can be controlled by the player.")] private bool _isPlayerControlled;
	[SerializeField, Tooltip("Set to true if any block that is part of this block group is destroyed.")] private bool _isModified;

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

			// If the other block group now has no more blocks, then destroy it
			/*if (previousBlockGroup.Blocks.Count == 0) {
				Destroy(previousBlockGroup);
			}*/
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
		for (int i = Blocks.Count - 1; i >= 0; i--) {
			Blocks[i].BlockGroup = blockGroup;
		}

		return blockGroup;
	}

	/// <summary>
	///		Merge two block groups together
	/// </summary>
	/// <param name="blockGroup1">The first block group to merge</param>
	/// <param name="blockGroup2">The second block group to merge</param>
	/// <returns>
	///		<strong>BlockGroup</strong> that has all of the blocks that were a part of both block groups
	/// </returns>
	public static BlockGroup MergeBlockGroups (BlockGroup blockGroup1, BlockGroup blockGroup2) {
		// If the block groups are the same, then just return
		if (blockGroup1 == blockGroup2) {
			return blockGroup1;
		}

		// If one of the block groups are a player controlled block group, make sure that one is always destroyed
		// This prevents the player from being able to control block groups from minos that have already been placed
		if (blockGroup1.IsPlayerControlled) {
			return blockGroup2.MergeToBlockGroup(blockGroup1);
		}
		if (blockGroup2.IsPlayerControlled) {
			return blockGroup1.MergeToBlockGroup(blockGroup2);
		}

		// Merge the smaller block group into the larger block group
		// This saves processing time
		if (blockGroup1.Blocks.Count >= blockGroup2.Blocks.Count) {
			return blockGroup2.MergeToBlockGroup(blockGroup1);
		} else {
			return blockGroup1.MergeToBlockGroup(blockGroup2);
		}
	}

	/// <summary>
	///		Merge all block groups in a list together
	/// </summary>
	/// <param name="blockGroups">The list of block groups to merge</param>
	/// <returns>
	///		<strong>BlockGroup</strong> that has all of the blocks that were a part of the list of block groups
	/// </returns>
	/*public static BlockGroup MergeAllBlockGroups (List<BlockGroup> blockGroups) {
		// Make the first block group in the merge list the block group that all blocks will merge into
		BlockGroup mergedBlockGroup = blockGroups[0];
		blockGroups.RemoveAt(0);

		// While there are still block groups to merge, merge them together
		while (blockGroups.Count > 0) {
			mergedBlockGroup = MergeBlockGroups(mergedBlockGroup, blockGroups[0]);
			blockGroups.RemoveAt(0);
		}

		return mergedBlockGroup;
	}*/
}
