using Xunit;
using Moq;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Mvc;
using HttpAudioControllers;
using AudioPersistenceService;
using AudioUtils;
using AudioObjects;
using System.Text;


namespace AppTests
{
    public class TestSuite
    {
        private Mock<IAudioPersistence> _persistenceMock;
        private IAudioService _audioService;
        private UserController _controller;
        private ITestOutputHelper _output;


        public TestSuite(ITestOutputHelper output)
        {
            _output = output;
            _persistenceMock = new Mock<IAudioPersistence>();
            _audioService = new AudioServices(_persistenceMock.Object);
            _controller = new UserController(_audioService);
        }



        // 3 A's:
        // Arrange
        // Act
        // Assert


        [Fact]
        public async Task GetAudioFileById_ShouldReturnNotFound_WhenAudioDoesNotExist()
        {
            // Arrange
            int invalidId = 99999999;
            _persistenceMock.Setup(p => p.LoadAudioListAsync()).ReturnsAsync(new List<Audio>());

            _audioService = new AudioServices(_persistenceMock.Object);
            await _audioService.InitializeAsync();
            _controller = new UserController(_audioService);

            // Act
            var result = await _controller.GetAudioFileById(invalidId);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"Audio with ID {invalidId} not found.", notFound.Value);

            _output.WriteLine("Verified GetAudioFileById returns 404 for missing audio");
        }


        [Fact]
        public async Task GetAudioFileById_ShouldReturnOk_WhenAudioExists()
        {
            // Arrange
            int validId = 86573;
            var mockAudios = new List<Audio>
            {
                new Audio {
                    Id = 86573,
                    Title = "OnlyOne",
                    Artist = "MockArtist",
                    Type = ".mp3",
                    Size = 999,
                    IsFavorite = false
                }
            };

            // Ensure data exists on the server too (audio with no data should not be accepted)
            string rootDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
            string sampleFilePath = Path.Combine(rootDirectory, "backend", "src", "resources", "audioSamples", "example.mp3");
            string uploadsDirectory = Path.Combine(rootDirectory, "backend", "src", "resources", "uploads");
            string testFilePath = Path.Combine(uploadsDirectory, $"{validId}.mp3");
            // Copy the sample MP3 file to the expected location
            File.Copy(sampleFilePath, testFilePath, overwrite: true);

            // Mock
            var persistenceMock = new Mock<IAudioPersistence>();
            persistenceMock.Setup(p => p.LoadAudioListAsync()).ReturnsAsync(mockAudios);

            var service = new AudioServices(persistenceMock.Object);
            await service.InitializeAsync();
            var controller = new UserController(service);

            // Act
            var result = await controller.GetAudioFileById(validId);

            // Assert
            var fileContentResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("audio/mpeg", fileContentResult.ContentType);
            Assert.NotNull(fileContentResult.FileContents);

            File.Delete(testFilePath);
            _output.WriteLine("Verified GetAudioById returns Ok when exists");
        }


        [Fact]
        public async Task GetAudioById_ShouldReturnOk_WhenAudioExists()
        {
            // Arrange
            int validId = 3;
            var mockAudios = new List<Audio>
            {
                new Audio
                {
                    Id = validId,
                    Title = "SampleTitle",
                    Artist = "SampleArtist",
                    Type = ".mp3",
                    Size = 1234,
                    IsFavorite = false
                }
            };

            _persistenceMock.Setup(p => p.LoadAudioListAsync()).ReturnsAsync(mockAudios);

            _audioService = new AudioServices(_persistenceMock.Object);
            await _audioService.InitializeAsync();
            _controller = new UserController(_audioService);

            // Act
            var result = await _controller.GetAudioById(validId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedAudio = Assert.IsType<Audio>(okResult.Value);

            Assert.Equal(validId, returnedAudio.Id);
            Assert.Equal("SampleTitle", returnedAudio.Title);
            Assert.Equal("SampleArtist", returnedAudio.Artist);
            Assert.Equal(".mp3", returnedAudio.Type);
            Assert.Equal(1234, returnedAudio.Size);
            Assert.False(returnedAudio.IsFavorite);

            _output.WriteLine("Verified GetAudioById returns Ok with correct audio object");
        }


        [Fact]
        public async Task GetAudioById_ShouldReturnNotFound_WhenAudioDoesNotExist()
        {
            // Arrange
            var mockAudios = new List<Audio>
            {
                new Audio
                {
                    Id = 1,
                    Title = "SongA",
                    Artist = "ArtistA",
                    Type = ".mp3",
                    Size = 1024,
                    IsFavorite = false
                }
            };

            int invalidId = 99999;

            _persistenceMock.Setup(p => p.LoadAudioListAsync()).ReturnsAsync(mockAudios);

            _audioService = new AudioServices(_persistenceMock.Object);
            await _audioService.InitializeAsync();
            _controller = new UserController(_audioService);

            // Act
            var result = await _controller.GetAudioById(invalidId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"Audio with ID {invalidId} not found.", notFoundResult.Value);

            _output.WriteLine("Verified GetAudioById returns NotFound when audio does not exist");
        }


        [Fact]
        public async Task GetAudioList_ShouldReturnOk_WithAudios()
        {
            // Arrange
            var mockAudios = new List<Audio>
            {
                new Audio {
                    Id = 1,
                    Title = "Song1",
                    Artist = "Artist1",
                    Type = ".mp3",
                    Size = 1000,
                    IsFavorite = false
                },
                new Audio {
                    Id = 5,
                    Title = "Song5",
                    Artist = "Artist5",
                    Type = ".mp3",
                    Size = 5000,
                    IsFavorite = false
                },
                new Audio {
                    Id = 6,
                    Title = "Song6",
                    Artist = "Artist6",
                    Type = ".mp3",
                    Size = 6000,
                    IsFavorite = false
                }
            };

            _persistenceMock.Setup(p => p.LoadAudioListAsync()).ReturnsAsync(mockAudios);

            _audioService = new AudioServices(_persistenceMock.Object);
            await _audioService.InitializeAsync();
            _controller = new UserController(_audioService);

            // Act
            var result = await _controller.GetAudioList();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var audios = Assert.IsType<List<Audio>>(okResult.Value);
            Assert.Equal(3, audios.Count);
            Assert.Equal("Song1", audios[0].Title);
            Assert.Equal("Artist5", audios[1].Artist);
            Assert.Equal(6000, audios[2].Size);

            _output.WriteLine("Verified GetAudioList returns Ok when exists and well initialized");
        }


        [Fact]
        public async Task GetAudioList_ShouldReturnNotFound_WhenEmpty()
        {
            // Arrange
            var mockAudios = new List<Audio>();

            _persistenceMock.Setup(p => p.LoadAudioListAsync()).ReturnsAsync(mockAudios);

            _audioService = new AudioServices(_persistenceMock.Object);
            await _audioService.InitializeAsync();
            _controller = new UserController(_audioService);

            // Act
            var result = await _controller.GetAudioList();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No audios found.", notFoundResult.Value);

            _output.WriteLine("Verified GetAudioList returns NotFound when no audio is present in the list");
        }


        [Fact]
        public async Task DeleteAudio_ShouldReturnNotFound_WhenAudioDoesNotExist()
        {
            // Arrange
            int validId = 7;
            int invalidId = 9;
            var mockAudios = new List<Audio>();
            var onlyAudioInList = new Audio
            {
                Id = validId,
                Title = "Title7",
                Artist = "Artist7",
                Type = ".mp3",
                Size = 999,
                IsFavorite = false
            };
            mockAudios.Add(onlyAudioInList);

            _persistenceMock.Setup(p => p.LoadAudioListAsync()).ReturnsAsync(mockAudios);

            _audioService = new AudioServices(_persistenceMock.Object);
            await _audioService.InitializeAsync();
            _controller = new UserController(_audioService);

            // Act
            var result = await _controller.DeleteAudio(invalidId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"Audio with id = {invalidId} not found", notFoundResult.Value);

            _output.WriteLine("Verified DeleteAudio returns NotFound when audio is not present in the list");
        }


        [Fact]
        public async Task DeleteAudio_ShouldReturnNoContent_WhenAudioDeleted()
        {
            // Arrange
            int validId = 7;
            var mockAudios = new List<Audio>();
            var onlyAudioInList = new Audio
            {
                Id = validId,
                Title = "Title7",
                Artist = "Artist7",
                Type = ".mp3",
                Size = 999,
                IsFavorite = false
            };
            mockAudios.Add(onlyAudioInList);

            _persistenceMock.Setup(p => p.LoadAudioListAsync()).ReturnsAsync(mockAudios);

            _audioService = new AudioServices(_persistenceMock.Object);
            await _audioService.InitializeAsync();
            _controller = new UserController(_audioService);

            // Act
            var result = await _controller.DeleteAudio(validId);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Empty(mockAudios);

            // Make sure system file on the server (audio data) is deleted
            string rootDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
            string uploadsDirectory = Path.Combine(rootDirectory, "backend", "src", "resources", "uploads");
            string testFilePath = Path.Combine(uploadsDirectory, $"{validId}.mp3");
            Assert.False(File.Exists(testFilePath));

            _output.WriteLine("Verified DeleteAudio returns NoContent when audio is successfully deleted");
        }


        [Fact]
        public async Task UpdateAudio_ShouldReturnNotFound_WhenAudioDoesNotExist()
        {
            // Arrange
            int validId = 7;
            int invalidId = 9;
            var mockAudios = new List<Audio>();
            var onlyAudioInList = new Audio
            {
                Id = validId,
                Title = "Title7",
                Artist = "Artist7",
                Type = ".mp3",
                Size = 999,
                IsFavorite = false
            };
            mockAudios.Add(onlyAudioInList);

            _persistenceMock.Setup(p => p.LoadAudioListAsync()).ReturnsAsync(mockAudios);

            _audioService = new AudioServices(_persistenceMock.Object);
            await _audioService.InitializeAsync();
            _controller = new UserController(_audioService);

            var updateRequest = new UpdateAudioRequest
            {
                Title = "Nothing",
                Artist = "StillNothing",
                IsFavorite = true
            };

            // Act
            var result = await _controller.UpdateAudio(invalidId, updateRequest);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"Audio with id = {invalidId} not found", notFoundResult.Value);

            _output.WriteLine("Verified UpdateAudio returns NotFound when audio is not present in the list");
        }


        [Fact]
        public async Task UpdateAudio_ShouldReturnOk_WhenRequestIsCorrect_AndAudioExists()
        {
            // Arrange
            int validId = 7;
            var mockAudios = new List<Audio>();
            var onlyAudioInList = new Audio
            {
                Id = validId,
                Title = "Title7",
                Artist = "Artist7",
                Type = ".mp3",
                Size = 999,
                IsFavorite = false
            };
            mockAudios.Add(onlyAudioInList);

            _persistenceMock.Setup(p => p.LoadAudioListAsync()).ReturnsAsync(mockAudios);

            _audioService = new AudioServices(_persistenceMock.Object);
            await _audioService.InitializeAsync();
            _controller = new UserController(_audioService);

            var updateRequest = new UpdateAudioRequest
            {
                IsFavorite = true
            };

            // Act
            var result = await _controller.UpdateAudio(validId, updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var audio = Assert.IsType<Audio>(okResult.Value);
            Assert.True(audio.IsFavorite);

            _output.WriteLine("Verified UpdateAudio returns Ok when audio is present in the list and request is correct");
        }


        [Fact]
        public async Task UpdateAudio_ShouldReturnBadRequest_WhenRequestIsIncorrect()
        {
            // Arrange
            int validId = 7;
            var mockAudios = new List<Audio>();
            var onlyAudioInList = new Audio
            {
                Id = validId,
                Title = "Title7",
                Artist = "Artist7",
                Type = ".mp3",
                Size = 999,
                IsFavorite = false
            };
            mockAudios.Add(onlyAudioInList);

            _persistenceMock.Setup(p => p.LoadAudioListAsync()).ReturnsAsync(mockAudios);

            _audioService = new AudioServices(_persistenceMock.Object);
            await _audioService.InitializeAsync();
            _controller = new UserController(_audioService);

            // No values for the object's attributes -> Bad request (nothing to update)
            var updateRequest = new UpdateAudioRequest();

            // Act
            var result = await _controller.UpdateAudio(validId, updateRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal($"Audio with id {validId} should have a field to be modified", badRequestResult.Value);

            _output.WriteLine("Verified UpdateAudio returns BadRequest when audio is present in the list but request is incorrect");
        }


        [Fact]
        public async Task UploadAudio_ShouldReturnCreated_WhenInputIsValid()
        {
            // Arrange
            var mockFileContent = "Fake MP3 Content";
            var fileName = "test.mp3";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(mockFileContent));
            
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.Length).Returns(stream.Length);
            mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                    .Returns<Stream, CancellationToken>((targetStream, _) =>
                        stream.CopyToAsync(targetStream));

            var uploadedAudios = new List<Audio>();
            _persistenceMock.Setup(p => p.LoadAudioListAsync()).ReturnsAsync(uploadedAudios);
            _persistenceMock.Setup(p => p.SaveAudioListAsync(It.IsAny<List<Audio>>()))
                            .Callback<List<Audio>>(list => uploadedAudios = list)
                            .Returns(Task.CompletedTask);

            _audioService = new AudioServices(_persistenceMock.Object);
            await _audioService.InitializeAsync();
            _controller = new UserController(_audioService);

            var title = "TestTitle_Unique";
            var artist = "TestArtist";

            // Act
            var result = await _controller.UploadAudio(mockFile.Object, title, artist);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnedAudio = Assert.IsType<Audio>(createdResult.Value);

            Assert.Equal(title, returnedAudio.Title);
            Assert.Equal(artist, returnedAudio.Artist);
            Assert.Equal(".mp3", returnedAudio.Type);
            Assert.Equal(mockFileContent.Length, returnedAudio.Size);

            _output.WriteLine("Verified UploadAudio returns Created when input is valid and file is mocked");
        }


        [Fact]
        public async Task UploadAudio_ShouldReturnBadRequest_WhenFileIsMissing()
        {
            // Arrange
            _audioService = new AudioServices(_persistenceMock.Object);
            await _audioService.InitializeAsync();
            _controller = new UserController(_audioService);

            var title = "TestTitle";
            var artist = "TestArtist";

            // Act
            var result = await _controller.UploadAudio(null, title, artist);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var error = badRequestResult.Value?.ToString();
            Assert.Contains("The file could not be uploaded.", error);

            _output.WriteLine("Verified UploadAudio returns BadRequest when file is missing");
        }


        [Fact]
        public async Task UploadAudio_ShouldReturnBadRequest_WhenFileTypeIsInvalid()
        {
            // Arrange
            var invalidContent = "This is not an audio file";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidContent));

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("invalid.txt");
            mockFile.Setup(f => f.Length).Returns(stream.Length);
            mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                    .Returns<Stream, CancellationToken>((target, _) =>
                        stream.CopyToAsync(target));

            _audioService = new AudioServices(_persistenceMock.Object);
            await _audioService.InitializeAsync();
            _controller = new UserController(_audioService);

            var title = "TestTitle";
            var artist = "TestArtist";

            // Act
            var result = await _controller.UploadAudio(mockFile.Object, title, artist);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var error = badRequestResult.Value?.ToString();
            Assert.Contains("Unsupported file type", error);

            _output.WriteLine("Verified UploadAudio returns BadRequest when file type is invalid");
        }


        [Fact]
        public async Task UploadAudio_ShouldReturnBadRequest_WhenTitleOrArtistMissing()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("example.mp3");
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[1024]));

            _audioService = new AudioServices(_persistenceMock.Object);
            _controller = new UserController(_audioService);

            // Act – Missing title and artist
            var result = await _controller.UploadAudio(mockFile.Object, null!, null!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var message = Assert.IsType<string>(badRequest.Value);

            Assert.Equal("Title and Artist are required.", message);


            _output.WriteLine("Verified UploadAudio returns BadRequest when title or artist is missing");
        }


        [Fact]
        public async Task UploadAudio_ShouldReturnConflict_WhenDuplicateAudioIsUploaded()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("example.mp3");
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[1024]));

            string title = "TestTitleDuplicate";
            string artist = "TestArtistDuplicate";

            // Setup mock: first call succeeds, second throws
            _persistenceMock.SetupSequence(p => p.SaveAndReturnIdAsync(It.IsAny<Audio>()))
                .ReturnsAsync(42)
                .ThrowsAsync(new InvalidOperationException("Audio already exists"));

            _audioService = new AudioServices(_persistenceMock.Object);
            _controller = new UserController(_audioService);

            // First upload (succeeds)
            await _controller.UploadAudio(mockFile.Object, title, artist);
            // Second upload (duplicate)
            var result = await _controller.UploadAudio(mockFile.Object, title, artist);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Contains("already exists", conflictResult.Value?.ToString());

            _output.WriteLine("Verified UploadAudio returns Conflict on duplicate");
        }
    }
}
