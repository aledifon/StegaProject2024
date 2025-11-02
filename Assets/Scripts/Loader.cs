using UnityEngine;

public class Loader : MonoBehaviour
{
    // I'll add the GameManager Prefab from the Inspector
    [SerializeField] private GameObject gameManagerPrefab;

    private void Awake()
    {
        if (GameManager.Instance == null)
        {
            GameObject gm = Instantiate(gameManagerPrefab);
            gm.name = "GameManager";
        }
    }
}
