using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BoardUILabel : MonoBehaviour {
	[Header("Components")]
	[SerializeField] public TextMeshProUGUI Label;
	[SerializeField] public TextMeshProUGUI Value;
	[Header("Properties")]
	[SerializeField] private float textBaseCharacterSpacing;
	[SerializeField] private float textBaseFontSize;
	[SerializeField] private float textAnimCharacterSpacing;
	[SerializeField] private float textAnimFontSize;
	[SerializeField, Range(0f, 1f)] private float textSmoothing;

	private Coroutine textCoroutine = null;

	private bool IsBaseCharacterSpacing {
		get {
			if (Mathf.Abs(Value.characterSpacing - textBaseCharacterSpacing) <= 0.01) {
				Value.characterSpacing = textBaseCharacterSpacing;

				return true;
			}

			return false;
		}
	}
	private bool IsBaseFontSize {
		get {
			if (Mathf.Abs(Value.fontSize - textBaseFontSize) <= 0.01) {
				Value.fontSize = textBaseFontSize;

				return true;
			}

			return false;
		}
	}

	private void OnValidate ( ) {
		Label = transform.Find("Label").GetComponent<TextMeshProUGUI>( );
		Value = transform.Find("Value").GetComponent<TextMeshProUGUI>( );

		textBaseCharacterSpacing = Value.characterSpacing;
		textBaseFontSize = Value.fontSize;
	}

	public void TriggerTextAnimation ( ) {
		if (textCoroutine != null) {
			StopCoroutine(textCoroutine);
		}
		textCoroutine = StartCoroutine(TextAnimation( ));
	}

	private IEnumerator TextAnimation ( ) {
		float characterSpacingVelocity = 0;
		float fontSizeVelocity = 0;

		Value.characterSpacing = textAnimCharacterSpacing;
		Value.fontSize = textAnimFontSize;

		// If the character spacing gets close enough, exit out of the loop
		while (!IsBaseCharacterSpacing || !IsBaseFontSize) {
			Value.characterSpacing = Mathf.SmoothDamp(Value.characterSpacing, textBaseCharacterSpacing, ref characterSpacingVelocity, textSmoothing);
			Value.fontSize = Mathf.SmoothDamp(Value.fontSize, textBaseFontSize, ref fontSizeVelocity, textSmoothing);

			yield return null;
		}
	}
}
