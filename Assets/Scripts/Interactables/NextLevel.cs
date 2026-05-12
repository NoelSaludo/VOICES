using UnityEngine;

class NextLevel : MonoBehaviour, IPlayerInteractable
{
    [SerializeField] private string interactionVerb = "Enter";
    [SerializeField] private string nextSceneName;

    public string InteractionVerb => interactionVerb;
    public Transform PromptAnchor => transform;

    public void Interact(Player player)
    {
        if (player == null || string.IsNullOrEmpty(nextSceneName))
        {
            return;
        }

        GameManager.Instance.LoadScene(nextSceneName);
     }
}