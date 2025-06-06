CREATE TABLE Audio (
    id SERIAL PRIMARY KEY,
    title VARCHAR(100),
    artist VARCHAR(100),
    audioFormat VARCHAR(10),
    audioSize INT,
    isFavorite BOOLEAN
);

DROP TABLE Audio;

CREATE INDEX idx_audio_isfavorite ON Audio (isFavorite);

CREATE TABLE Playlist (
  id SERIAL PRIMARY KEY,
  name TEXT UNIQUE NOT NULL
);

CREATE TABLE Playlist_Audio (
  playlist_id INTEGER REFERENCES Playlist(id) ON DELETE CASCADE,
  audio_id INTEGER REFERENCES Audio(id) ON DELETE CASCADE,
  position INTEGER,
  PRIMARY KEY (playlist_id, audio_id)
);

CREATE INDEX idx_playlist_audio_audio_id ON Playlist_Audio (audio_id);

CREATE INDEX idx_playlist_audio_playlist_position ON Playlist_Audio (playlist_id, position);
