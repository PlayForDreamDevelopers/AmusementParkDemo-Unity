using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float destroyTime;

    private void Start() { Destroy(gameObject, destroyTime); }
}