using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGroup {
	private Block[ , ] board;

	private List<Block> blocks;
	public Block this[int index] {
		get {
			return blocks[index];
		}
		set {
			blocks[index] = value;
		}
	}

	public BlockGroup (Block[ , ] board) {
		this.board = board;

		blocks = new List<Block>( );
	}

	public void AddBlock (Block block) {
		blocks.Add(block);
	}

	public void RemoveBlock (Block block) {

	}
}

