using MyBox;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

// https://www.youtube.com/watch?v=a2vLaKGCYsA

public class GameManager : Singleton<GameManager> {
	[Header("Properties")]
	[SerializeField, ReadOnly] private float moveValue;
	[SerializeField, ReadOnly] private float rotateValue;
	[SerializeField, ReadOnly] private float instantDropValue;
	[SerializeField, ReadOnly] private float fastDropValue;

	public void OnMove (InputValue inputValue) {
		moveValue = inputValue.Get<float>( );
	}

	public void OnRotate (InputValue inputValue) {
		rotateValue = inputValue.Get<float>( );
	}

	public void OnInstantDrop (InputValue inputValue) {
		instantDropValue = inputValue.Get<float>( );
	}

	public void OnFastDrop (InputValue inputValue) {
		fastDropValue = inputValue.Get<float>( );
	}
}
