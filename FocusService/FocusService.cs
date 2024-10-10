﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusService
{
    public class FocusService : DependencyObject
    {
        // https://learn.microsoft.com/en-us/windows/uwp/xaml-platform/custom-attached-properties
        // https://learn.microsoft.com/en-us/uwp/api/windows.ui.xaml.dependencyproperty.registerattached?view=winrt-26100
        public static readonly DependencyProperty IsFocusScopeProperty = DependencyProperty.RegisterAttached(
            "IsFocusScope",
            typeof(Boolean),
            typeof(FocusService),
            null);

        public static Boolean GetIsFocusScope(DependencyObject obj)
        {
            return (Boolean)obj.GetValue(IsFocusScopeProperty);
        }

        private static bool IsDescendantOf(DependencyObject parent, DependencyObject child)
        {
            if (parent == null || child == null)
            {
                return false;
            }

            var nextParent = VisualTreeHelper.GetParent(child);
            if (nextParent != null)
            {
                if (nextParent == parent)
                {
                    return true;
                }

                return IsDescendantOf(parent, nextParent);
            }

            return false;
        }

        public static void SetIsFocusScope(DependencyObject obj, Boolean value)
        {
            var element = obj as FrameworkElement;
            if (element == null)
            {
                throw new InvalidOperationException("IsFocusScope can only be set on a FrameworkElement");
            }

            if (value == true)
            {
                element.GettingFocus += (sender, args) =>
                {
                    // There is a bug in XAML where OldFocusedElement is null if focus wraps.
                    var isFocusEntering = args.OldFocusedElement == null || !IsDescendantOf(element, args.OldFocusedElement);
                    if (!isFocusEntering)
                    {
                        return;
                    }

                    if (TryGetDefaultFocusedDescendant(element) is FrameworkElement defaultFocusedElement)
                    {
                        // Lol this doesn't work when focus wraps! DERP!
                        if (args.TrySetNewFocusedElement(defaultFocusedElement))
                        {
                            args.Handled = true;
                        }
                    }
                };
            }
            else
            {
                // TODO: unregister?
            }

            obj.SetValue(IsFocusScopeProperty, value);
        }

        public static readonly DependencyProperty DefaultFocusedElementProperty = DependencyProperty.RegisterAttached(
            "DefaultFocusedElement",
            typeof(FrameworkElement),
            typeof(FocusService),
            null);

        // Returns the property value of the current element.
        // To get what we think the *actual* default focused element should be, use TryGetDefaultFocusedDescendant.
        public static FrameworkElement GetDefaultFocusedElement(DependencyObject obj)
        {
            return (FrameworkElement)obj.GetValue(DefaultFocusedElementProperty);
        }

        private static bool IsElementVisible(FrameworkElement element)
        {
            // We must walk the UI tree to determine if an element is visible or
            // not: if a parent is invisible, then the child is not visible either.
            var currentElement = element;
            while (currentElement != null)
            {
                if (currentElement.Visibility != Visibility.Visible)
                {
                    return false;
                }

                currentElement = VisualTreeHelper.GetParent(currentElement) as FrameworkElement;
            }

            return true;
        }

        private static bool IsFocusable(Control element)
        {
            // https://learn.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.control?view=winrt-26100#controls-and-focus
            var isVisible = element.Visibility == Visibility.Visible;
            var isEnabled = IsElementVisible(element);
            var isTabStop = element.IsTabStop;
            return isVisible && isEnabled && isTabStop;
        }

        // Return what we think the default focused descendant element of the current element should be.
        public static FrameworkElement TryGetDefaultFocusedDescendant(DependencyObject parent)
        {
            var currentElement = parent as FrameworkElement;
            while (true)
            {
                // Try getting the default focused element from the current element.
                if (currentElement.GetValue(DefaultFocusedElementProperty) is FrameworkElement defaultFocusedElement)
                {
                    currentElement = defaultFocusedElement;
                    continue;
                }

                // If the current element is focusable, don't check for focusable descendants.
                var isCurrentObjectFocusable = IsElementVisible(currentElement);
                var shouldFindFocusableDescendant = !isCurrentObjectFocusable;
                if (shouldFindFocusableDescendant && FocusManager.FindFirstFocusableElement(currentElement) is FrameworkElement firstFocusableElement)
                {
                    currentElement = firstFocusableElement;
                    continue;
                }

                // If we can't find a default focused element or a focusable element, we can't go any further.
                return currentElement;
            }
        }

        public static void SetDefaultFocusedElement(DependencyObject obj, FrameworkElement value)
        {
            obj.SetValue(DefaultFocusedElementProperty, value);
        }

        public static bool TryFocusDefaultElement(FrameworkElement element, FocusState focusMethod = FocusState.Programmatic)
        {
            if (element.GetValue(DefaultFocusedElementProperty) is FrameworkElement defaultFocusedElement)
            {
                return defaultFocusedElement.Focus(focusMethod);
            }

            return false;
        }
    }
}
