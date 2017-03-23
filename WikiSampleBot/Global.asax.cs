namespace WikiSampleBot
{
    using System.Configuration;
    using System.Web.Http;
    using Autofac;
    using Microsoft.Bot.Builder.Dialogs;
    using WikiSampleBot.Dialogs;

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            var qnaMakerSubscriptionKey = ConfigurationManager.AppSettings["QnAMakerSubscriptionKey"];
            var qnaMakerKnowledgeBaseId = ConfigurationManager.AppSettings["QnAMakerKnowledgeBaseId"];
            var defaultMessage = ConfigurationManager.AppSettings["DefaultMessage"];

            double.TryParse(ConfigurationManager.AppSettings["threshold"], out double threshold);

            var builder = new ContainerBuilder();

            builder.RegisterModule(new WikiModule(qnaMakerSubscriptionKey, qnaMakerKnowledgeBaseId, defaultMessage, threshold));

            builder.RegisterType<RootDialog>()
                .As<IDialog<object>>()
                .InstancePerDependency();

            builder.Update(Conversation.Container);
        }
    }
}
