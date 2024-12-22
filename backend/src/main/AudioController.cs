using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using AudioObjects;
using AudioPersistenceService;
using AudioUtils;
using System.Collections.Generic;
using System.Linq;

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
            try {
                await _audioService.InitializeAsync();
                return Ok("Audio service initialized successfully.");
            } catch(Exception ex){
                return StatusCode(500, "Error initializing audio service: " + ex.Message);
            }
        }

        [HttpGet]
        [Route("/")]
        public Task<string> basicGetRequest()
        {
            return Task.FromResult("Hello World! Welcome to AudigaMe!");
        }


        // Send POST request
        public async Task sendPostForAudio(Audio audio, string path)
        {
            using(var httpClient = new HttpClient())
            {
                var formData = new MultipartFormDataContent();

                formData.Add(new StringContent(audio.getTitle()), "title");
                formData.Add(new StringContent(audio.getArtist()), "artist");
                formData.Add(new StringContent(audio.getType()), "type");
                formData.Add(new StringContent(audio.getId().ToString()), "id");
                formData.Add(new StringContent(audio.isAudioFavorite().ToString()), "false");

                byte[] audioData = await System.IO.File.ReadAllBytesAsync(path);
                formData.Add(new ByteArrayContent(audioData),"data",Path.GetFileName(path));

                var response = await httpClient.PostAsync("http://localhost:5174/audios/",formData);

                if(response.IsSuccessStatusCode){
                    Console.WriteLine("Uploaded file successfully!");
                }
                else {
                    Console.WriteLine($"Error uploading file: {response.StatusCode}");
                }
            }
        }


        // Handle POST request
        [HttpPost("audios")]
        public async Task<IActionResult> uploadAudio([FromForm] IFormFile audioFile, [FromForm] string title, [FromForm] string artist)
        {
            // Check if audioFile has content
            if(audioFile == null || audioFile.Length == 0){
                return BadRequest("The file could not be uploaded.");
            }
            // Check if the media type is valid
            string fileExtension = Path.GetExtension(audioFile.FileName);
            if(fileExtension != ".mp3" && fileExtension != ".wav"){
                return BadRequest("Unsupported file type.");
            }

            // Read the file into a byte array
            byte[] fileData;
            using(var memoryStream = new MemoryStream())
            {
                await audioFile.CopyToAsync(memoryStream);
                fileData = memoryStream.ToArray();
            }

            try {
                var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(),"src","resources","uploads");
                // Save the file
                string newFileName = $"{title}{fileExtension}";
                var filePath = Path.Combine(uploadsDirectory,newFileName);
                using(var stream = new FileStream(filePath,FileMode.Create,FileAccess.Write)){
                    await audioFile.CopyToAsync(stream);
                }

                // Create a new Audio object
                var newAudio = new Audio(title,artist,fileData,fileExtension);
                await _audioService.addAudioToList(newAudio);

                Console.WriteLine($"Audio added with ID: {newAudio.getId()}, Title: {newAudio.getTitle()}, Artist: {newAudio.getArtist()}");

                return CreatedAtAction(nameof(getAudioById), new { id = newAudio.getId() }, newAudio);
            } catch (Exception e){
                return StatusCode(500, "Internal server error: " + e.Message);
            }
        }


        [HttpGet]
        [Route("/audios/{id}")]
        public async Task<IActionResult> getAudioById(int id)
        {
            var audioToRetrieve = await _audioService.retrieveAudioById(id);

            /*
            string? filePath = null;
            if(audioToRetrieve != null){
                var fileName = audioToRetrieve.getTitle();
                var fileExt = audioToRetrieve.getType();
                filePath = Path.Combine(uploadsDirectory,fileName+fileExt);
            }
            // Check if the file exists
            if(filePath == null || !System.IO.File.Exists(filePath)){
                return NotFound();
            }
            */

            if(audioToRetrieve == null){
                return NotFound($"Audio with ID {id} not found.");
            }

            var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "src", "resources", "uploads");
            string filePath = Path.Combine(uploadsDirectory, $"{audioToRetrieve.getTitle()}{audioToRetrieve.getType()}");

            if(!System.IO.File.Exists(filePath)){
                return NotFound();
            }

            // Read the file into a byte array
            byte[] fileData;
            try {
                fileData = await System.IO.File.ReadAllBytesAsync(filePath);
            } catch (IOException e){
                return StatusCode(500, "I/O error occurred: " + e.Message);
            } catch (Exception e){
                return StatusCode(500, "An unexpected error occurred: " + e.Message);
            }

            if(fileData.Length == 0){
                Console.WriteLine($"File found but it is empty: {filePath}");
                return NotFound();
            }

            // Return the byte array as a file
            return File(fileData,"audio/mpeg");;
        }


        [HttpDelete("audios/{id:int}")]
        public async Task<IActionResult> DeleteAudio(int id)
        {
            Console.WriteLine("Entering DeleteAudio method\n");
            try {
                var audioToDelete = await _audioService.retrieveAudioById(id);
                if(audioToDelete == null){
                    Console.WriteLine("Audio not found\n");
                    return NotFound($"Audio with id = {id} not found");
                }
                Console.WriteLine("Before remove method\n");
                await _audioService.removeAudioFromList(audioToDelete);
                Console.WriteLine("After remove method\n");
                return NoContent();
            } catch(Exception){
                return StatusCode(StatusCodes.Status500InternalServerError,"Error when trying to delete data");
            }
        }


        [HttpGet]
        [Route("/audios")]
        public async Task<IActionResult> GetAudioList()
        {
            var audios = await _audioService.getAudioList();
            if(audios == null || audios.Count == 0){
                return NotFound("No audios found");
            }

            var audioMetadataList = audios.Select(audio => new 
            {
                Id = audio.getId(),
                Title = audio.getTitle(),
                Artist = audio.getArtist(),
                Type = audio.getType(),
                DownloadUrl = Url.Action(nameof(getAudioById), new { id = audio.getId() }),
                IsFavorite = audio.isAudioFavorite()
            }).ToList();

            return Ok(audioMetadataList);
        }


        [HttpPut("audios/{id:int}")]
        public async Task<IActionResult> UpdateAudio(int id, [FromBody] UpdateAudioRequest updateRequest)
        {
            try {
                var audioToUpdate = await _audioService.retrieveAudioById(id);
                if(audioToUpdate == null){
                    return NotFound($"Audio with id = {id} not found");
                }
                if(!string.IsNullOrEmpty(updateRequest.Title)){
                    audioToUpdate.setTitle(updateRequest.Title);
                }
                if(!string.IsNullOrEmpty(updateRequest.Artist)){
                    audioToUpdate.setArtist(updateRequest.Artist);
                }
                if(updateRequest.IsFavorite.HasValue){
                    audioToUpdate.setFavoriteStatus(updateRequest.IsFavorite.Value);
                }

                await _audioService.SaveAsync(audioToUpdate);
                return Ok(audioToUpdate);
            } catch(Exception ex){
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating the audio: " + ex.Message);
            }
        }
    }
}