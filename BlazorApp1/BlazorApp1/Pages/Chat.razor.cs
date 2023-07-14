using System.Text.Json;
using BlazorApp1.Extensions;
using Cledev.OpenAI;
using Cledev.OpenAI.V1;
using Cledev.OpenAI.V1.Contracts.Chats;
using Microsoft.JSInterop;

namespace BlazorApp1;

public class ChatPage : PageComponentBase
{
    protected CreateChatCompletionRequest Request { get; set; } = null!;
    protected IList<string> ChatModels { get; set; } = new List<string>();

    protected string? Prompt { get; set; }
    protected IList<Message> Messages { get; set; } = new List<Message>();

    protected string NowUse { get; set; } = string.Empty;

    protected string MaxUse { get; set; } = string.Empty;

    protected Settings Settings { get; set; }

    protected override void OnInitialized()
    {
        Settings = new Settings();
        ChatModels = new List<string>
        {
            "gpt-3.5-turbo-0613",
            "gpt-3.5-turbo-16k-0613",
            "gpt-3.5-turbo",
            "gpt-3.5-turbo-0301",
            "gpt-3.5-turbo-16k"
        };
        Request = new CreateChatCompletionRequest
        {
            Model = ChatModels[0],
            N = 1,
            MaxTokens = 500,
            Stream = true,
            Messages = new List<ChatCompletionMessage>()
        };
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        //pre use localStorage key
        var str = await localStorage.GetItemAsStringAsync("keyvalue");
        if (str != null)
        {
            Settings.ApiKey = AesEncryption.Decrypt(str);
            UpdateClient();
        }
    }

    protected async Task OnSubmitAsync()
    {
        IsProcessing = true;
        Request.Messages.Add(new ChatCompletionMessage("user", Prompt ?? string.Empty));

        Messages.Add(new Message("User", Prompt ?? string.Empty, 0));
        
        Prompt = null;
        var ok = false;
        if (Request.Stream is true)
        {
            var completions = OpenAIClient.CreateChatCompletionAsStream(Request);
            Messages.Add(new Message($"{Request.Model}", "......", 0));
            await foreach (var completion in completions)
            {
                if (ok is not true)
                {
                    Messages.Last().Content = string.Empty;
                    ok = true;
                }
                Error = completion.Error;

                if (Error is not null) continue;

                Messages.Last().Content += completion.Choices[0].Message?.Content;

                if (Messages.Last().Content.ContainsCode())
                    Messages.Last().Content = Messages.Last().Content.FormatCode()!;

                await InvokeAsync(StateHasChanged);

                await JsRuntime.InvokeVoidAsync("scrollToTarget", "bottom");

                await SaveMessage();
            }
        }
        else
        {
            var response = await OpenAIClient.CreateChatCompletion(Request);
            Error = response?.Error;

            if (Error is null)
            {
                var content = response!.Choices[0].Message?.Content;

                Messages.Last().Content += content.FormatCode();

                await InvokeAsync(StateHasChanged);

                await JsRuntime.InvokeVoidAsync("scrollToTarget", "bottom");
            }
        }

        IsProcessing = false;
        await InvokeAsync(StateHasChanged);
    }

    protected void Reset()
    {
        Prompt = null;
        Messages?.Clear();
        JsRuntime.InvokeVoidAsync("scrollToTarget", "top");
    }

    private async Task SaveMessage()
    {
        var temp = Messages?.Where(x => x.IsDelete == false).ToList();
        Messages = temp;
        var json = JsonSerializer.Serialize(temp);
        await localStorage.SetItemAsStringAsync("Message", json);
    }


    protected async Task GetMessage()
    {
        var savedMessage = await localStorage.GetItemAsStringAsync("Message");
        if (savedMessage != null)
        {
            Console.WriteLine(savedMessage);
            Messages = JsonSerializer.Deserialize<IList<Message>>(savedMessage);
        }
    }

    protected async Task DeleteAllMessage()
    {
        await localStorage.RemoveItemAsync("Message");
        Reset();
        StateHasChanged();
    }

    //Delete uset massage
    protected async Task DeleteUserMessage(string content)
    {
        if (Messages != null)
            for (var i = 0; i < Messages.Count; i++)
                if (Messages[i].Content == content && Messages[i].Role == "User")
                {
                    Messages[i].IsDelete = true;
                    Messages[i].IsDelete = true;
                    break;
                }

        await SaveMessage();
        StateHasChanged();
    }


    //delete ChatGpt Message
    protected async Task DeleteGptMessage(string content)
    {
        if (Messages != null)
            for (var i = 0; i < Messages.Count; i++)
                if (Messages[i].Content == content && Messages[i].Role != "User")
                {
                    Messages[i].IsDelete = true;
                    Messages[i].IsDelete = true;
                    break;
                }

        await SaveMessage();
        StateHasChanged();
    }


    //update client setting
    public void UpdateClient()
    {
        OpenAIClient = new OpenAIClient(Settings);
    }

    public class Message
    {
        public Message(string role, string content, int tokens)
        {
            Role = role;
            Content = content;
            Tokens = tokens;
        }

        public string Role { get; set; }
        public string Content { get; set; }
        public int Tokens { get; set; }
        public bool IsDelete { get; set; }
    }
}