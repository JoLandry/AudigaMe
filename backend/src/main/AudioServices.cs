using AudioObjects;
using System.IO;
using AudioPersistenceService;
using System.Threading.Tasks;

namespace AudioUtils
{
    public class AudioServices: IAudioService
    {
        private List<Audio> audioList;
        private readonly IAudioPersistence _persistenceService;

        public AudioServices(IAudioPersistence persistenceService)
        {
            _persistenceService = persistenceService;
            this.audioList = new List<Audio>();
        }

        public async Task InitializeAsync()
        {
            await LoadAudioListAsync();
        }

        public Task<List<Audio>> getAudioList()
        {
            return Task.FromResult(audioList);
        }

        public async Task<bool> addAudioToList(Audio audio)
        {
            // Check if the element (Audio object) is already in the list
            foreach(Audio audioCmp in this.audioList){
                if(audioCmp.Equals(audio)){
                    return false;
                }
            }
            this.audioList.Add(audio);
            await SaveAudioListAsync();
            return true;
        }

        public async Task removeAudioFromList(Audio audio)
        {
            var audioToRemove = this.audioList.FirstOrDefault(a => a.Id == audio.Id);
            if(audioToRemove != null){
                this.audioList.Remove(audioToRemove);
                await SaveAudioListAsync();
            }
        }

        public async Task<Audio?> retrieveAudioById(int id)
        {
            List<Audio> listOfAudios = await getAudioList();
            return listOfAudios.FirstOrDefault(audio => audio.Id == id);
        }

        public async Task SaveAsync(Audio audio)
        {
            var existingAudio = audioList.FirstOrDefault(a => a.Id == audio.Id);
            if(existingAudio != null){
                existingAudio.Title = audio.Title;
                existingAudio.Artist = audio.Artist;
                existingAudio.IsFavorite = audio.IsFavorite;
            }
            await SaveAudioListAsync();
        }

        public async Task SaveAudioListAsync()
        {
            await _persistenceService.SaveAudioListAsync(this.audioList);
        }

        public async Task LoadAudioListAsync()
        {
            this.audioList = await _persistenceService.LoadAudioListAsync();
        }
    }
}