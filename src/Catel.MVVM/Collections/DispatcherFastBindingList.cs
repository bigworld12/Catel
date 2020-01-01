
#if NET || NETCORE
namespace Catel.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Catel.IoC;
    using Catel.Services;

    public class DispatcherFastBindingList<T> : FastBindingList<T>
    {
        private readonly Lazy<IDispatcherService> _dispatcherService = new Lazy<IDispatcherService>(() => IoCConfiguration.DefaultDependencyResolver.Resolve<IDispatcherService>());

        public DispatcherFastBindingList()
        {
        }

        public DispatcherFastBindingList(IEnumerable<T> collection) : base(collection)
        {
        }

        public DispatcherFastBindingList(IEnumerable collection) : base(collection)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether events should automatically be dispatched to the UI thread.
        /// </summary>
        /// <value><c>true</c> if events should automatically be dispatched to the UI thread; otherwise, <c>false</c>.</value>
        public bool AutomaticallyDispatchChangeNotifications { get; set; } = true;

        protected override void NotifyChanges()
        {
            if (AutomaticallyDispatchChangeNotifications)
            {
                _dispatcherService.Value.BeginInvokeIfRequired(base.NotifyChanges);
            }
            else
            {
                base.NotifyChanges();
            }
        }

        protected override void OnListChanged(ListChangedEventArgs e)
        {
            if (AutomaticallyDispatchChangeNotifications)
            {
                _dispatcherService.Value.BeginInvokeIfRequired(() => base.OnListChanged(e));
            }
            else
            {
                base.OnListChanged(e);
            }
        }
    }
}
#endif

