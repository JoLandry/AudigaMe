namespace AudioPlaylist
{
    public class Playlist
    {
        public string Name { get; set; }
        public List<int> AudioIds { get; set; }

        public Playlist(string playlistName, List<int> ids)
        {
            Name = playlistName;
            AudioIds = ids;
        }

        public Playlist()
        {
            Name = "unnamedPlaylist";
            AudioIds = new List<int>();
        }

        // Two Playlist objects are considered equal if the share the same name
        // or if they are the same reference of course
        public override bool Equals(object? o)
        {
            if (o == null || GetType() != o.GetType())
            {
                return false;
            }
            if (o == this)
            {
                return true;
            }
            Playlist otherPlaylist = (Playlist)o;

            return Name == otherPlaylist.Name;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}