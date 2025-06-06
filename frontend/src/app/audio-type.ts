export interface AudioType {
    id: number;
    title: string;
    artist: string;
    type: string;
    favorite: boolean;
    size: string;
}

export interface PlaylistType {
  playlistName: string;
  audios: AudioType[];
}
