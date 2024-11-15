using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
            new PropertyMetadata(false));

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

        private static void HandleGettingFocus(UIElement sender, GettingFocusEventArgs args)
        {
            // There is a bug in XAML where OldFocusedElement is null if focus wraps.
            var isFocusEntering = args.OldFocusedElement == null || !IsDescendantOf(sender, args.OldFocusedElement);
            if (!isFocusEntering)
            {
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Focus is entering ({args.OriginalSource.GetType()}; {args.OldFocusedElement?.GetType()} {(args.OldFocusedElement as FrameworkElement)?.Name} -> {args.NewFocusedElement.GetType()} {(args.NewFocusedElement as FrameworkElement).Name})...");
            if (TryGetDefaultFocusedDescendant(sender) is FrameworkElement defaultFocusedElement)
            {
                // Don't double-run our handler.
                if (defaultFocusedElement == args.NewFocusedElement)
                {
                    System.Diagnostics.Debug.WriteLine($"Already focused on default element {defaultFocusedElement.GetType()} {defaultFocusedElement.Name}.");
                    return;
                }

                // NOTE: this will not work correctly if focus is
                // *wrapping* to the first or last element in a
                // window/XAML island. Because XAML treats that as focus
                // "entering" the window for the first time, & first
                // focus cannot be redirected (due to what's presumably
                // a bug).
                //
                // Windows can handle this by marking their top-level
                // Grid/StackPanel as TabFocusNavigation="Cycle".
                //
                // See MainWindow.xaml.
                System.Diagnostics.Debug.WriteLine($"Setting focus to default element {defaultFocusedElement.GetType()} {defaultFocusedElement.Name}...");
                if (args.TrySetNewFocusedElement(defaultFocusedElement))
                {
                    System.Diagnostics.Debug.WriteLine($"Done!");
                    args.Handled = true;
                }
            }
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
                element.GettingFocus += HandleGettingFocus;
            }
            else
            {
                element.GettingFocus -= HandleGettingFocus;
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

        public static void SetDefaultFocusedElement(DependencyObject obj, FrameworkElement value)
        {
            obj.SetValue(DefaultFocusedElementProperty, value);
        }

        public static readonly DependencyProperty FocusedIndexProperty = DependencyProperty.RegisterAttached(
            "FocusedIndex",
            typeof(int),
            typeof(FocusService),
            new PropertyMetadata(0));

        public static int GetFocusedIndex(DependencyObject obj)
        {
            return (int)obj.GetValue(FocusedIndexProperty);
        }

        public static void SetFocusedIndex(DependencyObject obj, int value)
        {
            // TODO: should we check that keyboard nav is true?
            //var repeater = obj as ItemsRepeater;
            //if (repeater == null)
            //{
            //    throw new InvalidOperationException("FocusedIndex can only be set on an ItemsRepeater");
            //}

            obj.SetValue(FocusedIndexProperty, value);
        }

        public static readonly DependencyProperty RememberFocusedIndexProperty = DependencyProperty.RegisterAttached(
            "RememberFocusedIndex",
            typeof(Boolean),
            typeof(FocusService),
            new PropertyMetadata(false));

        public static Boolean GetRememberFocusedIndex(DependencyObject obj)
        {
            return (Boolean)obj.GetValue(RememberFocusedIndexProperty);
        }

        private static DependencyObject TryGetImmediateChildOfAncestor(DependencyObject ancestor, DependencyObject descendant)
        {
            if (ancestor == null || descendant == null)
            {
                return null;
            }

            var currentElement = descendant;
            while (true)
            {
                var parent = VisualTreeHelper.GetParent(currentElement);
                if (parent == null)
                {
                    return null;
                }

                if (parent == ancestor)
                {
                    return currentElement;
                }

                currentElement = parent;
            }
        }

        private static int? TryGetImmediateChildIndexOfAncestor(DependencyObject ancestor, DependencyObject descendant)
        {
            var immediateChild = TryGetImmediateChildOfAncestor(ancestor, descendant);
            if (immediateChild == null)
            {
                return null;
            }

            var childCount = VisualTreeHelper.GetChildrenCount(ancestor);
            for (var index = 0; index < childCount; index++)
            {
                var child = VisualTreeHelper.GetChild(ancestor, index);
                if (child == immediateChild)
                {
                    return index;
                }
            }

            return null;
        }

        public static void SetRememberFocusedIndex(DependencyObject obj, Boolean value)
        {
            var element = obj as FrameworkElement;
            if (element == null)
            {
                throw new InvalidOperationException("IsFocusScope can only be set on a FrameworkElement");
            }

            if (value == true)
            {
                element.GotFocus += (sender, args) =>
                {
                    var currentIndex = TryGetImmediateChildIndexOfAncestor(obj, (DependencyObject)args.OriginalSource);
                    SetFocusedIndex(obj, (int)currentIndex);
                };
            }
            else
            {
                // TODO unregister
            }

            obj.SetValue(RememberFocusedIndexProperty, value);
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
                // If the current element is focusable, don't check for focusable descendants.
                var control = currentElement as Control;
                var isCurrentObjectFocusable = control != null && IsFocusable(control);
                var shouldFindFocusableDescendant = !isCurrentObjectFocusable;
                if (shouldFindFocusableDescendant)
                {
                    // If we've chosen to remember the focused index for this
                    // element, use that.
                    var shouldUseFocusedIndex = GetRememberFocusedIndex(currentElement);
                    if (shouldUseFocusedIndex)
                    {
                        var focusedIndex = GetFocusedIndex(currentElement);
                        if (focusedIndex >= 0)
                        {
                            var childCount = VisualTreeHelper.GetChildrenCount(currentElement);
                            if (focusedIndex >= childCount)
                            {
                                throw new InvalidOperationException($"FocusedIndex {focusedIndex} is out of range {childCount}");
                            }

                            var child = VisualTreeHelper.GetChild(currentElement, focusedIndex);
                            currentElement = child as FrameworkElement;
                            continue;
                        }
                    }
                }

                // Try getting the default focused element from the current element.
                if (currentElement.GetValue(DefaultFocusedElementProperty) is FrameworkElement defaultFocusedElement)
                {
                    currentElement = defaultFocusedElement;
                    continue;
                }

                if (shouldFindFocusableDescendant)
                {
                    if (FocusManager.FindFirstFocusableElement(currentElement) is FrameworkElement firstFocusableElement)
                    {
                        currentElement = firstFocusableElement;
                        continue;
                    }
                }

                // If we can't find a default focused element or a focusable element, we can't go any further.
                return currentElement;
            }
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
