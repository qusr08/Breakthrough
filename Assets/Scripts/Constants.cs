using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants {
	#region Blocks
	public const float BLOCK_SCALE = 0.95f; // Blocks are scaled down this amount on the board
	#endregion

	#region Points
	public const int POINT_DSTRY_BLOCK = 6;
	public const int POINT_DROP_BLOCK = 12;
	public const int POINT_BRKTH = 600;
	public const int POINT_DSTRY_MINO = 60;
	public const int POINT_FAST_DROP = 2;
	#endregion

	#region UI
	public const float UI_FADE_TIME = 0.15f; // The fade time for all UI element transitions
	public const int UI_GRID_SIZE = 120; // The grid size for all UI elements
	public const float UI_MENU_TRANS_TIME = 0.3f; // How long it takes for the menus to change levels
	public const float UI_COLOR_AREA_SIZE = 4f; // How big the area of influence the mouse has on grid components
	#endregion

	public const float CLOSE_ENOUGH = 0.01f;
}
