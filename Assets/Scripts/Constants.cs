using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants {
	#region Blocks
	public const float BLOCK_SCALE = 0.95f; // Blocks are scaled down this amount on the board
	public const float BLOCK_ICON_ALPHA = 1f; // The alpha value of the boom block icons on top of blocks
	#endregion

	#region Points
	public const int POINT_DSTRY_BLOCK = 6;
	public const int POINT_DROP_BLOCK = 12;
	public const int POINT_BRKTH = 600;
	public const int POINT_DSTRY_MINO = 60;
	public const int POINT_FAST_DROP = 2;
	#endregion

	#region UI
	public const float UI_FADE_TIME = 0.25f; // The fade time for all UI element transitions
	public const int UI_GRID_SIZE = 120; // The grid size for all UI elements
	#endregion

	public const float CLOSE_ENOUGH = 0.01f;
}
