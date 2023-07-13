using Cledev.OpenAI;
using Cledev.OpenAI.V1;
using Cledev.OpenAI.V1.Contracts.Images;
using Cledev.OpenAI.V1.Helpers;
using Microsoft.JSInterop;

namespace BlazorApp1;

public class CreateImagePage : ImagePageBase
{
    protected CreateImageRequest Request { get; set; } = null!;
    protected Settings Set { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Set = new Settings();
        Request = new CreateImageRequest
        {
            Prompt = string.Empty,
            Size = ImageSize.Size512x512.ToStringSize(),
            ResponseFormat = ImageResponseFormat.B64Json.ToStringFormat(),
            N = 1
        };
    }

    protected async Task OnSubmitAsync()
    {
        if (Set.ApiKey != null)
        {
            OpenAIClient = new OpenAIClient(Set);
        }
        IsProcessing = true;

        Response = null;
        Error = null;
        Images.Clear();

        Response = await OpenAIClient.CreateImage(Request);
        Error = Response?.Error;

        if (Response is not null)
        {
            Images.AddRangeFromResponse(Response, ImageType.Created);
        }

        IsProcessing = false;
    }

    protected static class Tooltips
    {
        public static string Prompt = "Required. A text description of the desired image(s). The maximum length is 1000 characters.";
    }

}
