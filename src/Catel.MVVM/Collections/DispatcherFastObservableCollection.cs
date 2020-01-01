namespace Catel.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using Catel.IoC;
    using Catel.Services;

    public class DispatcherFastObservableCollection<T> : FastObservableCollection<T>
    {
        private static readonly Lazy<IDispatcherService> _dispatcherService = new Lazy<IDispatcherService>(() =>
        {
            var dependencyResolver = IoCConfiguration.DefaultDependencyResolver;
            return dependencyResolver.Resolve<IDispatcherService>();
        });

        public DispatcherFastObservableCollection()
        {
        }

        public DispatcherFastObservableCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        public DispatcherFastObservableCollection(IEnumerable collection) : base(collection)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether events should automatically be dispatched to the UI thread.
        /// </summary>
        /// <value><c>true</c> if events should automatically be dispatched to the UI thread; otherwise, <c>false</c>.</value>
        public bool AutomaticallyDispatchChangeNotifications { get; set; } = true;

        /// <inheritdoc cref="FastObservableCollection{T}.OnPropertyChanged(PropertyChangedEventArgs)"/>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (AutomaticallyDispatchChangeNotifications)
            {
                _dispatcherService.Value.BeginInvokeIfRequired(() => base.OnPropertyChanged(e));
            }
            else
            {
                base.OnPropertyChanged(e);
            }
        }

        /// <inheritdoc cref="FastObservableCollection{T}.OnCollectionChanged(NotifyCollectionChangedEventArgs)"/>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (AutomaticallyDispatchChangeNotifications)
            {
                _dispatcherService.Value.BeginInvokeIfRequired(() => base.OnCollectionChanged(e));
            }
            else
            {
                base.OnCollectionChanged(e);
            }   
        }
    }
}
