using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomBlockFrames {
	private Board board;
	private GameManager gameManager;
	private Block boomBlock;

	/// <summary>
	/// A list of all the positions that will be exploded when triggered.
	/// </summary>
	public List<List<Vector3>> frames;
	public List<Vector3> this[int index] {
		get {
			return frames[index];
		}
	}

	/// <summary>
	/// The number of frames
	/// </summary>
	public int Count {
		get {
			return frames.Count;
		}
	}

	public BoomBlockFrames (Board board, GameManager gameManager, Block boomBlock) {
		this.board = board;
		this.gameManager = gameManager;
		this.boomBlock = boomBlock;

		CalculateFrames( );
	}

	/// <summary>
	/// Calculate the frames for this class's boom block.
	/// </summary>
	public void CalculateFrames ( ) {
		// Create frame list
		frames = new List<List<Vector3>>( );
		// Add boom block to the first frame
		frames.Add(new List<Vector3>( ) { boomBlock.Position });

		// Whether or not a new block to be exploded was found
		bool foundBlockInRange;
		// The index of the current frame within a block frame list
		int frameIndex = 0;

		// Construct the frames of the boom block animation
		do {
			foundBlockInRange = false;

			// Loops through each boom block that will explode on the current frame
			foreach (Vector3 position in frames[frameIndex]) {
				// Add all neighboring blocks to this current boom block to the next frame
				foreach (Vector3 neighborPosition in Utils.GetCardinalPositions(position)) {
					/// TODO: Add a check to stop the frame if there are no more blocks to be exploded
					///			If there is just a long string where no blocks are destroyed, there is no need to pause the game and wait
					///			This happens a lot with vertical line boom blocks

					// If a neighbor is found, then there will be a next frame of the explosion
					// The neighbor block might be null, but the position is still within range of the boom block so it counts
					// Otherwise blocks across a gap will not be affected by the boom block which is not the behavior desired
					if (AddBlockToFrame(neighborPosition, frameIndex + 1)) {
						foundBlockInRange = true;
					}
				}
			}

			frameIndex++;
		} while (foundBlockInRange);

		/// TODO: This will be removed eventually as the player should see the full range of each boom block
		///	- This is good for testing for now though because there is no visual indication of the boom blocks exploding.
		CleanFrames( );
	}

	/// <summary>
	/// Add a block to a specific frame.
	/// </summary>
	/// <param name="position">The position to add to the frame</param>
	/// <param name="frameIndex">The frame to add to</param>
	/// <returns></returns>
	private bool AddBlockToFrame (Vector3 position, int frameIndex) {
		// Check to see if the position is within range of the boom block
		if (!boomBlock.IsWithinRange(position)) {
			return false;
		}

		// Check to see if the block has already been added to a frame
		// This makes sure that a loop does not occur of block positions being repeatedly destroyed
		foreach (List<Vector3> frame in frames) {
			if (frame.Contains(position)) {
				return false;
			}
		}

		// Make sure the block frame list is big enough for the frame index
		while (frames.Count <= frameIndex) {
			frames.Add(new List<Vector3>( ));
		}
		frames[frameIndex].Add(position);

		return true;
	}

	/// <summary>
	/// Make sure all positions pointing to null blocks are removed.
	/// </summary>
	private void CleanFrames ( ) {
		bool foundBlock = false;

		do {
			int lastFrameIndex = frames.Count - 1;

			// Loop through all positions in the last block frame
			for (int i = frames[lastFrameIndex].Count - 1; i >= 0; i--) {
				// If the position represents a null block, remove it from the list
				if (board.GetBlockAtPosition(frames[lastFrameIndex][i]) == null) {
					frames[lastFrameIndex].RemoveAt(i);
				}
			}

			// If all of the positions of the last block frame were removed, remove the frame entirely
			if (frames[lastFrameIndex].Count == 0) {
				frames.RemoveAt(lastFrameIndex);
			} else {
				foundBlock = true;
			}
		} while (!foundBlock);
	}

	/// <summary>
	/// Destroy the first frame
	/// </summary>
	public void DestroyFirstFrame ( ) {
		// Remove each block in the frame
		for (int i = frames[0].Count - 1; i >= 0; i--) {
			if (board.GetBlockAtPosition(frames[0][i]) != null) {
				gameManager.BoardPoints += gameManager.PointsPerDestroyedBlock;
				// Debug.Log("Points: Destroyed block");
				board.RemoveBlockFromBoard(frames[0][i]);
			}
		}

		// Remove the first frame
		frames.RemoveAt(0);
	}
}
