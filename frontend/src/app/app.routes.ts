import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { AudioViewComponent } from './audio-view/audio-view.component';

export const routes: Routes = [
    { path: '', component: HomeComponent },
    { path: 'audios/:id', component: AudioViewComponent }
];
