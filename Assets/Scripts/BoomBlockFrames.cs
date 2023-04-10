using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomBlockFrames {
	private Board board;
	private GameManager gameManager;
	private Block boomBlock;
	private Color color;

	private List<List<Vector2Int>> frames;

	#region Properties
	public List<Vector2Int> this[int i] => frames[i];
	public int Count => frames.Count;
	#endregion

	public BoomBlockFrames (Board board, GameManager gameManager, Block boomBlock) {
		this.board = board;
		this.gameManager = gameManager;
		this.boomBlock = boomBlock;

		color = boomBlock.Color;
		color.a = 0.25f;

		GenerateFrames( );
	}

	/// <summary>
	/// Generate the boom block frames
	/// </summary>
	private void GenerateFrames ( ) {
		// Add the boom block to the frames list
		// The first frame of a boom block exploding will always only be the boom block itself
		frames = new List<List<Vector2Int>> {
			new List<Vector2Int>( ) { boomBlock.Position }
		};

		bool hasBlockInRange;
		int frameIndex = 0;

		do {
			hasBlockInRange = false;

			// For each position in the previous frame, add all the cardinal positions around the current position to the next frame
			foreach (Vector2Int position in frames[frameIndex]) {
				List<Vector2Int> neighbors = Utils.GetCardinalPositions(position);
				foreach (Vector2Int neighborPosition in neighbors) {
					if (TryAddPositionToFrame(neighborPosition, frameIndex + 1)) {
						hasBlockInRange = true;
					}
				}
			}

			frameIndex++;
		} while (hasBlockInRange);
	}

	/// <summary>
	/// Try to add a the input position to the specified boom block frame.
	/// </summary>
	/// <param name="position">The position to try and add</param>
	/// <param name="frameIndex">The boom block frame index to try and add the position to</param>
	/// <returns>true if the input position was successfully added to the specified boom block frame, false otherwise.</returns>
	private bool TryAddPositionToFrame (Vector2Int position, int frameIndex) {
		// Check to make sure the position is on the board
		if (!board.IsPositionOnBoard(position)) {
			return false;
		}

		// Make sure the position was not in the previous frame or the current frame
		// This makes sure the boom block does not backtrack over previous positions
		foreach (Vector2Int framePosition in frames[frameIndex - 1]) {
			if (position == framePosition) {
				return false;
			}
		}
		if (frames.Count > frameIndex) {
			foreach (Vector2Int framePosition in frames[frameIndex]) {
				if (position == framePosition) {
					return false;
				}
			}
		}

		// Check to make sure the position is within the range of the boom block
		if (!boomBlock.IsWithinRange(position)) {
			return false;
		}

		// Make sure there are enough frames for the position to be added
		while (frames.Count <= frameIndex) {
			frames.Add(new List<Vector2Int>( ));
		}
		frames[frameIndex].Add(position);

		return true;
	}

	public void DestroyFirstFrame ( ) {
		// Remove each block in the first boom block frame
		for (int i = frames[0].Count - 1; i >= 0; i--) {
			board.DamageBlockAt(frames[0][i]);
		}

		// Remove the first frame
		frames.RemoveAt(0);
	}
}
