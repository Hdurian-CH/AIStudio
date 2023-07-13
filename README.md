# AIStudio
## How to use

git clone Blazor App. Cd to appsetting.json and input your api key

## Initial

* update Blazor LoacalStorage for ChatGPT
* update gpt3.5-t-0613 and gpt3.5-t-16k-0613
* you can check your billing by api key
* authentication provider(Initial password)

### 7/9/2023 update

* add NewBing from [BingChat](https://github.com/bsdayo/BingChat)

### 7/10/2023 update

* add openai key option
* add openai key localstorage
* remove model list
* fix some button icon
* fix openai billing usage showing
* remove default authenorization(you can use attribute, I just not use for default)

### 7/13/2023 update

* Unity Icon
* Add save key button for /Image page
* fix some probloms
* remove authenorization in App.razor

### 7/13/2023 update2

* Fix refresh bug (in chatpage or image page,refresh page will get error because of the JavaScript error call in Razor initializeAsync)
* Fix changing the apikey does not take effect
* Fully adaptable to mobile devices

Login：
![Login](/Images/Login.png)
index:
![index](/Images/Index.png)

Chat:
![chat](/Images/Chat.png)



NewBing:![NewBing](/Images/NewBing.png)







Image:
![Image](Images/Image.png)



## Thanks

* [BingChat](https://github.com/bsdayo/BingChat)
* [Cledev.OpenAI.Studio](https://github.com/lucabriguglia/Cledev.OpenAI.Studio)

* [MicrosoftBlazor](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor)