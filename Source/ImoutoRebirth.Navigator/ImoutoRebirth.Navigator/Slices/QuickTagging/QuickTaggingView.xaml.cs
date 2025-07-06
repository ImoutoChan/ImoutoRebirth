using MahApps.Metro.Controls.Dialogs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImoutoRebirth.Navigator.Slices.QuickTagging;

/// <summary>
///     Interaction logic for FileInfoView.xaml
/// </summary>
public partial class QuickTaggingView : UserControl
{
    public QuickTaggingView()
    {
        InitializeComponent();
    }

    internal QuickTaggingVM DataContextVM => (QuickTaggingVM)DataContext;

    private void SearchTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Down)
        {
            if (FoundTagsListBox.Items.Count == 0)
                return;

            FoundTagsListBox.Focus();
            FoundTagsListBox.SelectedIndex = 0;

            var item = (ListBoxItem)FoundTagsListBox.ItemContainerGenerator.ContainerFromIndex(0);
            item?.Focus();

            e.Handled = true;
        }
    }

    private void FoundTagsListBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var selected = FoundTagsListBox.SelectedItem;

            if (selected == null) 
                return;

            DataContextVM.SelectTagCommand.Execute(selected);
            SearchTextBox.Focus();
            e.Handled = true;
        }
        else if (e.Key is >= Key.A and <= Key.Z or >= Key.D0 and <= Key.D9 or >= Key.NumPad0 and <= Key.NumPad9)
        {
            SearchTextBox.Focus();
            VirtualKeyboard.PressAndRelease(e.Key);
            e.Handled = true;
        }
        else if (e.Key is Key.Back)
        {
            SearchTextBox.Focus();
            VirtualKeyboard.PressAndRelease(e.Key, VirtualKeyboard.GetPressedModifiers());
            e.Handled = true;
        }
    }

    private async void QuickTaggingView_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        // apply selected
        if (e.Key is Key.Enter && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            await DataContextVM.ApplySelectedTagsCommand.ExecuteAsync(null);
            e.Handled = true;
        }

        // create pack
        if (e.Key is Key.Right && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            DataContextVM.CreatePackCommand.Execute(null);
            e.Handled = true;
        }

        // clear selected
        if (e.Key is Key.X && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            DataContextVM.ClearSelectedTagsCommand.Execute(null);
            e.Handled = true;
        }

        // clear packs
        if (e.Key is Key.X && Keyboard.Modifiers.HasFlag(ModifierKeys.Control | ModifierKeys.Shift))
        {
            DataContextVM.ClearTagPacksCommand.Execute(null);
            e.Handled = true;
        }

        // select the next pack set
        if (e.Key is Key.Space && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            DataContextVM.AvailableTagPacksSets.SelectNextCommand.Execute(null);
            e.Handled = true;
        }

        // select the previous pack set
        if (e.Key is Key.Space && Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
        {
            DataContextVM.AvailableTagPacksSets.SelectPreviousCommand.Execute(null);
            e.Handled = true;
        }

        // apply packs
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            var numberedKey = GetNumberedOrLetterKey(e.Key);

            if (numberedKey != null)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                {
                    DataContextVM.UndoPackCommand.Execute(numberedKey);
                }
                else
                {
                    DataContextVM.ApplyPackCommand.Execute(numberedKey);
                }
                e.Handled = true;
            }
        }
    }

    private async void QuickTaggingView_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        await Task.Delay(50);
        if (Visibility == Visibility.Visible)
        {
            if (SearchTextBox.Focus()) 
                return;

            var repeatTimes = 10;
            while (!SearchTextBox.Focus() && repeatTimes-- > 0)
            {
                await Task.Delay(50);
            }
        }
    }

    private static char? GetNumberedOrLetterKey(Key key)
        => key switch
        {
            >= Key.D0 and <= Key.D9 => (char)('0' + (key - Key.D0)),
            >= Key.NumPad0 and <= Key.NumPad9 => (char)('0' + (key - Key.NumPad0)),
            >= Key.A and <= Key.Z => (char)('a' + (key - Key.A)),
            _ => null
        };

    private void TogglePanelPosition(object sender, RoutedEventArgs e)
        => View.VerticalAlignment
            = View.VerticalAlignment == VerticalAlignment.Top
                ? VerticalAlignment.Bottom
                : VerticalAlignment.Top;

    private async void RenameCurrentSet(object sender, RoutedEventArgs e)
    {
        var parentWindow = Window.GetWindow(this) as MainWindow;

        var result = await parentWindow.ShowInputAsync("Rename set", "New title");

        if (result == null)
            return;

        DataContextVM.RenameSelectedTagsPacksSetCommand.Execute(result);
    }
}
