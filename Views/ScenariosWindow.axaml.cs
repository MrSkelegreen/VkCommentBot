using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.IO;
using System.Reactive.Joins;

namespace VkCommentBot.Views
{
    public partial class ScenariosWindow : Window
    {   
        public ScenariosWindow()
        {
            InitializeComponent();

#if DEBUG
            this.AttachDevTools();
#endif
            this.DeleteConfirmation.IsVisible = false;
        }

        public void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            this.DeleteConfirmation.IsVisible = true;
        }

        public void CancelDeletion(object sender, RoutedEventArgs e)
        {
            this.DeleteConfirmation.IsVisible = false;
        }

    }

}