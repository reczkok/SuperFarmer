using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public static CameraScript Instance { get; private set; }
    private Camera _camera;
    private CameraPosition _currentCameraPosition;
    private CameraPosition _targetCameraPosition;
    private const float LerpTime = 1f;
    private float _currentLerpTime;
    private bool _isLerping;
    
    private void Awake()
    {
        Instance = this;
        _camera = GetComponent<Camera>();
        _currentCameraPosition = new CameraPosition {Position = new Vector3(0, 20, 0), Rotation = Quaternion.Euler(90, 0, 0)};
        _targetCameraPosition = CameraPosition.Player1;
        _isLerping = false;
    }

    private void Start()
    {
        _isLerping = true;
        _currentLerpTime = 0;
        _camera.transform.position = _currentCameraPosition.Position;
    }

    private void Update()
    {
        if (!_isLerping) return;
        _currentLerpTime += Time.deltaTime;
        if (_currentLerpTime > LerpTime)
        {
            _currentLerpTime = LerpTime;
            _isLerping = false;
        }
        float t = _currentLerpTime / LerpTime;
        _camera.transform.position = Vector3.Lerp(_currentCameraPosition.Position, _targetCameraPosition.Position, t);
        _camera.transform.rotation = Quaternion.Lerp(_currentCameraPosition.Rotation, _targetCameraPosition.Rotation, t);
        if (!_isLerping)
        {
            _currentCameraPosition = _targetCameraPosition;
        }
    }
    
    
    public void SetCameraPosition(CameraPositionType cameraPositionType)
    {
        _currentCameraPosition = _targetCameraPosition;
        var currentYRotation = _currentCameraPosition.Rotation.eulerAngles.y;
        var currentPos = _currentCameraPosition.Position;
        _targetCameraPosition = cameraPositionType switch
        {
            CameraPositionType.DiceView => CameraPosition.DiceView,
            CameraPositionType.Player1 => CameraPosition.Player1,
            CameraPositionType.Player2 => CameraPosition.Player2,
            CameraPositionType.Player3 => CameraPosition.Player3,
            CameraPositionType.Player4 => CameraPosition.Player4,
            CameraPositionType.EndGame => CameraPosition.EndGame,
            _ => _targetCameraPosition
        };
        if (cameraPositionType is CameraPositionType.DiceView or CameraPositionType.EndGame)
        {
            _targetCameraPosition.Rotation = Quaternion.Euler(90, currentYRotation, 0);
        }

        _currentLerpTime = 0;
        _isLerping = true;
    }
    
    public void SetCameraToPlayer(int playerNumber)
    {
        SetCameraPosition((CameraPositionType) playerNumber);
    }
    
    public Camera GetCamera()
    {
        return _camera;
    }
}

public enum CameraPositionType
{
    DiceView,
    Player1,
    Player2,
    Player3,
    Player4,
    EndGame
}

public class CameraPosition
{
    public Vector3 Position { get;  set; }
    public Quaternion Rotation { get;  set; }
    
    public static CameraPosition DiceView = new()
    {
        Position = new Vector3(0, 20, 0),
        Rotation = Quaternion.Euler(90, 0, 0)
    };
    
    public static CameraPosition Player3 = new()
    {
        Position = new Vector3(-9, 9, 0),
        Rotation = Quaternion.Euler(65, 90, 0)
    };
    
    public static CameraPosition Player4 = new()
    {
        Position = new Vector3(0, 9, -9),
        Rotation = Quaternion.Euler(65, 0, 0)
    };
    
    public static CameraPosition Player2 = new()
    {
        Position = new Vector3(9, 9, 0),
        Rotation = Quaternion.Euler(65, 270, 0)
    };
    
    public static CameraPosition Player1 = new()
    {
        Position = new Vector3(0, 9, 9),
        Rotation = Quaternion.Euler(65, 180, 0)
    };
    
    public static CameraPosition EndGame = new()
    {
        Position = new Vector3(0, 50, 0),
        Rotation = Quaternion.Euler(90, 0, 0)
    };
}