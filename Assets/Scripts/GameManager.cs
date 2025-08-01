using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int MaxNumberOfShots = 3;
    [SerializeField] private float _secondToWaitBeforeDeathCheck = 3f;
    [SerializeField] private GameObject _restartScreenObject;
    [SerializeField] private SlingShotHandler _slingShotHandler;
    [SerializeField] private Image _nextLevelImage;

    private int _usedNumberOfShot;

    private IconHandler _iconHandler;

    private List<Baddie> _baddies = new List<Baddie>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        _iconHandler = FindObjectOfType<IconHandler>();

        Baddie[] baddies = FindObjectsOfType<Baddie>();
        for (int i = 0; i < baddies.Length; i++)
        {
            _baddies.Add(baddies[i]); 
        }

        _nextLevelImage.enabled = false;
    }
    
    public void UsedShot()
    {
        _usedNumberOfShot++;
        _iconHandler.UsedShot(_usedNumberOfShot);

        CheckForLastShot();
    }

    public bool HasEnoghtShots()
    {
        if (_usedNumberOfShot < MaxNumberOfShots)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void CheckForLastShot()
    {
        if (_usedNumberOfShot == MaxNumberOfShots)
        {
            StartCoroutine(CheckAfterWaitTime());  
        }
    }

    private IEnumerator CheckAfterWaitTime()
    {
        yield return new WaitForSeconds(_secondToWaitBeforeDeathCheck);

        if(_baddies.Count == 0)
        {
            WinGame();
        }
        else
        {
            RestartGame();
        }

    }

    public void RempoveBaddies( Baddie baddie)
    {
        _baddies.Remove(baddie);
        CheckForAllDeadBaddies();
    }

    private void CheckForAllDeadBaddies()
    {
        if( _baddies.Count == 0)
        {
            WinGame();
        }
    }

    #region Win/Lose

    private void WinGame()
    {
        _restartScreenObject.SetActive(true);
        _slingShotHandler.enabled = false;

        int CurrentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int maxLevels = SceneManager.sceneCountInBuildSettings;
        if( CurrentSceneIndex +1 < maxLevels )
        {
            _nextLevelImage.enabled = true;
        }    
    }

    public void RestartGame()
    {
        DOTween.Clear(true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    #endregion
}


