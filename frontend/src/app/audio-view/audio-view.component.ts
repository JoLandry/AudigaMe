import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AudioType } from '../audio-type';
import { getAudio, downloadAudio, deleteAudio, isAudioInFavorites, getMainPlaylist, audioList, getPlaylist, getFavoritesList } from '../http-api';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-audio-view',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './audio-view.component.html',
  styleUrl: './audio-view.component.css'
})
export class AudioViewComponent implements OnInit {
  audio!: AudioType | null;
  playlist: AudioType[] = [];

  constructor(private route: ActivatedRoute, private router: Router) {}

  ngOnInit() {
    this.route.paramMap.subscribe(async params => {
      const id = Number(params.get('id'));

      // Reload playlist each time just in case
      await getMainPlaylist();
      this.playlist = audioList;
      // Load new audio
      this.audio = await getAudio(id);
    });
  }

  async delete(audio: AudioType) {
    if (confirm(`Are you sure you want to delete "${audio.title}"?`)) {
      try {
        await deleteAudio(audio.id);
      } catch(e) {
        console.log("Audio could not be deleted from the server:" ,e);
      }
    }
  }
  
  async download(audio: AudioType) {
    try {
      await downloadAudio(audio);
    } catch (error) {
      console.error('Download failed', error);
    }
  }

  isFavorite(audio: AudioType) {
    return isAudioInFavorites(audio);
  }

  sizeToNumber(audioSize: string) {
    return parseInt(audioSize);
  }

  switchToNext(currentAudio: AudioType) {
    const index = this.playlist.findIndex(a => a.id === currentAudio.id);
    if (this.playlist.length === 0){
      return;
    }
    // Make the change even if current audio is the last -> circular so swap to first
    const nextIndex = (index + 1) % this.playlist.length;
    const nextAudio = this.playlist[nextIndex];
    this.router.navigate(['/audios', nextAudio.id]);
  }

  switchToPrevious(currentAudio: AudioType) {
    const index = this.playlist.findIndex(a => a.id === currentAudio.id);
    if (this.playlist.length === 0){
      return;
    }
    // Make the change even if current audio is the first -> circular so swap to last
    const previousIndex = (index - 1 + this.playlist.length) % this.playlist.length;
    const previousAudio = this.playlist[previousIndex];
    this.router.navigate(['/audios', previousAudio.id]);
  }
}
