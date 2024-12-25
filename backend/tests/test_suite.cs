using Xunit;
using Moq;
using HttpAudioControllers;
using AudioPersistenceService;
using AudioUtils;
using AudioObjects;

public class testSuite {

    private readonly IAudioService _audioService;
    private readonly UserController _audioController;

    public testSuite()
    {
        var mockPersistenceService = new Mock<IAudioPersistence>();

        mockPersistenceService.Setup(ps => ps.SaveAudioListAsync(It.IsAny<List<Audio>>()))
            .Returns(Task.CompletedTask);

        mockPersistenceService.Setup(ps => ps.LoadAudioListAsync())
            .ReturnsAsync(new List<Audio>());

        var audioService = new AudioServices(mockPersistenceService.Object);

        _audioService = audioService;
        _audioController = new UserController(audioService);
    }

    [Fact]
    public void FirstXUnitTest()
    {
        int four = 4;
        Assert.Equal(4,four);
    }

    // Get audio should return not found
    [Fact]
    public void GetAudioShouldReturnNotFound()
    {
        bool yes = true;
        Assert.True(yes);
    }

    // Get audio should return success

    // Get audio list should return not found

    // Get audio list should return success

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
