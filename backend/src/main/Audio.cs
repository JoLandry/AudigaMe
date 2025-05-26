namespace AudioObjects
{
    public class Audio
    {
        public string Title {get;set;} = string.Empty;
        public string Artist {get;set;} = string.Empty;
        public string Path {get;set;} = string.Empty;
        public byte[] Data {get;set;} = Array.Empty<byte>();
        public string Type {get;set;} = string.Empty;
        public int Id {get;set;}
        public int Size {get;set;}
        public bool IsFavorite {get;set;}


        public Audio(string title, string artist, byte[] data, string type)
        {
            Title = title;
            Artist = artist;
            Type = type;
            Data = data;
            Size = data.Length;
            IsFavorite = false;
        }
        
        public Audio(){

        }
        

        // Two Audio objects are considered equal when their title AND artist(s) are the same
        // or when their id is the same
        public override bool Equals(object? o)
        {
            if(o == null || GetType() != o.GetType())
            {
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