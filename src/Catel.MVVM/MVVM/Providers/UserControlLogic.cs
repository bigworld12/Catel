﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserControlLogic.cs" company="Catel development team">
//   Copyright (c) 2008 - 2015 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Catel.MVVM.Providers
{
    using System;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using IoC;
    using Views;
    using Services;
    using Logging;
    using MVVM;
    using Reflection;

#if XAMARIN || XAMARIN_FORMS
    // TODO
#elif UWP
    using global::Windows.UI;
    using global::Windows.UI.Core;
    using global::Windows.UI.Xaml;
#else
    using System.Windows;
    using Windows.Controls;
#endif

    /// <summary>
    /// MVVM Provider behavior implementation for a user control.
    /// </summary>
    public class UserControlLogic : LogicBase
    {
        #region Fields
        /// <summary>
        /// The log.
        /// </summary>
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private IViewModelContainer _parentViewModelContainer;
        private IViewModel _parentViewModel;

#if NET || NETCORE
        private InfoBarMessageControl _infoBarMessageControl;
#endif
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes static members of the <see cref="UserControlLogic" /> class.
        /// </summary>
        static UserControlLogic()
        {
            DefaultSupportParentViewModelContainersValue = true;
            DefaultUnloadBehaviorValue = UnloadBehavior.SaveAndCloseViewModel;

#if NET || NETCORE
            DefaultCreateWarningAndErrorValidatorForViewModelValue = true;
#endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserControlLogic"/> class.
        /// </summary>
        /// <param name="targetView">The target control.</param>
        /// <param name="viewModelType">Type of the view model.</param>
        /// <param name="viewModel">The view model.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="targetView"/> is <c>null</c>.</exception>
        public UserControlLogic(IView targetView, Type viewModelType = null, IViewModel viewModel = null)
            : base(targetView, viewModelType, viewModel)
        {
            if (CatelEnvironment.IsInDesignMode)
            {
                return;
            }

            SupportParentViewModelContainers = DefaultSupportParentViewModelContainersValue;
            UnloadBehavior = DefaultUnloadBehaviorValue;

#if NET || NETCORE
            SkipSearchingForInfoBarMessageControl = DefaultSkipSearchingForInfoBarMessageControlValue;
            CreateWarningAndErrorValidatorForViewModel = DefaultCreateWarningAndErrorValidatorForViewModelValue;
#endif

#if XAMARIN
            CreateViewModelWrapper();
#elif !UWP
            // For non-UWP, we *cannot* use the ContentChanged event (it doesn't have the x:Name available)
            this.SubscribeToWeakGenericEvent<EventArgs>(targetView, nameof(FrameworkElement.Initialized), OnTargetViewInitialized, false);
#else
            // For UWP, we *can* use the ContentChanged event (it does have x:Name available)
            // NOTE: There is NO unsubscription for this subscription.
            // Hence target control content wrapper grid will be recreated each time content changes.
            targetView.SubscribeToPropertyChanged(nameof(global::Windows.UI.Xaml.Controls.UserControl.Content), OnTargetViewContentChanged);
#endif
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a value indicating whether parent view model containers are supported. If supported,
        /// the user control will search for the <see cref="IViewModelContainer"/> interface. During this search, the user control 
        /// will use both the visual and logical tree.
        /// <para />
        /// If a user control does not have any parent control implementing the <see cref="IViewModelContainer"/> interface, searching
        /// for it is useless and requires the control to search all the way to the top for the implementation. To prevent this from
        /// happening, set this property to <c>false</c>.
        /// <para />
        /// The default value is <c>true</c>.
        /// </summary>
        /// <value>
        /// <c>true</c> if parent view model containers are supported; otherwise, <c>false</c>.
        /// </value>
        public bool SupportParentViewModelContainers { get; set; }

        /// <summary>
        /// Gets or sets the default value for the <see cref="SupportParentViewModelContainers"/> property.
        /// <para />
        /// The default value is <c>true</c>.
        /// </summary>
        /// <value>The unload behavior.</value>
        public static bool DefaultSupportParentViewModelContainersValue { get; set; }

        /// <summary>
        /// Gets or sets the unload behavior when the data context of the target control changes.
        /// </summary>
        /// <value>The unload behavior.</value>
        public UnloadBehavior UnloadBehavior { get; set; }

        /// <summary>
        /// Gets or sets the default value for the <see cref="UnloadBehavior"/> property.
        /// <para />
        /// The default value is <see cref="Providers.UnloadBehavior.SaveAndCloseViewModel"/>.
        /// </summary>
        /// <value>The unload behavior.</value>
        public static UnloadBehavior DefaultUnloadBehaviorValue { get; set; }

#if NET || NETCORE
        /// <summary>
        /// Gets or sets a value indicating whether to skip the search for an info bar message control. If not skipped,
        /// the user control will search for a the first <see cref="InfoBarMessageControl"/> that can be found. 
        /// During this search, the user control will use both the visual and logical tree.
        /// <para />
        /// If a user control does not have any <see cref="InfoBarMessageControl"/>, searching
        /// for it is useless and requires the control to search all the way to the top for the implementation. To prevent this from
        /// happening, set this property to <c>true</c>.
        /// <para />
        /// The default value is determined by the <see cref="DefaultSkipSearchingForInfoBarMessageControlValue"/> property.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the search for an info bar message control should be skipped; otherwise, <c>false</c>.
        /// </value>
        public bool SkipSearchingForInfoBarMessageControl { get; set; }

        /// <summary>
        /// Gets or sets a value for the <see cref="SkipSearchingForInfoBarMessageControl"/> property. This way, the behavior
        /// can be changed an entire application to prevent disabling it on every control.
        /// <para />
        /// The default value is <c>false</c>.
        /// </summary>
        public static bool DefaultSkipSearchingForInfoBarMessageControlValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to create a <see cref="WarningAndErrorValidator"/> for the current
        /// control.
        /// <para />
        /// If a user control does not have any <see cref="InfoBarMessageControl"/> or equivalent control, it is useless to create
        /// a <see cref="WarningAndErrorValidator"/> for the current control.
        /// <para />
        /// The default value is determined by the <see cref="DefaultCreateWarningAndErrorValidatorForViewModelValue"/> property.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the <see cref="WarningAndErrorValidator"/> should be created; otherwise, <c>false</c>.
        /// </value>
        public bool CreateWarningAndErrorValidatorForViewModel { get; set; }

        /// <summary>
        /// Gets or sets a value for the <see cref="CreateWarningAndErrorValidatorForViewModel"/> property. This way, the behavior
        /// can be changed an entire application to prevent disabling it on every control.
        /// <para />
        /// The default value is <c>true</c>.
        /// </summary>
        public static bool DefaultCreateWarningAndErrorValidatorForViewModelValue { get; set; }
#endif

        /// <summary>
        /// Gets or sets a value indicating whether the user control should automatically be disabled when there is no
        /// active view model.
        /// </summary>
        /// <value>
        /// <c>true</c> if the user control should automatically be disabled when there is no active view model; otherwise, <c>false</c>.
        /// </value>
        public bool DisableWhenNoViewModel { get; set; }

        /// <summary>
        /// Gets a value indicating whether there is a parent view model container available.
        /// </summary>
        /// <value>
        /// <c>true</c> if there is a parent view model container available; otherwise, <c>false</c>.
        /// </value>
        protected bool HasParentViewModelContainer
        {
            get { return _parentViewModelContainer != null; }
        }

        /// <summary>
        /// Gets the parent view model container.
        /// </summary>
        /// <value>The parent view model container.</value>
        /// <remarks>
        /// For internal usage only.
        /// </remarks>
        internal IViewModelContainer ParentViewModelContainer
        {
            get { return _parentViewModelContainer; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is subscribed to a parent view model.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is subscribed to a parent view model; otherwise, <c>false</c>.
        /// </value>
        protected bool IsSubscribedToParentViewModel
        {
            get { return (_parentViewModel != null); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the data context of the target control.
        /// <para />
        /// This method is abstract because the real logic implementation knows how to set the data context (for example,
        /// by using an additional data context grid).
        /// </summary>
        /// <param name="newDataContext">The new data context.</param>
        protected override void SetDataContext(object newDataContext)
        {
            // not required, the VM grid automatically binds to the ViewModel property
        }

        /// <summary>
        /// Creates the view model wrapper.
        /// </summary>
        /// <param name="force">If set the <c>true</c>, this will add the <see cref="WrapOptions.Force"/> flag.</param>
        public IViewModelWrapper CreateViewModelWrapper(bool force = false)
        {
            var targetView = TargetView;

            var dependencyResolver = this.GetDependencyResolver();
            var viewModelWrapperService = dependencyResolver.Resolve<IViewModelWrapperService>();

            var wrapper = viewModelWrapperService.GetWrapper(targetView);
            if (wrapper is null)
            {
                var wrapOptions = WrapOptions.None;

#if NET || NETCORE
                if (CreateWarningAndErrorValidatorForViewModel)
                {
                    wrapOptions |= WrapOptions.CreateWarningAndErrorValidatorForViewModel;
                }
#endif

                if (force)
                {
                    wrapOptions |= WrapOptions.Force;
                }

                wrapper = viewModelWrapperService.Wrap(targetView, this, wrapOptions);
            }

            return wrapper;
        }

        /// <summary>
        /// Gets the view model wrapper. If the view is not wrapped, this method will return <c>null</c>.
        /// </summary>
        /// <returns>The view model wrapper or <c>null</c>.</returns>
        public object GetViewModelWrapper()
        {
            var dependencyResolver = this.GetDependencyResolver();
            var viewModelWrapperService = dependencyResolver.Resolve<IViewModelWrapperService>();
            return viewModelWrapperService.GetWrapper(TargetView);
        }

#if !UWP
        private void OnTargetViewInitialized(object sender, EventArgs e)
        {
            // Note: we can't use Content changed property notification (x:Name is not yet set), but Loaded event is too late,
            // this event should be in-between: https://docs.microsoft.com/en-us/dotnet/framework/wpf/advanced/object-lifetime-events
            CreateViewModelWrapper();
        }
#else  
        /// <summary>
        /// Called when the content of the target control has changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OnTargetViewContentChanged(object sender, PropertyChangedEventArgs e)
        {
            CreateViewModelWrapper();
        }
#endif

        /// <summary>
        /// Called when the <c>TargetView</c> has just been loaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public override async Task OnTargetViewLoadedAsync(object sender, EventArgs e)
        {
            await CompleteViewModelClosingAsync();

            // Do not call base because it will create a VM. We will create the VM ourselves
            //base.OnTargetControlLoaded(sender, e);

            // Manually updating target control content wrapper here (not by content property changed event handler),
            // because in WinRT UserControl does NOT update bindings while InitializeComponents() method is executing,
            // even if the Content property was changed while InitializeComponents() running there is no triggering of a binding update.
            CreateViewModelWrapper();

#if NET || NETCORE
            if (!SkipSearchingForInfoBarMessageControl)
            {
                Log.Debug("Searching for an instance of the InfoBarMessageControl");

                _infoBarMessageControl = TargetView.FindParentByPredicate(o => o is InfoBarMessageControl) as InfoBarMessageControl;

                Log.Debug("Finished searching for an instance of the InfoBarMessageControl");

                if (_infoBarMessageControl is null)
                {
                    Log.Warning($"No InfoBarMessageControl is found in the visual tree of '{GetType().Name}', consider using the SkipSearchingForInfoBarMessageControl property to improve performance");
                }
            }
            else
            {
                Log.Debug("Skipping the search for an instance of the InfoBarMessageControl");
            }
#endif

            if (ViewModel is null)
            {
                if (ViewModelLifetimeManagement != ViewModelLifetimeManagement.FullyManual)
                {
                    // Try to create view model based on data context
                    var dataContext = GetDataContext(TargetView);
                    await UpdateDataContextToUseViewModelAsync(dataContext);
                }
                else
                {
                    Log.Debug($"View model lifetime management is set to '{ViewModelLifetimeManagement}', not creating view model on loaded event for '{TargetViewType?.Name}'");
                }
            }
            else
            {
                if (ViewModelLifetimeManagement == ViewModelLifetimeManagement.PartlyManual)
                {
                    Log.Debug("Re-using existing view model");
                }
            }

            if (DisableWhenNoViewModel)
            {
                TargetView.IsEnabled = (ViewModel != null);
            }
        }

        /// <summary>
        /// Called when the <c>TargetView</c> has just been unloaded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public override async Task OnTargetViewUnloadedAsync(object sender, EventArgs e)
        {
            await base.OnTargetViewUnloadedAsync(sender, e);

            if (ViewModel != null)
            {
                ClearWarningsAndErrorsForObject(ViewModel);
            }

            UnsubscribeFromParentViewModelContainer();

            if (ViewModelLifetimeManagement == ViewModelLifetimeManagement.Automatic)
            {
                var result = GetViewModelResultValueFromUnloadBehavior();
                await CloseAndDisposeViewModelAsync(result);
            }
            else
            {
                Log.Debug($"View model lifetime management is set to '{ViewModelLifetimeManagement}', not closing view model on unloaded event for '{TargetViewType?.Name}'");
            }
        }

        /// <summary>
        /// Called when the <see cref="LogicBase.ViewModel"/> property is about to change.
        /// </summary>
        protected override void OnViewModelChanging()
        {
            UnregisterViewModelAsChild();
        }

        /// <summary>
        /// Called when the <see cref="LogicBase.ViewModel"/> property has just been changed.
        /// </summary>
        protected override void OnViewModelChanged()
        {
            RegisterViewModelAsChild();

            if (DisableWhenNoViewModel)
            {
                TargetView.IsEnabled = (ViewModel != null);
            }
        }

        /// <summary>
        /// Called when the <c>DataContext</c> property of the <c>TargetView</c> has changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
#pragma warning disable AvoidAsyncVoid // Avoid async void
        public override async void OnTargetViewDataContextChanged(object sender, Catel.MVVM.Views.DataContextChangedEventArgs e)
#pragma warning restore AvoidAsyncVoid // Avoid async void
        {
            if (IsCurrentDataContext(e))
            {
                return;
            }

            if (!IsTargetViewLoaded && !IsLoading)
            {
                return;
            }

            // Fix in WinRT to make sure inner grid is created
            CreateViewModelWrapper();

            // Fix for CTL-307: DataContextChanged is invoked before Unloaded because Parent is set to null
            var targetControlParent = TargetView.GetParent();
            if (targetControlParent is null)
            {
                return;
            }

            base.OnTargetViewDataContextChanged(sender, e);

            var dataContext = GetDataContext(TargetView);
            if (dataContext.IsSentinelBindingObject())
            {
                return;
            }

            if (!IsUnloading)
            {
                // Note: don't respect view model lifetime management here, the IViewModelFactory should return null if
                // no vm should be created and users *really* want to disable this kind of core Catel behavior
                await UpdateDataContextToUseViewModelAsync(dataContext);
            }
        }

        /// <summary>
        /// Called when the view manager is loading.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ViewLoadEventArgs"/> instance containing the event data.</param>
        protected override void OnViewLoadedManagerLoading(object sender, ViewLoadEventArgs e)
        {
            base.OnViewLoadedManagerLoading(sender, e);

            if (ReferenceEquals(e.View, ParentViewModelContainer))
            {
                OnParentViewModelContainerLoading(e.View, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called when the view manager is unloading.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ViewLoadEventArgs"/> instance containing the event data.</param>
        protected override void OnViewLoadedManagerUnloading(object sender, ViewLoadEventArgs e)
        {
            base.OnViewLoadedManagerUnloading(sender, e);

            if (ReferenceEquals(e.View, ParentViewModelContainer))
            {
                OnParentViewModelContainerUnloading(e.View, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Subscribes to the parent view model container.
        /// </summary>
        private void SubscribeToParentViewModelContainer()
        {
            if (!SupportParentViewModelContainers)
            {
                return;
            }

            if (HasParentViewModelContainer)
            {
                return;
            }

            _parentViewModelContainer = TargetView.FindParentViewModelContainer();
            if (_parentViewModelContainer != null)
            {
                Log.Debug("Found the parent view model container '{0}' for '{1}'", _parentViewModelContainer.GetType().Name, TargetView.GetType().Name);
            }
            else
            {
                Log.Debug("Couldn't find parent view model container");
            }

            if (_parentViewModelContainer != null)
            {
                _parentViewModelContainer.ViewModelChanged += OnParentViewModelContainerViewModelChanged;

                SubscribeToParentViewModel(_parentViewModelContainer.ViewModel);
            }
        }

        /// <summary>
        /// Unsubscribes from the parent view model container.
        /// </summary>
        private void UnsubscribeFromParentViewModelContainer()
        {
            if (_parentViewModelContainer != null)
            {
                // Fix for https://catelproject.atlassian.net/browse/CTL-182, we might be subscribed to a parent
                // while that doesn't change, we might be unloaded and we always need to unsubscribe from the parent view model
                UnsubscribeFromParentViewModel();

                _parentViewModelContainer.ViewModelChanged -= OnParentViewModelContainerViewModelChanged;

                _parentViewModelContainer = null;
            }
        }

        /// <summary>
        /// Subscribes to a parent view model.
        /// </summary>
        /// <param name="parentViewModel">The parent view model.</param>
        private void SubscribeToParentViewModel(IViewModel parentViewModel)
        {
            if ((parentViewModel != null) && !ObjectHelper.AreEqualReferences(parentViewModel, ViewModel))
            {
                _parentViewModel = parentViewModel;

                RegisterViewModelAsChild();

                _parentViewModel.SavingAsync += OnParentViewModelSavingAsync;
                _parentViewModel.CancelingAsync += OnParentViewModelCancelingAsync;
                _parentViewModel.ClosingAsync += OnParentViewModelClosingAsync;

                Log.Debug("Subscribed to parent view model '{0}'", parentViewModel.GetType());
            }
        }

        /// <summary>
        /// Unsubscribes from a parent view model.
        /// </summary>
        private void UnsubscribeFromParentViewModel()
        {
            if (_parentViewModel != null)
            {
                UnregisterViewModelAsChild();

                _parentViewModel.SavingAsync -= OnParentViewModelSavingAsync;
                _parentViewModel.CancelingAsync -= OnParentViewModelCancelingAsync;
                _parentViewModel.ClosingAsync -= OnParentViewModelClosingAsync;

                _parentViewModel = null;

                Log.Debug("Unsubscribed from parent view model");
            }
        }

        /// <summary>
        /// Registers the view model as child on the parent view model.
        /// </summary>
        private void RegisterViewModelAsChild()
        {
            var parentViewModel = _parentViewModel as IRelationalViewModel;
            if (parentViewModel is null)
            {
                return;
            }

            var viewModel = ViewModel as IRelationalViewModel;
            if (viewModel is null)
            {
                return;
            }

            if (ObjectHelper.AreEqualReferences(parentViewModel, viewModel))
            {
                return;
            }

            parentViewModel.RegisterChildViewModel(viewModel);
            viewModel.SetParentViewModel(_parentViewModel);
        }

        /// <summary>
        /// Unregisters the view model as child on the parent view model.
        /// </summary>
        private void UnregisterViewModelAsChild()
        {
            var parentViewModel = _parentViewModel as IRelationalViewModel;
            if (parentViewModel is null)
            {
                return;
            }

            var viewModel = ViewModel as IRelationalViewModel;
            if (viewModel is null)
            {
                return;
            }

            if (ObjectHelper.AreEqualReferences(parentViewModel, viewModel))
            {
                return;
            }

            viewModel.SetParentViewModel(null);
            parentViewModel.UnregisterChildViewModel(viewModel);
        }

        /// <summary>
        /// Updates the data context to use view model.
        /// </summary>
        /// <param name="newDataContext">The new data context.</param>
        private async Task UpdateDataContextToUseViewModelAsync(object newDataContext)
        {
            SubscribeToParentViewModelContainer();

            if (ViewModelBehavior == LogicViewModelBehavior.Injected)
            {
                Log.Debug("View model behavior is set to 'Injected' thus view model will not be updated");
                return;
            }

            var currentViewModel = ViewModel;
            object modelToInject = null;
            var constructNewViewModel = false;

            if (newDataContext != null)
            {
                var dataContextAsViewModel = newDataContext as IViewModel;
                if (dataContextAsViewModel != null)
                {
                    // If the DataContext is a view model, only create a new view model if required
                    if (currentViewModel is null)
                    {
                        ViewModel = ConstructViewModelUsingArgumentOrDefaultConstructor(newDataContext);
                    }
                }
                else if (!newDataContext.GetType().IsAssignableFromEx(ViewModelType))
                {
                    constructNewViewModel = true;
                    modelToInject = newDataContext;
                }
            }
            else
            {
                constructNewViewModel = true;

                // We closed our previous view-model, but it might be possible to construct a new view-model
                // with an empty constructor, so try that now
                modelToInject = null;
            }

            if (constructNewViewModel)
            {
                if (currentViewModel != null)
                {
                    var viewModelType = ViewModelType;

                    var canKeepViewModel = !ViewModelFactory.IsViewModelWithModelInjection(viewModelType);
                    if (canKeepViewModel)
                    {
                        Log.Debug($"DataContext has changed, but view model '{viewModelType}' is a view model without model injection, keeping current view model");
                        return;
                    }
                }

                if (currentViewModel != null)
                {
                    var result = GetViewModelResultValueFromUnloadBehavior();
                    await CloseAndDisposeViewModelAsync(result);
                }

                ViewModel = ConstructViewModelUsingArgumentOrDefaultConstructor(modelToInject);
            }
        }

        /// <summary>
        /// Gets the view model result value based on the <see cref="UnloadBehavior"/> property so it can be used for
        /// the <see cref="CloseAndDisposeViewModelAsync"/> method.
        /// </summary>
        /// <returns>The right value.</returns>
        private bool? GetViewModelResultValueFromUnloadBehavior()
        {
            bool? result = null;

            switch (UnloadBehavior)
            {
                case UnloadBehavior.CloseViewModel:
                    result = null;
                    break;

                case UnloadBehavior.SaveAndCloseViewModel:
                    result = true;
                    break;

                case UnloadBehavior.CancelAndCloseViewModel:
                    result = false;
                    break;
            }

            return result;
        }

        /// <summary>
        /// Closes and disposes the current view model.
        /// </summary>
        /// <param name="result"><c>true</c> if the view model should be saved; <c>false</c> if the view model should be canceled; <c>null</c> if it should only be closed.</param>
        private async Task CloseAndDisposeViewModelAsync(bool? result)
        {
            var vm = ViewModel;
            if (vm != null)
            {
                if (result.HasValue)
                {
                    if (result.Value)
                    {
                        await vm.SaveViewModelAsync();
                    }
                    else
                    {
                        await vm.CancelViewModelAsync();
                    }
                }

                await CloseViewModelAsync(result, true);
            }
        }

        /// <summary>
        /// Handles the ViewModelChanged event of the parent ViewModel container.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void OnParentViewModelContainerViewModelChanged(object sender, EventArgs e)
        {
            UnsubscribeFromParentViewModel();

            IViewModelContainer viewModelContainer;

            var senderAsLogic = sender as LogicBase;
            if (senderAsLogic != null)
            {
                viewModelContainer = senderAsLogic.TargetView;
            }
            else
            {
                viewModelContainer = sender as IViewModelContainer;
            }

            if (viewModelContainer != null)
            {
                var parentVm = viewModelContainer.ViewModel;
                SubscribeToParentViewModel(parentVm);
            }
        }

        private void OnParentViewModelContainerUnloading(object sender, EventArgs e)
        {
            if (!IgnoreNullDataContext)
            {
                Log.Debug("Parent IViewModelContainer.Unloading event fired, now ignoring null DataContext");

                IgnoreNullDataContext = true;
            }
        }

        private void OnParentViewModelContainerLoading(object sender, EventArgs e)
        {
            if (IgnoreNullDataContext)
            {
                Log.Debug("Parent IViewModelContainer.Loading event fired, no longer ignoring null DataContext");

                IgnoreNullDataContext = false;
            }
        }

        /// <summary>
        /// Handles the Canceling event of the parent ViewModel.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CancelingEventArgs"/> instance containing the event data.</param>
        private async Task OnParentViewModelCancelingAsync(object sender, CancelingEventArgs e)
        {
            // The parent view model is canceled, cancel our view model as well
            if (ViewModel != null)
            {
                if (ReferenceEquals(sender, ViewModel))
                {
                    Log.Warning("Parent view model '{0}' is exactly the same instance as the current view model, ignore Canceling event", sender.GetType().FullName);
                    return;
                }

                if (e.Cancel)
                {
                    Log.Info("Parent view model '{0}' is canceling, but canceling is canceled by another view model, canceling of view model '{1}' will not continue", _parentViewModel.GetType(), ViewModel.GetType());
                    return;
                }

                Log.Info("Parent view model '{0}' is canceled, cancelling view model '{1}' as well", _parentViewModel.GetType(), ViewModel.GetType());

                if (!ViewModel.IsClosed)
                {
                    e.Cancel = !await ViewModel.CancelViewModelAsync();
                }
            }
        }

        /// <summary>
        /// Handles the Saving event of the parent ViewModel.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SavingEventArgs"/> instance containing the event data.</param>
        private async Task OnParentViewModelSavingAsync(object sender, SavingEventArgs e)
        {
            // The parent view model is saved, save our view model as well
            if (ViewModel != null)
            {
                if (ReferenceEquals(sender, ViewModel))
                {
                    Log.Warning("Parent view model '{0}' is exactly the same instance as the current view model, ignore Saving event", sender.GetType().FullName);
                    return;
                }

                if (e.Cancel)
                {
                    Log.Info("Parent view model '{0}' is saving, but saving is canceled by another view model, saving of view model '{1}' will not continue", _parentViewModel.GetType(), ViewModel.GetType());
                    return;
                }

                Log.Info("Parent view model '{0}' is saving, saving view model '{1}' as well", _parentViewModel.GetType(), ViewModel.GetType());

                if (!ViewModel.IsClosed)
                {
                    e.Cancel = !await ViewModel.SaveViewModelAsync();
                }
            }
        }

        /// <summary>
        /// Called when Closing event of the parent ViewModel.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async Task OnParentViewModelClosingAsync(object sender, EventArgs e)
        {
            if (ViewModel != null)
            {
                if (ReferenceEquals(sender, ViewModel))
                {
                    Log.Warning("Parent view model '{0}' is exactly the same instance as the current view model, ignore Closing event", sender.GetType().FullName);
                    return;
                }

                Log.Debug("Parent ViewModel is closing, ignoring null DataContext");

                IgnoreNullDataContext = true;

                if (ViewModelLifetimeManagement == ViewModelLifetimeManagement.Automatic)
                {
                    await CloseAndDisposeViewModelAsync(null);
                }
                else
                {
                    Log.Debug($"View model lifetime management is set to '{ViewModelLifetimeManagement}', not closing view model on parent view model closing event for '{TargetViewType?.Name}'");
                }
            }
        }

        /// <summary>
        /// Clears the warnings and errors for the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <remarks>
        /// Since there is a "bug" in the .NET Framework (DataContext issue), this method clears the current
        /// warnings and errors in the InfoBarMessageControl if available.
        /// </remarks>
        private void ClearWarningsAndErrorsForObject(object obj)
        {
            if (obj is null)
            {
                return;
            }

#if NET || NETCORE
            if (_infoBarMessageControl != null)
            {
                _infoBarMessageControl.ClearObjectMessages(obj);

                Log.Debug("Cleared all warnings and errors caused by '{0}' since this is caused by a DataContext issue in the .NET Framework", obj);
            }
#endif
        }
        #endregion
    }
}
