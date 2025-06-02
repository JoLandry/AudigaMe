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
