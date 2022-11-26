using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BoardPointsEvent : MonoBehaviour {
	[Header("Properties")]
	[SerializeField] public float Lifetime = 1f;
	[SerializeField] public string Label = "";
	[SerializeField] public int Points = 0;
	[SerializeField] private int combo = 0;

	private float startTime = 0;

	private void Start ( ) {
		startTime = Time.time;
	}

	private void Update ( ) {
		if (Time.time - startTime >= Lifetime) {
			Destroy(gameObject);
		}
	}

	public void Trigger ( ) {
		// Increment the combo of this point event
		combo++;

		// Set the text values to display
		transform.Find("Label").GetComponent<TextMeshProUGUI>( ).text = $"{Label} x{combo}";
		transform.Find("Value").GetComponent<TextMeshProUGUI>( ).text = $"+{Points * combo} points";

		// Reset the lifetime counter
		// If the lifetime counter was already going, then resetting this also resets the progress
		// This is good for "combos"
		startTime = Time.time;
	}
}
