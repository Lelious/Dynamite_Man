using UnityEngine;
using Mirror;
using System;
using System.Collections;
using TMPro;

public class Player : NetworkBehaviour, IDamagable, IMapObject
{
    [SerializeField] private Animator _animator;
    [SerializeField] private GameObject _ragdoll;
    [SerializeField] private CharacterController _controller;
    [SerializeField] private TextMeshProUGUI _displayName;
    [SerializeField] private Transform _nameUI;
    [SerializeField] private InputService _inputService;
    [SerializeField] private NetworkMatch _match;
    [SerializeField] private PlayerController _playerController;

    [SyncVar(hook = nameof(HandleDisplayNameChanged))]
    private string _name = "noname";
    private PlayerBombVisualService _bombVisual;
    [SyncVar(hook = nameof(HandleDisplayNameChangedInt))]
    public int Wins;
    public int Kills, Deaths;
    [SyncVar]
    private float _bombExplodeTime;
    [SyncVar]
    [SerializeField] private int _bombCount;
    [SyncVar(hook = nameof(ValidateBombs))]
    private int _maxBombCount;
    [SyncVar]
    private int _bombPower;

    private Transform _reservedSpawnPoint;
    private GameplayService _gameplayService;

    private void Start()
    {
        if (isLocalPlayer && isClient)
        {
            ServiceLocator<IService>.OnServiceRegistered += ValidateService;
        }
        _controller = GetComponent<CharacterController>();
    }

    private void LateUpdate()
    {
        _nameUI.LookAt(_nameUI.position + Camera.main.transform.rotation * Vector3.back, Camera.main.transform.rotation * Vector3.up);
    }

    [Server]
    public int GetBombCount() => _bombCount;

    [Server]
    public void IncreaceSpeed(float value) => _playerController.IncreaceSpeed(value);

    [Server]
    public void SetSpeed(float value) => _playerController.SetSpeed(value);

    [Server]
    public int GetBombPower() => _bombPower;

    private void ValidateService(IService service)
    {
        if (service.GetType() == typeof(InputService))
        {
            ServiceLocator<IService>.Instance.Get<InputService>().SetPlayer(this);
        }
        if (service.GetType() == typeof(PlayerBombVisualService))
        {
            _bombVisual = ServiceLocator<IService>.Instance.Get<PlayerBombVisualService>();
        }
    }

    private void ValidateBombs(int oldValue, int newValue)
    {
        if (isLocalPlayer && isClient)
        {
            _bombVisual.ValidateBombs(newValue);
        }
    }

    [Command]
    public void CmdPlaceBomb()
    {
        _gameplayService.PlaceBomb(this);
    }

    [Server]
    public void SetGameplayService(GameplayService bombService) => _gameplayService = bombService;
    [Server]
    public void SetMatchId(Guid id) => _match.matchId = id;

    [ClientRpc]
    public void InitServices()
    {
        if (isLocalPlayer && isClient)
        {
            var inputService = Instantiate(_inputService);
            inputService.SetPlayer(this);
            ServiceLocator<IService>.Instance.Register(inputService);
            inputService.EnableControll();
            _bombVisual = ServiceLocator<IService>.Instance.Get<PlayerBombVisualService>();
        }
    }

    [TargetRpc]
    private void RpcTakeDamage()
    {
        if (isLocalPlayer && isClient)
        {
            ServiceLocator<IService>.Instance.Get<InputService>().DisableControll();
        }
    }

    [TargetRpc]
    private void RpcStartVisualBombReloading()
    {
        if (isLocalPlayer && isClient)
        {
            if (_bombVisual == null)
            {
                _bombVisual = ServiceLocator<IService>.Instance.Get<PlayerBombVisualService>();
            }

            _bombVisual.StartResettingBomb(_bombExplodeTime);
        }
    }

    [TargetRpc]
    private void RpcValidateBombCount()
    {
        if (_bombVisual == null)
        {
            _bombVisual = ServiceLocator<IService>.Instance.Get<PlayerBombVisualService>();
        }
        _bombVisual.ValidateBombs(_maxBombCount);
    }

    [TargetRpc]
    public void RpcDisableControll()
    {
        if (isLocalPlayer && isClient)
        {
            ServiceLocator<IService>.Instance.Get<InputService>().DisableControll();
        }
    }

    [TargetRpc]
    public void RpcEnableControll()
    {
        ServiceLocator<IService>.Instance.Get<InputService>().EnableControll();
    }

    #region Server
    [ServerCallback]
    public void ReserveSpawnPoint(Transform transform)
    {
        _reservedSpawnPoint = transform;
    }

    [ServerCallback]
    public void ReduseBombCount()
    {
        _bombCount--;        
        RpcStartVisualBombReloading();
    }

    private void HandleDisplayNameChanged(string oldValue, string newValue)
    {
        _displayName.text = $"{_name} ({Wins})";
    }
    private void HandleDisplayNameChangedInt(int oldValue, int newValue)
    {
        _displayName.text = $"{_name} ({Wins})";
    }

    [ServerCallback]
    public void IncreaceMaxBombCount(int addValue)
    {
        var previousBombs = _maxBombCount;
        _maxBombCount = _maxBombCount + addValue > 3 ? 3 : _maxBombCount + addValue;
        _bombCount += _maxBombCount - previousBombs;

        RpcValidateBombCount();
    }

    [ServerCallback]
    public void IncreaceBombCount()
    {
        _bombCount++;
    }

    [ServerCallback]
    public void TakeDamage()
    {
        RpcTakeDamage();
        RpcDisableControll();
        HidePlayer();
    }

    [ServerCallback]
    public void SetStartBombCount(int value)
    {
        _maxBombCount = value;
        _bombCount = _maxBombCount;
        RpcValidateBombCount();
    }

    [ServerCallback]
    public void SetBombPower(int value)
    {
        _bombPower = value;
    }

    [ServerCallback]
    public void IncreaceBombPower(int value)
    {
        _bombPower = _bombPower + value > 7 ? 7 : _bombPower + value;
    }

    [ServerCallback]
    public void SetBombExplodeTime(float time)
    {
        _bombExplodeTime = time;
    }

    [ServerCallback]
    public void SetDisplayName(string name)
    {
        _name = name;
    }

    [ServerCallback]
    public void HidePlayer()
    {
        StartCoroutine(TeleportPlayerRoutine(new Vector3(0f, 500f, 0f)));
    }

    [ServerCallback]
    public void AppearPlayer()
    {
        StartCoroutine(TeleportPlayerRoutine(_reservedSpawnPoint.position));
    }

    private IEnumerator TeleportPlayerRoutine(Vector3 pos)
    {
        _controller.enabled = false;
        yield return new WaitForEndOfFrame();
        transform.position = pos;
        transform.rotation = _reservedSpawnPoint.rotation;
        yield return new WaitForEndOfFrame();
        Debug.Log($"init pos {pos}, current pos {transform.position}");
        _controller.enabled = true;
    }

    [ServerCallback]
    public Vector2Int GetRoundedCoords() => new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
    [ServerCallback]
    public MapObjectType GetMapObjectType() => MapObjectType.Player;
    [ServerCallback]
    public GameObject GetObject() => gameObject;
    [Server]
    public Guid GetMatchGuid() => _match.matchId;
    #endregion
}
