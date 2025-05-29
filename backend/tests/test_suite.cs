using Xunit;
using Moq;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using HttpAudioControllers;
using AudioPersistenceService;
using AudioUtils;
using AudioObjects;
using System.Net;
using System.Text.Json;


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
        public async Task GetAudioById_ShouldReturnNotFound_WhenAudioDoesNotExist()
        {
            // Arrange
            int invalidId = 99999999;
            _persistenceMock.Setup(p => p.LoadAudioListAsync()).ReturnsAsync(new List<Audio>());

            _audioService = new AudioServices(_persistenceMock.Object);
            await _audioService.InitializeAsync();
            _controller = new UserController(_audioService);

            // Act
            var result = await _controller.GetAudioById(invalidId);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"Audio with ID {invalidId} not found.", notFound.Value);

            _output.WriteLine("Verified GetAudioById returns 404 for missing audio");
        }


        [Fact]
        public async Task GetAudioById_ShouldReturnOk_WhenAudioExists()
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
            var result = await controller.GetAudioById(validId);

            // Assert
            var fileContentResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("audio/mpeg", fileContentResult.ContentType);
            Assert.NotNull(fileContentResult.FileContents);

            File.Delete(testFilePath);
            _output.WriteLine("Verified GetAudioById returns Ok when exists");
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
            string fileName = "example.mp3";
            var filePath = GetSampleFilePath(fileName);
            var fileBytes = await File.ReadAllBytesAsync(filePath);

            var id = Guid.NewGuid();
            var content = new MultipartFormDataContent
            {
                { new ByteArrayContent(fileBytes), "audioFile", fileName },
                { new StringContent("TestTitle_Unique_" + id), "title" },
                { new StringContent("WhateverArtist"), "artist" }
            };

            var client = CreateTestClient();

            // Act
            var response = await client.PostAsync("/api/User/audios", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.Contains("TestTitle_Unique_" + id, responseBody);
            Assert.Contains("WhateverArtist", responseBody);

            _output.WriteLine("Verified UploadAudio returns Created when input is valid");
        }


        [Fact]
        public async Task UploadAudio_ShouldReturnBadRequest_WhenFileIsMissing()
        {
            // Arrange
            var content = new MultipartFormDataContent
            {
                { new StringContent("TestTitle"), "title" },
                { new StringContent("TestArtist"), "artist" }
            };

            var client = CreateTestClient();

            // Act
            var response = await client.PostAsync("/api/User/audios", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseBody = await response.Content.ReadAsStringAsync();

            var json = JsonDocument.Parse(responseBody);
            var message = json.RootElement.GetProperty("errors").GetProperty("audioFile")[0].GetString();
            Assert.Equal("The audioFile field is required.", message);
        }


        [Fact]
        public async Task UploadAudio_ShouldReturnBadRequest_WhenFileTypeIsInvalid()
        {
            // Arrange
            string fileName = "invalid.txt";
            var filePath = GetSampleFilePath("example.mp3");
            var fileBytes = await File.ReadAllBytesAsync(filePath);

            var content = new MultipartFormDataContent
            {
                { new ByteArrayContent(fileBytes), "audioFile", fileName },
                { new StringContent("TestTitle"), "title" },
                { new StringContent("TestArtist"), "artist" }
            };

            var client = CreateTestClient();

            // Act
            var response = await client.PostAsync("/api/User/audios", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.Contains("Unsupported file type", responseBody);
        }


        [Fact]
        public async Task UploadAudio_ShouldReturnBadRequest_WhenTitleOrArtistMissing()
        {
            // Arrange
            string fileName = "example.mp3";
            var filePath = GetSampleFilePath(fileName);
            var fileBytes = await File.ReadAllBytesAsync(filePath);

            var content = new MultipartFormDataContent
            {
                { new ByteArrayContent(fileBytes), "audioFile", fileName },
            };

            var client = CreateTestClient();

            // Act
            var response = await client.PostAsync("/api/User/audios", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseBody = await response.Content.ReadAsStringAsync();

            var json = JsonDocument.Parse(responseBody);
            var errors = json.RootElement.GetProperty("errors");

            var titleError = errors.GetProperty("title")[0].GetString();
            var artistError = errors.GetProperty("artist")[0].GetString();

            Assert.Equal("The title field is required.", titleError);
            Assert.Equal("The artist field is required.", artistError);
        }


        [Fact]
        public async Task UploadAudio_ShouldReturnConflict_WhenDuplicateAudioIsUploaded()
        {
            // Arrange
            string fileName = "example.mp3";
            var filePath = GetSampleFilePath(fileName);
            var fileBytes = await File.ReadAllBytesAsync(filePath);

            var content = new MultipartFormDataContent
            {
                { new ByteArrayContent(fileBytes), "audioFile", fileName },
                { new StringContent("TestTitle"), "title" },
                { new StringContent("TestArtist"), "artist" }
            };

            var client = CreateTestClient();

            // First upload
            await client.PostAsync("/api/User/audios", content);
            // Second upload (conflict)
            var duplicateResponse = await client.PostAsync("/api/User/audios", content);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
            var responseBody = await duplicateResponse.Content.ReadAsStringAsync();
            Assert.Contains("already exists", responseBody);
        }


        // Utils methods and variables for the tests
        private static string GetSampleFilePath(string fileName) =>
            Path.Combine(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..")),
                        "backend", "src", "resources", "audioSamples", fileName);

        private HttpClient CreateTestClient()
        {
            DotNetEnv.Env.Load(Path.Combine(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..")),
                                            "backend", ".env"));

            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        var testConfig = new Dictionary<string, string?>
                        {
                            ["DB_HOST"] = Environment.GetEnvironmentVariable("DB_HOST"),
                            ["DB_USER"] = Environment.GetEnvironmentVariable("DB_USER"),
                            ["DB_PASSWORD"] = Environment.GetEnvironmentVariable("DB_PASSWORD"),
                            ["DB_NAME"] = Environment.GetEnvironmentVariable("DB_NAME"),
                            ["DB_PORT"] = Environment.GetEnvironmentVariable("DB_PORT")
                        };

                        config.AddInMemoryCollection(testConfig!);
                    });
                });

            return factory.CreateClient();
        }
    }
}
