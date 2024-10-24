using AudioObjects;
using System.IO;
using System.Threading.Tasks;

namespace AudioUtils
{
    public class AudioServices: IAudioService
    {
        private static List<Audio> audioList = new List<Audio>();

        public AudioServices(){}


        public static List<Audio> getAudioList()
        {
            return audioList;
        }

        public static bool addAudioToList(Audio audio)
        {
            List<Audio> listOfAudios = getAudioList();

            // Check if the element (Audio object) is already in the list
            foreach(Audio audioCmp in listOfAudios){
                if(audioCmp.Equals(audio)){
                    return false;
                }
            }
            audioList.Add(audio);
            return true;
        }

        public static async Task removeAudioFromList(Audio audio)
        {
            List<Audio> listOfAudios = getAudioList();
            if (listOfAudios.Contains(audio)){
                listOfAudios.Remove(audio);
                await Task.CompletedTask;
            }
        }

        public static Audio? retrieveAudioById(int id)
        {
            List<Audio> listOfAudios = getAudioList();
            foreach(Audio audioCmp in listOfAudios){
                if(audioCmp.getId() == id){
                    return audioCmp;
                }
            }
            return null;
        }


        public async Task SaveAsync(Audio audio)
        {
            // Example: Save the audio data to the file system
            var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(),"src","resources","uploads");

            // Ensure the directory exists
            if(!Directory.Exists(uploadsDirectory)){
                Directory.CreateDirectory(uploadsDirectory);
            }

            // Define the file path
            var filePath = Path.Combine(uploadsDirectory, audio.getTitle() + audio.getType());

            // Write the audio data to the file
            await File.WriteAllBytesAsync(filePath,audio.getData());

            //addAudioToList(audio);
        }
    }
}