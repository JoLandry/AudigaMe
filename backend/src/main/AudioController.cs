using Microsoft.AspNetCore.Mvc;
using AudioObjects;
using AudioUtils;

namespace HttpAudioControllers
{

    public class UpdateAudioRequest
    {
        public string? Title { get; set; }
        public string? Artist { get; set; }
        public bool? IsFavorite { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IAudioService _audioService;
        public UserController(IAudioService audioService)
        {
            _audioService = audioService;
        }

        public async Task<IActionResult> InitializeAudioService()
        {
            try
            {
                await _audioService.InitializeAsync();
                return Ok("Audio service initialized successfully.");
            }
            catch(Exception ex)
            {
                return StatusCode(500, "Error initializing audio service: " + ex.Message);
            }
        }


        // Send POST request
        public async Task sendPostForAudio(Audio audio, string path)
        {
            using(var httpClient = new HttpClient())
            {
                var formData = new MultipartFormDataContent();

                formData.Add(new StringContent(audio.Title), "Title");
                formData.Add(new StringContent(audio.Artist), "Artist");
                formData.Add(new StringContent(audio.Type), "Type");
                formData.Add(new StringContent(audio.Id.ToString()), "Id");
                formData.Add(new StringContent(audio.Size.ToString()), "Size");
                formData.Add(new StringContent(audio.IsFavorite.ToString()), "False");

                byte[] audioData = await System.IO.File.ReadAllBytesAsync(path);
                formData.Add(new ByteArrayContent(audioData),"Data",Path.GetFileName(path));

                var response = await httpClient.PostAsync("http://localhost:5174/audios/",formData);

                if(response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Uploaded file successfully!");
                }
                else
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error uploading file: {response.StatusCode}. Message: {responseBody}");
                }
            }
        }


        // Handle POST request
        [HttpPost("audios")]
        public async Task<IActionResult> UploadAudio([FromForm] IFormFile? audioFile, [FromForm] string title, [FromForm] string artist)
        {
            // Check if audioFile has content
            if(audioFile == null || audioFile.Length == 0)
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
            if(fileExtension != ".mp3" && fileExtension != ".wav")
            {
                return BadRequest("Unsupported file type.");
            }

            // Read the file into a byte array
            byte[] fileData;
            using(var memoryStream = new MemoryStream())
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
        [Route("/audios/{id}")]
        public async Task<IActionResult> GetAudioById(int id)
        {
            var audioToRetrieve = await _audioService.RetrieveAudioById(id);
            if(audioToRetrieve == null)
            {
                return NotFound($"Audio with ID {id} not found.");
            }

            var rootDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"..", "..", "..", ".."));
            var uploadsDirectory = Path.Combine(rootDirectory,"backend","src","resources","uploads");
            string filePath = Path.Combine(uploadsDirectory,$"{id}{audioToRetrieve.Type}");

            if(!System.IO.File.Exists(filePath))
            {
                return NotFound($"File for audio ID {id} not found.");
            }

            // Read the file into a byte array
            byte[] fileData;
            try
            {
                fileData = await System.IO.File.ReadAllBytesAsync(filePath);
                if(fileData.Length == 0)
                {
                    Console.WriteLine($"File found but it is empty: {filePath}");
                    return NotFound($"File for audio ID {id} not found.");
                }
            }
            catch(IOException e)
            {
                return StatusCode(500, "I/O error occurred: " + e.Message);
            }
            catch(Exception e)
            {
                return StatusCode(500, "An unexpected error occurred: " + e.Message);
            }

            // Return the byte array as a file
            return File(fileData,"audio/mpeg");;
        }


        [HttpDelete("audios/{id:int}")]
        public async Task<IActionResult> DeleteAudio(int id)
        {
            try
            {
                var audioToDelete = await _audioService.RetrieveAudioById(id);
                if(audioToDelete == null)
                {
                    return NotFound($"Audio with id = {id} not found");
                }
                await _audioService.RemoveAudioFromList(audioToDelete);

                // Delete the file itself from disk (since local server)
                var rootDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"..", "..", "..", ".."));
                var uploadsDirectory = Path.Combine(rootDirectory,"backend","src","resources","uploads");
                string filePath = Path.Combine(uploadsDirectory,$"{id}{audioToDelete.Type}");

                if(System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                return NoContent();
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,"Error when trying to delete data");
            }
        }


        [HttpGet]
        [Route("/audios")]
        public async Task<IActionResult> GetAudioList()
        {
            var audios = await _audioService.GetAudioList();
            if(audios == null || audios.Count == 0)
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
                if(audioToUpdate == null)
                {
                    return NotFound($"Audio with id = {id} not found");
                }
                // Update if Title, Artist or IsFavorite changes
                if (updateRequest.IsFavorite.HasValue && (updateRequest.IsFavorite.Value.GetType() == typeof(bool)) ||
                    updateRequest.Title != null || updateRequest.Artist != null)
                {
                    // isFavorite
                    if (updateRequest.IsFavorite != null)
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
                }
                else
                {
                    return BadRequest($"Audio with id {id} should have a field to be modified");
                }

                return Ok(audioToUpdate);
            }
            catch(Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating the audio: " + ex.Message);
            }
        }
    }
}