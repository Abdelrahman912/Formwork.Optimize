using FormworkOptimize.App.ViewModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormworkOptimize.App.ViewModels.Mediators
{
    public interface IMediator
    {
        bool Subscribe<T>(object subscriber, Action<T> onNotified, Context context);

        bool UnSubscribe<T>(object subscriber, Context context, Action onSubscribtionEnd);

        void NotifyColleagues<T>(T message, Context context);

        void Reset();
    }
}
