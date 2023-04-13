using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundBlockSpawner : MonoBehaviour {
	[Header("Prefabs")]
	[SerializeField, Tooltip("The prefab for the background blocks that will spawn.")] private GameObject backgroundBlockPrefab;
	[Header("Background Block Variables")]
	[SerializeField, Min(0f), Tooltip("The maximum size that a background block can be.")] private float maxBlockSize;
	[SerializeField, Min(0f), Tooltip("The minimum size that a background block can be.")] private float minBlockSize;
	[SerializeField, Min(0f), Tooltip("The maximum speed that a background block can have.")] private float maxBlockSpeed;
	[SerializeField, Min(0f), Tooltip("The minimum speed that a background block can have.")] private float minBlockSpeed;
	[SerializeField, Min(0f), Tooltip("The maximum rotational speed that a background block can have.")] private float maxBlockRotateSpeed;
	[SerializeField, Min(0f), Tooltip("The minimum rotational speed that a background block can have.")] private float minBlockRotateSpeed;
	[SerializeField, Tooltip("A list of all the hex codes that the background blocks can change into. The background blocks will slowly fade to different colors as they move around to add more dynamicness to the background.")] private List<string> colors;
	[SerializeField, Range(0f, 1f), Tooltip("The alpha of the color to set the background blocks to.")] private float alpha;
	[Space]
	[SerializeField, Tooltip("The bounds of the area that background blocks can occupy.")] private Bounds _backgroundBlockBounds;
	[SerializeField, Min(0f), Tooltip("The maximum amount of blocks that the can be spawned in.")] private int blockCount;

	private float boundsWidth;
	private float boundsHeight;

	public Bounds BackgroundBlockBounds {
		get {
			_backgroundBlockBounds = new Bounds((Vector2) Camera.main.transform.position, new Vector2(boundsWidth, boundsHeight));

			return _backgroundBlockBounds;
		}
	}

	private void OnValidate ( ) {
		boundsHeight = (maxBlockSize * Mathf.Sqrt(2)) + (Camera.main.orthographicSize * 2f);
		boundsWidth = (maxBlockSize * Mathf.Sqrt(2)) + (Camera.main.aspect * Camera.main.orthographicSize * 2f);
	}

	private void Awake ( ) {
		OnValidate( );
	}

	private void Start ( ) {
		for (int i = 0; i < blockCount; i++) {
			BackgroundBlock backgroundBlock = Instantiate(backgroundBlockPrefab, Vector3.zero, Quaternion.identity).GetComponent<BackgroundBlock>( );
			backgroundBlock.BackgroundBlockSpawner = this;

			CalculateValues(backgroundBlock, true);
		}
	}

	/// <summary>
	/// Set the input background block to a new position (and randomize other values like color, velocity, etc.)
	/// </summary>
	/// <param name="backgroundBlock">The background block to relocate</param>
	/// <param name="spawnInsideBounds">Whether or not to spawn the background block in the inside of the bounds or along the edges of the bounds.</param>
	public void CalculateValues (BackgroundBlock backgroundBlock, bool spawnInsideBounds = false) {
		// Set the block to have a random color
		Color color = Utils.GetColorFromHex(colors[Random.Range(0, colors.Count)]);
		color.a = alpha;
		backgroundBlock.GetComponent<SpriteRenderer>( ).color = color;

		// Get a random position for the background block
		Vector3 position;
		if (spawnInsideBounds) {
			// Spawn the blocks anywhere inside the bounds
			float x = Random.Range(_backgroundBlockBounds.min.x, _backgroundBlockBounds.max.x);
			float y = Random.Range(_backgroundBlockBounds.min.y, _backgroundBlockBounds.max.y);
			position = new Vector3(x, y);
		} else {
			if (Random.Range(0, 2) == 0) {
				// Spawn the block along either the left or right edge of the bounds (off screen)
				float x = (Random.Range(0, 2) == 0 ? _backgroundBlockBounds.min.x : _backgroundBlockBounds.max.x);
				float y = Random.Range(_backgroundBlockBounds.min.y, _backgroundBlockBounds.max.y);
				position = new Vector3(x, y);
			} else {
				// Spawn the block along either the top or bottom edge of the bounds (off screen)
				float y = (Random.Range(0, 2) == 0 ? _backgroundBlockBounds.min.y : _backgroundBlockBounds.max.y);
				float x = Random.Range(_backgroundBlockBounds.min.x, _backgroundBlockBounds.max.x);
				position = new Vector3(x, y);
			}
		}

		// Get a random rotation for the background block
		Quaternion rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

		// Get a random size (scale) for the background block
		float size = Random.Range(minBlockSize, maxBlockSize);

		// Set the position and rotation of the background block
		backgroundBlock.transform.SetParent(transform, true);
		backgroundBlock.transform.SetPositionAndRotation(position, rotation);
		backgroundBlock.transform.localScale = new Vector3(size, size, 1f);

		// Get a random linear velocity for the background block
		Vector2 velocity = Random.insideUnitCircle.normalized;
		bool isRightOfCameraCenter = backgroundBlock.transform.position.x > Camera.main.transform.position.x;
		bool isAboveCameraCenter = backgroundBlock.transform.position.y > Camera.main.transform.position.y;
		velocity.x = Mathf.Abs(velocity.x) * (isRightOfCameraCenter ? -1f : 1f);
		velocity.y = Mathf.Abs(velocity.y) * (isAboveCameraCenter ? -1f : 1f);
		velocity *= Random.Range(minBlockSpeed, maxBlockSpeed);

		// Get a random angular velocity for the background block
		float angularVelocity = Random.Range(minBlockRotateSpeed, maxBlockRotateSpeed);

		// Set the linear and angular velocity of the background block
		backgroundBlock.GetComponent<Rigidbody2D>( ).velocity = velocity;
		backgroundBlock.GetComponent<Rigidbody2D>( ).angularVelocity = angularVelocity;
	}

	/// <summary>
	/// Check to see if the input position is within the bounds of the background
	/// </summary>
	/// <param name="position">The position to check</param>
	/// <returns>Returns true if the position is within the bounds, false otherwise</returns>
	public bool IsWithinBackgroundBounds (Vector3 position) {
		Bounds currentBounds = BackgroundBlockBounds;

		return (
			position.x >= currentBounds.min.x &&
			position.x <= currentBounds.max.x &&
			position.y >= currentBounds.min.y &&
			position.y <= currentBounds.max.y
		);
	}
}