namespace AudioObjects
{
    public interface IAudioService
    {
        Task InitializeAsync();
        Task<List<Audio>> getAudioList();
        Task<bool> addAudioToList(Audio audio);
        Task removeAudioFromList(Audio audio);
        Task<Audio?> retrieveAudioById(int id);
        Task SaveAsync(Audio audio);
        Task SaveAudioListAsync();
    }


    public class Audio
    {
        public string Title {get;set;} = string.Empty;
        public string Artist {get;set;} = string.Empty;
        public string Path {get;set;} = string.Empty;
        public byte[] Data {get;set;} = Array.Empty<byte>();
        public string Type {get;set;} = string.Empty;
        public int Id {get;set;}
        public bool IsFavorite {get;set;}

        private static int count = 0;


        public Audio(string title, string artist, byte[] data, string type)
        {
            count++;
            Id = count;
            Title = title;
            Artist = artist;
            Type = type;
            Path = "/resources/audios/" + title + type;
            Data = data;
            IsFavorite = false;
        }
        
        public Audio(){

        }
        

        // Two Audio objects are considered equal when their title AND artist(s) are the same
        // or when their id is the same
        public override bool Equals(object? o)
        {
            if(o == null || GetType() != o.GetType()){
                return false;
            }
            Audio otherAudio = (Audio)o;
            return Id == otherAudio.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}