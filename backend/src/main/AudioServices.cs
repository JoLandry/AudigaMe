using AudioObjects;
using AudioPersistenceService;

namespace AudioUtils
{

    public interface IAudioService
    {
        Task InitializeAsync();
        Task<List<Audio>> GetAudioList();
        Task<List<Audio>> GetFavoritesList();
        Task<bool> AddAudioToList(Audio audio);
        Task RemoveAudioFromList(Audio audio);
        void AddAudioToFavorites(Audio audio);
        void RemoveAudioFromFavorites(Audio audio);
        Task<Audio?> RetrieveAudioById(int id);
        Task SaveAsync(Audio audio);
        Task SaveAudioListAsync();
        Task LoadFavoritesListAsync();
        Task LoadAudioListAsync();
    }

    public class AudioServices : IAudioService
    {
        private List<Audio> audioList;
        private readonly IAudioPersistence _persistenceService;
        private List<Audio> favoritesList;

        public AudioServices(IAudioPersistence persistenceService)
        {
            _persistenceService = persistenceService;
            audioList = new List<Audio>();
            favoritesList = new List<Audio>();
        }

        public AudioServices()
        {
            audioList = new List<Audio>();
            favoritesList = new List<Audio>();
            _persistenceService = new AudioPersistence();
        }

        public async Task InitializeAsync()
        {
            await LoadAudioListAsync();
            await LoadFavoritesListAsync();
        }

        public Task<List<Audio>> GetAudioList()
        {
            return Task.FromResult(audioList);
        }

        public Task<List<Audio>> GetFavoritesList()
        {
            return Task.FromResult(favoritesList);
        }

        public async Task<bool> AddAudioToList(Audio audio)
        {
            // Check if the element (Audio object) is already in the list
            foreach (Audio audioCmp in audioList)
            {
                if (audioCmp.Equals(audio))
                {
                    return false;
                }
            }
            audio.Id = await _persistenceService.SaveAndReturnIdAsync(audio);
            audioList.Add(audio);

            return true;
        }

        public async Task RemoveAudioFromList(Audio audio)
        {
            var audioToRemove = audioList.FirstOrDefault(a => a.Id == audio.Id);
            if (audioToRemove != null)
            {
                await _persistenceService.DeleteAudioAsync(audioToRemove);
                audioList.Remove(audioToRemove);
            }
        }

        public void AddAudioToFavorites(Audio audio)
        {
            // Check if the element (Audio object) is already in the list
            foreach (Audio audioCmp in favoritesList)
            {
                if (audioCmp.Equals(audio))
                {
                    return;
                }
            }
            favoritesList.Add(audio);
        }

        public void RemoveAudioFromFavorites(Audio audio)
        {
            var audioToRemove = favoritesList.FirstOrDefault(a => a.Id == audio.Id);
            if (audioToRemove != null)
            {
                favoritesList.Remove(audioToRemove);
            }
        }

        public async Task<Audio?> RetrieveAudioById(int id)
        {
            List<Audio> listOfAudios = await GetAudioList();
            return listOfAudios.FirstOrDefault(audio => audio.Id == id);
        }

        public async Task SaveAsync(Audio audio)
        {
            await _persistenceService.UpdateAudioAsync(audio);
        }

        public async Task SaveAudioListAsync()
        {
            await _persistenceService.SaveAudioListAsync(audioList);
        }

        public async Task LoadAudioListAsync()
        {
            audioList = await _persistenceService.LoadAudioListAsync();
        }

        public async Task LoadFavoritesListAsync()
        {
            favoritesList = await _persistenceService.LoadFavoritesListAsync();
        }
    }
}