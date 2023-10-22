using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VkCommentBot.Entities;
using VkNet;
using VkNet.Model;
using ReactiveUI;
using System.Reactive;
using VkCommentBot.Views;
using Avalonia.Controls.ApplicationLifetimes;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Reactive.Linq;


namespace VkCommentBot.ViewModels
{
    public class PostsVM : ViewModelBase
    {
        public DbContext db = new DbContext();
        public VkApi vk = new VkApi();
        public ObservableCollection<VkPost> Posts { get; set; }

        private VkPost _selectedPost;
        public VkPost SelectedPost
        {
            get { return _selectedPost; }
            set { this.RaiseAndSetIfChanged(ref _selectedPost, value); OpenPost(); }
        }

        private bool _botStatus;
        public bool BotStatus
        {
            get { return _botStatus; }
            set { this.RaiseAndSetIfChanged(ref _botStatus, value); }
        }

        public static bool staticBotStatus;
        public static ObservableCollection<string> Messages { get; set; }

        public static Random random = new Random();

        public Thread botThread;

        public bool firstStart = true;

        public Helper Helper { get; set; }

        private bool _isDGridVisible;
        public bool IsDGridVisible
        {
            get { return _isDGridVisible; }
            set { this.RaiseAndSetIfChanged(ref _isDGridVisible, value); }
        }

        private bool _isGroupSettingsVisible;
        public bool IsGroupSettingsVisible
        {
            get { return _isGroupSettingsVisible; }
            set { this.RaiseAndSetIfChanged(ref _isGroupSettingsVisible, value); }
        }

        private bool _isSettingsButtonVisible;
        public bool IsSettingsButtonVisible
        {
            get { return _isSettingsButtonVisible; }
            set { this.RaiseAndSetIfChanged(ref _isSettingsButtonVisible, value); }
        }

        private GroupSettings _groupSettings;
        public GroupSettings GroupSettings
        {
            get { return _groupSettings; }
            set { this.RaiseAndSetIfChanged(ref _groupSettings, value); }
        }

        public PostsVM(Helper helper)
        {

            Helper = helper;

            Posts = new ObservableCollection<VkPost>();
            Messages = new ObservableCollection<string>();

            GroupSettings = new GroupSettings();

            GroupSettings.owner_id = ConfigurationManager.AppSettings["GroupId"];
            GroupSettings.user_token = ConfigurationManager.AppSettings["UserToken"];
            GroupSettings.group_token = ConfigurationManager.AppSettings["GroupToken"];

            LoadPostsCommand = ReactiveCommand.Create(LoadPostsAsync);
            OpenPostCommand = ReactiveCommand.Create(OpenPost);
            CheckBotStatusCommand = ReactiveCommand.CreateFromTask<Helper>(CheckBotStatusTask);
            ChangeSettingsVisibilityCommand = ReactiveCommand.Create(ChangeSettingsVisibility);
            ChangeBotStatusCommand = ReactiveCommand.CreateFromTask<Helper>(ChangeBotStatus);
            ChangeGroupSettingsCommand = ReactiveCommand.CreateFromTask<GroupSettings>(ChangeGroupSettings);
            DeleteMessageCommand = ReactiveCommand.Create<string>(DeleteMessage);

            IsDGridVisible = false;
            IsGroupSettingsVisible = true;
            IsSettingsButtonVisible = false;

            if (Helper.IsFirstStart)
            {
                Helper.IsFirstStart = false;
                CheckBotStatusCommand.Execute(helper);
            }
            else
            {
                IsDGridVisible = true;
                Posts = Helper.VkPosts;
                IsGroupSettingsVisible = false;
                IsSettingsButtonVisible = true;
                CheckBotStatusCommand.Execute(Helper);
            }

        }

        public ReactiveCommand<Unit, Unit> LoadPostsCommand { get; set; }
        public void LoadPostsAsync()
        {
            //Проверка на заполненность всех полей
            if (GroupSettings.owner_id != "" && GroupSettings.user_token != "" && GroupSettings.owner_id != null && GroupSettings.user_token != null && GroupSettings.group_token != "" && GroupSettings.group_token != null)
            {
                //Проверка id группы
                if (GroupSettings.owner_id.ToCharArray()[0] == '-' && GroupSettings.owner_id.ToCharArray().Length == 10)
                {

                    try
                    {
                        Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                        configuration.AppSettings.Settings.Remove("GroupId");
                        configuration.AppSettings.Settings.Remove("UserToken");
                        configuration.AppSettings.Settings.Remove("GroupToken");

                        configuration.AppSettings.Settings.Add("GroupId", GroupSettings.owner_id);
                        configuration.AppSettings.Settings.Add("UserToken", GroupSettings.user_token);
                        configuration.AppSettings.Settings.Add("GroupToken", GroupSettings.group_token);

                        configuration.Save(ConfigurationSaveMode.Full, true);
                        ConfigurationManager.RefreshSection("appSettings");

                        Posts.Clear();

                        if (CheckGroupToken() && CheckUserToken() && CheckDbConnection())
                        {
                            WallGetParams wallSearchParams = new WallGetParams() { OwnerId = long.Parse(GroupSettings.owner_id) };

                            WallGetObject? posts = new WallGetObject();

                            posts = vk.Wall.Get(wallSearchParams);

                            var postsCollection = posts.WallPosts;

                            List<VkPost> dbPosts = new List<VkPost>();

                            dbPosts = db.VkPosts.Include(p => p.Scenarios).ToList();

                            foreach (var post in postsCollection)
                            {

                                VkPost findedPost = dbPosts.FirstOrDefault(p => p.VkId == post.Id);

                                if (findedPost != null)
                                {
                                    Posts.Add(new VkPost() { VkId = post.Id, PostText = post.Text, PostStatus = findedPost.PostStatus, Scenarios = findedPost.Scenarios, KeyWord = findedPost.KeyWord });
                                }
                                else
                                {
                                    Posts.Add(new VkPost() { VkId = post.Id, PostText = post.Text });
                                }

                            }

                            if (!IsDGridVisible) { IsDGridVisible = true; }
                            if (!IsSettingsButtonVisible) { IsSettingsButtonVisible = true; }
                            IsGroupSettingsVisible = false;

                            ChangeGroupSettingsCommand.Execute(GroupSettings);

                        }
                    }
                    catch (Exception e)
                    {
                        Messages.Add($"{e.Message}");
                    }
                }
                else
                {
                    Messages.Add("id группы должно начинаться с (-). Например -184613138");
                }
            }
            else
            {
                Messages.Add("Одно из полей настроек не заполнено");
            }

        }

        public bool CheckGroupToken()
        {
            try
            {
                vk.Authorize(new ApiAuthParams
                {
                    AccessToken = GroupSettings.group_token
                });

                ulong groupId = ulong.Parse(GroupSettings.owner_id.Substring(1));

                var status = vk.Groups.GetOnlineStatus(groupId);

                return true;
            }
            catch
            {
                Messages.Add("Неверный токен группы");
                return false;
            }

        }

        public bool CheckUserToken()
        {
            try
            {
                vk.Authorize(new ApiAuthParams
                {
                    AccessToken = GroupSettings.user_token
                });

                IEnumerable<long> userIds = new List<long>();

                vk.Users.Get(userIds);

                return true;
            }
            catch
            {
                Messages.Add("Неверный токен пользователя");
                return false;
            }
        }

        public bool CheckDbConnection()
        {
            if (db.Database.CanConnect())
            {
                return true;
            }
            else
            {
                Messages.Add("Ошибка подключения к базе данных (Can't connent)");
                return false;
            }
        }

        public ReactiveCommand<Unit, Unit> OpenPostCommand { get; set; }
        public void OpenPost()
        {
            if (SelectedPost != null)
            {
                Helper.VkPosts = Posts;

                ScenariosWindow scenariosWindow = new ScenariosWindow(); //Инициализация нового окна
                scenariosWindow.DataContext = new ScenariosVM(SelectedPost, Helper); //Установка контекста для нового окна          
                scenariosWindow.Show(); //Открыли новое окно
                if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lf) lf.MainWindow.Close(); //Закрыли старое              
                if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime) lifetime.MainWindow = scenariosWindow; //Сделали новое окно главным                            
            }
        }

        public ReactiveCommand<Helper, Unit> CheckBotStatusCommand { get; set; }
        static async Task CheckBotStatusTask(Helper helper)
        {
            using var client = new HttpClient();
            try
            {

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var apiAddress = ConfigurationManager.AppSettings["ApiAddress"];
                var url = $"http://{apiAddress}/checkbotstatus";

                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var readedResponse = await response.Content.ReadAsStringAsync();

                if (readedResponse == "true")
                {
                    helper.BotStatus = true;
                }
                else if (readedResponse == "false")
                {
                    helper.BotStatus = false;
                }

            }
            catch (Exception e)
            {
                Messages.Add($"Ошибка проверки статуса бота:\n{e.Message}");
            }
        }

        public ReactiveCommand<Helper, Unit> ChangeBotStatusCommand { get; set; }
        static async Task ChangeBotStatus(Helper helper)
        {
            using var client = new HttpClient();
            try
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var apiAddress = ConfigurationManager.AppSettings["ApiAddress"];
                var apiKey = ConfigurationManager.AppSettings["ApiKey"];
                var url = $"http://{apiAddress}/changebotstatus?api_key={apiKey}";

                StringContent stringContent = new StringContent("");

                HttpResponseMessage response = await client.PutAsync(url, stringContent);
                response.EnsureSuccessStatusCode();

            }
            catch (Exception e)
            {
                Messages.Add($"Ошибка изменения статуса бота:\n{e.Message}");
            }
        }

        public ReactiveCommand<GroupSettings, Unit> ChangeGroupSettingsCommand { get; set; }
        static async Task ChangeGroupSettings(GroupSettings settings)
        {
            using var client = new HttpClient();
            try
            {

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var apiAddress = ConfigurationManager.AppSettings["ApiAddress"];
                var apiKey = ConfigurationManager.AppSettings["ApiKey"];
                var url = $"http://{apiAddress}/changegroupsettings?group_id={settings.owner_id}&group_token={settings.group_token}&user_token={settings.user_token}&api_key={apiKey}";

                StringContent stringContent = new StringContent("");

                HttpResponseMessage response = await client.PutAsync(url, stringContent);
                response.EnsureSuccessStatusCode();

            }
            catch (Exception e)
            {
                Messages.Add($"Ошибка изменения настроек группы:\n{e.Message}");
            }
        }

        public ReactiveCommand<string, Unit> DeleteMessageCommand { get; set; }
        public void DeleteMessage(string message)
        {
            var findedMessage = Messages.FirstOrDefault(m => m == message);

            if (findedMessage != null)
            {
                Messages.Remove(findedMessage);
            }
        }

        public ReactiveCommand<Unit, Unit> ChangeSettingsVisibilityCommand { get; set; }
        public void ChangeSettingsVisibility()
        {
            if (IsSettingsButtonVisible)
            {
                IsSettingsButtonVisible = false;
                IsGroupSettingsVisible = true;
            }
            else
            {
                IsSettingsButtonVisible = true;
                IsGroupSettingsVisible = false;
            }
        }

    }
}
