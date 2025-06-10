import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterModule, Router } from '@angular/router';
import { getMainPlaylist, uploadAudio, getAllPlaylists, allPlaylists, favoritesList, audioList, getFavoritesList } from './http-api';
import { createPlaylist, deletePlaylist, currentPlaylistContext } from './utils';
import { FormsModule } from '@angular/forms';
import { PlaylistType } from './audio-type';
import { ToastrService } from 'ngx-toastr';

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
  settingsOpen = false;
  aboutVisible = false;

  constructor(private router: Router, private toastr: ToastrService) {}

  async ngOnInit() {
    await this.loadPlaylists();
    document.addEventListener('click', this.closeSettingsIfClickedOutside.bind(this));
  }

  ngOnDestroy() {
    document.removeEventListener('click', this.closeSettingsIfClickedOutside.bind(this));
  }

  async loadPlaylists() {
    this.playlists = await getAllPlaylists();
  }

  toggleSettingsMenu() {
    this.settingsOpen = !this.settingsOpen;
  }

  clearCache() {
    localStorage.clear();
    this.toastr.success('Cache was successfully cleared');
  }

  openAbout() {
    this.aboutVisible = true;
  }

  closeAbout() {
    this.aboutVisible = false;
  }

  private closeSettingsIfClickedOutside(event: MouseEvent) {
    if (!this.settingsOpen){
      return;
    }

    const settingsEl = document.querySelector('.settings-container');
    if (settingsEl && !settingsEl.contains(event.target as Node)) {
      this.settingsOpen = false;
    }
  }

  // Upload audio
  async handleFileSelect(event: Event) {
    // File selector
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (file == null) {
      this.toastr.error(`No file was selected`);
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
      await uploadAudio(file, title, artist);
      this.toastr.success(`Audio uploaded successfully`);

      // Reset attributes when POST is successful
      this.title = '';
      this.artist = '';
    } catch (error) {
      this.toastr.error(`The upload of the audio failed`);
    }

    // Reset file input so same file can be re-selected if needed
    input.value = '';
    // Get playlist after uploading the audio (for refresh)
    getMainPlaylist();
  }

  async createNewPlaylist() {
    // Ask for playlist name
    const playlistName = prompt("Enter the name of the new playlist:");
    if (playlistName == null){
      return;
    }

    const success = await createPlaylist(playlistName);
    if (success) {
      this.toastr.success(`Playlist : "${playlistName}" created`);
      this.loadPlaylists();
    } else {
      this.toastr.error(`Failed to create playlist : "${playlistName}"`);
    }
  }

  async removePlaylist() {
    // Ask for playlist name
    const playlistName = prompt("Enter the name of the playlist to delete:");
    if (playlistName == null){
      this.toastr.error(`No playlist name was specified, nothing happened`);
      return;
    } 

    const confirmed = confirm(`Are you sure you want to delete "${playlistName}"?`);
    if (!confirmed){
      return;
    }

    const success = await deletePlaylist(playlistName);
    if (success) {
      this.toastr.success(`Deleted playlist : "${playlistName}"`);
      this.loadPlaylists();
    } else {
      this.toastr.error(`Failed to delete playlist : "${playlistName}"`);
    }
  }

  onSearch(event: Event) {
    const input = event.target as HTMLInputElement;
    const query = input.value.trim();
    if (query.length > 0) {
      this.router.navigate(['/search', query]);
    }
  }

  triggerSearch(input: HTMLInputElement): void {
    this.onSearch({ target: input } as unknown as Event);
  }

  async setCurrentPlaylistContext(playlistName: string) {
    const playlist = allPlaylists.find(p => p.playlistName === playlistName);
    if (playlist != null) {
      currentPlaylistContext.length = 0;
      currentPlaylistContext.push(...playlist.audios);
    } else {
      console.error(`Playlist "${playlistName}" not found in allPlaylists`);
    }
  }

  async setCurrentPlaylistContextFavorites() {
    // Refresh
    await getFavoritesList();
    if (favoritesList != null) {
      currentPlaylistContext.length = 0;
      currentPlaylistContext.push(...favoritesList);
    } else {
      console.error(`The Favorites could not be retrieved or does not contain any audio`);
    }
  }

  async setCurrentPlaylistContextMain() {
    // Refresh
    await getMainPlaylist();
    if (audioList != null) {
      currentPlaylistContext.length = 0;
      currentPlaylistContext.push(...audioList);
    } else {
      console.error(`The main playlist could not be retrieved or does not contain any audio`);
    }
  }
}