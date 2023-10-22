using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VkCommentBot.Entities
{
    public class Helper : ReactiveObject
    {
        private bool _botStatus;
        public bool BotStatus
        {
            get { return _botStatus; }
            set { this.RaiseAndSetIfChanged(ref _botStatus, value); }
        }

        private bool _isFirstStart;
        public bool IsFirstStart
        {
            get { return _isFirstStart; }
            set { this.RaiseAndSetIfChanged(ref _isFirstStart, value); }
        }

        private ObservableCollection<VkPost> _vkPosts;
        public ObservableCollection<VkPost> VkPosts
        {
            get { return _vkPosts; }
            set { this.RaiseAndSetIfChanged(ref _vkPosts, value); }
        }

        public Helper()
        {
            this.BotStatus = false;
            this.IsFirstStart = true;
            this.VkPosts = new ObservableCollection<VkPost>();
        }
    }
}
