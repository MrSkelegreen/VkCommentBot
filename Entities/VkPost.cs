using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace VkCommentBot.Entities;

public partial class VkPost
{
    public int Id { get; set; }

    public long? VkId { get; set; } 

    public string PostStatus { get; set; } = null!;

    public string? KeyWord { get; set; }

    public virtual ICollection<Scenario> Scenarios { get; set; } = new List<Scenario>();

    [NotMapped]
    public string? PostText { get; set; }

    [NotMapped]
    public ObservableCollection<Scenario> ScenariosCollection { get; set; } = new ObservableCollection<Scenario>();

    public VkPost()
    {
        PostStatus = "Не активен";       
    }

}
