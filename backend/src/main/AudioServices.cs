using AudioObjects;
using System.IO;
using AudioPersistenceService;
using System.Threading.Tasks;

namespace AudioUtils
{
    public class AudioServices: IAudioService
    {
        private List<Audio> audioList = new List<Audio>();
        private readonly AudioPersistence _persistenceService;

        public AudioServices(AudioPersistence persistenceService)
        {
            _persistenceService = persistenceService;
        }

        public async Task InitializeAsync()
        {
            await LoadAudioListAsync();
        }

        public async Task<List<Audio>> getAudioList()
        {
            return await Task.FromResult(audioList);
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
            var audioToRemove = this.audioList.FirstOrDefault(a => a.getId() == audio.getId());
            if(audioToRemove != null){
                this.audioList.Remove(audioToRemove);
                await SaveAudioListAsync();
            }
        }

        public async Task<Audio?> retrieveAudioById(int id)
        {
            List<Audio> listOfAudios = await getAudioList();
            return listOfAudios.FirstOrDefault(audio => audio.getId() == id);
        }

        public async Task SaveAsync(Audio audio)
        {
            await addAudioToList(audio);
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