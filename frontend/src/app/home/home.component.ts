import { Component, ElementRef, ViewChild, ViewEncapsulation } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { OnInit } from '@angular/core';
import { AudioType } from '../audio-type';
import { getMainPlaylist, audioList, deleteAudio, changeTitleAudio, changeArtistAudio, changeFavoriteStatusAudio, getPlaylist, appURL, audiosURL } from '../http-api';
import { downloadAudio, addAudioToPlaylist } from '../utils'
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterOutlet, CommonModule, RouterModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css','../shared-playlist-style.css'],
  encapsulation: ViewEncapsulation.None
})

export class HomeComponent implements OnInit {
  audios: AudioType[] = [];
  hoveredAudio: number | null = null;
  openMenuId: number | null = null;

  @ViewChild('audioPlayer') audioPlayerRef!: ElementRef<HTMLAudioElement>;
  currentIndex: number = 0;

  constructor(private router: Router, private toastr: ToastrService) {}

  ngOnInit(){
    this.loadAudios();
  }

  async loadAudios(){
    await getMainPlaylist();
    this.audios = audioList;
  }

  toggleMenu(id: number) {
    this.openMenuId = this.openMenuId === id ? null : id;
  }

  playPlaylist() {
    if (this.audios.length === 0){
      return;
    }
    this.currentIndex = 0;
    this.playCurrentAudio();
  }

  playCurrentAudio() {
    const currentAudio = this.audios[this.currentIndex];
    if (!currentAudio){
      return;
    }
    const audioPlayer = this.audioPlayerRef.nativeElement;
    audioPlayer.src = appURL + audiosURL + currentAudio.id + '/file'
    audioPlayer.play().catch(err => console.error("Playback failed", err));
  }

  onAudioEnded() {
    this.currentIndex++;
    if (this.currentIndex < this.audios.length) {
      this.playCurrentAudio();
    }
  }

  async delete(audio: AudioType) {
    if (confirm(`Are you sure you want to delete "${audio.title}"?`)) {
      await deleteAudio(audio.id);
      this.toastr.success(`Audio was successfully removed from the server`);
      this.loadAudios();
    }
    // Close menu when done
    this.openMenuId = null;
  }

  async download(audio: AudioType) {
    try {
      await downloadAudio(audio);
      this.toastr.success(`Downloaded audio successfully`);
    } catch (error) {
      this.toastr.error(`The download of the audio failed`);
    }
    this.openMenuId = null;
  }

  async changeTitle(audio: AudioType) {
    try {
      const title = prompt('Enter title for the audio:');
      if (title != null) {
        await changeTitleAudio(audio.id,title);
        this.toastr.success(`Update was successful, audio's title is now : "${title}"`);
      }
    } catch (error) {
      this.toastr.error(`The update of the title failed`);
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
      this.toastr.error(`The update of the artist failed`);
    }
    this.openMenuId = null;
  }

  async addToFavorites(audio: AudioType) {
    await changeFavoriteStatusAudio(audio.id,true);
    this.toastr.success(`Audio was successfully added to Favorites`);
    this.openMenuId = null;
  }

  async addToPlaylist(audio: AudioType){
    const playlist = prompt('Enter the name of the playlist:');

    if (playlist != null && getPlaylist(playlist) != null) {
      await addAudioToPlaylist(playlist,audio.id);
      this.toastr.success(`Audio was successfully added to playlist : "${playlist}"`);
    } else {
      this.toastr.error(`Audio could not be added to a playlist`);
    }
    this.openMenuId = null;
  }
}