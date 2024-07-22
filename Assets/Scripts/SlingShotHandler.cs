using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class SlingShotHandler : MonoBehaviour
{
    [Header("Line Renderers")]
    [SerializeField] private LineRenderer _leftLineRenderer;
    [SerializeField] private LineRenderer _rightLineRenderer;

    [Header("Transform References")]
    [SerializeField] private Transform _leftStartPosition;
    [SerializeField] private Transform _rightStartPosition;
    [SerializeField] private Transform _centerPosition;
    [SerializeField] private Transform _idlePosition;
    [SerializeField] private Transform _elasticTransform;

    [Header("SlingShot Start")]
    [SerializeField] private float _maxDistance = 3.5f;
    [SerializeField] private float _shotForce = 5f;
    [SerializeField] private float _timeBetweenBirdRespawns = 2f;
    [SerializeField] private float _elasticDivider = 1.2f;
    [SerializeField] private AnimationCurve _elasticCurve;
    [SerializeField] private float _maxAnimationTime = 1f;

    [Header("Scripts")]
    [SerializeField] private SlingShotArea _slingShotArea;
    [SerializeField] private CameraManager _cameraManager;

    [Header("Bird")]
    [SerializeField] private AngieBird _angieBirdPrefab;
    [SerializeField] private float _angieBirdPositionOffset = 2f;

    [Header("Sound")]
    [SerializeField] private AudioClip _elasticPulledClip;
    [SerializeField] private AudioClip[] _elasticReleasedClip;

    private Vector2 _slingShotLinePosition;

    private Vector2 _direction;
    private Vector2 _directionNormalized;

    private bool _clickedWithinArea;
    private bool _birdonSlingshot;

    private AngieBird _spanwedAngieBird;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        _leftLineRenderer.enabled = false;
        _rightLineRenderer.enabled = false;

        SpawnAngieBird();
    }

    private void Update()
    {
        if ( InputManager.WasLeftMosueButtonPressed   && _slingShotArea.IsWithinSlingshotArea())
        {
            _clickedWithinArea = true;

            if(_birdonSlingshot)
            {
                SoundManager.instance.PlayClip(_elasticPulledClip, _audioSource);
                _cameraManager.SwitchToFollowCam(_spanwedAngieBird.transform);
            }
        }

        if (InputManager.IsLeftMousePressed && _clickedWithinArea && _birdonSlingshot)
        {
            DrawSlingShot();
            PositionAndRotateAngieBird();
        }

        if (InputManager.WasLeftMouseButtonReleased && _birdonSlingshot && _clickedWithinArea )
        {
            if (GameManager.instance.HasEnoghtShots())
            {
                _clickedWithinArea = false;
                _birdonSlingshot = false;

                _spanwedAngieBird.LaunchBird(_direction, _shotForce);

                SoundManager.instance.PlayRandomClip(_elasticReleasedClip, _audioSource);

                GameManager.instance.UsedShot();
                AnimateSlingshot();

                if(GameManager.instance.HasEnoghtShots() )
                {
                    
                    StartCoroutine(SpawnAngieBirdAfterTime());

                }
            }
        }
    }

    #region SlingShot Methods

    private void DrawSlingShot()
    {
        Vector3 touchPosition = Camera.main.ScreenToWorldPoint(InputManager.MousePosition);

        _slingShotLinePosition = _centerPosition.position + Vector3.ClampMagnitude(touchPosition - _centerPosition.position, _maxDistance);

        SetLines(_slingShotLinePosition);

        _direction = (Vector2)_centerPosition.position - _slingShotLinePosition;
        _directionNormalized = _direction.normalized;
    }

    private void SetLines(Vector2 position)
    {
        if (!_leftLineRenderer.enabled && !_rightLineRenderer.enabled)
        {
            _leftLineRenderer.enabled = true;
            _rightLineRenderer.enabled = true;
        }

        _leftLineRenderer.SetPosition(0, position);
        _leftLineRenderer.SetPosition(1, _leftStartPosition.position);

        _rightLineRenderer.SetPosition(0, position);
        _rightLineRenderer.SetPosition(1, _rightStartPosition.position);
    }

    #endregion

    #region Angie Bird Methods 

    private void SpawnAngieBird()
    {
        _elasticTransform.DOComplete();
        SetLines(_idlePosition.position);

        Vector2 dir = (_centerPosition.position - _idlePosition.position).normalized;
        Vector2 spawnPosition = (Vector2)_idlePosition.position + dir * _angieBirdPositionOffset;

        _spanwedAngieBird = Instantiate(_angieBirdPrefab, spawnPosition, Quaternion.identity);
        _spanwedAngieBird.transform.right = dir;

        _birdonSlingshot = true;
    }

    private void PositionAndRotateAngieBird()
    {
        _spanwedAngieBird.transform.position = _slingShotLinePosition + _directionNormalized * _angieBirdPositionOffset;
        _spanwedAngieBird.transform.right = _directionNormalized;
    }

    private IEnumerator SpawnAngieBirdAfterTime()
    {
        yield return new WaitForSeconds(_timeBetweenBirdRespawns);

        SpawnAngieBird();

        _cameraManager.SwitchtoIdleCam();
    }
    #endregion

    #region Animate Slingshot

    private void AnimateSlingshot()
    {
        _elasticTransform.position = _leftLineRenderer.GetPosition(0);

        float dist = Vector2.Distance(_elasticTransform.position, _centerPosition.position);

        float time = dist / _elasticDivider;

        _elasticTransform.DOMove(_centerPosition.position, time).SetEase(_elasticCurve);
        StartCoroutine(AnimateSlingShotLine(_elasticTransform, time));  
    }

    private IEnumerator AnimateSlingShotLine(Transform trans, float time)
    {
        float elapsedTime = 0f;
        while (elapsedTime < time && elapsedTime < _maxAnimationTime)
        {
            elapsedTime += Time.deltaTime;

            SetLines(trans.position);

            yield return null;
        }

    }
    #endregion
}
