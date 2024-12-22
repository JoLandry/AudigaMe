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
        private string title;
        private string artist;
        private string path;
        private byte[] data;
        /* Take a look at MediaTypeHeaderValue class */ private string type;
        private int id;
        private bool isFavorite;

        private static int count = 0;


        public Audio(string _title, string _artist, byte[] _data, string _type)
        {
            count++;
            id = count;
            title = _title;
            artist = _artist;
            type = _type;
            path = "/resources/audios/" + title + type;
            data = _data;
            isFavorite = false;
        }


        public int getId()
        {
            return this.id;
        }

        public string getType()
        {
            return this.type;
        }

        public string getArtist()
        {
            return this.artist;
        }

        public void setArtist(string artist)
        {
            this.artist = artist;
        }

        public string getTitle()
        {
            return this.title;
        }

        public void setTitle(string title)
        {
            this.title = title;
        }

        public byte[] getData()
        {
            return this.data;
        }

        public string getPath()
        {
            return this.path;
        }

        public bool isAudioFavorite()
        {
            return this.isFavorite;
        }

        public void setFavoriteStatus(bool status)
        {
            this.isFavorite = status;
        }


        // Two Audio objects are considered equal when their title AND artist(s) are the same
        // or when their id is the same
        public override bool Equals(object? o)
        {
            if(o == null || GetType() != o.GetType()){
                return false;
            }
            Audio otherAudio = (Audio)o;
            return this.id == otherAudio.getId();
        }

        public override int GetHashCode()
        {
            return this.id.GetHashCode();
        }
    }
}