using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PWManager.Avalonia.Services;
using System;
using System.Security.Cryptography;
using System.Text;

namespace PWManager.Avalonia.ViewModels;

public partial class PasswordGeneratorViewModel : ViewModelBase
{
    private readonly ClipboardService _clipboardService;

    [ObservableProperty] private string _generatedPassword = string.Empty;
    [ObservableProperty] private bool _isPasswordVisible;
    [ObservableProperty] private int _length = 20;
    [ObservableProperty] private bool _useUppercase = true;
    [ObservableProperty] private bool _useLowercase = true;
    [ObservableProperty] private bool _useNumbers = true;
    [ObservableProperty] private bool _useSymbols = true;
    [ObservableProperty] private int _strengthValue;
    [ObservableProperty] private string _strengthLabel = string.Empty;

    public IBrush StrengthBrush => StrengthLabel switch
    {
        "Weak"       => new SolidColorBrush(Color.Parse("#E05252")),
        "Fair"       => new SolidColorBrush(Color.Parse("#FFC107")),
        "Strong"     => new SolidColorBrush(Color.Parse("#8BC34A")),
        _            => new SolidColorBrush(Color.Parse("#4CAF50")) // Very Strong
    };

    public int StrengthBarCount => StrengthLabel switch
    {
        "Very Strong" => 5,
        "Strong"      => 4,
        "Fair"        => 3,
        _             => 2
    };

    public double Bar1Opacity => StrengthBarCount >= 1 ? 1.0 : 0.2;
    public double Bar2Opacity => StrengthBarCount >= 2 ? 1.0 : 0.2;
    public double Bar3Opacity => StrengthBarCount >= 3 ? 1.0 : 0.2;
    public double Bar4Opacity => StrengthBarCount >= 4 ? 1.0 : 0.2;
    public double Bar5Opacity => StrengthBarCount >= 5 ? 1.0 : 0.2;

    public PasswordGeneratorViewModel(ClipboardService clipboardService)
    {
        _clipboardService = clipboardService;
        GenerateCommand.Execute(null);
    }

    [RelayCommand]
    private void Generate()
    {
        GeneratedPassword = GeneratePassword(Length, UseUppercase, UseLowercase, UseNumbers, UseSymbols);
        UpdateStrength();
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task CopyAsync()
    {
        if (!string.IsNullOrEmpty(GeneratedPassword))
            await _clipboardService.SetTextAsync(GeneratedPassword);
    }

    partial void OnLengthChanged(int value) => Generate();
    partial void OnUseUppercaseChanged(bool value) => Generate();
    partial void OnUseLowercaseChanged(bool value) => Generate();
    partial void OnUseNumbersChanged(bool value) => Generate();
    partial void OnUseSymbolsChanged(bool value) => Generate();

    private void UpdateStrength()
    {
        int score = 0;
        if (UseUppercase) score++;
        if (UseLowercase) score++;
        if (UseNumbers) score++;
        if (UseSymbols) score++;
        if (Length >= 16) score++;
        if (Length >= 24) score++;

        StrengthValue = (int)((score / 6.0) * 100);
        StrengthLabel = score switch
        {
            <= 2 => "Weak",
            3    => "Fair",
            4    => "Strong",
            _    => "Very Strong"
        };

        OnPropertyChanged(nameof(StrengthBrush));
        OnPropertyChanged(nameof(StrengthBarCount));
        OnPropertyChanged(nameof(Bar1Opacity));
        OnPropertyChanged(nameof(Bar2Opacity));
        OnPropertyChanged(nameof(Bar3Opacity));
        OnPropertyChanged(nameof(Bar4Opacity));
        OnPropertyChanged(nameof(Bar5Opacity));
    }

    public static string GeneratePassword(int length, bool upper, bool lower, bool numbers, bool symbols)
    {
        var chars = new StringBuilder();
        if (upper) chars.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        if (lower) chars.Append("abcdefghijklmnopqrstuvwxyz");
        if (numbers) chars.Append("0123456789");
        if (symbols) chars.Append("!@#$%^&*()-+=._:;,");
        if (chars.Length == 0) chars.Append("abcdefghijklmnopqrstuvwxyz");

        string charset = chars.ToString();
        var password = new char[length];
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        for (int i = 0; i < length; i++)
        {
            rng.GetBytes(bytes);
            password[i] = charset[(int)(BitConverter.ToUInt32(bytes, 0) % (uint)charset.Length)];
        }
        return new string(password);
    }
}
