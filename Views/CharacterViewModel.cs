using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;

namespace WolfGame.Avalonia.ViewModels;

public partial class CharacterViewModel : ObservableObject
{
    [ObservableProperty] private string _id = "";
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private string _status = "normal";
    [ObservableProperty] private bool _isAlive = true;
    [ObservableProperty] private bool _isThinking;

    public double X { get; set; }
    public double Y { get; set; }
    [ObservableProperty] private double _pixelX;
    [ObservableProperty] private double _pixelY;

    [ObservableProperty] private string _speechBubbleShort = "";
    [ObservableProperty] private bool _isSpeaking;
    [ObservableProperty] private bool _isSpeechBubbleTruncated;
    public string SpeechBubbleFull { get; private set; } = "";
    public double Opacity => IsAlive ? 1.0 : 0.45;
    [ObservableProperty] private int _turnNumber;
    public bool HasTurn => TurnNumber > 0;

    partial void OnTurnNumberChanged(int value) => OnPropertyChanged(nameof(HasTurn));

    public string? Image
    {
        get
        {
            string[] candidates;
            if (!IsAlive)
                candidates = new[] { $"{Id}_death.gif", $"{Id}_idle.gif", $"{Id}_normal.gif" };
            else
                candidates = new[] { $"{Id}_{Status}.gif", $"{Id}_idle.gif", $"{Id}_normal.gif" };

            string baseDir = Path.Combine(AppContext.BaseDirectory, "Config", "sprites", Id);
            foreach (var file in candidates)
            {
                var fullPath = Path.Combine(baseDir, file);
                if (File.Exists(fullPath))
                    return fullPath;
            }
            return null;
        }
    }

    public string NamePlateColor => IsAlive ? "#000000" : "#444444";

    public void SetSpeech(string text)
    {
        SpeechBubbleFull = text;
        const int MaxChars = 70;
        if (text.Length > MaxChars)
        {
            SpeechBubbleShort = text[..MaxChars] + "…";
            IsSpeechBubbleTruncated = true;
        }
        else
        {
            SpeechBubbleShort = text;
            IsSpeechBubbleTruncated = false;
        }
        IsSpeaking = true;
    }

    public void ClearSpeech() => IsSpeaking = false;

    partial void OnStatusChanged(string value) => OnPropertyChanged(nameof(Image));
    partial void OnIsThinkingChanged(bool value) => OnPropertyChanged(nameof(Image));
    partial void OnIsAliveChanged(bool value)
    {
        OnPropertyChanged(nameof(Image));
        OnPropertyChanged(nameof(NamePlateColor));
        OnPropertyChanged(nameof(Opacity));
    }
}