using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class AIAgent : MonoBehaviour
{
    [Header("Компоненты")]
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private Animator animator;

    [Header("Настройки")]
    [SerializeField] private float searchRadius = 5f;
    [SerializeField] private float idleTime = 5f;

    private IAIState currentState;

    void Awake()
    {
        if (navMeshAgent == null) navMeshAgent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();

        // Сбросим аниматор
        animator.Rebind();
        animator.Update(0f);

        // Начинаем с Idle
        ChangeState(new IdleState());
    }

    void Update()
    {
        currentState?.Update(this);
    }

    public void ChangeState(IAIState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        currentState.Enter(this);

        Debug.Log($"Перешел в состояние: {currentState.StateType}");

        // ПРИНУДИТЕЛЬНОЕ переключение анимации
        ForceAnimationChange();
    }

    private void ForceAnimationChange()
    {
        if (animator == null || currentState == null) return;

        // 1. Устанавливаем параметр
        animator.SetInteger("AIState", (int)currentState.StateType);

        // 2. Прямой вызов анимации по имени
        switch (currentState.StateType)
        {
            case AIStateType.Idle:
                animator.Play("Idle", 0, 0f);
                break;
            case AIStateType.Search:
                animator.Play("Search", 0, 0f);
                break;
            case AIStateType.Collect:
                animator.Play("Collect", 0, 0f);
                break;
        }

        // 3. Форсируем обновление
        animator.Update(0.01f);

        Debug.Log($"Анимация: {currentState.StateType}");
    }

    // Свойства для состояний
    public NavMeshAgent NavAgent => navMeshAgent;
    public Animator CharacterAnimator => animator;
    public float SearchRadius => searchRadius;
    public float IdleTime => idleTime;
    public float IdleTimer { get; set; }
    public GameObject TargetItem { get; set; }
}