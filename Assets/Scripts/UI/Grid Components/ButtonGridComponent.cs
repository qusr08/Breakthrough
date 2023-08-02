using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonGridComponent : TextGridComponent {
	[Space]
	[SerializeField] private OnClickFunction[ ] onClickFunctions;

	private delegate void OnClickFunction ( );

	#region Unity Functions

	#endregion

	public override void OnPointerEnter (PointerEventData eventData) {
		LeanTween.scale(textRectTransform, new Vector3(Constants.UI_TEXT_SIZE, Constants.UI_TEXT_SIZE, 1f), Constants.UI_FADE_TIME);
		LeanTween.value(textMeshProUGUI.gameObject, (Color color) => textMeshProUGUI.color = color, textMeshProUGUI.color, themeManager.GetRandomButtonColor( ), Constants.UI_FADE_TIME);
		LeanTween.color(backgroundRectTransform, themeManager.GetRandomBackgrounDetailColor( ), Constants.UI_FADE_TIME);
		Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
	}

	public override void OnPointerExit (PointerEventData eventData) {
		LeanTween.scale(textRectTransform, Vector3.one, Constants.UI_FADE_TIME);
		LeanTween.value(textMeshProUGUI.gameObject, (Color color) => textMeshProUGUI.color = color, textMeshProUGUI.color, themeManager.ActiveTheme.TextColor, Constants.UI_FADE_TIME);
		LeanTween.color(backgroundRectTransform, themeManager.GetRandomButtonColor(ref backgroundColorIndex), Constants.UI_FADE_TIME);
	}

	public override void OnPointerClick (PointerEventData eventData) {
		Debug.Log("Click " + transform.name);

		// Call all of the functions that need to be called when the button is clicked
		foreach (OnClickFunction function in onClickFunctions) {
			function( );
		}
	}
}
