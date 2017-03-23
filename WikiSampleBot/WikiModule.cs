namespace WikiSampleBot
{
    using Autofac;
    using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Builder.Internals.Fibers;
    using WikiSampleBot.Services;

    public class WikiModule : Module
    {
        private readonly string qnaMakerSubscriptionKey;
        private readonly string qnaMakerKnowledgeBaseId;
        private string defaultMessage;
        private double threshold;

        public WikiModule(string qnaMakerSubscriptionKey, string qnaMakerKnowledgeBaseId, string defaultMessage, double threshold)
        {
            SetField.NotNull(out this.qnaMakerSubscriptionKey, nameof(qnaMakerSubscriptionKey), qnaMakerSubscriptionKey);
            SetField.NotNull(out this.qnaMakerKnowledgeBaseId, nameof(qnaMakerKnowledgeBaseId), qnaMakerKnowledgeBaseId);

            this.threshold = threshold;
            this.defaultMessage = defaultMessage;
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder
                .Register(c => new QnAMakerAttribute(this.qnaMakerSubscriptionKey, this.qnaMakerKnowledgeBaseId, this.defaultMessage, this.threshold))
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder
                .RegisterType<QnAMakerService>()
                .Keyed<IQnAService>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .SingleInstance();

            builder
              .Register(c => new UpdateQnAKnowledgeBaseService(this.qnaMakerSubscriptionKey, this.qnaMakerKnowledgeBaseId))
              .AsSelf()
              .SingleInstance();

            builder
                .Register(c => new WikiScorable(
                    new QnAMakerScorable(
                        new QnAMakerServiceScorable(c.Resolve<IQnAService>(), c.Resolve<IBotToUser>()), c.Resolve<ITraits<double>>()), 
                    c.Resolve<IDialogStack>(),
                    c.Resolve<IBotData>()))
                .AsImplementedInterfaces()
                .InstancePerMatchingLifetimeScope(DialogModule.LifetimeScopeTag);
        }
    }
}