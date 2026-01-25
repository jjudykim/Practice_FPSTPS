using UnityEngine;

namespace jjudy
{
    public class Weapon : MonoBehaviour
    {
        public GameObject Model;
        
        public Transform AttachPoint;

        [Range(0.0f, 1.0f)] public float ikWeight;

        public void SetActive(bool active)
        {
            if (Model != null)
                Model.SetActive(active);
        }
    }
}

