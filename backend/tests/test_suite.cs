using Xunit;
using Moq;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Mvc;
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
        audioServiceMock.Setup(s => s.retrieveAudioById(invalidId))
            .ReturnsAsync((Audio)null);
        var controller = new UserController(audioServiceMock.Object);

        // Act
        var result = await controller.getAudioById(invalidId);

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
        audioServiceMock.Setup(s => s.retrieveAudioById(validId))
            .ReturnsAsync(audioList.FirstOrDefault(a => a.Id == validId));
        var controller = new UserController(audioServiceMock.Object);

        // Act
        var result = await controller.getAudioById(validId);

        // Assert
        Assert.IsType<FileContentResult>(result);
        var fileContentResult = result as FileContentResult;
        Assert.NotNull(fileContentResult);
        Assert.Equal("audio/mpeg",fileContentResult.ContentType);
        Assert.Equal(3,fileContentResult.FileContents.Length);

        CleanupMockFiles();
    }


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


    [Fact]
    public async Task GetAudioListShouldReturnSuccess()
    {
        // Arrange
        var audioList = SetupMockAudioList();

        _persistenceService.Setup(ps => ps.LoadAudioListAsync())
            .ReturnsAsync(audioList);

        var audioServiceMock = new Mock<IAudioService>();
        audioServiceMock.Setup(s => s.getAudioList())
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

        var audioMetadataList = result.Value as List<AudioMetadata>;
        Assert.NotNull(audioMetadataList);
        Assert.Equal(audioList.Count,audioMetadataList.Count);

        for(int i = 0; i < audioList.Count; i++){
            var audio = audioList[i];
            var metadataAudio = audioMetadataList[i];

            Assert.Equal(audio.Id,metadataAudio.Id);
            Assert.Equal(audio.Title,metadataAudio.Title);
            Assert.Equal(audio.Artist,metadataAudio.Artist);
            Assert.Equal(audio.Type,metadataAudio.Type);
            Assert.Equal($"/audios/{audio.Id}",metadataAudio.DownloadUrl);
            Assert.Equal(audio.IsFavorite,metadataAudio.IsFavorite);
        }

        CleanupMockFiles();
    }


    // Get audio list should return not found

    // Post audio should return failure for wrong mediatype

    // Post audio should return failure cuz wrong route

    // Search for audio should return success

    // Search for audio should return failure

    // Change favorite status should return success

    // Change favorite status should return failure cuz not boolean

    // Get list of favorites should return success

    // Get playlist should return success

    // Playlist is correctly formed 

    // Delete audio should return success

    // Delete audio should return not found


}
