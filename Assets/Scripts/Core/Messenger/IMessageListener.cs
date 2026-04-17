namespace Core.MessageSystem
{
    public interface IMessageListener { }

    public interface IMessageListener<TMessage> : IMessageListener
    {
        void OnMessage(TMessage message);
    }
}