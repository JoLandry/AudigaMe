import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AudioType } from '../audio-type';
import { getAudio, downloadAudio, deleteAudio, isAudioInFavorites } from '../http-api';
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

  constructor(private route: ActivatedRoute) {
    
  }

  async ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.audio = await getAudio(id);
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
}
