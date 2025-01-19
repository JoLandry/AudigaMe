import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { OnInit } from '@angular/core';
import { AudioType } from '../audio-type';
import { getMainPlaylist, audioList } from '../http-api';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterOutlet, CommonModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})

export class HomeComponent implements OnInit {
  audios: AudioType[] = [];

  ngOnInit(){
    this.loadAudios();
  }

  async loadAudios(){
    await getMainPlaylist();
    this.audios = audioList;
  }
}