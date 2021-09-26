using FormworkOptimize.App.ViewModels.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormworkOptimize.App.ViewModels.Mediators
{
    public class Mediator : IMediator
    {
        #region Private Fields

        private readonly ConcurrentDictionary<SubscriberKey, object> _subscribers;

        private static readonly Lazy<Mediator> _instance = new Lazy<Mediator>(() => new Mediator(), true);

        #endregion

        #region Properties

        public static Mediator Instance => _instance.Value;

        #endregion

        #region Constructors

        private Mediator()
        {
            _subscribers = new ConcurrentDictionary<SubscriberKey, object>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Notifies the colleagues.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message.</param>
        /// <param name="context">The context.</param>
        public void NotifyColleagues<T>(T message, Context context)
        {
            _subscribers.Where(s => s.Key.Context == context)
                        .Select(kvp => kvp.Value)
                        .OfType<Action<T>>()
                        .ToList()
                        .ForEach(action => action?.Invoke(message));
        }

        /// <summary>
        /// Unsubscribe all the current subscribers.
        /// </summary>
        public void Reset() =>
            _subscribers.Clear();

        /// <summary>
        /// Subscribes the specified subscriber.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriber">The subscriber.</param>
        /// <param name="onNotified">Action runs when the subscriber is notified.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public bool Subscribe<T>(object subscriber, Action<T> onNotified, Context context)
        {
            var key = new SubscriberKey(subscriber, context, typeof(T));
            return _subscribers.TryAdd(key, onNotified);
        }

        /// <summary>
        /// UnSubscribe a current subscriber with a give context.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriber">The subscriber.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public bool UnSubscribe<T>(object subscriber, Context context)
        {
            object action = null;
            var key = new SubscriberKey(subscriber, context, typeof(T));
            return _subscribers.TryRemove(key, out action);
        }

        /// <summary>
        /// UnSubscribe a current subscriber with a given context.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriber">The subscriber.</param>
        /// <param name="context">The context.</param>
        /// <param name="onSubscribtionEnd">Action that runs when the subscribtion is ended successfully.</param>
        /// <returns></returns>
        public bool UnSubscribe<T>(object subscriber, Context context, Action onSubscribtionEnd)
        {
            var result = UnSubscribe<T>(subscriber, context);
            if (result)
                onSubscribtionEnd?.Invoke();
            return result;
        }

        #endregion
    }
}
