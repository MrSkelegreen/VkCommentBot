using ReactiveUI;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Drawing.Imaging;
using VkCommentBot.ViewModels;

namespace VkCommentBot.Entities;

public partial class Scenario : ViewModelBase
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public long? PostId { get; set; }

    public virtual VkPost Post { get; set; } = null!;

    public byte[]? CommentImage { get; set; }

    [NotMapped]
    private System.Drawing.Bitmap irBitmap = new System.Drawing.Bitmap(256, 192, PixelFormat.Format24bppRgb);

    [NotMapped]
    private Avalonia.Media.Imaging.Bitmap _gridImage = null!;

    [NotMapped]
    public Avalonia.Media.Imaging.Bitmap GridImage
    {
        get => _gridImage;
        set { this.RaiseAndSetIfChanged(ref _gridImage, value);}
    }
       
}
