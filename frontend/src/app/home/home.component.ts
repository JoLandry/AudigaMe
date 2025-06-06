import { Component, ViewEncapsulation } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { OnInit } from '@angular/core';
import { AudioType } from '../audio-type';
import { getMainPlaylist, audioList, deleteAudio, downloadAudio, changeTitleAudio, changeArtistAudio, changeFavoriteStatusAudio, addAudioToPlaylist, getPlaylist } from '../http-api';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterOutlet, CommonModule, RouterModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css','../shared-playlist-style.css'],
  encapsulation: ViewEncapsulation.None
})

export class HomeComponent implements OnInit {
  audios: AudioType[] = [];
  hoveredAudio: number | null = null;
  openMenuId: number | null = null;

  ngOnInit(){
    this.loadAudios();
  }

  async loadAudios(){
    await getMainPlaylist();
    this.audios = audioList;
  }

  toggleMenu(id: number) {
    this.openMenuId = this.openMenuId === id ? null : id;
  }

  async delete(audio: AudioType) {
    if (confirm(`Are you sure you want to delete "${audio.title}"?`)) {
      await deleteAudio(audio.id);
      this.loadAudios();
    }
    // Close menu when done
    this.openMenuId = null;
  }

  async download(audio: AudioType) {
    try {
      await downloadAudio(audio);
    } catch (error) {
      console.error('Download failed', error);
    }
    this.openMenuId = null;
  }

  async changeTitle(audio: AudioType) {
    try {
      const title = prompt('Enter title for the audio:');
      if (title != null) {
        await changeTitleAudio(audio.id,title);
      }
    } catch (error) {
      console.error('Update failed', error);
    }
    this.openMenuId = null;
  }

  async changeArtist(audio: AudioType) {
    try {
      const artist = prompt('Enter artist for the audio:');
      if (artist != null) {
        await changeArtistAudio(audio.id,artist);
      }
    } catch (error) {
      console.error('Update failed', error);
    }
    this.openMenuId = null;
  }

  async addToFavorites(audio: AudioType) {
    await changeFavoriteStatusAudio(audio.id,true);
    this.openMenuId = null;
  }

  async addToPlaylist(audio: AudioType){
    const playlist = prompt('Enter the name of the playlist:');

    if (playlist != null && getPlaylist(playlist) != null) {
      await addAudioToPlaylist(playlist,audio.id);
    }
    this.openMenuId = null;
  }
}