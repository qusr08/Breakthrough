using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGroup : MonoBehaviour {
	// [Header("References")]
	[Header("Properties")]
	[SerializeField] private List<Block> blocks;

	/// <summary>
	///		Add a block to this block group
	/// </summary>
	/// <param name="block">The block to add</param>
	public void AddBlock (Block block) {
		// Add the block to the list of blocks that are a part of this block group
		blocks.Add(block);
		block.transform.SetParent(transform, true);
	}

	/// <summary>
	///		Remove a block from this block group
	/// </summary>
	/// <param name="block">The block to remove</param>
	public void RemoveBlock (Block block) {
		// Remove the block from the list of blocks that are a part of this block group
		blocks.Remove(block);
		block.transform.SetParent(BoardManager.Instance.transform, true);
	}

	private void OnValidate ( ) {
	}

	private void Awake ( ) {
		OnValidate( );

		blocks = new List<Block>( );
	}
}
