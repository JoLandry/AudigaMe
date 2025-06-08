import { AudioType, PlaylistType } from './audio-type';

export const audiosURL = '/audios/';
export const appURL = 'http://localhost:5174';
export const controllerEndpoint = '/api/User';
export const favoritesURL = '/favorites';
export const playlistsURL = '/playlists/';

// Global variable containing all the audios present on the server
export const audioList: AudioType[] = [];

// Global variable containing all the audios saved as Favorites
export const favoritesList: AudioType[] = [];

// Global variable containing all the playlists in the app
export const allPlaylists: PlaylistType[] = [];


/* HTTP request to GET all the audios upload on the server */
export async function getMainPlaylist(){
  // fetch
  try {
    const response = await fetch(audiosURL, {
      method: "GET",
    });

    if(!response.ok)
    {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    // Fill the list of audios
    const data = (await response.json()) as AudioType[];
    audioList.length = 0;
    audioList.push(...data);
    console.log("Main playlist loaded successfully");
  } catch(error){
    console.error("Error loading playlist:", error);
  }
}


getMainPlaylist();


/* GET request to retrieve audio by its id */
export async function getAudio(id: number): Promise<AudioType | null> {
  // fetch
  try {
    const response = await fetch(`${appURL}${audiosURL}${id}`);

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    } 

    // Catch in JSON
    const audio = await response.json() as AudioType;
    return audio;
  } catch (error) {
    console.error("Error fetching audio:", error);
    return null;
  }
}


/* HTTP request to POST an audio on the server */
export async function uploadAudio(file: File, title: string, artist: string) {
  // Prepare request
  const formData = new FormData();
  formData.append('audioFile', file);
  formData.append('title', title);
  formData.append('artist', artist);

  // POST
  const response = await fetch(appURL + controllerEndpoint + audiosURL, {
    method: 'POST',
    body: formData
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(`Upload failed: ${errorText}`);
  }

  return response.json();
}


/* HTTP request to remove audio with id {id} from the server */
export async function deleteAudio(id: number) {
  try {
    const response = await fetch(`${appURL}${controllerEndpoint}${audiosURL}${id}`, {
      method: 'DELETE',
    });

    if (!response.ok) {
      throw new Error(`Failed to delete audio. Status: ${response.status}`);
    }

    // Only parse JSON if there's a non-empty body
    const text = await response.text();
    const result = text ? JSON.parse(text) : null;

    // Refresh the playlist after deletion
    await getMainPlaylist();
  } catch (error) {
    console.error("Error deleting audio:", error);
  }
}


/* HTTP PUT request to change the status of favorite field for a given audio
removes or adds the audio to or from the Favorites playlist */
export async function changeFavoriteStatusAudio(id: number, newStatus: boolean) {
  // Prepare the request
  const updateRequest = {
    title: null,
    artist: null,
    isFavorite: newStatus
  };

  try {
    const response = await fetch(`${appURL}${controllerEndpoint}${audiosURL}${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(updateRequest)
    });

    if (!response.ok) {
      throw new Error(`Failed to update favorite status. Status: ${response.status}`);
    }

    // Update the local favoritesList
    const updatedAudio = audioList.find(audio => audio.id === id);
    if (updatedAudio != null) {
      updatedAudio.favorite = newStatus;
      const indexInFavorites = favoritesList.findIndex(audio => audio.id === id);

      if (newStatus) {
        // Add to favorites if not already present
        if (indexInFavorites === -1) {
          favoritesList.push(updatedAudio);
        }
      } else {
        // Remove from favorites if present
        if (indexInFavorites !== -1) {
          favoritesList.splice(indexInFavorites, 1);
        }
      }
    }
  } catch (error) {
    console.error("Error updating favorite status:", error);
  }
}


/* HTTP PUT request to change the Title of an audio */
export async function changeTitleAudio(id: number, newTitle: string) {
  const updateRequest = {
    title: newTitle,
    artist: null,
    isFavorite: null
  };

  const response = await fetch(`${appURL}${controllerEndpoint}${audiosURL}${id}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(updateRequest)
  });

  if (!response.ok) {
    throw new Error(`Failed to update title. Status: ${response.status}`);
  }

  // Refresh after update was successful
  await getMainPlaylist();
}


/* HTTP PUT request to change the Artist of an audio */
export async function changeArtistAudio(id: number, newArtist: string) {
  const updateRequest = {
    title: null,
    artist: newArtist,
    isFavorite: null
  };

  const response = await fetch(`${appURL}${controllerEndpoint}${audiosURL}${id}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(updateRequest)
  });

  if (!response.ok) {
    throw new Error(`Failed to update artist. Status: ${response.status}`);
  }

  // Refresh after update was successful
  await getMainPlaylist();
}


/* HTTP request to GET the favorites playlist */
export async function getFavoritesList(){
  // fetch
  try {
    const response = await fetch(appURL + favoritesURL, {
      method: "GET",
    });

    if(!response.ok)
    {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    // Fill the favorites with audios
    const data = (await response.json()) as AudioType[];
    favoritesList.length = 0;
    favoritesList.push(...data);
    console.log("Favorites list loaded successfully");
  } catch(error){
    console.error("Error loading list of favorites:", error);
  }
}


/* GET HTTP method to retrieve all the playlists on the server */
export async function getAllPlaylists(): Promise<PlaylistType[]> {
  // fetch
  try {
    const response = await fetch(appURL + playlistsURL, {
      method: 'GET',
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    // Catch response in JSON
    const playlists: PlaylistType[] = await response.json();

    // Populate global variable containing the playlists of the app
    allPlaylists.length = 0;
    for (const playlist of playlists) {
      allPlaylists.push(playlist);
    }
    // Cache
    localStorage.setItem('allPlaylists', JSON.stringify(playlists));
    return playlists;
  } catch (error) {
    console.error("Error loading all playlists:", error);
    return [];
  }
}


/* GET HTTP method to retrieve a specific playlist based on its name */
export async function getPlaylist(playlistName: string): Promise<AudioType[] | null> {
  // fetch
  try {
    const response = await fetch(appURL + playlistsURL + playlistName, {
      method: 'GET',
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    // Catch response in JSON
    const audios = await response.json() as AudioType[];
    return audios;
  } catch (error) {
    console.error(`Error fetching playlist "${playlistName}":`, error);
    return null;
  }
}