using FormworkOptimize.App.ViewModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormworkOptimize.App.ViewModels.Mediators
{
    public class SubscriberKey
    {
        #region Properties
        public object Subscriber { get; }

        public Context Context { get; }

        public Type MessageType { get; }

        #endregion

        #region Constructors

        public SubscriberKey(object subscriber, Context context, Type messageType)
        {
            Subscriber = subscriber;
            Context = context;
            MessageType = messageType;
        }

        #endregion

        #region Methods

        public override int GetHashCode()
        {
            var prime1 = 17;
            var prime2 = 23;
            unchecked
            {
                var hash = prime1 * prime2 * (Subscriber.GetHashCode() + Context.GetHashCode() + MessageType.GetHashCode());
                return hash;
            }
        }

        #endregion
    }
}
