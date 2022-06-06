using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallBlock : Block {
	private int cracked;
	private BlockType[ ] crackedStages = new BlockType[ ] { BlockType.NONE, BlockType.CRACK_STAGE_1, BlockType.CRACK_STAGE_2 };
	private BlockColor[ ] colorStages = new BlockColor[ ] { BlockColor.LIGHT_COAL, BlockColor.MEDIUM_COAL, BlockColor.DARK_COAL };

	public override int Health {
		set {
			if (value <= 0) {
				DestroyImmediate(gameObject);
			} else {
				BlockColor = colorStages[value - 1];

				if (value < _health) {
					BlockType = crackedStages[++cracked];
				}
			}

			_health = value;
		}
	}
}
