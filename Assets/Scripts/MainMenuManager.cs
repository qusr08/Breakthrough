using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour {
	[Header("Components")]
	[SerializeField] private List<PanelButton> panelButtons;
	[Header("Properties")]
	[SerializeField, Min(0f)] private float panelButtonSpacing;
	[SerializeField, Range(0f, 1f)] private float panelButtonScaling;

	private void OnValidate ( ) {
		// Get the width of the panel button
		// This assumes all of the panel buttons are the same size
		float panelButtonWidth = panelButtons[0].GetComponent<RectTransform>( ).sizeDelta.x;

		// Position all of the panel buttons
		for (int i = 0; i < panelButtons.Count; i++) {
			Transform panelButtonTransform = panelButtons[i].transform;

			// Calculate the x position of the buttons
			float positionX;
			float a = panelButtonWidth + panelButtonSpacing;
			if (i < panelButtons.Count / 2f) {
				positionX = i * a;
			} else {
				positionX = (i - panelButtons.Count) * a;
			}

			// Calculate the scale of the buttons
			float b = (panelButtons.Count * panelButtonScaling) / 2f;
			float scale = Mathf.Max(0, Mathf.Abs((-panelButtonScaling * i) + b) + (1 - b));

			// Set the position and scale of the panel buttons
			panelButtonTransform.localPosition = new Vector3(positionX, 0, 0);
			panelButtonTransform.localScale = new Vector3(scale, scale, 1);
		}
	}

	private void Awake ( ) {
		OnValidate( );
	}
}
