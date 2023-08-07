using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BoxGridComponent : GridComponent {
	#region Unity Functions
	protected override void Awake ( ) {
		base.Awake( );

		backgroundImage.color = themeManager.GetRandomBackgroundDetailColor( );
	}

	protected override void Update ( ) {
		// If the mouse position is close to this grid component, fade the colors of the background
		if (Utils.DistanceSquared(MouseWorldPosition, transform.position) < 3f) {
			if (!isHovered) {
				FadeBackgroundColor(themeManager.GetRandomMinoColor( ), Constants.UI_FADE_TIME);
				isHovered = true;
			}
		} else {
			if (isHovered) {
				FadeBackgroundColor(themeManager.GetRandomBackgroundDetailColor( ), Constants.UI_FADE_TIME * 3);
				isHovered = false;
			}
		}
	}
	#endregion
}
