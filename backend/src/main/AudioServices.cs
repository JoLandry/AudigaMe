using AudioObjects;

namespace AudioUtils
{
    public class AudioServices
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
            // Check if element (Audio object) is already in the list
            foreach(Audio audioCmp in listOfAudios){
                if(audioCmp.Equals(audio)){
                    return false;
                }
            }
            audioList.Add(audio);
            return true;
        }

        public static void removeAudioFromList(Audio audio)
        {
            List<Audio> listOfAudios = getAudioList();
            if(listOfAudios.Contains(audio)){
                listOfAudios.Remove(audio);
            }
        }
    }
}