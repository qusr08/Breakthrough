using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BoxGridComponent : GridComponent {
	private Color hoverColor;
	private Color idleColor;
	private bool isHovered;

	#region Unity Functions
	protected override void Awake ( ) {
		base.Awake( );

		hoverColor = themeManager.GetRandomMinoColor( );
		idleColor = themeManager.GetRandomBackgroundDetailColor( );
		backgroundImage.color = idleColor;
		isHovered = false;
	}

	private void Update ( ) {
		// Get the world position of the mouse
		Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue( ));

		// If the mouse position is close to this grid component, fade the colors of the background
		if (Vector2.Distance(mouseWorldPosition, transform.position) < 2f) {
			if (!isHovered) {
				FadeBackgroundColor(hoverColor, Constants.UI_FADE_TIME);
			}

			isHovered = true;
		} else {
			if (isHovered) {
				FadeBackgroundColor(idleColor, Constants.UI_FADE_TIME * 3);
			}

			isHovered = false;
		}
	}
	#endregion

	public override void OnPointerEnter (PointerEventData eventData) {
	}

	public override void OnPointerExit (PointerEventData eventData) {
	}
}
