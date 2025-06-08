import { favoritesList, appURL, audiosURL, playlistsURL } from './http-api'
import { AudioType } from './audio-type'


/* Check if an audio is in the list of favorites */
export function isAudioInFavorites(audio: AudioType) {
  return favoritesList.some(fav => fav.id === audio.id);
}


/* Donwnload audio file from the server to disk */
export async function downloadAudio(audioToDL: AudioType) {
  // Create filename from metadata
  const sanitize = (str: string) => str.replace(/[<>:"/\\|?*]+/g, '').trim();
  const audioTitle = sanitize(audioToDL.title || 'Untitled');
  const audioArtist = sanitize(audioToDL.artist || 'Unknown Artist');
  const fileName = `${audioTitle} - ${audioArtist}.mp3`;

  // fetch
  try {
    const response = await fetch(appURL + audiosURL + audioToDL.id + '/file', {
      method: 'GET'
    });

    if (!response.ok) {
      throw new Error(`Failed to download audio. Status: ${response.status}`);
    }

    // Create BLOB
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


/* Download favorites playlist to disk */
export async function downloadFavoritesPlaylist(){
  favoritesList.forEach(function(audio){
    downloadAudio(audio)
  });
}


/* HTTP Delete request to remove an audio from a given playlist */
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


/* HTTP POST request to add an audio to a given playlist */
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


/* HTTP POST request to create a new playlist */
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


/* HTTP DELETE request to delete a playlist by name */
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