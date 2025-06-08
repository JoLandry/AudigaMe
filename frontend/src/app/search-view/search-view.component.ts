import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterModule, RouterOutlet } from '@angular/router';
import { AudioType } from '../audio-type';
import { getMainPlaylist, audioList, deleteAudio, changeArtistAudio, changeTitleAudio, changeFavoriteStatusAudio, getPlaylist } from '../http-api';
import { downloadAudio, addAudioToPlaylist } from '../utils'
import { CommonModule } from '@angular/common';
import { ToastrService } from 'ngx-toastr';

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

  constructor(private route: ActivatedRoute, private toastr: ToastrService) {}

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
      this.toastr.success('Downloaded audio successfully');
    } catch (err) {
      this.toastr.error('Failed to download audio');
    }
    this.openMenuId = null;
  }

  async delete(audio: AudioType) {
    if (confirm(`Are you sure you want to delete "${audio.title}"?`)) {
      try {
        await deleteAudio(audio.id);
        this.toastr.success('Audio successfully deleted from the server');
        this.searchedAudios = this.searchedAudios.filter(a => a.id !== audio.id);
      } catch (err) {
        this.toastr.success('Failed to delete audio from the server');
      }
    }
  }

  async changeTitle(audio: AudioType) {
    try {
      const title = prompt('Enter title for the audio:');
      if (title != null) {
        await changeTitleAudio(audio.id,title);
        this.toastr.success(`Update was successful, audio's title is now : "${title}"`);
      }
    } catch (error) {
      this.toastr.error(`Update for the title failed`);
    }
    this.openMenuId = null;
  }

  async changeArtist(audio: AudioType) {
    try {
      const artist = prompt('Enter artist for the audio:');
      if (artist != null) {
        await changeArtistAudio(audio.id,artist);
        this.toastr.success(`Update was successful, audio's artist is now : "${artist}"`);
      }
    } catch (error) {
      this.toastr.error(`Update for the artist failed`);
    }
    this.openMenuId = null;
  }

  async addToFavorites(audio: AudioType) {
    await changeFavoriteStatusAudio(audio.id,true);
    this.toastr.success('Aduio was successfully added to Favorites');
    this.openMenuId = null;
  }

  async addToPlaylist(audio: AudioType){
    const playlist = prompt('Enter the name of the playlist:');

    if (playlist != null && getPlaylist(playlist) != null) {
      await addAudioToPlaylist(playlist,audio.id);
      this.toastr.success(`Audio was successfully added to playlist : "${playlist}"`);
    } else {
      this.toastr.error('Audio could not be added to a playlist');
    }
    this.openMenuId = null;
  }
}
