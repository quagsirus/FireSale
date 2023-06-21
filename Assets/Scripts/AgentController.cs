using UnityEngine;
using UnityEngine.AI;

public class AgentController : MonoBehaviour
{
    // NavMeshAgent component (located on Awake)
    private NavMeshAgent _navMeshAgent;

    // True when player has entered area collider
    private bool _playerSpotted;

    // Assigned to the player's transform in OnPlayerEnteredArea
    private Transform _playerTransform;

    private void Awake()
    {
        // Cache NavMeshAgent component on spawn
        _navMeshAgent = GetComponent<NavMeshAgent>();

        // Prevent unity updating our position, because it's designed for 3d it moves us on the z axis
        _navMeshAgent.updatePosition = false;
    }

    private void Update()
    {
        // Remain dormant until player has been spotted
        if (!_playerSpotted) return;

        // Update destination to current player position
        _navMeshAgent.destination = _playerTransform.position;

        // Update current agent position to calculated next location, without z axis
        var nextPosition = _navMeshAgent.nextPosition;
        transform.position = new Vector3(nextPosition.x, nextPosition.y, 0);
    }

    // Subscribed to PlayerEnteredArea event in AreaManager
    public void OnPlayerEnteredArea(object sender, AreaManager.PlayerEnteredAreaArgs e)
    {
        _playerSpotted = true;
        _playerTransform = e.TargetPlayer.transform;
    }
}