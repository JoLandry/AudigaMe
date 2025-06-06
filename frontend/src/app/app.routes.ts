import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { AudioViewComponent } from './audio-view/audio-view.component';
import { FavoritesComponent } from './favorites/favorites.component';
import { PlaylistViewComponent } from './playlist-view/playlist-view.component';

export const routes: Routes = [
    { path: '', component: HomeComponent },
    { path: 'audios/:id', component: AudioViewComponent },
    { path: 'favorites', component: FavoritesComponent },
    { path: 'playlists/:name', component: PlaylistViewComponent },
];
