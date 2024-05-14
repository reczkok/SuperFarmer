using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalDice : MonoBehaviour
{
    [field: SerializeField]
    public DiceFace CurrentFace { get; private set; }
    public bool IsRolling => _rigidbody.velocity.magnitude > 0.1f || _rigidbody.angularVelocity.magnitude > 0.1f;
    public bool debugThrow = false;
    public event Action<DiceFace> OnDiceStoppedRolling;
    private readonly List<DiceFaceDetector> _faces = new List<DiceFaceDetector>();
    private Rigidbody _rigidbody;
    private IEnumerator _updateCurrentFaceWhenStopped;
    private int _iterationsNotMoving = 0;

    public void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        var faces = GetComponentsInChildren<DiceFaceDetector>();
        _faces.AddRange(faces);
        UpdateCurrentFace();
    }
    
    public void Update()
    {
        if (!debugThrow) return;
        debugThrow = false;
        Throw();
    }
    
    public void Throw()
    {
        var force = UnityEngine.Random.Range(5f, 10f);
        var torquex = UnityEngine.Random.Range(5f, 10f);
        var torquey = UnityEngine.Random.Range(5f, 10f);
        var torquez = UnityEngine.Random.Range(5f, 10f);
        
        _rigidbody.AddForce(Vector3.up * force, ForceMode.Impulse);
        _rigidbody.AddTorque(torquex, torquey, torquez, ForceMode.Impulse);
        
        if (_updateCurrentFaceWhenStopped != null)
        {
            StopCoroutine(_updateCurrentFaceWhenStopped);
        }
        _updateCurrentFaceWhenStopped = UpdateCurrentFaceWhenStopped();
        StartCoroutine(_updateCurrentFaceWhenStopped);
    }
    
    private IEnumerator UpdateCurrentFaceWhenStopped()
    {
        yield return new WaitForSeconds(0.5f);
        while (IsRolling || _iterationsNotMoving < 10)
        {
            if (!IsRolling)
            {
                _iterationsNotMoving++;
            }
            else
            {
                _iterationsNotMoving = 0;
            }
            yield return new WaitForSeconds(0.1f);
        }
        UpdateCurrentFace();
        OnDiceStoppedRolling?.Invoke(CurrentFace);
    }
    
    private void UpdateCurrentFace()
    {
        var highestY = float.MinValue;
        DiceFaceDetector highestFace = null;
        if(_faces.Count == 0) throw new Exception("No faces found");
        foreach (var face in _faces)
        {
            var y = face.GetYPositionGlobal();
            if (y <= highestY) continue;
            highestY = y;
            highestFace = face;
        }
        CurrentFace = highestFace!.DiceFace;
    }
}
