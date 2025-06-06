import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterModule, RouterOutlet } from '@angular/router';
import { AudioType } from '../audio-type';
import { getMainPlaylist, audioList, downloadAudio, deleteAudio, changeArtistAudio, changeTitleAudio, changeFavoriteStatusAudio, getPlaylist,addAudioToPlaylist } from '../http-api';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-search-view',
  standalone: true,
  imports: [RouterOutlet, CommonModule, RouterModule],
  templateUrl: './search-view.component.html',
  styleUrls: ['./search-view.component.css', '../shared-playlist-style.css']
})
export class SearchViewComponent implements OnInit {
  searchedAudios: AudioType[] = [];
  hoveredAudio: number | null = null;
  openMenuId: number | null = null;
  query: string = '';

  constructor(private route: ActivatedRoute) {}

  async ngOnInit() {
    this.route.params.subscribe(async params => {
      this.query = (params['query'] ?? '').toLowerCase().trim();

      await getMainPlaylist();
      const allAudios = audioList;

      // Filter audios by title or artist
      this.searchedAudios = allAudios.filter(audio =>
        audio.title.toLowerCase().includes(this.query) ||
        audio.artist.toLowerCase().includes(this.query)
      );
    });
  }

  toggleMenu(id: number): void {
    this.openMenuId = this.openMenuId === id ? null : id;
  }

  async download(audio: AudioType) {
    try {
      await downloadAudio(audio);
    } catch (err) {
      console.error('Download failed:', err);
    }
    this.openMenuId = null;
  }

  async delete(audio: AudioType) {
    if (confirm(`Are you sure you want to delete "${audio.title}"?`)) {
      try {
        await deleteAudio(audio.id);
        this.searchedAudios = this.searchedAudios.filter(a => a.id !== audio.id);
      } catch (err) {
        console.error('Delete failed:', err);
      }
    }
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
