using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mutopic
{
    public static class IPubSubExtensions
    {
        public static IPubSubSubscription Subscribe<T>(this IPubSub pubsub, string topicName, Func<T, Task> asyncHandler) => pubsub.Subscribe<T>(topicName, t => { asyncHandler(t).Wait(); });
        
        public static IPubSubSubscription SubscribeWhere<T>(this IPubSub pubsub, string topicName, Func<T, bool> where, Func<T, Task> handler)
            => pubsub.Subscribe(
                topicName,
                (T m) => (where(m)) ? handler(m) : Task.CompletedTask
                );
        public static IPubSubSubscription SubscribeWhere<T>(this IPubSub pubsub, string topicName, Func<T, bool> where, Action<T> handler)
            => pubsub.Subscribe(
                topicName,
                (T m) => { if (where(m)) handler(m); }
                );
    }
}
