using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private AudioClip doorOpenSound;
    [SerializeField] private LayerMask playerLayer;

    private Animator animator;
    private AudioSource audioSource;
    private bool isTransitioning = false;

    private void Awake()
    {
        Debug.Log("=== АWAKE: Начинаем инициализацию ===");

        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // ПРОВЕРКА 1: Есть ли Animator?
        if (animator != null)
        {
            Debug.Log($"Animator найден на объекте {gameObject.name}");
            Debug.Log($"  - Controller: {(animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "НЕТ!")}");
            Debug.Log($"  - Layer count: {animator.layerCount}");
            Debug.Log($"  - Parameter count: {animator.parameterCount}");

            // Выводим все параметры аниматора
            for (int i = 0; i < animator.parameterCount; i++)
            {
                AnimatorControllerParameter param = animator.GetParameter(i);
                Debug.Log($"  - Параметр {i}: {param.name}, тип: {param.type}");
            }

            // Специальная проверка параметра "Open"
            bool hasOpenParam = false;
            for (int i = 0; i < animator.parameterCount; i++)
            {
                if (animator.GetParameter(i).name == "Open")
                {
                    hasOpenParam = true;
                    break;
                }
            }

            if (hasOpenParam)
            {
                Debug.Log("Параметр 'Open' найден в аниматоре!");
            }
            else
            {
                Debug.LogError(" Параметр 'Open' НЕ НАЙДЕН в аниматоре! Создайте параметр типа Trigger с именем 'Open'");
            }

            // Сбрасываем анимацию
            animator.ResetTrigger("Open");
            animator.Play("Idle", 0, 0f);
            Debug.Log(" Анимация сброшена в Idle");
        }
        else
        {
            Debug.LogError($"Animator НЕ НАЙДЕН на объекте {gameObject.name}! Добавьте компонент Animator");
        }

        // ПРОВЕРКА 2: AudioSource
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("AudioSource создан автоматически");
        }
        else
        {
            Debug.Log("AudioSource найден");
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;

        // ПРОВЕРКА 3: Collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Debug.Log($"Collider найден, isTrigger: {col.isTrigger}");
        }
        else
        {
            Debug.LogError("Collider НЕ НАЙДЕН! Добавьте коллайдер с isTrigger = true");
        }

        // ПРОВЕРКА 4: AudioClip
        if (doorOpenSound != null)
        {
            Debug.Log($"AudioClip найден: {doorOpenSound.name}, длина: {doorOpenSound.length} сек");
        }
        else
        {
            Debug.Log("AudioClip не назначен, звук не будет проигрываться");
        }

        Debug.Log("=== АWAKE: Инициализация завершена ===");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"=== OnTriggerEnter: {other.gameObject.name}, тег: {other.tag} ===");

        // Проверяем, что объект на слое игрока
        if ((playerLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            Debug.Log($"Объект на слое игрока: {other.gameObject.name}");

            if (!isTransitioning)
            {
                OpenDoor();
            }
        }
        else
        {
            Debug.Log($"Не игрок, слой: {LayerMask.LayerToName(other.gameObject.layer)}");
        }
    }

    private void OpenDoor()
    {
        Debug.Log("=== OpenDoor() ===");
        isTransitioning = true;
        Debug.Log("isTransitioning = true");

        // Анимация
        if (animator != null)
        {
            Debug.Log("Пытаемся вызвать SetTrigger('Open')");

            // Проверяем текущее состояние перед вызовом
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"  - Текущее состояние: {currentState.fullPathHash}, length: {currentState.length}, normalizedTime: {currentState.normalizedTime}");
            Debug.Log($"  - Имя состояния: {GetStateName(animator, currentState.shortNameHash)}");

            // ВЫЗЫВАЕМ ТРИГГЕР
            animator.SetTrigger("Open");
            Debug.Log("SetTrigger('Open') выполнен");

            // Проверяем состояние ПОСЛЕ вызова
            AnimatorStateInfo newState = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"  - Состояние после триггера: {newState.fullPathHash}, length: {newState.length}");

            // Даем время аниматору обработать триггер
            StartCoroutine(CheckAnimationState());
        }
        else
        {
            Debug.LogError("animator = null! Анимация не проиграется");
        }

        // Звук
        if (audioSource != null && doorOpenSound != null)
        {
            Debug.Log($"Проигрываем звук: {doorOpenSound.name}");
            audioSource.PlayOneShot(doorOpenSound);
        }
        else
        {
            Debug.Log("Звук не проиграется: " +
                     (audioSource == null ? "AudioSource null, " : "") +
                     (doorOpenSound == null ? "AudioClip null, " : ""));
        }

        // Отключаем коллайдер
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
            Debug.Log("Коллайдер отключен");
        }

        Debug.Log("Запускаем корутину WaitForAnimation()");
        StartCoroutine(WaitForAnimation());
    }

    private System.Collections.IEnumerator CheckAnimationState()
    {
        yield return new WaitForSeconds(0.1f); // Ждем кадр

        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"=== ПРОВЕРКА через 0.1 сек ===");
            Debug.Log($"  - Состояние сейчас: {stateInfo.fullPathHash}, length: {stateInfo.length}");
            Debug.Log($"  - Имя состояния: {GetStateName(animator, stateInfo.shortNameHash)}");

            // Проверяем, изменилось ли состояние
            if (stateInfo.fullPathHash == 0)
            {
                Debug.LogError(" Состояние не изменилось! Триггер не сработал");
            }
            else
            {
                Debug.Log("Состояние изменилось, анимация должна проигрываться");
            }
        }
    }

    private string GetStateName(Animator anim, int hash)
    {
        // Этот метод пытается получить имя состояния по хэшу
        // В Unity нет прямого способа, но для отладки попробуем:
        if (anim.runtimeAnimatorController != null)
        {
            return $"Hash: {hash} (имя не определено)";
        }
        return "No Controller";
    }

    private System.Collections.IEnumerator WaitForAnimation()
    {
        Debug.Log("=== WaitForAnimation() ===");

        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float animationLength = stateInfo.length;
            Debug.Log($"  - Длина анимации: {animationLength} сек");
            Debug.Log($"  - normalizedTime: {stateInfo.normalizedTime}");

            if (animationLength > 0)
            {
                Debug.Log($"Ждем {animationLength} секунд...");
                yield return new WaitForSeconds(animationLength);
                Debug.Log("Ожидание завершено");
            }
            else
            {
                Debug.LogWarning(" Длина анимации = 0, ждем 1 секунду");
                yield return new WaitForSeconds(1f);
            }
        }
        else
        {
            Debug.LogWarning(" animator = null, ждем 1 секунду");
            yield return new WaitForSeconds(1f);
        }
        yield return new WaitForSeconds(1.5f);

        Debug.Log($"Вызываем TransitionToScene({targetSceneName})");
        GameManager.Instance.TransitionToScene(targetSceneName);
        Destroy(gameObject);
    }

    // Для проверки параметров в инспекторе во время игры
    private void Update()
    {
        // Нажмите P в игре, чтобы проверить состояние аниматора
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("=== МАНУАЛЬНАЯ ПРОВЕРКА ===");
            if (animator != null)
            {
                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
                Debug.Log($"Текущее состояние: hash={state.fullPathHash}, length={state.length}");

                // Пробуем вызвать триггер вручную
                Debug.Log("Вызываем SetTrigger('Open') вручную...");
                animator.SetTrigger("Open");
            }
        }
    }
}