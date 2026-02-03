using DG.Tweening;
using UnityEngine;

public class PathChoiceManager : MonoBehaviour
{
    [Header("Точки спавна на сцене")]
    public Transform spawnLeft;
    public Transform spawnRight;

    [Header("Префабы дорожек")]
    public GameObject pathWithCoinsPrefab;
    public GameObject pathWithObstaclesPrefab;

    [Header("Персонаж")]
    public GameObject player;
    public CharacterMovement characterMovement;

    [Header("Game Controller")] 
    public GameController gameController;

    [Header("Настройки анимации")]
    public float fadeDuration = 0.5f;
    public float pathAppearDuration = 0.7f;

    private GameObject leftPathInstance;
    private GameObject rightPathInstance;
    private Renderer leftGateRenderer;
    private Renderer rightGateRenderer;
    private GameObject chosenPath;
    private bool isLeftChosen;

    void Start()
    {
        FindGates();
        HideAllPaths();
    }

    void FindGates()
    {
        if (spawnLeft.childCount > 0)
            leftGateRenderer = spawnLeft.GetChild(0).GetComponent<Renderer>();

        if (spawnRight.childCount > 0)
            rightGateRenderer = spawnRight.GetChild(0).GetComponent<Renderer>();
    }

    public void ShowChoice()
    {
        if (leftGateRenderer != null)
            leftGateRenderer.gameObject.SetActive(true);

        if (rightGateRenderer != null)
            rightGateRenderer.gameObject.SetActive(true);

        AnimateGatesAppearance();
    }

    public void OnPathChosen(bool chooseLeft)
    {
        isLeftChosen = chooseLeft;
        FadeOutGates();
        Invoke("SpawnPaths", fadeDuration);
    }

    void FadeOutGates()
    {
        if (leftGateRenderer != null)
        {
            Material leftMat = leftGateRenderer.material;
            leftMat.DOFade(0f, fadeDuration)
                .OnComplete(() => leftGateRenderer.gameObject.SetActive(false));
        }

        if (rightGateRenderer != null)
        {
            Material rightMat = rightGateRenderer.material;
            rightMat.DOFade(0f, fadeDuration)
                .OnComplete(() => rightGateRenderer.gameObject.SetActive(false));
        }
    }

    void SpawnPaths()
    {
        DestroyOldPaths();

        bool coinsOnLeft = Random.value > 0.5f;

        leftPathInstance = Instantiate(
            coinsOnLeft ? pathWithCoinsPrefab : pathWithObstaclesPrefab,
            spawnLeft.position,
            Quaternion.identity
        );

        rightPathInstance = Instantiate(
            coinsOnLeft ? pathWithObstaclesPrefab : pathWithCoinsPrefab,
            spawnRight.position,
            Quaternion.identity
        );

        SetPathTypes(leftPathInstance, coinsOnLeft);
        SetPathTypes(rightPathInstance, !coinsOnLeft);

        AnimatePathsAppearance();
        Invoke("StartCharacterMovement", pathAppearDuration + 0.3f);
    }

    void SetPathTypes(GameObject path, bool isCoinPath)
    {
        PathContainer container = path.GetComponent<PathContainer>();
        if (container != null)
        {
            container.pathType = isCoinPath ?
                PathContainer.PathType.Coins :
                PathContainer.PathType.Obstacles;
        }
    }

    void AnimatePathsAppearance()
    {
        if (leftPathInstance != null)
        {
            leftPathInstance.transform.localScale = Vector3.zero;
            leftPathInstance.transform.DOScale(Vector3.one, pathAppearDuration)
                .SetEase(Ease.OutBack);
        }

        if (rightPathInstance != null)
        {
            rightPathInstance.transform.localScale = Vector3.zero;
            rightPathInstance.transform.DOScale(Vector3.one, pathAppearDuration)
                .SetEase(Ease.OutBack);
        }
    }

    void StartCharacterMovement()
    {
        chosenPath = isLeftChosen ? leftPathInstance : rightPathInstance;

        PathContainer pathContainer = chosenPath.GetComponent<PathContainer>();
        if (pathContainer == null)
        {
            Debug.LogError("На дорожке нет PathContainer!");
            return;
        }

        if (characterMovement != null)
        {
            characterMovement.MoveAlongPath(
                pathContainer.startPoint,
                pathContainer.endPoint
            );

            characterMovement.OnPathCompleted += OnCharacterReachedEnd;
        }
    }

    void OnCharacterReachedEnd() 
    {
        Debug.Log("Персонаж дошёл до конца!");

        
        if (characterMovement != null)
            characterMovement.OnPathCompleted -= OnCharacterReachedEnd;

       
        if (gameController != null)
        {
            gameController.OnPathCompleted();
        }
        else
        {
            Debug.LogError("GameController не назначен в PathChoiceManager!");
        }
    }

    void DestroyOldPaths()
    {
        if (leftPathInstance != null) Destroy(leftPathInstance);
        if (rightPathInstance != null) Destroy(rightPathInstance);
    }

    void HideAllPaths()
    {
        if (leftPathInstance != null) leftPathInstance.SetActive(false);
        if (rightPathInstance != null) rightPathInstance.SetActive(false);
    }

    void AnimateGatesAppearance()
    {
        if (leftGateRenderer != null)
        {
            leftGateRenderer.material.color = new Color(
                leftGateRenderer.material.color.r,
                leftGateRenderer.material.color.g,
                leftGateRenderer.material.color.b,
                0f
            );

            leftGateRenderer.material.DOFade(1f, 0.5f);
        }

        if (rightGateRenderer != null)
        {
            rightGateRenderer.material.color = new Color(
                rightGateRenderer.material.color.r,
                rightGateRenderer.material.color.g,
                rightGateRenderer.material.color.b,
                0f
            );

            rightGateRenderer.material.DOFade(1f, 0.5f);
        }
    }

    public void ClearPaths()
    {
        if (player != null)
        {
            player.transform.position = new Vector3(1f, 0, -2f);
        }
        if (leftPathInstance != null)
        {
            Destroy(leftPathInstance);
            leftPathInstance = null;
        }

        if (rightPathInstance != null)
        {
            Destroy(rightPathInstance);
            rightPathInstance = null;
        }

        if (leftGateRenderer != null)
        {
            leftGateRenderer.gameObject.SetActive(true);
            Material leftMat = leftGateRenderer.material;
            leftMat.color = new Color(leftMat.color.r, leftMat.color.g, leftMat.color.b, 1f);
        }

        if (rightGateRenderer != null)
        {
            rightGateRenderer.gameObject.SetActive(true);
            Material rightMat = rightGateRenderer.material;
            rightMat.color = new Color(rightMat.color.r, rightMat.color.g, rightMat.color.b, 1f);
        }

        Debug.Log("Все дорожки очищены, готово к новому раунду!");
    }
}