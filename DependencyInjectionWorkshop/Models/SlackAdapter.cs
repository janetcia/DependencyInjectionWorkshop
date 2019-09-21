using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public interface INotification
    {
        void Send(string accountId);
    }

    public class SlackAdapter : INotification
    {
        public SlackAdapter()
        {
        }

        public void Send(string accountId)
        {
            var message = $"{accountId} failed to authenticate.";
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", message, "my bot name");
        }
    }
}