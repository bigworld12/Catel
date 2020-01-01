﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TriggerBase.cs" company="Catel development team">
//   Copyright (c) 2008 - 2015 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

#if NET || NETCORE

namespace Catel.Windows.Interactivity
{
    using System;
    using System.Windows;
    using IoC;

#if UWP
    using global::Windows.UI.Xaml;
    using UIEventArgs = global::Windows.UI.Xaml.RoutedEventArgs;
#else
    using Microsoft.Xaml.Behaviors;
    using UIEventArgs = System.EventArgs;
#endif

    /// <summary>
    /// Trigger base class that handles a safe unsubscribe and clean up because the default
    /// Trigger class does not always call <see cref="OnDetaching"/>.
    /// <para />
    /// This class also adds some specific features such as <see cref="ValidateRequiredProperties"/>
    /// which is automatically called when the trigger is attached.
    /// </summary>
    /// <typeparam name="T">The <see cref="FrameworkElement"/> this trigger should attach to.</typeparam>
    public abstract class TriggerBase<T> : Microsoft.Xaml.Behaviors.TriggerBase<T>, ITrigger
        where T : FrameworkElement
    {
        #region Fields
        private bool _isClean = true;
        private int _loadCounter;

        private bool _isSubscribedToLoadedEvent = false;
        private bool _isSubscribedToUnloadedEvent = false;
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating whether the <c>TriggerBase{T}.AssociatedObject</c> is loaded.
        /// </summary>
        /// <value>
        /// <c>true</c> if the <c>TriggerBase{T}.AssociatedObject</c> is loaded; otherwise, <c>false</c>.
        /// </value>
        public bool IsAssociatedObjectLoaded { get { return _loadCounter > 0; } }

        /// <summary>
        /// Gets a value indicating whether this instance is in design mode.
        /// </summary>
        /// <value><c>true</c> if this instance is in design mode; otherwise, <c>false</c>.</value>
        protected bool IsInDesignMode
        {
            get { return CatelEnvironment.IsInDesignMode; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this trigger is enabled.
        /// </summary>
        /// <value><c>true</c> if this trigger is enabled; otherwise, <c>false</c>.</value>
        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        /// <summary>
        /// The IsEnabled property registration.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register("IsEnabled", typeof(bool),
            typeof(TriggerBase<T>), new PropertyMetadata(true, (sender, e) => ((TriggerBase<T>)sender).OnIsEnabledChanged()));
        #endregion

        #region Methods
        /// <summary>
        /// Called when the <see cref="IsEnabled" /> property has changed.
        /// </summary>
        protected virtual void OnIsEnabledChanged()
        {
        }

        /// <summary>
        /// Called after the action is attached to an AssociatedObject.
        /// </summary>
        protected override void OnAttached()
        {
            if (IsInDesignMode)
            {
                return;
            }

            base.OnAttached();

            if (!_isSubscribedToLoadedEvent)
            {
                AssociatedObject.Loaded += OnAssociatedObjectLoadedInternal;
                _isSubscribedToLoadedEvent = true;
            }

            _isClean = false;

            ValidateRequiredProperties();

            Initialize();
        }

        /// <summary>
        /// Called when the action is being detached from its AssociatedObject, but before it has actually occurred.
        /// </summary>
        protected override void OnDetaching()
        {
            if (IsInDesignMode)
            {
                return;
            }

            CleanUp();

            if (AssociatedObject != null)
            {
                AssociatedObject.Loaded -= OnAssociatedObjectLoadedInternal;
                _isSubscribedToLoadedEvent = false;
            }

            base.OnDetaching();
        }

        /// <summary>
        /// Validates the required properties. This method is called when the object is attached, but before
        /// the <see cref="Initialize"/> is invoked.
        /// </summary>
        protected virtual void ValidateRequiredProperties()
        {
        }

        /// <summary>
        /// Initializes the trigger action. This method is called instead of the <see cref="OnAttached"/> which is sealed
        /// to protect the additional trigger action.
        /// </summary>
        protected virtual void Initialize()
        {

        }

        /// <summary>
        /// Uninitializes the behavior. This method is called when <see cref="OnDetaching"/> is called, or when the
        /// <see cref="Microsoft.Xaml.Behaviors.TriggerBase{T}.AssociatedObject"/> is unloaded.
        /// <para />
        /// If dependency properties are used, it is very important to use <see cref="DependencyObject.ClearValue(System.Windows.DependencyProperty)"/> 
        /// to clear the value of the dependency properties in this method.
        /// </summary>
        protected virtual void Uninitialize()
        {

        }

        /// <summary>
        /// Called when the <see cref="Microsoft.Xaml.Behaviors.TriggerBase{T}.AssociatedObject"/> is loaded. This method is introduced to prevent
        /// double initialization when the <see cref="Microsoft.Xaml.Behaviors.TriggerBase{T}.AssociatedObject"/> is already loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnAssociatedObjectLoadedInternal(object sender, UIEventArgs e)
        {
            _loadCounter++;

            // Yes, 1, because we just increased the counter
            if (_loadCounter != 1)
            {
                return;
            }

            if (!_isSubscribedToUnloadedEvent)
            {
                AssociatedObject.Unloaded += OnAssociatedObjectUnloadedInternal;
                _isSubscribedToUnloadedEvent = true;
            }

            OnAssociatedObjectLoaded();
        }

        /// <summary>
        /// Called when the AssociatedObject is loaded.
        /// </summary>
        protected virtual void OnAssociatedObjectLoaded()
        {
        }

        /// <summary>
        /// Called when the <see cref="Microsoft.Xaml.Behaviors.TriggerBase{T}.AssociatedObject"/> is unloaded. This 
        /// method is introduced to prevent double uninitialization when the <see cref="Microsoft.Xaml.Behaviors.TriggerBase{T}.AssociatedObject"/> is already unloaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnAssociatedObjectUnloadedInternal(object sender, UIEventArgs e)
        {
            _loadCounter--;

            if (_loadCounter < 0)
            {
                _loadCounter = 0;
                return;
            }

            if (_loadCounter != 0)
            {
                return;
            }

            OnAssociatedObjectUnloaded();

            CleanUp();
        }

        /// <summary>
        /// Called when the AssociatedObject is unloaded.
        /// </summary>
        protected virtual void OnAssociatedObjectUnloaded()
        {
        }

        /// <summary>
        /// Actually cleans up the trigger action because <see cref="OnDetaching"/> is not always called.
        /// </summary>
        private void CleanUp()
        {
            if (_isClean)
            {
                return;
            }

            _isClean = true;

            if (AssociatedObject != null)
            {
                AssociatedObject.Unloaded -= OnAssociatedObjectUnloadedInternal;
                _isSubscribedToUnloadedEvent = false;
            }

            Uninitialize();
        }
        #endregion
    }
}

#endif
