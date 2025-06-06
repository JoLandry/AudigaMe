namespace AudioPlaylist
{
    public class Playlist
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class PlaylistAudio
    {
        public int PlaylistId { get; set; }
        public int AudioId { get; set; }
        public int Position { get; set; }
    }
}