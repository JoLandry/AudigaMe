import { AudioType, PlaylistType } from './audio-type';

export const audiosURL = '/audios/';
export const appURL = 'http://localhost:5174';
export const controllerEndpoint = '/api/User';
export const favoritesURL = '/favorites';
export const playlistsURL = '/playlists/';

export const audioList: AudioType[] = [];
export const favoritesList: AudioType[] = [];
export const allPlaylists: PlaylistType[] = [];


// HTTP request to GET all the audios upload on the server
export async function getMainPlaylist(){
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


// GET request to retrieve audio by its id
export async function getAudio(id: number): Promise<AudioType | null> {
  try {
    const response = await fetch(`${appURL}${audiosURL}${id}`);

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    } 

    const audio = await response.json() as AudioType;
    return audio;
  } catch (error) {
    console.error("Error fetching audio:", error);
    return null;
  }
}


// HTTP request to POST an audio on the server
export async function uploadAudio(file: File, title: string, artist: string) {
  const formData = new FormData();
  formData.append('audioFile', file);
  formData.append('title', title);
  formData.append('artist', artist);

  const response = await fetch(appURL + controllerEndpoint + audiosURL, {
    method: 'POST',
    body: formData
  });

  if (!response.ok) {
    const errorText = await response.text();
    console.error("Full backend error:", errorText);
    console.error("HTTP Status:", response.status);
    throw new Error(`Upload failed: ${errorText}`);
  }

  return response.json();
}


// HTTP request to remove audio with id {id} from the server
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


// HTTP PUT request to change the status of favorite field for a given audio
// removes or adds the audio to or from the Favorites playlist
export async function changeFavoriteStatusAudio(id: number, newStatus: boolean) {
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


// HTTP PUT request to change the Title of an audio
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


// HTTP PUT request to change the Artist of an audio
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


// HTTP request to GET the favorites playlist
export async function getFavoritesList(){
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


// Check if an audio is in the list of favorites
export function isAudioInFavorites(audio: AudioType) {
  return favoritesList.some(fav => fav.id === audio.id);
}


// Donwnload audio file from the server to disk
export async function downloadAudio(audioToDL: AudioType) {
  // Create filename from metadata
  const sanitize = (str: string) => str.replace(/[<>:"/\\|?*]+/g, '').trim();
  const audioTitle = sanitize(audioToDL.title || 'Untitled');
  const audioArtist = sanitize(audioToDL.artist || 'Unknown Artist');
  const fileName = `${audioTitle} - ${audioArtist}.mp3`;

  try {
    const response = await fetch(appURL + audiosURL + audioToDL.id + '/file', {
      method: 'GET'
    });

    if (!response.ok) {
      throw new Error(`Failed to download audio. Status: ${response.status}`);
    }

    const blob = await response.blob();
    const blobUrl = URL.createObjectURL(blob);

    // Create and trigger download link
    const a = document.createElement('a');
    a.href = blobUrl;
    a.download = fileName;
    a.style.display = 'none';
    document.body.appendChild(a);
    a.click();

    // Cleanup
    URL.revokeObjectURL(blobUrl);
    document.body.removeChild(a);
  } catch (err) {
    console.error("Error downloading audio:", err);
  }
}


// Download favorites playlist to disk
export async function downloadFavoritesPlaylist(){
  favoritesList.forEach(function(audio){
    downloadAudio(audio)
  });
}


// GET HTTP method to retrieve all the playlists on the server
export async function getAllPlaylists(): Promise<PlaylistType[]> {
  try {
    const response = await fetch(appURL + playlistsURL, {
      method: 'GET',
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const playlists: PlaylistType[] = await response.json();

    allPlaylists.length = 0;
    for (const playlist of playlists) {
      allPlaylists.push(playlist);
    }

    localStorage.setItem('allPlaylists', JSON.stringify(playlists));
    return playlists;
  } catch (error) {
    console.error("Error loading all playlists:", error);
    return [];
  }
}


// GET HTTP method to retrieve a specific playlist based on its name
export async function getPlaylist(playlistName: string): Promise<AudioType[] | null> {
  try {
    const response = await fetch(appURL + playlistsURL + playlistName, {
      method: 'GET',
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const audios = await response.json() as AudioType[];

    return audios;
  } catch (error) {
    console.error(`Error fetching playlist "${playlistName}":`, error);
    return null;
  }
}


// HTTP Delete request to remove an audio from a given playlist
export async function removeAudioFromPlaylist(playlistName: string, audioId: number): Promise<boolean> {
  try {
    const response = await fetch(appURL + playlistsURL + playlistName + audiosURL + audioId, {
      method: 'DELETE',
    });

    return response.ok;
  } catch (err) {
    console.error(`Error removing audio ${audioId} from playlist ${playlistName}:`, err);
    return false;
  }
}


// HTTP POST request to add an audio to a given playlist
export async function addAudioToPlaylist(playlistName: string, audioId: number): Promise<boolean> {
  try {
    const response = await fetch(appURL + playlistsURL + playlistName + audiosURL, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ audioId })
    });

    return response.ok;
  } catch (err) {
    console.error(`Error removing audio ${audioId} from playlist ${playlistName}:`, err);
    return false;
  }
}


// HTTP POST request to create a new playlist
export async function createPlaylist(playlistName: string): Promise<boolean> {
  try {
    const response = await fetch(appURL + playlistsURL + playlistName + '/', {
      method: 'POST',
    });

    return response.ok;
  } catch (error) {
    console.error(`Error creating playlist "${playlistName}":`, error);
    return false;
  }
}


// HTTP DELETE request to delete a playlist by name
export async function deletePlaylist(playlistName: string): Promise<boolean> {
  try {
    const response = await fetch(appURL + playlistsURL + playlistName + '/', {
      method: 'DELETE',
    });

    return response.ok;
  } catch (error) {
    console.error(`Error deleting playlist "${playlistName}":`, error);
    return false;
  }
}