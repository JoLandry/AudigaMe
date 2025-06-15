using Microsoft.AspNetCore.Mvc;
using AudioObjects;
using AudioUtils;
using PlaylistService;

namespace HttpAudioControllers
{

    public class UpdateAudioRequest
    {
        public string? Title { get; set; }
        public string? Artist { get; set; }
        public bool? IsFavorite { get; set; }
    }

    public class AudioAddToPlaylistRequest
    {
        public int AudioId { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IAudioService _audioService;
        private readonly IPlaylistManager _playlistManager;

        public UserController(IAudioService audioService, IPlaylistManager playlistManager)
        {
            _audioService = audioService;
            _playlistManager = playlistManager;
        }

        public async Task<IActionResult> InitializeAudioService()
        {
            try
            {
                await _audioService.InitializeAsync();
                await _playlistManager.LoadPlaylistsAsync();

                return Ok("Audio service and Playlist manager initialized successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error initializing audio service and playlist manager: " + ex.Message);
            }
        }


        // Handle POST request
        [HttpPost("audios")]
        public async Task<IActionResult> UploadAudio([FromForm] IFormFile? audioFile, [FromForm] string title, [FromForm] string artist)
        {
            // Check if audioFile has content
            if (audioFile == null || audioFile.Length == 0)
            {
                return BadRequest("The file could not be uploaded.");
            }
            // Check if title and artist are present
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(artist))
            {
                return BadRequest("Title and Artist are required.");
            }
            // Check if the media type is valid
            string fileExtension = Path.GetExtension(audioFile.FileName);
            if (fileExtension != ".mp3" && fileExtension != ".wav")
            {
                return BadRequest("Unsupported file type.");
            }

            // Read the file into a byte array
            byte[] fileData;
            using (var memoryStream = new MemoryStream())
            {
                await audioFile.CopyToAsync(memoryStream);
                fileData = memoryStream.ToArray();
            }

            // Create a new Audio object
            var newAudio = new Audio
            {
                Title = title,
                Artist = artist,
                Type = fileExtension,
                Size = fileData.Length,
                IsFavorite = false
            };

            try
            {
                // Save audio metadata to DB and generate ID
                bool addedSuccessfully = await _audioService.AddAudioToList(newAudio);
                if (!addedSuccessfully)
                {
                    return Conflict("This audio file already exists.");
                }

                // Save the file to disk (local server)
                var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "src", "resources", "uploads");

                if (!Directory.Exists(uploadsDirectory))
                {
                    Directory.CreateDirectory(uploadsDirectory);
                }

                string newFileName = $"{newAudio.Id}{fileExtension}";
                var filePath = Path.Combine(uploadsDirectory, newFileName);
                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await audioFile.CopyToAsync(stream);
                }

                Console.WriteLine($"Audio added with ID: {newAudio.Id}, Title: {newAudio.Title}, Artist: {newAudio.Artist}");

                return CreatedAtAction(nameof(GetAudioById), new { id = newAudio.Id }, newAudio);
            }
            catch (Exception e)
            {
                return StatusCode(500, "Internal server error: " + e.Message);
            }
        }


        [HttpGet]
        [Route("/audios/{id}/file")]
        public async Task<IActionResult> GetAudioFileById(int id)
        {
            var audioToRetrieve = await _audioService.RetrieveAudioById(id);
            if (audioToRetrieve == null)
            {
                return NotFound($"Audio with ID {id} not found.");
            }

            var rootDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
            var uploadsDirectory = Path.Combine(rootDirectory, "backend", "src", "resources", "uploads");
            string filePath = Path.Combine(uploadsDirectory, $"{id}{audioToRetrieve.Type}");

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound($"File for audio ID {id} not found.");
            }

            // Read the file into a byte array
            byte[] fileData;
            try
            {
                fileData = await System.IO.File.ReadAllBytesAsync(filePath);
                if (fileData.Length == 0)
                {
                    Console.WriteLine($"File found but it is empty: {filePath}");
                    return NotFound($"File for audio ID {id} not found.");
                }
            }
            catch (IOException e)
            {
                return StatusCode(500, "I/O error occurred: " + e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, "An unexpected error occurred: " + e.Message);
            }

            // Return the byte array as a file
            return File(fileData, "audio/mpeg"); ;
        }


        [HttpGet]
        [Route("/audios/{id}")]
        public async Task<IActionResult> GetAudioById(int id)
        {
            var audioToRetrieve = await _audioService.RetrieveAudioById(id);
            if (audioToRetrieve == null)
            {
                return NotFound($"Audio with ID {id} not found.");
            }

            // Return Audio object in JSON format
            return Ok(audioToRetrieve);
        }


        [HttpDelete("audios/{id:int}")]
        public async Task<IActionResult> DeleteAudio(int id)
        {
            try
            {
                var audioToDelete = await _audioService.RetrieveAudioById(id);
                if (audioToDelete == null)
                {
                    return NotFound($"Audio with id = {id} not found");
                }
                // Remove audio from the list containing all audios
                await _audioService.RemoveAudioFromList(audioToDelete);
                // Remove the audio from all the playlists it was in
                var playlists = await _playlistManager.GetAllPlaylists();
                foreach (var playlist in playlists)
                {
                    await _playlistManager.RemoveAudioFromPlaylist(playlist.Name,id);
                }

                // Delete the file itself from disk (since local server)
                var rootDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
                var uploadsDirectory = Path.Combine(rootDirectory, "backend", "src", "resources", "uploads");
                string filePath = Path.Combine(uploadsDirectory, $"{id}{audioToDelete.Type}");

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error when trying to delete data");
            }
        }


        [HttpGet]
        [Route("/audios")]
        public async Task<IActionResult> GetAudioList()
        {
            var audios = await _audioService.GetAudioList();
            if (audios == null || audios.Count == 0)
            {
                return NotFound("No audios found.");
            }

            var audioMetadataList = audios.Select(audio => new Audio
            {
                Id = audio.Id,
                Title = audio.Title,
                Artist = audio.Artist,
                Type = audio.Type,
                Size = audio.Size,
                IsFavorite = audio.IsFavorite
            }).ToList();

            return Ok(audioMetadataList);
        }


        [HttpPut("audios/{id:int}")]
        public async Task<IActionResult> UpdateAudio(int id, [FromBody] UpdateAudioRequest updateRequest)
        {
            try
            {
                var audioToUpdate = await _audioService.RetrieveAudioById(id);
                if (audioToUpdate == null)
                {
                    return NotFound($"Audio with id = {id} not found");
                }
                // Update if Title, Artist or IsFavorite changes
                if (updateRequest.IsFavorite.HasValue && (updateRequest.IsFavorite.Value.GetType() == typeof(bool)) ||
                    updateRequest.Title != null || updateRequest.Artist != null)
                {
                    // isFavorite
                    if (updateRequest.IsFavorite.HasValue)
                    {
                        audioToUpdate.IsFavorite = updateRequest.IsFavorite.Value;
                    }
                    // Title
                    if (updateRequest.Title != null)
                    {
                        audioToUpdate.Title = updateRequest.Title;
                    }
                    // Artist
                    if (updateRequest.Artist != null)
                    {
                        audioToUpdate.Artist = updateRequest.Artist;
                    }
                    await _audioService.SaveAsync(audioToUpdate);
                    await _audioService.LoadFavoritesListAsync();
                }
                else
                {
                    return BadRequest($"Audio with id {id} should have a field to be modified");
                }

                return Ok(audioToUpdate);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating the audio: " + ex.Message);
            }
        }


        [HttpGet]
        [Route("/favorites")]
        public async Task<IActionResult> GetFavoritesList()
        {
            var favoritesList = await _audioService.GetFavoritesList();
            if (favoritesList == null || favoritesList.Count == 0)
            {
                return NotFound("No audios found.");
            }

            var audioMetadataList = favoritesList.Select(audio => new Audio
            {
                Id = audio.Id,
                Title = audio.Title,
                Artist = audio.Artist,
                Type = audio.Type,
                Size = audio.Size,
                IsFavorite = audio.IsFavorite
            }).ToList();

            return Ok(audioMetadataList);
        }


        [HttpGet]
        [Route("/playlists")]
        public async Task<IActionResult> GetAllPlaylists()
        {
            var playlists = await _playlistManager.GetAllPlaylists();
            if (playlists == null || playlists.Count == 0)
            {
                return NotFound("No playlists found.");
            }

            var result = new List<object>();

            foreach (var playlist in playlists)
            {
                var audioIds = await _playlistManager.GetAudioIdsForPlaylistAsync(playlist.Id);
                var audioList = new List<Audio>();

                foreach (var audioId in audioIds)
                {
                    var audio = await _audioService.RetrieveAudioById(audioId);
                    if (audio != null)
                    {
                        audioList.Add(audio);
                    }
                }

                result.Add(new
                {
                    PlaylistName = playlist.Name,
                    Audios = audioList.Select(audio => new
                    {
                        audio.Id,
                        audio.Title,
                        audio.Artist,
                        audio.Type,
                        audio.Size,
                        audio.IsFavorite
                    }).ToList()
                });
            }

            return Ok(result);
        }


        [HttpGet]
        [Route("/playlists/{name}")]
        public async Task<IActionResult> GetPlaylist(string name)
        {
            var playlist = await _playlistManager.GetPlaylistByNameAsync(name);
            if (playlist == null)
            {
                return NotFound($"Playlist named '{name}' does not exist.");
            }

            var audioIds = await _playlistManager.GetAudioIdsForPlaylistAsync(playlist.Id);
            var audioList = new List<Audio>();

            // Populate with audios
            foreach (var audioId in audioIds)
            {
                var audio = await _audioService.RetrieveAudioById(audioId);
                if (audio != null)
                {
                    audioList.Add(audio);
                }
            }

            var result = audioList.Select(audio => new
            {
                audio.Id,
                audio.Title,
                audio.Artist,
                audio.Type,
                audio.Size,
                audio.IsFavorite
            }).ToList();

            return Ok(result);
        }


        [HttpPost]
        [Route("/playlists/{name}/audios")]
        public async Task<IActionResult> AddAudioToPlaylist(string name, [FromBody] AudioAddToPlaylistRequest request)
        {
            var playlist = await _playlistManager.GetPlaylistByNameAsync(name);

            if (playlist == null)
            {
                return NotFound($"Playlist '{name}' not found.");
            }

            await _playlistManager.AddAudioToPlaylist(name,request.AudioId);

            return Ok($"Audio with id : {request.AudioId} added to playlist '{name}'.");
        }


        [HttpDelete]
        [Route("/playlists/{name}/audios/{audioId}")]
        public async Task<IActionResult> RemoveAudioFromPlaylist(string name, int audioId)
        {
            var playlist = await _playlistManager.GetPlaylistByNameAsync(name);

            if (playlist == null)
            {
                return NotFound($"Playlist '{name}' not found.");
            }

            await _playlistManager.RemoveAudioFromPlaylist(name,audioId);

            return Ok($"Audio with id : {audioId} removed from playlist '{name}'.");
        }


        [HttpDelete]
        [Route("/playlists/{name}/")]
        public async Task<IActionResult> DeletePlaylist(string name)
        {
            var playlist = await _playlistManager.GetPlaylistByNameAsync(name);

            if (playlist == null)
            {
                return NotFound($"Playlist '{name}' not found.");
            }

            await _playlistManager.DeletePlaylist(name);

            return Ok($"Playlist named : {name} removed from the list of playlists.");
        }


        [HttpPost]
        [Route("/playlists/{name}/")]
        public async Task<IActionResult> CreatePlaylist(string name)
        {
            var playlist = await _playlistManager.GetPlaylistByNameAsync(name);

            if (playlist != null)
            {
                return Conflict($"Playlist '{name}' already exists.");
            }

            await _playlistManager.CreatePlaylist(name);

            return Ok($"Playlist named : {name} was successfully created.");
        }
    }
}