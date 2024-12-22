using AudioObjects;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AudioPersistenceService
{
    public class AudioPersistence
    {
        private readonly string _filePath;

        public AudioPersistence(string filePath)
        {
            _filePath = filePath;
        }

        public async Task SaveAudioListAsync(List<Audio> audioList)
        {
            var jsonData = JsonSerializer.Serialize(audioList, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath,jsonData);
        }

        public async Task<List<Audio>> LoadAudioListAsync()
        {
            if(!File.Exists(_filePath)){
                return new List<Audio>();
            }
            var jsonData = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<Audio>>(jsonData) ?? new List<Audio>();
        }
    }
}
