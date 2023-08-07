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
		LeanTween.value(textMeshProUGUI.gameObject, (Color color) => textMeshProUGUI.color = color, textMeshProUGUI.color, themeManager.GetRandomButtonColor( ), Constants.UI_FADE_TIME);
		LeanTween.value(backgroundImage.gameObject, (Color color) => backgroundImage.color = color, backgroundImage.color, themeManager.GetRandomBackgroundDetailColor( ), Constants.UI_FADE_TIME);
	}

	public override void OnPointerExit (PointerEventData eventData) {
		LeanTween.value(textMeshProUGUI.gameObject, (Color color) => textMeshProUGUI.color = color, textMeshProUGUI.color, themeManager.ActiveTheme.TextColor, Constants.UI_FADE_TIME);
		LeanTween.value(backgroundImage.gameObject, (Color color) => backgroundImage.color = color, backgroundImage.color, themeManager.GetRandomButtonColor( ), Constants.UI_FADE_TIME);
	}

	public override void OnPointerClick (PointerEventData eventData) {
		onClickEvents.Invoke( );
	}
}
