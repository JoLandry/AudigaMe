//import fetch from 'node-fetch';
import { AudioType } from './audio-type';

export const audiosURL = '/audios/'

export const audioList: AudioType[] = [];




// HTTP request to GET all the audios upload on the server
export async function getMainPlaylist(){
    try {
      const response = await fetch(audiosURL, {
        method: "GET",
      });
  
      if(!response.ok){
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      // Fill the list of audios
      const data = (await response.json()) as AudioType[];
      audioList.length = 0;
      audioList.push(...data);
      console.log("Main playlist loaded successfully");
    } catch (error){
      console.error("Error loading playlist:", error);
    }
}


getMainPlaylist();


// Returns the audio corresponding to the one given in parameter
export function getAudio(){

}

// HTTP request to POST an audio on the server
export function uploadAudio(){

}

// HTTP request to DELETE the a given audio from the server
export function deleteAudio(){

}

// HTTP PUT request to change the status of favorite field for a given audio
// removes or adds the audio to or from the favorites playlist
export function changeFavoriteStatus(id:number, status: boolean){

}

// HTTP request to GET the favorites playlist
export function getFavorites(){

}

// Donwnload audio file from the server to disk
export async function downloadAudio(id: number){

}

// Download favorites playlist
export function downloadFavoritesPlaylist(){

}