using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundBlockSpawner : MonoBehaviour
{
    [SerializeField, Tooltip("The prefab for the background blocks that will spawn.")] private GameObject prefabBackgroundBlock;
    [Space]
    [SerializeField, Min(0f), Tooltip("The maximum size that a background block can be.")] private float maxBlockSize;
    [SerializeField, Min(0f), Tooltip("The minimum size that a background block can be.")] private float minBlockSize;
    [SerializeField, Min(0f), Tooltip("The maximum speed that a background block can have.")] private float maxBlockSpeed;
    [SerializeField, Min(0f), Tooltip("The minimum speed that a background block can have.")] private float minBlockSpeed;
    [SerializeField, Min(0f), Tooltip("The maximum rotational speed that a background block can have.")] private float maxBlockRotateSpeed;
    [SerializeField, Min(0f), Tooltip("The minimum rotational speed that a background block can have.")] private float minBlockRotateSpeed;
    [SerializeField, Tooltip("A list of all the hex codes that the background blocks can change into. The background blocks will slowly fade to different colors as they move around to add more dynamicness to the background.")] private List<string> colors;
    [SerializeField, Range(0f, 1f), Tooltip("The alpha of the color to set the background blocks to.")] private float alpha;
    [Space]
    [SerializeField, Tooltip("The bounds of the area that background blocks can occupy.")] public Bounds BackgroundBlockBounds;
    [SerializeField, Min(0f), Tooltip("The rate at which to spawn background blocks. This is in blocks per second.")] private float spawnRate;
    [SerializeField, Min(0f), Tooltip("The maximum amount of blocks that the can be spawned in.")] private int maxBlocks;

    private List<BackgroundBlock> disabledBackgroundBlocks;

    private float spawnTimer;

    private int GetActiveBackgroundBlocks
    {
        get => transform.childCount - disabledBackgroundBlocks.Count;
    }

    private void OnValidate()
    {
        float boundsHeight = (maxBlockSize * Mathf.Sqrt(2)) + (Camera.main.orthographicSize * 2f);
        float boundsWidth = (maxBlockSize * Mathf.Sqrt(2)) + (Camera.main.aspect * Camera.main.orthographicSize * 2f);
        BackgroundBlockBounds = new Bounds((Vector2)Camera.main.transform.position, new Vector2(boundsWidth, boundsHeight));
    }

    private void Awake()
    {
        OnValidate();
    }

    private void Start()
    {
        spawnTimer = 0;
        disabledBackgroundBlocks = new List<BackgroundBlock>();

        for (int i = 0; i < maxBlocks; i++)
        {
            SpawnBackgroundBlock(true);
        }
    }

    private void Update()
    {
        // If it is time to spawn a new background block and there are under the max amount of blocks currently active on the screen, then it is valid to spawn a new block
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnRate && GetActiveBackgroundBlocks < maxBlocks)
        {
            SpawnBackgroundBlock();
            spawnTimer -= spawnRate;
        }
    }

    private void SpawnBackgroundBlock(bool spawnInside = false)
    {
        BackgroundBlock newBackgroundBlock;
        // If there are some disabled background blocks, use them instead of spawning new ones
        // If there are no disabled blocks, however, then instantiate a new background block
        if (disabledBackgroundBlocks.Count > 0)
        {
            newBackgroundBlock = disabledBackgroundBlocks[0];
            newBackgroundBlock.gameObject.SetActive(true);
            disabledBackgroundBlocks.RemoveAt(0);
        }
        else
        {
            newBackgroundBlock = Instantiate(prefabBackgroundBlock, Vector3.zero, Quaternion.identity).GetComponent<BackgroundBlock>();
            newBackgroundBlock.BackgroundBlockSpawner = this;
        }

        // Set the block to have a random color
        if (ColorUtility.TryParseHtmlString(colors[Random.Range(0, colors.Count)], out Color color))
        {
            color.a = alpha;
            newBackgroundBlock.GetComponent<SpriteRenderer>().color = color;
        }

        // Get a random position for the background block
        Vector3 position;
        if (spawnInside)
        {
            // Spawn the blocks anywhere inside the bounds
            float x = Random.Range(BackgroundBlockBounds.min.x, BackgroundBlockBounds.max.x);
            float y = Random.Range(BackgroundBlockBounds.min.y, BackgroundBlockBounds.max.y);
            position = new Vector3(x, y);
        }
        else
        {
            if (Random.Range(0, 2) == 0)
            {
                // Spawn the block along either the left or right edge of the bounds (off screen)
                float x = (Random.Range(0, 2) == 0 ? BackgroundBlockBounds.min.x : BackgroundBlockBounds.max.x);
                float y = Random.Range(BackgroundBlockBounds.min.y, BackgroundBlockBounds.max.y);
                position = new Vector3(x, y);
            }
            else
            {
                // Spawn the block along either the top or bottom edge of the bounds (off screen)
                float y = (Random.Range(0, 2) == 0 ? BackgroundBlockBounds.min.y : BackgroundBlockBounds.max.y);
                float x = Random.Range(BackgroundBlockBounds.min.x, BackgroundBlockBounds.max.x);
                position = new Vector3(x, y);
            }
        }

        // Get a random rotation for the background block
        Quaternion rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

        // Get a random size (scale) for the background block
        float size = Random.Range(minBlockSize, maxBlockSize);

        // Set the position and rotation of the background block
        newBackgroundBlock.transform.SetParent(transform, true);
        newBackgroundBlock.transform.SetPositionAndRotation(position, rotation);
        newBackgroundBlock.transform.localScale = new Vector3(size, size, 1f);

        // Get a random linear velocity for the background block
        Vector2 velocity = Random.insideUnitCircle.normalized;
        bool isRightOfCameraCenter = newBackgroundBlock.transform.position.x > Camera.main.transform.position.x;
        bool isAboveCameraCenter = newBackgroundBlock.transform.position.y > Camera.main.transform.position.y;
        velocity.x = Mathf.Abs(velocity.x) * (isRightOfCameraCenter ? -1f : 1f);
        velocity.y = Mathf.Abs(velocity.y) * (isAboveCameraCenter ? -1f : 1f);
        velocity *= Random.Range(minBlockSpeed, maxBlockSpeed);

        // Get a random angular velocity for the background block
        float angularVelocity = Random.Range(minBlockRotateSpeed, maxBlockRotateSpeed);

        // Set the linear and angular velocity of the background block
        newBackgroundBlock.GetComponent<Rigidbody2D>().velocity = velocity;
        newBackgroundBlock.GetComponent<Rigidbody2D>().angularVelocity = angularVelocity;
    }

    /// <summary>
    /// Disable a background block
    /// </summary>
    /// <param name="backgroundBlock">The background block to disable</param>
    public void DisableBackgroundBlock(BackgroundBlock backgroundBlock)
    {
        disabledBackgroundBlocks.Add(backgroundBlock);
        backgroundBlock.gameObject.SetActive(false);
    }
}
