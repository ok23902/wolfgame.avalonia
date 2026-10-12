using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using mano;
using WolfGame.Avalonia.ViewModels;

namespace WolfGame.Avalonia.Views;

public partial class MainWindow : Window
{
    private bool _initialized;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();

        Opened += async (_, _) =>
        {
            if (DataContext is not MainWindowViewModel vm) return;
            if (_initialized) return;
            _initialized = true;

            vm.ScrollToBottom += () =>
            {
                if (vm.Events.Count > 0)
                    EventList.ScrollIntoView(vm.Events[^1]);
            };

            try
            {
                await vm.InitializeAsync();
                vm.UpdateCanvasSize(CharacterArea.Bounds.Width, CharacterArea.Bounds.Height);
            }
            catch (Exception ex)
            {
                vm.ShowFatalError(ex);
            }
        };

        CharacterArea.SizeChanged += (_, _) =>
        {
            if (DataContext is MainWindowViewModel vm)
                vm.UpdateCanvasSize(CharacterArea.Bounds.Width, CharacterArea.Bounds.Height);
        };
    }

    private void OnSubmitText(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm) vm.SubmitText();
    }

    private void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is MainWindowViewModel vm)
        {
            vm.SubmitText();
            e.Handled = true;
        }
    }

    private void OnChoiceClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button b && b.DataContext is ChoiceOption opt
            && DataContext is MainWindowViewModel vm)
        {
            vm.SubmitChoice(opt);
        }
    }

    private void OnSpeechBubbleClick(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Control ctl && ctl.DataContext is CharacterViewModel cvm
            && DataContext is MainWindowViewModel vm
            && cvm.IsSpeechBubbleTruncated)
        {
            vm.ShowBubbleDetail(cvm);
            e.Handled = true;
        }
    }

    private void OnBubbleDetailClose(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm) vm.CloseBubbleDetail();
    }

    private void OnBubbleDetailBackgroundClick(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source == sender && DataContext is MainWindowViewModel vm)
            vm.CloseBubbleDetail();
    }

    private void OnStartGameClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm) vm.RequestStartGame();
    }

    private void OnProceedNextRound(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm) vm.ProceedToNextRound();
    }

    private void OnResumeRateLimit(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm) vm.ResumeRateLimit();
    }

    private void OnAbortRound(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm) vm.AbortRound();
    }
}