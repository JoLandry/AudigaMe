import { Component } from '@angular/core';
import { RouterOutlet, RouterModule } from '@angular/router';
import { getMainPlaylist, uploadAudio } from './http-api';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, FormsModule, RouterModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})

export class AppComponent {
  title: string = '';
  artist: string = '';

  async handleFileSelect(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (file == null) {
      console.error('No file selected');
      return;
    }

    // Ask for title and artist
    const title = prompt('Enter title for the audio:', this.title);
    const artist = prompt('Enter artist name for the audio:', this.artist);

    if (title == null || artist == null) {
      console.error('Upload cancelled: missing title or artist');
      return;
    }

    try {
      console.log("Uploading file:", file);
      console.log("Title:", title);
      console.log("Artist:", artist);

      const uploadedAudio = await uploadAudio(file, title, artist);
      console.log('Uploaded successfully:', uploadedAudio);

      // Reset attributes when POST is successful
      this.title = '';
      this.artist = '';
    } catch (error) {
      console.error('Upload error:', error);
    }

    // Reset file input so same file can be re-selected if needed
    input.value = '';
    // Get playlist after uploading the audio (for refresh)
    getMainPlaylist();
  }
}