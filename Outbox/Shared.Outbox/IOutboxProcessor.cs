namespace Shared.Outbox
{
    internal interface IOutboxProcessor
    {
        void ProcessOutboxMessages(CancellationToken cancellationToken);
        void RegisterOutboxMessageType(Type type);
    }
}
