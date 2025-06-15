import { Component, ElementRef, ViewChild, ViewEncapsulation } from '@angular/core';
import { AudioType } from '../audio-type';
import { getPlaylist, appURL, audiosURL } from '../http-api';
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

  isPlaying: boolean = false;
    currentAudioInfo: string | null = null;
  
  @ViewChild('audioPlayer') audioPlayerRef!: ElementRef<HTMLAudioElement>;
  currentIndex: number = 0;

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

  playPlaylist() {
    if (this.audios.length === 0){
      return;
    }
    this.currentIndex = 0;
    this.isPlaying = true;
    setTimeout(() => {
      this.playCurrentAudio();
    });
  }

  stopPlaylist() {
    if (this.audioPlayerRef?.nativeElement) {
      this.audioPlayerRef.nativeElement.pause();
      this.audioPlayerRef.nativeElement.currentTime = 0;
    }
    this.isPlaying = false;
    this.currentIndex = 0;
    this.currentAudioInfo = null;
  }

  playCurrentAudio() {
    const currentAudio = this.audios[this.currentIndex];
    if (currentAudio == null){
      return;
    }
    const audioPlayer = this.audioPlayerRef.nativeElement;
    audioPlayer.src = appURL + audiosURL + currentAudio.id + '/file';

    const audioTitle = currentAudio.title;
    const audioArtist = currentAudio.artist;
    this.currentAudioInfo = `${audioTitle} - ${audioArtist}`;
    
    audioPlayer.play().catch(err => console.error("Playback failed", err));
  }

  onAudioEnded() {
    this.currentIndex++;
    if (this.currentIndex < this.audios.length) {
      this.playCurrentAudio();
    }
  }
}
