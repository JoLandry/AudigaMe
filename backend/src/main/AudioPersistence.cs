using AudioObjects;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AudioPersistenceService
{
    public interface IAudioPersistence
    {
        Task SaveAudioListAsync(List<Audio> audioList);
        Task<List<Audio>> LoadAudioListAsync();
    }

    public class AudioPersistence : IAudioPersistence
    {
        private readonly string _filePath;

        public AudioPersistence(string filePath)
        {
            _filePath = filePath;
        }

        public async Task SaveAudioListAsync(List<Audio> audioList)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var jsonString = JsonSerializer.Serialize(audioList,options);
            await File.WriteAllTextAsync(_filePath, jsonString);
        }

        public async Task<List<Audio>> LoadAudioListAsync()
        {
            if (!File.Exists(_filePath))
            {
                return new List<Audio>();
            }
            var jsonString = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<Audio>>(jsonString) ?? new List<Audio>();
        }
    }
}