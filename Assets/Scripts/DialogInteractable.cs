using System.Collections;
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
            Debug.Log($"[Interact] {gameObject.name} BEFORE trigger — hasSeenComputer={ConditionContext.GetBool("hasSeenComputer")}");
            UIManager.Instance.ShowDialog();
            trigger.Interact();
            Debug.Log($"[Interact] {gameObject.name} AFTER trigger — hasSeenComputer={ConditionContext.GetBool("hasSeenComputer")}");
            StartCoroutine(WaitForDialogEnd());
        }

        private IEnumerator WaitForDialogEnd()
        {
            yield return new WaitUntil(() => !DialogRunner.Instance.IsDialogActive);
            UIManager.Instance.ShowHUD();
        }
    }
}