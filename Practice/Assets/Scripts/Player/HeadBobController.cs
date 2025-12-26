using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadBobController : MonoBehaviour
{
    [SerializeField] private bool _enable = true;
    [SerializeField, Range(0, 0.1f)] private float _amplitude;
    [SerializeField, Range(0, 30)] private float _frequency;

    [Header("Walk and Run Settings")]
    [SerializeField, Range(0, 0.1f)] private float _runAmplitude;
    [SerializeField, Range(0, 30)] private float _runFrequency;
    [SerializeField, Range(0, 0.1f)] private float _walkAmplitude;
    [SerializeField, Range(0, 30)] private float _walkFrequency;
    [SerializeField] private Transform _camera;
    [SerializeField] private Transform _cameraHolder;
    private PlayerInput playerInput;

    private float _toggleSpeed = 3.0f;
    private Vector3 _startPos;
    private CharacterController _controller;

    private float _t;
    private Vector3 _smoothVel;
    private float _returnSpeed = 10f;
    private float _rotateLerp = 12f;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        _controller = GetComponent<CharacterController>();
        _startPos = _camera.localPosition;
    }

    private void CheckMotion()
    {
        float speed = new Vector3(_controller.velocity.x, 0, _controller.velocity.z).magnitude;
        if (speed < _toggleSpeed) return;
        if (!_controller.isGrounded) return;
        if (playerInput.sprintHeld)
        {
            _amplitude = _runAmplitude;
            _frequency = _runFrequency;
        }
        else
        {
            _amplitude = _walkAmplitude;
            _frequency = _walkFrequency;
        }
            PlayMotion(FootStepMotion());
    }

    private Vector3 FootStepMotion()
    {
        float s = Mathf.Sin(_t * _frequency);
        float c = Mathf.Cos(_t * _frequency * 0.5f);
        return new Vector3(c * _amplitude * 2f, s * _amplitude, 0f);
    }

    private void ResetPosition()
    {
        _camera.localPosition = Vector3.SmoothDamp(_camera.localPosition, _startPos, ref _smoothVel, 1f / _returnSpeed);
    }

    void Update()
    {
        if (!_enable) return;

        _t += Time.deltaTime;

        CheckMotion();
        ResetPosition();

        if (_cameraHolder != null)
        {
            Vector3 dir = FocusTarget() - _camera.position;
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                _camera.rotation = Quaternion.Slerp(_camera.rotation, targetRot, Time.deltaTime * _rotateLerp);
            }
        }
    }

    private void PlayMotion(Vector3 motion)
    {
        Vector3 target = _startPos + motion;
        _camera.localPosition = Vector3.Lerp(_camera.localPosition, target, Time.deltaTime * _returnSpeed);
    }

    private Vector3 FocusTarget()
    {
        Vector3 pos = new Vector3(transform.position.x, transform.position.y + _cameraHolder.localPosition.y, transform.position.z);
        pos += _cameraHolder.forward * 15.0f;
        return pos;
    }
}
