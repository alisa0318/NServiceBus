using Common.Logging;
using NServiceBus.Messages;

namespace NServiceBus.Unicast.Distributor
{
    public class ReadyMessageHandler : IMessageHandler<ReadyMessage>
    {
        public IBus Bus { get; set; }

        public void Handle(ReadyMessage message)
        {
            logger.Debug("Server available: " + this.Bus.SourceOfMessageBeingHandled);

            if (message.ClearPreviousFromThisAddress) //indicates worker started up
                this.workerManager.ClearAvailabilityForWorker(this.Bus.SourceOfMessageBeingHandled);

            this.workerManager.WorkerAvailable(this.Bus.SourceOfMessageBeingHandled);
        }

        private IWorkerAvailabilityManager workerManager;
        public virtual IWorkerAvailabilityManager WorkerManager
        {
            set { workerManager = value; }
        }


        private readonly static ILog logger = LogManager.GetLogger(typeof(ReadyMessageHandler));
    }
}
