using AudioObjects;
using Npgsql;

namespace AudioPersistenceService
{
    public interface IAudioPersistence
    {
        Task SaveAudioListAsync(List<Audio> audioList);
        Task<List<Audio>> LoadAudioListAsync();
        Task<List<Audio>> LoadFavoritesListAsync();
        Task UpdateAudioAsync(Audio audio);
        Task DeleteAudioAsync(Audio audio);
        Task<int> SaveAndReturnIdAsync(Audio audio);
    }

    public class AudioPersistence : IAudioPersistence
    {
        private readonly string _connectionString;

        public AudioPersistence(string connectionString)
        {
            _connectionString = connectionString;
        }

        public AudioPersistence()
        {
            _connectionString = "";
        }
        

        public async Task<List<Audio>> LoadAudioListAsync()
        {
            var audioList = new List<Audio>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // Retrieve all Audios (rows) from Database
            var cmd = new NpgsqlCommand("SELECT id, title, artist, audioFormat, audioSize, isFavorite FROM Audio;", conn);
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    audioList.Add(new Audio
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        Title = reader.GetString(reader.GetOrdinal("title")),
                        Artist = reader.GetString(reader.GetOrdinal("artist")),
                        Type = reader.GetString(reader.GetOrdinal("audioFormat")),
                        Size = reader.GetInt32(reader.GetOrdinal("audioSize")),
                        IsFavorite = reader.GetBoolean(reader.GetOrdinal("isFavorite"))
                    });
                }
            }

            return audioList;
        }


        public async Task SaveAudioListAsync(List<Audio> audioList)
        {
            foreach (var audio in audioList)
            {
                await UpdateAudioAsync(audio);
            }
        }


        public async Task UpdateAudioAsync(Audio audio)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // Update audio changes in the Database
            var cmd = new NpgsqlCommand(@"
                UPDATE Audio
                SET title = @title,
                    artist = @artist,
                    audioFormat = @format,
                    audioSize = @size,
                    isFavorite = @fav
                WHERE id = @id;", conn);

            cmd.Parameters.AddWithValue("@id", audio.Id);
            cmd.Parameters.AddWithValue("@title", audio.Title);
            cmd.Parameters.AddWithValue("@artist", audio.Artist);
            cmd.Parameters.AddWithValue("@format", audio.Type);
            cmd.Parameters.AddWithValue("@size", audio.Size);
            cmd.Parameters.AddWithValue("@fav", audio.IsFavorite);

            // Commit changes (validate transaction)
            await cmd.ExecuteNonQueryAsync();
        }


        public async Task DeleteAudioAsync(Audio audio)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // Delete Audio from Database
            var cmd = new NpgsqlCommand(@"DELETE FROM Audio WHERE id = @id;", conn);
            cmd.Parameters.AddWithValue("@id", audio.Id);

            // Commit changes (validate transaction)
            await cmd.ExecuteNonQueryAsync();
        }


        public async Task<int> SaveAndReturnIdAsync(Audio audio)
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(
                @"INSERT INTO Audio (title, artist, audioFormat, audioSize, isFavorite)
                VALUES (@title, @artist, @format, @size, @fav)
                RETURNING id;", conn);

            cmd.Parameters.AddWithValue("@title", audio.Title);
            cmd.Parameters.AddWithValue("@artist", audio.Artist);
            cmd.Parameters.AddWithValue("@format", audio.Type);
            cmd.Parameters.AddWithValue("@size", audio.Size);
            cmd.Parameters.AddWithValue("@fav", audio.IsFavorite);

            var id = await cmd.ExecuteScalarAsync();

            return Convert.ToInt32(id);
        }


        public async Task<List<Audio>> LoadFavoritesListAsync()
        {
            var favoritesList = new List<Audio>();
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // Retrieve Audios with favorite status true
            var cmd = new NpgsqlCommand("SELECT id, title, artist, audioFormat, audioSize, isFavorite FROM Audio WHERE isFavorite = TRUE;", conn);
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    favoritesList.Add(new Audio
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        Title = reader.GetString(reader.GetOrdinal("title")),
                        Artist = reader.GetString(reader.GetOrdinal("artist")),
                        Type = reader.GetString(reader.GetOrdinal("audioFormat")),
                        Size = reader.GetInt32(reader.GetOrdinal("audioSize")),
                        IsFavorite = reader.GetBoolean(reader.GetOrdinal("isFavorite"))
                    });
                }
            }

            return favoritesList;
        }
    }
}