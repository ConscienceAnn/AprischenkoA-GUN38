public interface IAIState
{
    AIStateType StateType { get; }

    void Enter(AIAgent agent);
    void Update(AIAgent agent);
    void Exit(AIAgent agent);
}
