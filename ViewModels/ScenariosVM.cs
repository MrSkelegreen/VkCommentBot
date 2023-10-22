using ReactiveUI;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using VkCommentBot.Entities;
using System.Linq;
using System;
using Avalonia.Controls.ApplicationLifetimes;
using VkCommentBot.Views;
using System.Reactive.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Platform.Storage;
using System.Drawing.Imaging;


namespace VkCommentBot.ViewModels
{
    public class ScenariosVM : ViewModelBase
    {
        private VkPost _vkPost;
        public VkPost SelectedVkPost 
        {
            get { return  _vkPost; }
            set { this.RaiseAndSetIfChanged(ref _vkPost, value); }
        }     

        private Scenario _selectedScenario;
        public Scenario SelectedScenario 
        {
            get { return _selectedScenario; }
            set { this.RaiseAndSetIfChanged(ref _selectedScenario, value); ChangeMinusBtnVisible(); }
        }

        private ObservableCollection<string> _messages;
        public ObservableCollection<string> Messages
        {
            get => _messages;
            set { this.RaiseAndSetIfChanged(ref _messages, value); }
        }
         
        private bool _isMinusBtnVisible;
        public bool IsMinusBtnVisible 
        {
            get { return _isMinusBtnVisible; }
            set { this.RaiseAndSetIfChanged(ref _isMinusBtnVisible, value); } 
        }

        private bool _isSwitchVisible;
        public bool IsSwitchVisible
        {
            get { return _isSwitchVisible; }
            set { this.RaiseAndSetIfChanged(ref _isSwitchVisible, value); }
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set { this.RaiseAndSetIfChanged(ref _isChecked, value); }
        }

        public Helper Helper { get; set; }

        public ScenariosVM(VkPost vkPost, Helper helper)
        {
            Messages = new ObservableCollection<string>();          

            Helper = helper;

            SelectedVkPost = vkPost;
            SelectedVkPost.ScenariosCollection.Clear();
            foreach (Scenario scenario in SelectedVkPost.Scenarios)
            {
                SelectedVkPost.ScenariosCollection.Add(scenario);
            }                        

            if (SelectedVkPost.PostStatus == "Включен")
            {
                IsChecked = true;
            }
            else IsChecked = false;

            if (SelectedVkPost.PostStatus == "Не активен") IsSwitchVisible = false;
            else IsSwitchVisible = true;

            LoadImages();

            SaveScenariosCommand = ReactiveCommand.Create(SaveScenarios);
            AddScenarioCommand = ReactiveCommand.Create(AddScenario);
            DeleteScenarioCommand = ReactiveCommand.Create(DeleteScenario);
            OpenPostsWindowCommand = ReactiveCommand.Create(OpenPostsWindow);
            MakePostDisabledCommand = ReactiveCommand.Create(MakePostDisabled);
            MakePostActiveCommand = ReactiveCommand.Create(MakePostActive);
            ChangePostStatusCommand = ReactiveCommand.Create(ChangePostStatus);
            DeletePostCommand = ReactiveCommand.Create(DeletePost);
            ChangeCommentImageCommand = ReactiveCommand.CreateFromTask(ChangeCommentImage);
            DeleteMessageCommand = ReactiveCommand.Create<string>(DeleteMessage);
            DeleteCommentImageCommand = ReactiveCommand.Create(DeleteCommentImage);
            IsMinusBtnVisible = false;                                         
          
        }       

        public ReactiveCommand<Unit, Unit> SaveScenariosCommand { get; set; }
        public void SaveScenarios() 
        {
          
                if(SelectedVkPost.PostStatus == "Не активен")
                {
                    SelectedVkPost.Scenarios.Clear();

                    foreach (var scenario in SelectedVkPost.ScenariosCollection)
                    {
                        SelectedVkPost.Scenarios.Add(scenario);
                    }

                    MakePostDisabled();
                }
                else
                {
                    try
                    {                     

                        DbContext db = new DbContext();

                        db.VkPosts.Include(p => p.Scenarios).ToList();

                        var findedPost = db.VkPosts.FirstOrDefault(p => p.VkId == SelectedVkPost.VkId);                                         

                        var dbScenarios = db.Scenarios.Where(s => s.PostId == SelectedVkPost.VkId).ToList(); 

                        foreach (var scenario in dbScenarios)
                        {
                            if(!SelectedVkPost.ScenariosCollection.Any(s => s.Id == scenario.Id))
                            {
                                var scenarioToDelete = db.Scenarios.FirstOrDefault(s => s.Id == scenario.Id);
                                db.Scenarios.Remove(scenarioToDelete);
                                db.SaveChanges();
                            }                  
                        }                     
                        
                        UpdatePostScenarios();

                        Messages.Add("Изменения сохранены");                        

                    }
                    catch (Exception e)
                    {
                        Messages.Add($"Ошибка базы данных:\n" +
                                              $"{e.Message}");   
                    }                
                }        
        }

        void UpdatePostScenarios()
        {
           DbContext db = new DbContext();

            var findedPost = db.VkPosts.FirstOrDefault(p => p.VkId == SelectedVkPost.VkId);

            findedPost.Scenarios.Clear();

            foreach (var scenario in SelectedVkPost.ScenariosCollection)
            {
                findedPost.Scenarios.Add(scenario);
            }

            findedPost.KeyWord = SelectedVkPost.KeyWord;
            db.VkPosts.Update(findedPost);
            db.SaveChanges();
            
        }

        public ReactiveCommand<Unit, Unit> DeleteScenarioCommand { get; set; }
        void DeleteScenario()
        {
            if(SelectedScenario != null)
            {
                SelectedVkPost.ScenariosCollection.Remove(SelectedScenario);
            }
        }

        public ReactiveCommand<Unit, Unit> AddScenarioCommand { get; set; }
        void AddScenario()
        {
            Scenario newScenario = new Scenario() { Title = "", Content = "", PostId = SelectedVkPost.VkId };
            SelectedVkPost.ScenariosCollection.Add(newScenario);
        }

        public ReactiveCommand<Unit, Unit> ChangePostStatusCommand { get; set; }
        public void ChangePostStatus()
        {
            if (SelectedVkPost.PostStatus == "Выключен") MakePostActive();
            else if (SelectedVkPost.PostStatus == "Включен") MakePostDisabled();
        }

        public ReactiveCommand<Unit, Unit> MakePostDisabledCommand { get; set; }
        public void MakePostDisabled()
        {
            if (SelectedVkPost.ScenariosCollection.Count > 0 && SelectedVkPost.KeyWord != "" && SelectedVkPost.KeyWord != null && SelectedVkPost.KeyWord != string.Empty)
            {
                try
                {
                    DbContext db = new DbContext();

                    var findedPost = db.VkPosts.FirstOrDefault(p => p.VkId == SelectedVkPost.VkId);

                    if (findedPost != null)
                    {
                        findedPost.PostStatus = "Выключен";
                        db.VkPosts.Update(findedPost);
                        db.SaveChanges();
                        SelectedVkPost.PostStatus = "Выключен";
                        IsChecked = false;
                    }
                    else
                    {
                        SelectedVkPost.PostStatus = "Выключен";
                        SelectedVkPost.Id = 0;
                        db.VkPosts.Add(SelectedVkPost);
                        db.SaveChanges();
                        IsSwitchVisible = true;
                    }
                    
                }
                catch(Exception e)
                {
                    IsChecked = true;
                    Messages.Add($"Ошибка базы данных:\n" +
                                         $"{e.Message}");
                }
            }
            else
            {
                IsChecked = true;
                Messages.Add("Ключевое слово или сценарии не заданы");
            }
        }

        public ReactiveCommand<Unit, Unit> MakePostActiveCommand { get; set; }
        public void MakePostActive()
        {

            if(SelectedVkPost.ScenariosCollection.Count > 0 && SelectedVkPost.KeyWord != "" && SelectedVkPost.KeyWord != null && SelectedVkPost.KeyWord != string.Empty)
            {
                try
                {
                    DbContext db = new DbContext();

                    var findedPost = db.VkPosts.FirstOrDefault(p => p.VkId == SelectedVkPost.VkId);

                    if (findedPost != null)
                    {
                        findedPost.PostStatus = "Включен";
                        db.VkPosts.Update(findedPost);
                        db.SaveChanges();
                        SelectedVkPost.PostStatus = "Включен";
                        IsChecked = true;
                    }
                    else
                    {
                        IsChecked = false;
                        Messages.Add("Пост не найден");
                    }
                   
                }
                catch(Exception e)
                {
                    IsChecked = false;
                    Messages.Add($"Ошибка базы данных:\n" +
                                         $"{e.Message}");
                }             
            }
            else
            {
                IsChecked = false;
                Messages.Add("Ключевое слово или сценарии не заданы");
            }
        }

        public ReactiveCommand<Unit, Unit> DeletePostCommand { get; set; }
        public void DeletePost()
        {
            
            DbContext db = new DbContext();

            try
            {
                var findedPost = db.VkPosts.FirstOrDefault(p => p.VkId == SelectedVkPost.VkId);
                db.VkPosts.Remove(findedPost);
                db.SaveChanges();
           
                SelectedVkPost.PostStatus = "Не активен";
                SelectedVkPost.Scenarios.Clear();
                SelectedVkPost.ScenariosCollection.Clear();
                SelectedVkPost.KeyWord = string.Empty;
            }
            catch(Exception e)
            {
                Messages.Add($"Ошибка базы данных:\n" +
                                         $"{e.Message}");
            }

            OpenPostsWindow();

        }

        public ReactiveCommand<Unit, Unit> OpenPostsWindowCommand { get; set; }
        void OpenPostsWindow()
        {           
            PostsWindow postsWindow= new PostsWindow(); //Инициализация нового окна
            postsWindow.DataContext = new PostsVM(Helper); //Установка контекста для нового окна          
            postsWindow.Show(); //Открыли новое окно
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lf) lf.MainWindow.Close(); //Закрыли старое              
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime) lifetime.MainWindow = postsWindow; //Сделали новое окно главным
        }

        public ReactiveCommand<Unit, Unit> ChangeCommentImageCommand { get; set; }
        public void LoadImages()
        {
            try
            {
                foreach (var scenario in SelectedVkPost.ScenariosCollection)
                {
                    if (scenario.CommentImage != null)
                    {
                        using (var ms = new MemoryStream(scenario.CommentImage))
                        {
                            using (System.Drawing.Image image = System.Drawing.Image.FromStream(ms))
                            {

                                ms.Position = 0;

                                Avalonia.Media.Imaging.Bitmap AvIrBitmap = new Avalonia.Media.Imaging.Bitmap(ms);

                                scenario.GridImage = AvIrBitmap;

                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Messages.Add($"Ошибка загрузки изображений:\n" +
                                     $"{e.Message}");
            }
            
        }
        public async Task ChangeCommentImage()
        {
           var file =  await PickImage();
            if (file is null) return;

            var filePath = file.TryGetLocalPath();

            var size = (await file.GetBasicPropertiesAsync()).Size;

            //Файл объёмом не более 50 Мбайт
            if (size < 52428800)
            {                        
                try
                {
                    using (var fileStream = File.OpenRead(filePath))
                    using (var image = System.Drawing.Image.FromStream(fileStream))
                    {                    

                        int gcd = GCD(image.Width, image.Height);

                        int numerator = image.Width / gcd;
                        int denominator = image.Height / gcd;

                        double quotient = (double)numerator / denominator;
                        double minRatio = 1.0 / 20.0;

                        //Соотношение сторон не менее 1:20.
                        if (quotient >= minRatio)
                        {
                            int sumIfPixels = image.Width + image.Height;
                            //Сумма ширины и высоты не более 14000
                            if (sumIfPixels <= 14000)
                            {
                                using (MemoryStream memory = new MemoryStream())
                                {
                                    image.Save(memory, ImageFormat.Jpeg);

                                    SelectedScenario.CommentImage = memory.ToArray();

                                    memory.Position = 0;
                                   
                                    Avalonia.Media.Imaging.Bitmap AvIrBitmap = new Avalonia.Media.Imaging.Bitmap(memory);

                                    SelectedScenario.GridImage = AvIrBitmap;

                                    Messages.Add($"Изображение изменено");
                                }
                            }
                            else
                            {
                                Messages.Add($"Сумма высоты и ширины должна быть не более 14000 пикселей.\n" +
                                                      $"Сумма пикселей выбранного изображения: {sumIfPixels}");
                            }                       

                        }
                        else
                        {
                            Messages.Add($"Соотношение сторон должно быть не менее 1:20.\n" +
                                                  $"Соотношение сторон выбранного изображения {numerator}:{denominator}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Messages.Add($"{e.Message}");
                }                               
            }
            else
            {
                Messages.Add("Файл должен быть объёмом не более 50 Мбайт");
            }
            
        }

        public async Task<IStorageFile?> PickImage()
        {

            try
            {
                if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow?.StorageProvider is not { } provider)
                    throw new NullReferenceException("Missing StorageProvider instance.");

                var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions()
                {
                    Title = "Выберите изображение",
                    AllowMultiple = false,
                    FileTypeFilter = new FilePickerFileType[] { new("") { Patterns = new[] { "*.png", "*.jpg", "*.gif", "*.jpeg" } } }
                });

                return files?.Count >= 1 ? files[0] : null;
            }
            catch(Exception e)
            {
                Messages.Add(e.Message);
                return null;
            }
            
        }

        public ReactiveCommand<Unit, Unit> DeleteCommentImageCommand { get; set; }
        public void DeleteCommentImage()
        {
            try
            {
                if (SelectedScenario.CommentImage != null)
                {

                    SelectedScenario.CommentImage = null;
                    Avalonia.Media.Imaging.Bitmap clearedImage = null!;
                    SelectedScenario.GridImage = clearedImage;
                    Messages.Add("Изображение удалено");
                }
            }
            catch(Exception e)
            {
                Messages.Add($"Ошибка при удалении изображения:\n{e.Message}");
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

        void ChangeMinusBtnVisible()
        {
            IsMinusBtnVisible = SelectedScenario == null ? false : true; 
        }

        //Возвращает наибольший общий делитель
        static int GCD(int a, int b)
        {
            if (a == 0)
            {
                return b;
            }
            else
            {
                while (b != 0)
                {
                    if (a > b)
                    {
                        a -= b;
                    }
                    else
                    {
                        b -= a;
                    }
                }

                return a;
            }
        }

    }
}
