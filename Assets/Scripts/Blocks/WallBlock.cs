using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallBlock : Block, IThemeElement {
	protected override void OnHealthChange ( ) {
		base.OnHealthChange( );

		if (Health > 0) {
			BlockColor = ThemeSettingsManager.Instance.ActiveThemeSettings.WallBlockColors[Health];
		}
	}

	public void UpdateThemeElements ( ) {
		BlockColor = ThemeSettingsManager.Instance.ActiveThemeSettings.WallBlockColors[Health];
	}
}
