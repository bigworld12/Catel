namespace Catel.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading.Tasks;
    using Catel.IoC;
    using Catel.Services;

    public class DispatcherFastObservableDictionary<TKey, TValue> : FastObservableDictionary<TKey, TValue>
    {
        private readonly Lazy<IDispatcherService> _dispatcherService = new Lazy<IDispatcherService>(() => IoCConfiguration.DefaultDependencyResolver.Resolve<IDispatcherService>());

        public DispatcherFastObservableDictionary()
        {
        }

        public DispatcherFastObservableDictionary(int capacity) : base(capacity)
        {
        }

        public DispatcherFastObservableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> originalDict) : base(originalDict)
        {
        }

        public DispatcherFastObservableDictionary(IEqualityComparer<TKey> comparer) : base(comparer)
        {
        }

        public DispatcherFastObservableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary)
        {
        }

        public DispatcherFastObservableDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer)
        {
        }

        public DispatcherFastObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer)
        {
        }

        protected DispatcherFastObservableDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether events should automatically be dispatched to the UI thread.
        /// </summary>
        /// <value><c>true</c> if events should automatically be dispatched to the UI thread; otherwise, <c>false</c>.</value>
        public bool AutomaticallyDispatchChangeNotifications { get; set; } = true;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs eventArgs)
        {
            if (AutomaticallyDispatchChangeNotifications)
            {
                _dispatcherService.Value.BeginInvokeIfRequired(() => base.OnCollectionChanged(eventArgs));
            }
            else
            {
                base.OnCollectionChanged(eventArgs);
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs eventArgs)
        {
            if (AutomaticallyDispatchChangeNotifications)
            {
                _dispatcherService.Value.BeginInvokeIfRequired(() => base.OnPropertyChanged(eventArgs));
            }
            else
            {
                base.OnPropertyChanged(eventArgs);
            }

        }

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
    }

}
