namespace AudioObjects
{
    public class Audio
    {
        private string title;
        private string artist;
        private string path;
        private byte[] data;
        /* Take a look at MediaTypeHeaderValue class */ private string type;
        private int id;

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

        public string getTitle()
        {
            return this.title;
        }

        public byte[] getData()
        {
            return this.data;
        }
    }
}