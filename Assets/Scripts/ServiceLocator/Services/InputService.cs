using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputService : ServiceBase
{
    [SerializeField] private Joystick _joystick;
    [SerializeField] private Button _bombButton;
    [SerializeField] private List<GameObject> _visuals = new List<GameObject>();
    [SerializeField] private PlayerBombVisualService _bombVisual;

    private bool _disableControll = true;
    private PlayerController _controller;
    private BombService _bombService;
    private Player _player;
    private Vector3 _movementInput;

    private void Awake()
    {
        _bombButton.onClick.AddListener(PlaceBomb);
    }

    private void Update()
    {
        if (_controller== null) return;
        if (_disableControll) return;

        MoveInput();
    }

    public void SetPlayerController(PlayerController controller)
    {
        _controller = controller;
    }

    public void SetPlayer(Player player)
    {
        _player = player;
        _bombService = _player.GetBombService();
    }

    public void DisableControll()
    {
        _disableControll = true;
        _movementInput = Vector3.zero;
        _joystick.SetToZero();
        foreach (var item in _visuals)
        {
            item.SetActive(false);
        }
    }

    public PlayerBombVisualService GetBombVisual() => _bombVisual;

    public void EnableControll()
    {
        _disableControll = false;

        foreach (var item in _visuals)
        {
            item.SetActive(true);
        }
    }

    private void PlaceBomb()
    {
        if (_player == null) return;
        if (_disableControll) return;

        _bombService.CmdPlaceBomb(_player);
    }

    private void MoveInput()
    {
        _movementInput.x = _joystick.Horizontal;
        _movementInput.z = _joystick.Vertical;
        _controller.CmdMovePlayer(_movementInput);
        _controller.SetMovementVector(_movementInput);
    }
}
