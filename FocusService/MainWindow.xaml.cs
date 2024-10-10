using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FocusService
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            myButton.Content = "Clicked";
        }

        private void StackPanel_LosingFocus(UIElement sender, LosingFocusEventArgs args)
        {
            // Wrap any focus leaving the app the appropriate place manually.
            var isFocusLeavingTheApp = args.NewFocusedElement == null;
            if (isFocusLeavingTheApp)
            {
                var lastElement = FocusManager.FindLastFocusableElement(sender);
                var firstElement = FocusManager.FindFirstFocusableElement(sender);

                var isLastElement = (DependencyObject)args.OldFocusedElement == lastElement;
                if (isLastElement)
                {
                    System.Diagnostics.Debug.WriteLine("Setting focus to first element...");
                    if (args.TrySetNewFocusedElement(firstElement))
                    {
                        System.Diagnostics.Debug.WriteLine("Focus set to first element.");
                        args.Handled = true;
                    }
                }

                var isFirstElement = (DependencyObject)args.OldFocusedElement == firstElement;
                if (isFirstElement)
                {
                    System.Diagnostics.Debug.WriteLine("Setting focus to last element...");
                    if (args.TrySetNewFocusedElement(lastElement))
                    {
                        System.Diagnostics.Debug.WriteLine("Focus set to last element.");
                        args.Handled = true;
                    }
                }
            }
        }
    }
}
