using AudioPlaylist;
using Npgsql;

namespace PlaylistService
{
    public interface IPlaylistManager
    {
        Task<List<Playlist>> GetAllPlaylists();
        Task<List<Playlist>> LoadPlaylistsAsync();
        Task DeletePlaylist(string playlistName);
        Task CreatePlaylist(string playlistName);
        Task AddAudioToPlaylist(string playlistName, int audioId);
        Task RemoveAudioFromPlaylist(string playlistName, int audioId);
        Task<Playlist?> GetPlaylistByNameAsync(string name);
        Task<List<int>> GetAudioIdsForPlaylistAsync(int playlistId);
    }

    public class PlaylistManager : IPlaylistManager
    {
        private readonly string _connectionString;

        public PlaylistManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<Playlist>> GetAllPlaylists()
        {
            var playlists = new List<Playlist>();

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand("SELECT id,name FROM Playlist;", conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                playlists.Add(new Playlist
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
            }

            return playlists;
        }


        public Task<List<Playlist>> LoadPlaylistsAsync()
        {
            return GetAllPlaylists();
        }


        public async Task CreatePlaylist(string playlistName)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand("INSERT INTO Playlist (name) VALUES (@name);", conn);
            cmd.Parameters.AddWithValue("name", playlistName);

            // Commit changes (validate transaction)
            await cmd.ExecuteNonQueryAsync();
        }


        public async Task DeletePlaylist(string playlistName)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand("DELETE FROM Playlist WHERE name = @name;", conn);
            cmd.Parameters.AddWithValue("name", playlistName);

            // Commit changes (validate transaction)
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AddAudioToPlaylist(string playlistName, int audioId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // Retrieve playlist ID
            var getPlaylistIdCmd = new NpgsqlCommand("SELECT id FROM Playlist WHERE name = @name;", conn);
            getPlaylistIdCmd.Parameters.AddWithValue("name", playlistName);
            var playlistId = (int?)await getPlaylistIdCmd.ExecuteScalarAsync();

            if (playlistId == null)
            {
                throw new Exception("Playlist not found.");
            }

            // Determine the next position with COALESCE keyword
            var getPositionCmd = new NpgsqlCommand("SELECT COALESCE(MAX(position),0)+1 FROM Playlist_Audio WHERE playlist_id = @playlistId;", conn);
            getPositionCmd.Parameters.AddWithValue("playlistId", playlistId);
            var position = (int?)await getPositionCmd.ExecuteScalarAsync();

            // Insert into Playlist_Audio table
            var insertCmd = new NpgsqlCommand();
            if (position != null)
            {
                insertCmd = new NpgsqlCommand(
                    "INSERT INTO Playlist_Audio (playlist_id, audio_id, position) VALUES (@playlistId, @audioId, @position);", conn);
                insertCmd.Parameters.AddWithValue("playlistId", playlistId);
                insertCmd.Parameters.AddWithValue("audioId", audioId);
                insertCmd.Parameters.AddWithValue("position", position);
            }

            // Commit changes (validate transaction)
            await insertCmd.ExecuteNonQueryAsync();
        }

        public async Task RemoveAudioFromPlaylist(string playlistName, int audioId)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // Retrieve playlist ID
            var getPlaylistIdCmd = new NpgsqlCommand("SELECT id FROM Playlist WHERE name = @name;", conn);
            getPlaylistIdCmd.Parameters.AddWithValue("name", playlistName);
            var playlistId = (int?)await getPlaylistIdCmd.ExecuteScalarAsync();

            if (playlistId == null)
            {
                throw new Exception("Playlist not found.");
            }

            // Delete from Playlist_Audio
            var deleteCmd = new NpgsqlCommand("DELETE FROM Playlist_Audio WHERE playlist_id = @playlistId AND audio_id = @audioId;", conn);
            deleteCmd.Parameters.AddWithValue("playlistId", playlistId);
            deleteCmd.Parameters.AddWithValue("audioId", audioId);

            // Commit changes (validate transaction)
            await deleteCmd.ExecuteNonQueryAsync();
        }


        public async Task<Playlist?> GetPlaylistByNameAsync(string name)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string query = "SELECT id, name FROM Playlist WHERE name = @name";
            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("name", name);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Playlist
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                };
            }

            return null;
        }
        

        public async Task<List<int>> GetAudioIdsForPlaylistAsync(int playlistId)
        {
            var audioIds = new List<int>();

            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            const string query = "SELECT audio_id FROM Playlist_Audio WHERE playlist_id = @playlistId ORDER BY position ASC";
            await using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("playlistId", playlistId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                audioIds.Add(reader.GetInt32(0));
            }

            return audioIds;
        }
    }
}
