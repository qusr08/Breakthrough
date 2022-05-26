using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants {
	public const int BOARD_WIDTH = 16;
	public const int BOARD_HEIGHT = 28;
	public const int BOARD_TOP_PADDING = 4;
	public const int BOARD_BOTTOM_PADDING = 2;
	public const int BOARD_CAMERA_PADDING = 3;
	public const int BOARD_WALL_HEIGHT = 7;

	public const float BLOCK_PERCENT_BOMB = 0.6f;
	public const float BLOCK_FALL_TIME = 1.0f;
	public const float BLOCK_MOVE_TIME = 0.1f;
	public const float BLOCK_ROTATE_TIME = 0.25f;
	public const float BLOCK_ROTATE_DIRECTION = -1; // Clockwise
	public const float BLOCK_TILE_SCALE = 0.95f;
	public const float BLOCK_DAMP_SPEED = 0.04f;

	public const float CLOSE_ENOUGH = 0.01f;
}
