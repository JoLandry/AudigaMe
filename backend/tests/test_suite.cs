using Xunit;
using Moq;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using HttpAudioControllers;
using AudioPersistenceService;
using AudioUtils;
using AudioObjects;

public class testSuite {

    private readonly IAudioService _audioService;
    private readonly UserController _audioController;
    private readonly Mock<IAudioPersistence> _persistenceService;

    private readonly ITestOutputHelper _output;


    public testSuite(ITestOutputHelper output)
    {
        _output = output;

        _persistenceService = new Mock<IAudioPersistence>();
        var audioService = new AudioServices(_persistenceService.Object);
        _audioService = audioService;
        _audioController = new UserController(audioService);
    }

    
    // 3 A's:
    // Arrange
    // Act
    // Assert


    [Fact]
    public async Task LoadAudioListShouldReturnNonEmptyList()
    {
        string filePath = "src/resources/audioMetadata.json";
        var audioPersistence = new AudioPersistence(filePath);

        var audioList = await audioPersistence.LoadAudioListAsync();

        Assert.NotEmpty(audioList);
        _output.WriteLine($"Successfully loaded {audioList.Count} audio elements from JSON file.");
    }


    [Fact]
    public async Task GetAudioShouldReturnNotFound()
    {
        // Arrange
        int invalidId = -1;

        var audioList = SetupMockAudioList();

        _persistenceService.Setup(ps => ps.LoadAudioListAsync())
            .ReturnsAsync(audioList);

        var audioServiceMock = new Mock<IAudioService>();
        audioServiceMock.Setup(s => s.RetrieveAudioById(invalidId))
            .ReturnsAsync((Audio?)null);
        var controller = new UserController(audioServiceMock.Object);

        // Act
        var result = await controller.GetAudioById(invalidId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        var notFoundResult = result as NotFoundObjectResult;
        Assert.NotNull(notFoundResult);
        Assert.Equal($"Audio with ID {invalidId} not found.",notFoundResult.Value);

        CleanupMockFiles();
    }


    [Fact]
    public async Task GetAudioShouldReturnSuccess()
    {
        // Arrange
        int validId = 1;

        var audioList = SetupMockAudioList();

        _persistenceService.Setup(ps => ps.LoadAudioListAsync())
            .ReturnsAsync(audioList);

        var audioServiceMock = new Mock<IAudioService>();
        audioServiceMock.Setup(s => s.RetrieveAudioById(validId))
            .ReturnsAsync(audioList.FirstOrDefault(a => a.Id == validId));
        var controller = new UserController(audioServiceMock.Object);

        // Act
        var result = await controller.GetAudioById(validId);

        // Assert
        Assert.IsType<FileContentResult>(result);
        var fileContentResult = result as FileContentResult;
        Assert.NotNull(fileContentResult);
        Assert.Equal("audio/mpeg",fileContentResult.ContentType);
        Assert.Equal(3,fileContentResult.FileContents.Length);

        CleanupMockFiles();
    }


    [Fact]
    public async Task GetAudioListShouldReturnSuccess()
    {
        // Arrange
        var audioList = SetupMockAudioList();

        _persistenceService.Setup(ps => ps.LoadAudioListAsync())
            .ReturnsAsync(audioList);

        var audioServiceMock = new Mock<IAudioService>();
        audioServiceMock.Setup(s => s.GetAudioList())
            .ReturnsAsync(audioList);
        var controller = new UserController(audioServiceMock.Object);

        var urlMock = new Mock<IUrlHelper>();
        urlMock
            .Setup(h => h.Action(It.IsAny<Microsoft.AspNetCore.Mvc.Routing.UrlActionContext>()))
            .Returns<Microsoft.AspNetCore.Mvc.Routing.UrlActionContext>(ctx => 
            {
                if(ctx.Values is Dictionary<string, object> values && values.ContainsKey("id")){
                    return $"/audios/{values["id"]}";
                }
                return "/audios";
            });

        controller.Url = urlMock.Object;

        // Act
        var resultList = await controller.GetAudioList();

        // Assert
        Assert.IsType<OkObjectResult>(resultList);
        var result = resultList as OkObjectResult;
        Assert.NotNull(result);

        var audioMetadataList = result.Value as List<Audio>;
        Assert.NotNull(audioMetadataList);
        Assert.Equal(audioList.Count,audioMetadataList.Count);

        for(int i = 0; i < audioList.Count; i++){
            var audio = audioList[i];
            var metadataAudio = audioMetadataList[i];

            Assert.Equal(audio.Id,metadataAudio.Id);
            Assert.Equal(audio.Title,metadataAudio.Title);
            Assert.Equal(audio.Artist,metadataAudio.Artist);
            Assert.Equal(audio.Type,metadataAudio.Type);
            Assert.Equal(audio.IsFavorite,metadataAudio.IsFavorite);
        }

        CleanupMockFiles();
    }


    [Fact]
    public async Task GetAudioListShouldReturnNotFound()
    {
        // Arrange
        var audioList = new List<Audio>();

        _persistenceService.Setup(ps => ps.LoadAudioListAsync())
            .ReturnsAsync(audioList);

        var audioServiceMock = new Mock<IAudioService>();
        audioServiceMock.Setup(s => s.GetAudioList())
            .ReturnsAsync(audioList);
        var controller = new UserController(audioServiceMock.Object);

        var urlMock = new Mock<IUrlHelper>();
        urlMock
            .Setup(h => h.Action(It.IsAny<Microsoft.AspNetCore.Mvc.Routing.UrlActionContext>()))
            .Returns<Microsoft.AspNetCore.Mvc.Routing.UrlActionContext>(ctx => 
            {
                if(ctx.Values is Dictionary<string, object> values && values.ContainsKey("id")){
                    return $"/audios/{values["id"]}";
                }
                return "/audios";
            });

        controller.Url = urlMock.Object;

        // Act
        var resultList = await controller.GetAudioList();

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultList);
        var notFoundResult = resultList as NotFoundObjectResult;
        Assert.NotNull(notFoundResult);
        Assert.Equal("No audios found.",notFoundResult.Value);

        CleanupMockFiles();
    }


    [Fact]
    public async Task DeleteAudioShouldReturnNotFound()
    {
        // Arrange
        int invalidIdToDelete = -1;

        var audioList = SetupMockAudioList();

        _persistenceService.Setup(ps => ps.LoadAudioListAsync())
            .ReturnsAsync(audioList);

        var audioServiceMock = new Mock<IAudioService>();
        audioServiceMock.Setup(s => s.RetrieveAudioById(invalidIdToDelete))
            .ReturnsAsync((Audio?)null);
        var controller = new UserController(audioServiceMock.Object);

        // Act
        var result = await controller.DeleteAudio(invalidIdToDelete);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        var notFoundResult = result as NotFoundObjectResult;
        Assert.NotNull(notFoundResult);
        Assert.Equal($"Audio with id = {invalidIdToDelete} not found",notFoundResult.Value);

        CleanupMockFiles();
    }


    [Fact]
    public async Task DeleteAudioShouldReturnSuccess()
    {
        // Arrange
        int id = 1;

        var audioList = SetupMockAudioList();

        _persistenceService.Setup(ps => ps.LoadAudioListAsync())
            .ReturnsAsync(audioList);

        var audioServiceMock = new Mock<IAudioService>();
        audioServiceMock.Setup(s => s.RetrieveAudioById(id))
            .ReturnsAsync(audioList.FirstOrDefault(a => a.Id == id));
        var controller = new UserController(audioServiceMock.Object);

        // Act
        var result = await controller.DeleteAudio(id);

        // Assert
        Assert.IsType<NoContentResult>(result);
        var noContentResult = result as NoContentResult;
        Assert.NotNull(noContentResult);
        Assert.Equal(204,noContentResult.StatusCode);

        CleanupMockFiles();
    }


    [Fact]
    public async Task PostAudioShouldReturnFailureForWrongMediaType()
    {
        // Arrange
        var audio = new Audio
        {
            Id = 100,
            Title = "InvalidAudio",
            Artist = "RandomArtist",
            Type = ".txt",
            IsFavorite = false
        };

        string rootDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"..","..","..",".."));
        string invalidFilePath = Path.Combine(rootDirectory,"backend","src","resources","uploads","InvalidAudio.txt");
        await File.WriteAllTextAsync(invalidFilePath,"This is text for my test.");

        // Act
        using(var httpClient = new HttpClient()){
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(audio.Title),"Title");
            formData.Add(new StringContent(audio.Artist),"Artist");
            formData.Add(new StringContent(audio.Type),"Type");
            formData.Add(new StringContent(audio.Id.ToString()), "Id");
            formData.Add(new StringContent(audio.IsFavorite.ToString()), "False");

            byte[] audioData = await System.IO.File.ReadAllBytesAsync(invalidFilePath);
            formData.Add(new ByteArrayContent(audioData),"audioFile",Path.GetFileName(invalidFilePath));

            var response = await httpClient.PostAsync("http://localhost:5174/api/user/audios/",formData);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest,response.StatusCode);
            var responseBody = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Response Body: {responseBody}");
            Assert.Contains("Unsupported file type.", responseBody);
        }
        if(File.Exists(invalidFilePath)){
            File.Delete(invalidFilePath);
        }
    }


    [Fact]
    public async Task PostAudioShouldReturnFailureDueToWrongRoute()
    {
        // Arrange
        var audio = new Audio
        {
            Id = 500,
            Title = "ValidAudio",
            Artist = "TestArtist",
            Type = ".mp3",
            IsFavorite = false
        };

        string rootDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"..","..","..",".."));
        string validFilePath = Path.Combine(rootDirectory,"backend","src","resources","uploads","ValidAudio.mp3");
        await File.WriteAllBytesAsync(validFilePath,new byte[]{1,2,3});

        // Act
        using(var httpClient = new HttpClient()){
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(audio.Title),"Title");
            formData.Add(new StringContent(audio.Artist),"Artist");
            formData.Add(new StringContent(audio.Type),"Type");
            formData.Add(new StringContent(audio.Id.ToString()),"Id");
            formData.Add(new StringContent(audio.IsFavorite.ToString()),"False");

            byte[] audioData = await File.ReadAllBytesAsync(validFilePath);
            formData.Add(new ByteArrayContent(audioData),"audioFile",Path.GetFileName(validFilePath));

            var response = await httpClient.PostAsync("http://localhost:5174/wrongroute/",formData);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.MethodNotAllowed,response.StatusCode);
            var responseBody = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Response Body: {responseBody}");
        }
        if(File.Exists(validFilePath)){
            File.Delete(validFilePath);
        }
    }


    [Fact]
    public async Task PostAudioShouldReturnSuccess()
    {
        // Arrange
        var audio = new Audio
        {
            Id = 100,
            Title = "validAudio",
            Artist = "RandomArtist",
            Type = ".mp3",
            IsFavorite = false
        };

        string rootDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"..","..","..",".."));
        string validFilePath = Path.Combine(rootDirectory,"backend","src","resources","uploads","validAudio.mp3");
        await File.WriteAllBytesAsync(validFilePath,new byte[]{1,2,3});

        // Act
        using(var httpClient = new HttpClient()){
            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(audio.Title),"Title");
            formData.Add(new StringContent(audio.Artist),"Artist");
            formData.Add(new StringContent(audio.Type),"Type");
            formData.Add(new StringContent(audio.Id.ToString()), "Id");
            formData.Add(new StringContent(audio.IsFavorite.ToString()), "False");

            byte[] audioData = await System.IO.File.ReadAllBytesAsync(validFilePath);
            formData.Add(new ByteArrayContent(audioData),"audioFile",Path.GetFileName(validFilePath));

            var response = await httpClient.PostAsync("http://localhost:5174/api/user/audios/",formData);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Created,response.StatusCode);
            var responseBody = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Response Body: {responseBody}");

            var returnedAudio = JsonConvert.DeserializeObject<Audio>(responseBody);
            Assert.NotNull(returnedAudio);
            Assert.Equal(audio.Title,returnedAudio.Title);
            Assert.Equal(audio.Artist,returnedAudio.Artist);
            Assert.Equal(audio.Type,returnedAudio.Type);

            // Clean up test's modifications by executing DELETE 
            if(returnedAudio?.Id != null){
                string command = "curl";
                string arguments = " -X DELETE http://localhost:5174/api/user/audios/" + returnedAudio.Id;

                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process? process = Process.Start(processStartInfo);
                if(process != null){
                    try {
                        using(System.IO.StreamReader reader = process.StandardOutput){
                            string output = reader.ReadToEnd();
                            Console.WriteLine("Output: " + output);
                        }
                    } finally {
                        process.Dispose();
                    }
                }
            }
        }
        if(File.Exists(validFilePath)){
            File.Delete(validFilePath);
        }
    }


    [Fact]
    public async Task UpdateAudioShouldReturnBadRequestSinceNoBoolean()
    {
        // Arrange
        int id = 1;

        var requestPUT = new UpdateAudioRequest {
            Title = "Whatever",
            Artist = "Whoever"
        };

        var audioList = SetupMockAudioList();

        _persistenceService.Setup(ps => ps.LoadAudioListAsync())
            .ReturnsAsync(audioList);

        var audioServiceMock = new Mock<IAudioService>();
        audioServiceMock.Setup(s => s.RetrieveAudioById(id))
            .ReturnsAsync(audioList.FirstOrDefault(a => a.Id == id));
        var controller = new UserController(audioServiceMock.Object);

        // Act
        var result = await controller.UpdateAudio(id,requestPUT);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        var reqResult = result as BadRequestObjectResult;
        Assert.NotNull(reqResult);
        Assert.Equal($"Audio with id {id} should either be in the Favorites playlist or not.",reqResult.Value);

        CleanupMockFiles();
    }


    // Change favorite status should return success
    [Fact]
    public async Task UpdateAudioShouldReturnSuccess()
    {
        // Arrange
        int id = 1;

        var requestPUT = new UpdateAudioRequest {
            Title = "Whatever",
            Artist = "Whoever",
            IsFavorite = true
        };

        var audioList = SetupMockAudioList();

        _persistenceService.Setup(ps => ps.LoadAudioListAsync())
            .ReturnsAsync(audioList);

        var audioServiceMock = new Mock<IAudioService>();
        audioServiceMock.Setup(s => s.RetrieveAudioById(id))
            .ReturnsAsync(audioList.FirstOrDefault(a => a.Id == id));
        var controller = new UserController(audioServiceMock.Object);

        // Act
        var result = await controller.UpdateAudio(id,requestPUT);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var reqResult = result as OkObjectResult;
        Assert.NotNull(reqResult);
        var audioObject = reqResult.Value as Audio;
        if(audioObject != null){
            Assert.Equal(requestPUT.IsFavorite,audioObject.IsFavorite);
        }

        CleanupMockFiles();
    }
    

    // WHEN IMPLEMENTED, ADD THE FOLLOWING TESTS
    // Get list of favorites should return success
    // Get playlist should return success
    // Playlist is correctly formed 



    // Utils methods
    private List<Audio> SetupMockAudioList()
    {
        var rootDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"..","..","..",".."));
        var uploadsDirectory = Path.Combine(rootDirectory,"backend","src","resources","uploads");
        _output.WriteLine($"Uploads Directory: {uploadsDirectory}");

        if(!System.IO.Directory.Exists(uploadsDirectory)){
            Directory.CreateDirectory(uploadsDirectory);
        }

        var audioList = new List<Audio>();
        var data = new byte[]{1,2,3};
        audioList.Add(new Audio("Song1","Artist1",data,".mp3"));
        audioList.Add(new Audio("Song2","Artist2",data,".mp3"));
        
        int id = 0;
        foreach(var audio in audioList){
            id++;
            audio.Id = id;
            string path = Path.Combine(uploadsDirectory,$"{audio.Title}{audio.Type}");
            _output.WriteLine($"File path generated: {path}");
            try {
                System.IO.File.WriteAllBytes(path,audio.Data);
            } catch(Exception e){
                _output.WriteLine(e.ToString());
            }
        }

        foreach(Audio a in audioList){
            _output.WriteLine($"Here is my id: {a.Id}");
        }

        return audioList;
    }


    private void CleanupMockFiles()
    {
        var rootDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"..","..","..",".."));
        var uploadsDirectory = Path.Combine(rootDirectory,"backend","src","resources","uploads");

        if(Directory.Exists(uploadsDirectory)){
            foreach(var file in Directory.GetFiles(uploadsDirectory)){
                string? name = Path.GetFileName(file);
                if(name == "Song1.mp3" || name == "Song2.mp3"){
                    File.Delete(file);
                }
            }
        }
    }
}
