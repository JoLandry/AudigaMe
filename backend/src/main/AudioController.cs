using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using AudioObjects;
using AudioUtils;

namespace HttpAudioControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IAudioService _audioService;

        // Constructor to inject the audio service
        public UserController(IAudioService audioService)
        {
            _audioService = audioService;
        }


        [HttpGet]
        [Route("/")]
        public Task<string> basicGetRequest()
        {
            return Task.FromResult("Hello World!");
        }


        // Send POST request
        public async Task sendPostForAudio(Audio audio, string path)
        {
            using (var httpClient = new HttpClient())
            {
                var formData = new MultipartFormDataContent();

                formData.Add(new StringContent(audio.getTitle()), "title");
                formData.Add(new StringContent(audio.getArtist()), "artist");
                formData.Add(new StringContent(audio.getType()), "type");
                formData.Add(new StringContent(audio.getId().ToString()), "id");

                byte[] audioData = await System.IO.File.ReadAllBytesAsync(path);
                formData.Add(new ByteArrayContent(audioData), "data", Path.GetFileName(path));

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
            using (var memoryStream = new MemoryStream())
            {
                await audioFile.CopyToAsync(memoryStream);
                fileData = memoryStream.ToArray();
            }

            try {
                var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(),"src","resources","uploads");
                // Save the file
                string newFileName = $"{title}{fileExtension}";
                var filePath = Path.Combine(uploadsDirectory, newFileName);
                using(var stream = new FileStream(filePath,FileMode.Create,FileAccess.Write)){
                    await audioFile.CopyToAsync(stream);
                }

                // Create a new Audio object
                var newAudio = new Audio(title,artist,fileData,fileExtension);
                AudioServices.addAudioToList(newAudio);

                Console.WriteLine($"Audio added with ID: {newAudio.getId()}, Title: {newAudio.getTitle()}, Artist: {newAudio.getArtist()}");
                
                // Save the Audio object
                await _audioService.SaveAsync(newAudio);

                return CreatedAtAction(nameof(getAudioById), new { id = newAudio.getId() }, newAudio);
            } catch (Exception e){
                return StatusCode(500, "Internal server error: " + e.Message);
            }
        }


        [HttpGet]
        [Route("/audios/{id}")]
        public async Task<IActionResult> getAudioById(int id)
        {
            var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(),"src","resources","uploads");

            Audio? audioToRetrieve = AudioServices.retrieveAudioById(id);

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


        [HttpDelete("{id:int}")]
        [Route("/audios/{id}")]
        public async Task<IActionResult> DeleteAudio(int id)
        {
            try{
                var audioToDelete = AudioServices.retrieveAudioById(id);
                if(audioToDelete == null){
                    return NotFound($"Audio with id = {id} not found");
                }
                await AudioServices.removeAudioFromList(audioToDelete);
                return NoContent();
            } catch(Exception){
                return StatusCode(StatusCodes.Status500InternalServerError,"Error when trying to delete data");
            }
        }


        [HttpGet]
        [Route("/audios")]
        public IActionResult GetAudioList()
        {
            List<Audio> audios = AudioServices.getAudioList();
            if(audios == null || audios.Count == 0){
                return NotFound("No audios found");
            }

            var audioMetadataList = audios.Select(audio => new 
            {
                Id = audio.getId(),
                Title = audio.getTitle(),
                Artist = audio.getArtist(),
                Type = audio.getType(),
                DownloadUrl = Url.Action(nameof(getAudioById), new { id = audio.getId() })
            }).ToList();

            return Ok(audioMetadataList);
        }


        /*
        Need of PUT Http method ?
        => Change name of audio ????

        [HttpPut("{id:int}")]
        [Route("/audios/{id}")]
        public async Task<IActionResult> UpdateAudio(int id)
        {
            // Smth
        }
        */
    }
}