using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using VkCommentBot.Entities;
using VkCommentBot.ViewModels;
using VkCommentBot.Views;

namespace VkCommentBot
{
    public partial class App : Application
    {

        public Helper Helper { get; set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {

            Helper = new Helper();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new PostsWindow
                {                    
                    DataContext = new PostsVM(Helper),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}