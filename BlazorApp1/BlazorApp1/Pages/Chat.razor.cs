using System.Globalization;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using BlazorApp1.Extensions;
using Blazored.LocalStorage;
using Cledev.OpenAI;
using Cledev.OpenAI.V1;
using Cledev.OpenAI.V1.Contracts.Chats;
using Cledev.OpenAI.V1.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using OneOf.Types;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BlazorApp1;

public class ChatPage : PageComponentBase
{
    protected CreateChatCompletionRequest Request { get; set; } = null!;
    protected IList<string> ChatModels { get; set; } = new List<string>();

    protected string? Prompt { get; set; }
    protected IList<Message> Messages { get; set; } = new List<Message>();

    protected string NowUse { get; set; } = string.Empty;

    protected string MaxUse { get; set; } = string.Empty;
    
    protected override void OnInitialized()
    {

        Request = new CreateChatCompletionRequest
        {
            Model = ChatModel.Gpt_35_Turbo.ToStringModel(),
            N = 1,
            MaxTokens = 500,
            Stream = true,
            Messages = new List<ChatCompletionMessage>()
        };

        /*ChatModels = Enum.GetValues(typeof(ChatModel)).Cast<ChatModel>().Select(x => x.ToStringModel()).ToList();*/
        ChatModels = new List<string>()
        {
            "gpt-3.5-turbo",
            "gpt-3.5-turbo-0301",
            "gpt-3.5-turbo-0613",
            "gpt-3.5-turbo-16k-0613",
            "gpt-3.5-turbo-16k"
        };
    }


    protected async Task OnSubmitAsync()
    {
        IsProcessing = true;
        Request.Messages.Add(new ChatCompletionMessage("user", Prompt ?? string.Empty));

        Messages.Add(new Message("User", Prompt ?? string.Empty, 0));
        Messages.Add(new Message($"{Request.Model}", string.Empty, 0));

        Prompt = null;

        if (Request.Stream is true)
        {
            var completions = OpenAIClient.CreateChatCompletionAsStream(Request);

            await foreach (var completion in completions)
            {
                Error = completion.Error;

                if (Error is not null)
                {
                    continue;
                }

                Messages.Last().Content += completion.Choices[0].Message?.Content;

                if (Messages.Last().Content.ContainsCode())
                {
                    Messages.Last().Content = Messages.Last().Content.FormatCode()!;
                }

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
        await localStorage.SetItemAsStringAsync("Message",json);
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

    protected async Task DeleteUserMessage(string content)
    {
        if (Messages != null)
        {
            for (int i = 0; i < Messages.Count; i++)
            {
                if (Messages[i].Content == content && Messages[i].Role == "User")
                {
                    Messages[i].IsDelete = true;
                    Messages[i].IsDelete = true;
                    break;
                }
            }
        }

        await SaveMessage();
        StateHasChanged();
    }

    protected async Task DeleteGptMessage(string content)
    {
        if (Messages != null)
        {
            for (int i = 0; i < Messages.Count; i++)
            {
                if (Messages[i].Content == content && Messages[i].Role != "User")
                {
                    Messages[i].IsDelete = true;
                    Messages[i].IsDelete = true;
                    break;
                }
            }
        }

        await SaveMessage();
        StateHasChanged();
    }
    public async Task GetHistoryMessage()
    {
        await GetMessage();
    }



    public async Task GetLimit()
    {
        var apikey = Configuration.GetSection("OpenAI:ApiKey").Value;
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + apikey);
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36");
        var response = await client.GetAsync("https://api.openai.com/dashboard/billing/subscription");
        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            dynamic jsonResponse = JsonConvert.DeserializeObject(responseBody);
            MaxUse = jsonResponse.hard_limit_usd;
        }
        else
        {
            await JsRuntime.InvokeVoidAsync("alert", "Error");
            await Task.CompletedTask;
        }
        var today = DateTime.Today;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
        var lastDayOfMonth = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
        var startDate = firstDayOfMonth.ToString("yyyy-M-d");
        var endDate = lastDayOfMonth.ToString("yyyy-M-d");
        var response1 =
            await client.GetAsync(
                $"https://api.openai.com/dashboard/billing/usage?start_date={startDate}&end_date={endDate}");
        if (response1.IsSuccessStatusCode)
        {
            var responseBody = await response1.Content.ReadAsStringAsync();
            dynamic jsonResponse = JsonConvert.DeserializeObject(responseBody);
            NowUse = jsonResponse.total_usage;
        }
        else
        {
            await JsRuntime.InvokeVoidAsync("alert", "Error");
            await Task.CompletedTask;
        }

        await JsRuntime.InvokeVoidAsync("alert", $"Max:{MaxUse},NowUsed:{NowUse}");
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
        public bool IsDelete { get; set; } = false;
    }


    
} 