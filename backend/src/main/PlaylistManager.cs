using System.Text.Json;
using AudioPlaylist;

namespace PlaylistService
{
    public interface IPlaylistManager
    {
        Task<List<Playlist>> GetPlaylists();
        Task<List<Playlist>> LoadPlaylistsAsync();
        Task SavePlaylistsAsync(List<Playlist> playlists);
        Task AddAudioToPlaylist(string playlistName, int audioId);
        Task RemoveAudioFromPlaylist(string playlistName, int audioId);
        Task DeletePlaylist(string playlistName);
        Task CreatePlaylist(string playlistName);
    }

    public class PlaylistManager : IPlaylistManager
    {
        private readonly string? _filePath;
        private List<Playlist> _playlists = new List<Playlist>();

        public PlaylistManager()
        {
            var rootDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
            var resourcesDirectory = Path.Combine(rootDirectory, "backend", "src", "resources");
            _filePath = Path.Combine(resourcesDirectory, "playlists.json");

            if (_filePath == null)
            {
                return;
            }

            // Load existing playlists into memory
            if (File.Exists(_filePath))
            {
                _playlists = JsonSerializer.Deserialize<List<Playlist>>(File.ReadAllText(_filePath)) ?? new List<Playlist>();
            }
        }

        public async Task<List<Playlist>> GetPlaylists()
        {
            return await Task.FromResult(_playlists);
        }

        public async Task SavePlaylistsAsync(List<Playlist> playlists)
        {
            var json = JsonSerializer.Serialize(playlists, new JsonSerializerOptions { WriteIndented = true });
            if (_filePath != null)
            {
                await File.WriteAllTextAsync(_filePath, json);
            }
            _playlists = playlists;
        }

        public async Task<List<Playlist>> LoadPlaylistsAsync()
        {
            if (!File.Exists(_filePath))
            {
                return new List<Playlist>();
            }

            var json = await File.ReadAllTextAsync(_filePath);
            var playlists = JsonSerializer.Deserialize<List<Playlist>>(json) ?? new List<Playlist>();
            _playlists = playlists;

            return playlists;
        }

        public async Task AddAudioToPlaylist(string playlistName, int audioId)
        {
            var playlist = _playlists.FirstOrDefault(p => p.Name == playlistName);

            if (playlist == null)
            {
                playlist = new Playlist(playlistName, new List<int>());
                _playlists.Add(playlist);
            }
            if (!playlist.AudioIds.Contains(audioId))
            {
                playlist.AudioIds.Add(audioId);
            }

            if (_filePath != null)
            {
                var json = JsonSerializer.Serialize(_playlists, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_filePath, json);
            }
        }

        public async Task RemoveAudioFromPlaylist(string playlistName, int audioId)
        {
            var playlist = _playlists.FirstOrDefault(p => p.Name == playlistName);

            if (playlist == null)
            {
                return;
            }
            if (playlist.AudioIds.Contains(audioId))
            {
                playlist.AudioIds.Remove(audioId);
            }

            if (_filePath != null)
            {
                var json = JsonSerializer.Serialize(_playlists, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_filePath, json);
            }
        }

        public async Task DeletePlaylist(string playlistName)
        {
            var playlist = _playlists.FirstOrDefault(p => p.Name == playlistName);
            if (playlist != null)
            {
                _playlists.Remove(playlist);
                if (_filePath != null)
                {
                    var json = JsonSerializer.Serialize(_playlists, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(_filePath, json);
                }
            }
        }

        public async Task CreatePlaylist(string playlistName)
        {
            var playlist = _playlists.FirstOrDefault(p => p.Name == playlistName);
            if (playlist == null)
            {
                playlist = new Playlist(playlistName);
                _playlists.Add(playlist);
                
                if (_filePath != null)
                {
                    var json = JsonSerializer.Serialize(_playlists, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(_filePath, json);
                }
            }
        }
    }
}
