import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterModule } from '@angular/router';
import { getMainPlaylist, uploadAudio, getAllPlaylists, createPlaylist, deletePlaylist } from './http-api';
import { FormsModule } from '@angular/forms';
import { PlaylistType, AudioType } from './audio-type';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, FormsModule, RouterModule, CommonModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})

export class AppComponent {
  title: string = '';
  artist: string = '';

  playlists: PlaylistType[] = [];

  async ngOnInit() {
    await this.loadPlaylists();
  }

  async loadPlaylists() {
    this.playlists = await getAllPlaylists();
    console.log('Playlists loaded:', this.playlists);
  }

  // Upload audio
  async handleFileSelect(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (file == null) {
      console.error('No file selected');
      return;
    }

    // Ask for title and artist
    const title = prompt('Enter title for the audio:', this.title);
    const artist = prompt('Enter artist name for the audio:', this.artist);

    if (title == null || artist == null) {
      console.error('Upload cancelled: missing title or artist');
      return;
    }

    try {
      console.log("Uploading file:", file);
      console.log("Title:", title);
      console.log("Artist:", artist);

      const uploadedAudio = await uploadAudio(file, title, artist);
      console.log('Uploaded successfully:', uploadedAudio);

      // Reset attributes when POST is successful
      this.title = '';
      this.artist = '';
    } catch (error) {
      console.error('Upload error:', error);
    }

    // Reset file input so same file can be re-selected if needed
    input.value = '';
    // Get playlist after uploading the audio (for refresh)
    getMainPlaylist();
  }

  async createNewPlaylist() {
    const playlistName = prompt("Enter the name of the new playlist:");
    if (playlistName == null){
      return;
    }

    const success = await createPlaylist(playlistName);
    if (success) {
      this.loadPlaylists();
    } else {
      alert(`Failed to create playlist "${playlistName}".`);
    }
  }

  async removePlaylist() {
    const playlistName = prompt("Enter the name of the playlist to delete:");
    if (playlistName == null){
      return;
    } 

    const confirmed = confirm(`Are you sure you want to delete "${playlistName}"?`);
    if (!confirmed){
      return;
    }

    const success = await deletePlaylist(playlistName);
    if (success) {
      this.loadPlaylists();
    } else {
      alert(`Failed to delete playlist "${playlistName}".`);
    }
  }
}