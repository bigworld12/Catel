﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelectTextOnFocus.cs" company="Catel development team">
//   Copyright (c) 2008 - 2015 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

#if !XAMARIN && !XAMARIN_FORMS

namespace Catel.Windows.Interactivity
{
#if UWP
    using global::Windows.UI.Xaml;
    using global::Windows.UI.Xaml.Controls;
    using UIEventArgs = global::Windows.UI.Xaml.RoutedEventArgs;
#else
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.Xaml.Behaviors;
    using UIEventArgs = System.EventArgs;
#endif

    /// <summary>
    /// Selects all the text when the <see cref="TextBox"/> or <see cref="PasswordBox"/> is focused.
    /// </summary>
    public class SelectTextOnFocus : BehaviorBase<Control>
    {
        /// <summary>
        /// Called when the <see cref="Behavior{T}.AssociatedObject"/> is loaded.
        /// </summary>
        protected override void OnAssociatedObjectLoaded()
        {
            AssociatedObject.GotFocus += OnGotFocus;

#if NET || NETCORE
            AssociatedObject.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            AssociatedObject.GotMouseCapture += OnGotMouseCapture;
            AssociatedObject.GotKeyboardFocus += OnGotKeyboardFocus;
#endif
        }

        /// <summary>
        /// Called when the <see cref="Behavior{T}.AssociatedObject"/> is unloaded.
        /// </summary>
        protected override void OnAssociatedObjectUnloaded()
        {
            AssociatedObject.GotFocus -= OnGotFocus;

#if NET || NETCORE
            AssociatedObject.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            AssociatedObject.GotMouseCapture -= OnGotMouseCapture;
            AssociatedObject.GotKeyboardFocus -= OnGotKeyboardFocus;
#endif
        }

        /// <summary>
        /// Called when the <c>UIElement.OnGotFocus</c> event occurs.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            SelectAllText();
        }

#if NET || NETCORE
        /// <summary>
        /// Called when the <see cref="UIElement.PreviewMouseLeftButtonDown"/> event occurs.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnPreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            if (!AssociatedObject.IsKeyboardFocusWithin)
            {
                e.Handled = true;

                AssociatedObject.Focus();
            }
        }

        /// <summary>
        /// Called when the <see cref="UIElement.GotMouseCapture"/> event occurs.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnGotMouseCapture(object sender, RoutedEventArgs e)
        {
            SelectAllText();
        }

        /// <summary>
        /// Called when the <see cref="UIElement.GotKeyboardFocus"/> event occurs.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnGotKeyboardFocus(object sender, RoutedEventArgs e)
        {
            SelectAllText();
        }
#endif

        /// <summary>
        /// Selects all the text in the associated object.
        /// </summary>
        private void SelectAllText()
        {
            if (!IsEnabled)
            {
                return;
            }

            var textBox = AssociatedObject as TextBox;
            if (textBox != null)
            {
                textBox.SelectAll();
            }

            var passwordBox = AssociatedObject as PasswordBox;
            if (passwordBox != null)
            {
                passwordBox.SelectAll();
            }
        } 
    }
}

#endif
