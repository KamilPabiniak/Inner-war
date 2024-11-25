using System.Collections;
using UnityEngine;

public class Climb : MonoBehaviour
{
    private Player.Player _player;

    public float bottomRaycastDistance = 0.5f; // Odległość raycasta w dół
    public float topRaycastAngle = 15f; // Kąt raycasta w górę
    public float entryCooldown = 1f; // Czas cooldownu po wejściu na drabinę

    private bool _entryCooldownActive = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _player = other.GetComponent<Player.Player>();
        if (_player == null) return;

        AlignToLadder();
        _player.ToggleGravity();
        _player.state = Player.Player.State.Climbing;

        // Aktywujemy cooldown
        _entryCooldownActive = true;
        StartCoroutine(ResetEntryCooldown());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player") || _player == null) return;

        _player.ToggleGravity();
        _player.state = Player.Player.State.Walking;
    }

    private void AlignToLadder()
    {
        Quaternion ladderRotation = Quaternion.Euler(0, transform.eulerAngles.y + 180, 0);
        _player.transform.rotation = ladderRotation;

        Vector3 alignedPosition = _player.transform.position;
        alignedPosition.x = transform.position.x;
        alignedPosition.z = transform.position.z;
        _player.transform.position = alignedPosition;
    }

    private IEnumerator ResetEntryCooldown()
    {
        yield return new WaitForSeconds(entryCooldown);
        _entryCooldownActive = false;
    }

    private void Update()
    {
        if (_player != null && _player.state == Player.Player.State.Climbing)
        {
            if (!_entryCooldownActive)
            {
                HandleBottomExit();
            }

            HandleTopExit();
        }
    }

    private void HandleBottomExit()
    {
        Ray ray = new Ray(_player.transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, bottomRaycastDistance))
        {
            ExitLadderAtBottom();
        }
    }

    private void HandleTopExit()
    {
        Vector3 direction = Quaternion.Euler(-topRaycastAngle, 0, 0) * _player.transform.forward;
        Ray ray = new Ray(_player.transform.position, direction);

        if (!Physics.Raycast(ray, out RaycastHit hit, 2f)) // Długość raycasta do dostosowania
        {
            ExitLadderAtTop();
        }
    }

    private void ExitLadderAtTop()
    {
        _player.state = Player.Player.State.Walking;
        _player.ToggleGravity();
        Vector3 exitPosition = _player.transform.position + Vector3.forward * 0.5f;
        _player.transform.position = exitPosition;
    }

    private void ExitLadderAtBottom()
    {
        _player.state = Player.Player.State.Walking;
        _player.ToggleGravity();
        Vector3 exitPosition = _player.transform.position - Vector3.forward * 0.5f;
        _player.transform.position = exitPosition;
    }
}
