import { AudioType, UpdateRequest } from './audio-type';

export const audiosURL = '/audios/';
export const appURL = 'http://localhost:5174';
export const controllerEndpoint = '/api/User';

export const audioList: AudioType[] = [];
export const favoritesList: AudioType[] = [];



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
  const updateRequest = new UpdateRequest(null, null, newStatus);

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

    // Refresh when update was successful
    await getMainPlaylist();

  } catch (error) {
    console.error("Error updating favorite status:", error);
  }
}


// HTTP PUT request to change the Title of an audio
export async function changeTitleAudio(id: number, newTitle: string) {
  const updateRequest = new UpdateRequest(newTitle, null, null);

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
  const updateRequest = new UpdateRequest(null, newArtist, null);

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
export function getFavorites() : AudioType[] {
  return favoritesList;
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