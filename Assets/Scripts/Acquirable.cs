using UnityEngine;
using UnityEngine.Events;

namespace NodeTree
{
    public class Acquirable : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject indicator;
        [SerializeField] private UnityEvent onAcquired;

        private void Awake()
        {
            if (indicator != null)
                indicator.SetActive(false);
        }

        public void OnFocused()
        {
            if (indicator != null)
                indicator.SetActive(true);
        }

        public void OnUnfocused()
        {
            if (indicator != null)
                indicator.SetActive(false);
        }

        public void Interact()
        {
            onAcquired.Invoke();
            Destroy(gameObject);
        }
    }
}
