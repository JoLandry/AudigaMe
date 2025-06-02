export interface AudioType {
    id: number;
    title: string;
    artist: string;
    type: string;
    favorite: boolean;
    size: string;
}

export class UpdateRequest {
    title: string | null;
    artist: string | null;
    favoriteStatus: boolean | null;

    constructor(title: string | null, artist: string | null, favoriteStatus: boolean | null) {
        this.title = title;
        this.artist = artist;
        this.favoriteStatus = favoriteStatus;
    }
}