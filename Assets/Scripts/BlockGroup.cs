using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlockGroup : MonoBehaviour {
	[SerializeField, Tooltip("A reference to the game board.")] private Board board;
	[SerializeField, Tooltip("A list of all the blocks that are a part of this block group.")] private List<Block> _blocks;
	[SerializeField, Tooltip("Whether or not this block group can be controlled by the player.")] private bool _isPlayerControlled;
	[SerializeField, Tooltip("Set to true when the block group is going to be destroyed during the next merge.")] private bool _isOld;

	#region Properties
	public List<Block> Blocks { get => _blocks; protected set => _blocks = value; }
	public bool IsPlayerControlled { get => _isPlayerControlled; set => _isPlayerControlled = value; }
	public bool IsOld { get => _isOld; set => _isOld = value; }
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

	public BlockGroup MergeToBlockGroup (BlockGroup blockGroup) {
		while (Blocks.Count > 0) {
			blockGroup.TransferBlock(Blocks[0]);
		}

		return blockGroup;
	}

	public static BlockGroup MergeBlockGroups (BlockGroup blockGroup1, BlockGroup blockGroup2) {
		// If the block groups are the same, then just return
		if (blockGroup1 == blockGroup2) {
			return blockGroup1;
		}

		// If one of the block groups are a player controlled block group, make sure that one is always destroyed
		if (blockGroup1.IsPlayerControlled) {
			return blockGroup2.MergeToBlockGroup(blockGroup1);
		}
		if (blockGroup2.IsPlayerControlled) {
			return blockGroup1.MergeToBlockGroup(blockGroup2);
		}

		// Merge the smaller block group into the larger block group
		if (blockGroup1.Blocks.Count >= blockGroup2.Blocks.Count) {
			return blockGroup2.MergeToBlockGroup(blockGroup1);
		} else {
			return blockGroup1.MergeToBlockGroup(blockGroup2);
		}
	}

	public static BlockGroup MergeAllBlockGroups (List<BlockGroup> blockGroups) {
		BlockGroup mergedBlockGroup = blockGroups[0];
		blockGroups.RemoveAt(0);

		while (blockGroups.Count > 0) {
			mergedBlockGroup = MergeBlockGroups(mergedBlockGroup, blockGroups[0]);
			blockGroups.RemoveAt(0);
		}

		return mergedBlockGroup;
	}
}
