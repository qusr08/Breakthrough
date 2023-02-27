using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundBlock : MonoBehaviour
{
    [SerializeField, Tooltip("A reference to the background block spawner in the scene.")] public BackgroundBlockSpawner BackgroundBlockSpawner;

    private void Update()
    {
        if (transform.position.x < BackgroundBlockSpawner.BackgroundBlockBounds.min.x || 
            transform.position.x > BackgroundBlockSpawner.BackgroundBlockBounds.max.x || 
            transform.position.y < BackgroundBlockSpawner.BackgroundBlockBounds.min.y || 
            transform.position.y > BackgroundBlockSpawner.BackgroundBlockBounds.max.y)
        {
            BackgroundBlockSpawner.DisableBackgroundBlock(this);
        }
    }
}
