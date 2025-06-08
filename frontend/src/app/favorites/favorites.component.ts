import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { getFavoritesList, favoritesList, changeFavoriteStatusAudio } from '../http-api';
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
}

