using Rebus.Topic;

namespace LIM.SharedKernel.Rebus;

public class CustomTopicNameConvention : ITopicNameConvention
{
    public string GetTopic(Type eventType)
    {
        return eventType.Name;
    }
}