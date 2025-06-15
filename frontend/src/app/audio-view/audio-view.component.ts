import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AudioType } from '../audio-type';
import { getAudio, deleteAudio, getMainPlaylist, audioList } from '../http-api';
import { isAudioInFavorites, downloadAudio, currentPlaylistContext } from '../utils'
import { CommonModule } from '@angular/common';
import { ToastrService } from 'ngx-toastr';

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

  constructor(private route: ActivatedRoute, private router: Router, private toastr: ToastrService) {}

  ngOnInit() {
    this.route.paramMap.subscribe(async params => {
      const id = Number(params.get('id'));

      // Use the playlist used in the current context 
      this.playlist = [...currentPlaylistContext];
      this.audio = await getAudio(id);
    });
  }

  async delete(audio: AudioType) {
    if (confirm(`Are you sure you want to delete "${audio.title}"?`)) {
      try {
        await deleteAudio(audio.id);
        this.toastr.success(`Deleted audio successfully from the server`);
        // Navigate back to home after deletion
        this.router.navigate(['/home']);
      } catch(e) {
        this.toastr.error(`Failed to delete audio form the server`);
      }
    }
  }
  
  async download(audio: AudioType) {
    try {
      await downloadAudio(audio);
      this.toastr.success(`Downloaded audio successfully`);
    } catch (error) {
      this.toastr.error(`Failed to download audio`);
    }
  }

  isFavorite(audio: AudioType) {
    return isAudioInFavorites(audio);
  }

  sizeToNumber(audioSize: string) {
    return parseInt(audioSize);
  }

  /* Method used to switch to the next audio in the playlist */
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

  /* Method used to switch to the previous audio in the playlist */
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
