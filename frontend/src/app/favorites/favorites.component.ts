import { Component, ElementRef, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { getFavoritesList, favoritesList, changeFavoriteStatusAudio, appURL, audiosURL } from '../http-api';
import { downloadAudio } from '../utils'
import { AudioType } from '../audio-type';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-favorites',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './favorites.component.html',
  styleUrls: ['./favorites.component.css','../shared-playlist-style.css'],
  encapsulation: ViewEncapsulation.None
})
export class FavoritesComponent implements OnInit {
  favorites: AudioType[] = [];
  hoveredAudio: number | null = null;
  openMenuId: number | null = null;

  isPlaying: boolean = false;
  currentAudioInfo: string | null = null;

  @ViewChild('audioPlayer') audioPlayerRef!: ElementRef<HTMLAudioElement>;
    currentIndex: number = 0;

  constructor(private router: Router, private toastr: ToastrService) {}

  ngOnInit() {
    this.loadFavorites();
  }

  async loadFavorites() {
    await getFavoritesList();
    this.favorites = favoritesList;
  }

  toggleMenu(id: number) {
    this.openMenuId = this.openMenuId === id ? null : id;
  }

  async removeFromFavorites(audio: AudioType) {
    if (confirm(`Remove "${audio.title}" from favorites?`)) {
      await changeFavoriteStatusAudio(audio.id,false);
      this.toastr.success(`Audio was successfully removed from Favorites`);
      // Refresh after deletion 
      this.loadFavorites();
    }
    this.openMenuId = null;
  }

  async download(audio: AudioType) {
    try {
      await downloadAudio(audio);
      this.toastr.success(`Downloaded audio successfully`);
    } catch (err) {
      this.toastr.error(`Failed to download audio`);
    }
    this.openMenuId = null;
  }

  playPlaylist() {
    if (this.favorites.length === 0){
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
    const currentAudio = this.favorites[this.currentIndex];
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
    if (this.currentIndex < this.favorites.length) {
      this.playCurrentAudio();
    }
  }
}

