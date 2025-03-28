using UnityEngine;

namespace PFDM.Demos.AmusementPark
{
    public class AutoDestroy : MonoBehaviour
    {
        public float destroyTime;

        private void Start() { Destroy(gameObject, destroyTime); }
    }
}