using UnityEngine;

namespace NodeTree
{
    [RequireComponent(typeof(NodeTreeTrigger))]
    public class DialogInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject indicator;

        private NodeTreeTrigger trigger;

        private void Awake()
        {
            trigger = GetComponent<NodeTreeTrigger>();
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
            trigger.Interact();
        }
    }
}