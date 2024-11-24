using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    [SerializeField] private GameEvent e_OnPlayerSpawnEvent;
    [SerializeField] private GameEvent e_OnPlayerDespawnEvent;

    private void OnEnable()
    {
        e_OnPlayerSpawnEvent.Raise(this, this.gameObject);
    }

    private void OnDisable()
    {
        e_OnPlayerSpawnEvent.Raise(this, this.gameObject);
    }

    private void Awake()
    {
        e_OnPlayerSpawnEvent.Raise(this, this.gameObject);
    }
}
