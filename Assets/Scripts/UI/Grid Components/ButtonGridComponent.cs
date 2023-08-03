using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonGridComponent : TextGridComponent {
	[Space]
	[SerializeField] private UnityEvent onClickEvents;

	#region Unity Functions

	#endregion

	public override void OnPointerEnter (PointerEventData eventData) {
		LeanTween.scale(textRectTransform, new Vector3(Constants.UI_TEXT_SIZE, Constants.UI_TEXT_SIZE, 1f), Constants.UI_FADE_TIME);
		LeanTween.value(textMeshProUGUI.gameObject, (Color color) => textMeshProUGUI.color = color, textMeshProUGUI.color, themeManager.GetRandomButtonColor( ), Constants.UI_FADE_TIME);
		LeanTween.value(backgroundImage.gameObject, (Color color) => backgroundImage.color = color, backgroundImage.color, themeManager.GetRandomBackgrounDetailColor( ), Constants.UI_FADE_TIME);
	}

	public override void OnPointerExit (PointerEventData eventData) {
		LeanTween.scale(textRectTransform, Vector3.one, Constants.UI_FADE_TIME);
		LeanTween.value(textMeshProUGUI.gameObject, (Color color) => textMeshProUGUI.color = color, textMeshProUGUI.color, themeManager.ActiveTheme.TextColor, Constants.UI_FADE_TIME);
		LeanTween.value(backgroundImage.gameObject, (Color color) => backgroundImage.color = color, backgroundImage.color, themeManager.GetRandomButtonColor(ref backgroundColorIndex), Constants.UI_FADE_TIME);
	}

	public override void OnPointerClick (PointerEventData eventData) {
		onClickEvents.Invoke( );
	}
}
