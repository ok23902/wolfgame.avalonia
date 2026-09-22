using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using mano;

namespace WolfGame.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly GameEngine _engine = new();

    [ObservableProperty] private string _headerText = "ManoRoom";
    [ObservableProperty] private bool _isInputVisible;
    [ObservableProperty] private string _inputPrompt = "";
    [ObservableProperty] private string _inputText = "";
    [ObservableProperty] private bool _isChoiceVisible;
    [ObservableProperty] private string _choicePrompt = "";
    public ObservableCollection<ChoiceOption> ChoiceOptions { get; } = new();
    public ObservableCollection<CharacterViewModel> Characters { get; } = new();
    public ObservableCollection<EventViewModel> Events { get; } = new();

    private TaskCompletionSource<string>? _textTcs;
    private TaskCompletionSource<string?>? _choiceTcs;
    private double _canvasWidth = 1;
    private double _canvasHeight = 1;
    private const double CharWidth = 180;
    private const double CharHeight = 320;

    [ObservableProperty] private bool _isBubbleDetailVisible;
    [ObservableProperty] private string _bubbleDetailText = "";
    [ObservableProperty] private string _bubbleDetailSpeaker = "";
    [ObservableProperty] private bool _isStartButtonVisible = true;
    [ObservableProperty] private string _phaseText = "";
    [ObservableProperty] private bool _isVoting;
    [ObservableProperty] private double _voteProgress;
    [ObservableProperty] private bool _isGameOverVisible;
    [ObservableProperty] private string _gameOverText = "";
    [ObservableProperty] private string _gameOverDetails = "";

    public async Task InitializeAsync()
    {
        try
        {
            InitLlm();
            Debug.WriteLine("[INIT] LLM initialized");

            _engine.EventEmitted += OnEventEmitted;
            _engine.StateChanged += OnStateChanged;
            _engine.ThinkingChanged += OnThinkingChanged;
            _engine.GameEnded += OnGameEnded;
            _engine.VoteProgressChanged += OnVoteProgressChanged;
            _engine.ChatRoundStarted += () =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    Events.Clear();
                    IsStartButtonVisible = true;
                });
            };

            Input.SetHandlers(RequestTextAsync, RequestChoiceAsync);

            var jsonPath = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, "WolfGame", "json", "character.json"));
            Debug.WriteLine($"[INIT] json: {jsonPath} exists={File.Exists(jsonPath)}");

            await _engine.SetupAsync(jsonPath);
            Debug.WriteLine($"[INIT] setup done. Wolf chars: {_engine.Wolf.Children.Count}");

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                foreach (var c in _engine.Wolf.Direct<CharacterObject>())
                {
                    Characters.Add(new CharacterViewModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Status = c.Status,
                        IsAlive = c.IsAlive,
                        X = c.X,
                        Y = c.Y,
                    });
                }
                RecomputePixels();
            });

            _ = Task.Run(() => _engine.RunAsync());

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[INIT FAIL] {ex.GetType().Name}: {ex.Message}");
            Debug.WriteLine(ex.StackTrace);
            throw;
        }
    }

    private static void InitLlm()
    {
        // .env を探す（複数の候補地を順に試す）
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "WolfGame", ".env")
        };

        foreach (var p in candidates)
        {
            var full = Path.GetFullPath(p);
            if (File.Exists(full))
            {
                DotNetEnv.Env.Load(full);
                Debug.WriteLine($"[LLM] loaded .env: {full}");
                break;
            }
        }

        var apiKey = Environment.GetEnvironmentVariable("MANO_LLM_API_KEY")
            ?? throw new InvalidOperationException("MANO_LLM_API_KEY is not set.");
        var model = Environment.GetEnvironmentVariable("MANO_LLM_MODEL")
            ?? throw new InvalidOperationException("MANO_LLM_MODEL is not set.");
        var endpoint = Environment.GetEnvironmentVariable("MANO_LLM_ENDPOINT")
            ?? "https://api.deepseek.com";

        var openai = new OpenAIClient(
            new ApiKeyCredential(apiKey),
            new OpenAIClientOptions { Endpoint = new Uri(endpoint) });
        Llm.Init(openai.GetChatClient(model).AsIChatClient());
    }

    public void UpdateCanvasSize(double w, double h)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => UpdateCanvasSize(w, h));
            return;
        }
        _canvasWidth = Math.Max(1, w);
        _canvasHeight = Math.Max(1, h);
        RecomputePixels();
    }

    private void RecomputePixels()
    {
        var rangeX = Math.Max(1, _canvasWidth - CharWidth);
        var rangeY = Math.Max(1, _canvasHeight - CharHeight);

        foreach (var c in Characters)
        {
            c.PixelX = c.X * rangeX;
            c.PixelY = c.Y * rangeY;
        }
    }

    // ---------- テキスト入力 ----------

    private Task<string> RequestTextAsync(string prompt)
    {
        _textTcs = new TaskCompletionSource<string>();
        Dispatcher.UIThread.Post(() =>
        {
            InputPrompt = prompt;
            InputText = "";
            IsInputVisible = true;
        });
        return _textTcs.Task;
    }

    public void SubmitText()
    {
        var tcs = _textTcs;
        _textTcs = null;
        var text = InputText;
        InputText = "";
        IsInputVisible = false;
        tcs?.SetResult(text);
    }

    // ---------- 選択入力 ----------

    private Task<string?> RequestChoiceAsync(ChoiceRequest req)
    {
        _choiceTcs = new TaskCompletionSource<string?>();
        Dispatcher.UIThread.Post(() =>
        {
            ChoicePrompt = req.Prompt;
            ChoiceOptions.Clear();
            foreach (var o in req.Options) ChoiceOptions.Add(o);
            IsChoiceVisible = true;
        });
        return _choiceTcs.Task;
    }

    public void SubmitChoice(ChoiceOption opt)
    {
        var tcs = _choiceTcs;
        _choiceTcs = null;
        IsChoiceVisible = false;
        tcs?.SetResult(opt.Id);
    }

    // ---------- イベント ----------

    private void OnEventEmitted(GameEvent e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Events.Add(new EventViewModel { Text = FormatEvent(e) });
            // 自動スクロール（次のフレームで）
            Dispatcher.UIThread.Post(() => ScrollToBottom?.Invoke(), DispatcherPriority.Background);

            if (e.Kind == "utterance")
            {
                var c = Characters.FirstOrDefault(x => x.Id == e.SpeakerId);
                if (c != null)
                {
                    c.Status = e.Status;
                    ShowBubble(c, e.Text);
                }
            }
        });
    }

    public event Action? ScrollToBottom;

    private void ShowBubble(CharacterViewModel c, string text)
    {
        c.SetSpeech(text);
        _ = HideBubbleAfterAsync(c, text, 5000);
    }

    private async Task HideBubbleAfterAsync(CharacterViewModel c, string shownText, int ms)
    {
        await Task.Delay(ms);
        Dispatcher.UIThread.Post(() =>
        {
            if (c.SpeechBubbleFull == shownText)
                c.ClearSpeech();
        });
    }
    public void ShowBubbleDetail(CharacterViewModel c)
    {
        BubbleDetailSpeaker = c.Name;
        BubbleDetailText = c.SpeechBubbleFull;
        IsBubbleDetailVisible = true;
    }

    private void OnStateChanged(WolfObject wolf)
    {
        Dispatcher.UIThread.Post(() =>
        {
            PhaseText = wolf.Phase;
            HeaderText = wolf.Phase == "Chat"
                ? "雑談タイム"
                : $"Day {wolf.Day} / {wolf.Phase}";

            var order = wolf.TurnOrder;
            for (int i = 0; i < Characters.Count; i++)
            {
                var idx = order.IndexOf(Characters[i].Id);
                Characters[i].TurnNumber = idx >= 0 ? idx + 1 : 0;
            }

            foreach (var c in Characters)
            {
                var obj = wolf.Direct<CharacterObject>().FirstOrDefault(x => x.Id == c.Id);
                if (obj != null) { c.IsAlive = obj.IsAlive; c.Status = obj.Status; }
            }
        });
    }
    public void CloseBubbleDetail() => IsBubbleDetailVisible = false;


    private void OnGameEnded(string winner)
    {
        Dispatcher.UIThread.Post(() =>
        {
            GameOverText = winner == "wolf" ? "人狼陣営の勝利" : "村人陣営の勝利";

            var sb = new System.Text.StringBuilder();
            foreach (var kv in _engine.Wolf.Roles)
            {
                var c = Characters.FirstOrDefault(x => x.Id == kv.Key);
                var name = c?.Name ?? kv.Key;
                var role = kv.Value switch
                {
                    "wolf" => "人狼",
                    "seer" => "占い師",
                    "villager" => "村人",
                    _ => kv.Value
                };
                var alive = c?.IsAlive == true ? "生存" : "死亡";
                sb.AppendLine($"{name}（{role}・{alive}）");
            }
            GameOverDetails = sb.ToString();

            IsGameOverVisible = true;
        });
    }

    private static string FormatEvent(GameEvent e) => e.Kind switch
    {
        "utterance" => $"{e.SpeakerName}: {e.Text}",
        "gm" => $"[GM] {e.Text}",
        "system" => $"[SYS] {e.Text}",
        _ => e.Text
    };
    private void OnThinkingChanged(string? id)
    {
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var c in Characters)
                c.IsThinking = (c.Id == id);
        });
    }
    private void OnVoteProgressChanged()
    {
        Dispatcher.UIThread.Post(() =>
        {
            var wolf = _engine.Wolf;
            IsVoting = wolf.VotesExpected > 0 && wolf.VotesReceived < wolf.VotesExpected;
            VoteProgress = wolf.VotesExpected == 0 ? 0 : (double)wolf.VotesReceived / wolf.VotesExpected;
        });
    }
    public void RequestStartGame()
    {
        Console.WriteLine("[VM] RequestStartGame");

        // ★ 保留中の入力・選択を強制解除
        var textTcs = _textTcs;
        _textTcs = null;
        IsInputVisible = false;
        InputText = "";
        textTcs?.TrySetResult("……。");   // 沈黙で返す

        var choiceTcs = _choiceTcs;
        _choiceTcs = null;
        IsChoiceVisible = false;
        choiceTcs?.TrySetResult(null);    // 選択なし

        IsStartButtonVisible = false;
        _engine.RequestStartGame();
    }
    public void ProceedToNextRound()
    {
        IsGameOverVisible = false;
        _engine.ProceedToNextRound();
    }
}

public class EventViewModel
{
    public string Text { get; set; } = "";
}