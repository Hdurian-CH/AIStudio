using Cledev.OpenAI;
using Cledev.OpenAI.V1;
using Cledev.OpenAI.V1.Contracts.Images;
using Cledev.OpenAI.V1.Helpers;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorApp1;

public class CreateImageVariationPage : ImagePageBase
{
    protected CreateImageVariationRequest Request { get; set; } = null!;
    
    protected override void OnInitialized()
    {
        base.OnInitialized();

        Request = new CreateImageVariationRequest
        {
            Image = new byte[1],
            ImageName = "Something",
            Size = ImageSize.Size512x512.ToStringSize(),
            ResponseFormat = ImageResponseFormat.B64Json.ToStringFormat(),
            N = 1
        };
        Set = new Settings();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Set.ApiKey != null)
        {
            var str = await localStorage.GetItemAsStringAsync("keyvalue");
            if (str != null)
            {
                Set.ApiKey = AesEncryption.Decrypt(str);
            }

        }

    }
    public async Task OnInputFileForImageChange(InputFileChangeEventArgs e)
    {
        Request.Image = await GetFileBytes(e);
        Request.ImageName = e.File.Name;
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

        Response = await OpenAIClient.CreateImageVariation(Request);
        Error = Response?.Error;

        if (Response is not null)
        {
            Images.AddOriginalFromBytes(Request.Image);
            Images.AddRangeFromResponse(Response, ImageType.Variation);
        }

        IsProcessing = false;
    }

    protected static class Tooltips
    {
        public static string Image = "Required. The image to use as the basis for the variation(s). Must be a valid PNG file, less than 4MB, and square.";
    }
}
