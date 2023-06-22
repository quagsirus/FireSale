using System;
using UnityEngine;

public class AreaManager : MonoBehaviour
{
    // Which layer's entities we should subscribe to the PlayerEnteredArea event
    public LayerMask agentLayerMask;

    // BoxCollider2D component (located on Awake)
    private BoxCollider2D _boxCollider2D;

    private void Awake()
    {
        // Cache component on spawn
        _boxCollider2D = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        // ReSharper disable once Unity.PreferNonAllocApi
        // because we don't know how many agents there will be

        // Gather all agents in this area
        var agentsInRoom = Physics2D.OverlapBoxAll((Vector2)transform.position + _boxCollider2D.offset,
            _boxCollider2D.size, 0, agentLayerMask);

        // Subscribe all of them to the event that will be triggered when the player enters
        // this saves many GetComponentInParent calls during gameplay which could cause a lag spike
        foreach (var agent in agentsInRoom)
        {
            var agentController = agent.GetComponentInParent<AgentController>();
            // Tell AgentController who we are so they can unsubscribe upon death
            agentController.assignedAreaManager = this;
            // Subscribe to event
            PlayerEnteredArea += agentController.OnPlayerEnteredArea;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check that it was a player that entered
        var otherGameObject = other.gameObject;
        if (!otherGameObject.CompareTag("Player")) return;

        // Notifies all child agents that a player has entered the area, pass the player's GameObject
        PlayerEnteredArea?.Invoke(this, new PlayerEnteredAreaArgs(otherGameObject));

        // Disable collider because we don't need to check for collisions after this
        _boxCollider2D.enabled = false;
    }


    // Define event and arguments
    public event EventHandler<PlayerEnteredAreaArgs> PlayerEnteredArea;

    public class PlayerEnteredAreaArgs : EventArgs
    {
        public PlayerEnteredAreaArgs(GameObject playerGameObject)
        {
            TargetPlayer = playerGameObject;
        }

        public GameObject TargetPlayer { get; private set; }
    }
}