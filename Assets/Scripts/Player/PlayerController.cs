using UnityEngine;
using Mirror;
using System.Collections;

public sealed class PlayerController : NetworkBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private CharacterController _characterController;


    [SyncVar]
    [SerializeField] private float _walkSpeed;

    private Vector3 _direction;
    private float _moveFactor;
    private bool _isInited;

    [ClientRpc]
    public void InitController()
    {
        if (_isInited) return;

        if (isLocalPlayer && isClient)
        {
            ServiceLocator<IService>.Instance.Get<InputService>().SetPlayerController(this);
        }
    }

    [ClientCallback]
    private void Update()
    {
        if (_direction != Vector3.zero)
        {
            MovePlayer();
        }
    }

    [Client]
    public void MovePlayer()
    {
        _characterController.Move(_walkSpeed * Time.fixedDeltaTime * _direction);
        transform.rotation = Quaternion.LookRotation(_direction, Vector3.up);
    }

    [Client]
    public void SetMovementVector(Vector3 direction)
    {
        _direction = direction.normalized;
    }

    #region Server

    [ServerCallback]
    private void FixedUpdate()
    {
        _moveFactor = _direction.magnitude;
        UpdateAnimator(_direction);
    }

    [Command]
    public void CmdMovePlayer(Vector3 direction)
    {
        _direction = direction.normalized;
    }

    [ServerCallback]
    public void UpdateAnimator(Vector3 velocity)
    {
        if (velocity == Vector3.zero)
        {
            _moveFactor -= Time.deltaTime * 5f;
        }
        _animator.SetFloat("Speed", _moveFactor);
    }

    [ServerCallback]
    public void SetSpeed(float speed) => _walkSpeed = speed;
    #endregion
}
