# Focus Service

> [!NOTE]
> This service is implemented as a demo to aid discussion with the XAML team.

This is an example implementation of a WPF-style focus helper for WinAppSDK
(formerly UWP) XAML. They help you write **containers that work well with arrow
keys.**

Specifically, these tools help you:

* **Add focus "memory" to any container** - when I tab away from your container,
  remember that focus so that I may return to it when I tab back.
* **Support default-focused elements** for complicated controls - e.g. in a
  dialog, reliably put default focus on the primary button.

## Usage

The tool is best used through XAML attached properties. To make an arbitrary
panel in the UI "fully" keyboard accessible:

```xml
<StackPanel
    Style="{StaticResource CardStyle}"
    XYFocusKeyboardNavigation="Enabled"
    TabFocusNavigation="Once"
    local:FocusService.RememberFocusedIndex="True"
    local:FocusService.DefaultFocusedElement="{x:Bind ButtonB}"
    local:FocusService.IsFocusScope="True">
    <Button>Remember focus</Button>
    <Button x:Name="ButtonB">Default</Button>
    <Button>Test</Button>
    <Button>Test</Button>
</StackPanel>
```

* `XYFocusKeyboardNavigation="Enabled"` - builtin to enable arrow keys through
  the container.
* `TabFocusNavigation="Once"` - only take tab focus once for the entire
  container.
* `local:FocusService.RememberFocusedIndex="True"` - remember the last-focused
  element in the container & refocus it when focus returns.
* Optional: `local:FocusService.DefaultFocusedElement="..."` - set a default
  first focus for the container (useful for complicated UI, e.g. a dialog with a
  default button)
* `local:FocusService.IsFocusScope="True"` - attach a `GettingFocus` handler
  that automatically

Also required for accessibility, but not implemented here (for laziness):

* **Size of set/Pos in set** (UIA properties)
* **Home & end support**

> [!NOTE]
> None of the `FocusService` flags will have any effect by default unless you add `local:FocusService.IsFocusScope="True"`.
