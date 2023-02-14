using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum PointsEventType {
	DESTROYED_BLOCK, DROPPED_BLOCK, BREAKTHROUGH, DESTROYED_MINO, FAST_DROP
}

public class PointsEvent : MonoBehaviour {
	[Header("Properties")]
	[SerializeField] private int pointsPerDestroyedBlock = 6;
	[SerializeField] private int pointsPerDroppedBlock = 12;
	[SerializeField] private int pointsPerBreakthrough = 600;
	[SerializeField] private int pointsPerDestroyedMino = 60;
	[SerializeField] private int pointsPerFastDrop = 2;
	[Space]
	[SerializeField] public PointsEventType _pointsEventType = PointsEventType.DESTROYED_BLOCK;
	[SerializeField] public float Lifetime = 1f;
	[SerializeField] public int Combo = 0;

	private float startTime = 0;
	private string label = "";
	private int points = 0;

	public PointsEventType PointsEventType {
		get {
			return _pointsEventType;
		}

		set {
			_pointsEventType = value;

			/// TODO: Make some point values scale with the level of the game

			switch (_pointsEventType) {
				case PointsEventType.DESTROYED_BLOCK:
					label = "Destroyed Block";
					points = pointsPerDestroyedBlock;
					break;
				case PointsEventType.DROPPED_BLOCK:
					label = "Dropped Block";
					points = pointsPerDroppedBlock;
					break;
				case PointsEventType.BREAKTHROUGH:
					label = "Breakthrough";
					points = pointsPerBreakthrough;
					break;
				case PointsEventType.DESTROYED_MINO:
					label = "Full Mino";
					points = pointsPerDestroyedMino;
					break;
				case PointsEventType.FAST_DROP:
					label = "Fast Drop";
					points = pointsPerFastDrop;
					break;
			}
		}
	}

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
		Combo++;

		// Only display the combo if it is more than one
		string comboText = (Combo > 1 ? $"x{Combo}" : "");
		// Set the text values to display
		transform.Find("Label").GetComponent<TextMeshProUGUI>( ).text = $"{label} {comboText}";
		transform.Find("Value").GetComponent<TextMeshProUGUI>( ).text = $"+{points * Combo} points";

		// Reset the lifetime counter
		// If the lifetime counter was already going, then resetting this also resets the progress
		// This is good for "combos"
		startTime = Time.time;
	}
}