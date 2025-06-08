import { Component, ViewEncapsulation } from '@angular/core';
import { AudioType } from '../audio-type';
import { getPlaylist } from '../http-api';
import { downloadAudio, removeAudioFromPlaylist } from '../utils'
import { ActivatedRoute, RouterModule, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-playlist-view',
  standalone: true,
  imports: [RouterOutlet, CommonModule, RouterModule],
  templateUrl: './playlist-view.component.html',
  styleUrls: ['./playlist-view.component.css','../shared-playlist-style.css'],
  encapsulation: ViewEncapsulation.None
})
export class PlaylistViewComponent {
  playlistName: string = '';
  audios: AudioType[] = [];
  hoveredAudio: number | null = null;
  openMenuId: number | null = null;

  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.playlistName = params['name'] ?? '';
      this.loadPlaylist();
    });
  }

  async loadPlaylist() {
    const fetched = await getPlaylist(this.playlistName);
    if (fetched) {
      this.audios = fetched;
    }
  }

  toggleMenu(id: number): void {
    this.openMenuId = this.openMenuId === id ? null : id;
  }

  async removeAudio(audio: AudioType) {
    if (confirm(`Are you sure you want to delete "${audio.title}"?`)) {
      const success = await removeAudioFromPlaylist(this.playlistName, audio.id);
      if (success) {
        this.audios = this.audios.filter(a => a.id !== audio.id);
      }
    }
  }

  async download(audio: AudioType) {
    try {
      await downloadAudio(audio);
    } catch (err) {
      console.error('Download failed:', err);
    }
    this.openMenuId = null;
  }
}
