import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { getFavoritesList, favoritesList, changeFavoriteStatusAudio, downloadAudio } from '../http-api';
import { AudioType } from '../audio-type';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

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
      // Refresh after deletion 
      this.loadFavorites();
    }
    this.openMenuId = null;
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

