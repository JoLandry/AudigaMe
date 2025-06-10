# 🎵 AudigaMe

**AudigaMe** is a full-stack web application for managing and interacting with audio files and playlists.

Built with:
- **.NET 8 (C#)** for the backend
- **Angular 18** for the frontend
- **PostgreSQL** as the database
  (Any PostgreSQL-compatible DBMS will do, including remote or SSH-tunneled instances)


---

## Features

AudigaMe offers a simple and efficient interface to manage audio files and playlists, including:

**Audio Upload**  
Upload and store audio files securely in the connected database and play them

**Playlist Creation & Management**  
Create custom playlists and manage audio tracks

**Search Functionality**  
Easily search through available audio files by title or artist

**Audio Download**  
Download any uploaded audio file for offline use

**Delete Audio Files or Playlists**  
Clean up your library by removing unneeded audio files or entire playlists

**REST API**  
A clean and extendable HTTP API allowing interaction with all backend features and metadata vizualisation


---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- [Node.js & npm](https://nodejs.org/) (LTS recommended)
- [Angular CLI v18](https://angular.dev/cli)
- Access to a **PostgreSQL** database — you can use:
  - A **local** installation
  - A **Docker** container
  - A **remote** database (e.g., university-hosted, cloud provider)

--- 

## Setup Guide – Installing Tools

To run **AudigaMe**, you need to install and configure a few tools.
This section walks you through everything needed to set up your environment from scratch.

### 1. Install .NET 8 SDK (for the backend)
- Go to the [.NET Download Page](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Select your OS (Linux, Windows, macOS) and follow the installer instructions.
- After installation, verify it works:

dotnet --version

It should normally print something like: 8.0.x

### 2. Install Node and NPM (for the frontend)
- Download the LTS version of Node.js from the official website (https://nodejs.org/en/download)
- This will install both node and npm.
- Check the installation:

npm -v 

node -v

### 3. Install Angular CLI version 18 (for the frontend)
Once Node.js and npm are correctly installed, install Angular by running :

npm install -g @angular/cli@18

After that, check the installation:

ng version

It should display something such as: Angular CLI: 18.x.x

### 4. Setup PostgreSQL (backend)
You must also possess the required tools concerning the database integration (PostgreSQL). Run:

sudo apt update 

sudo apt install postgresql postgresql-contrib

### 5. Quick note - Docker
You can also configure your own Docker container etc. to run the app, particularly for the database. 

--- 

## Environment Variables

For security reasons, this project does not contain a .env file where sould be stored the environment variables for database connection.
Therefore, you have to locally create a `.env` file in `backend/` directory. You will find an .env.example file to show you
how to fill the necessary information to grant access to your database so it can be integrated to the app.

One very important note :
You will find the required SQL code to execute inside your database before running anything (creating tables, creating index, etc.) in the following
path : `backend/src/main/sql-related.sql`

Other quick note:
If you're using an SSH tunnel, make sure it’s active before launching the backend. Run :

ssh -L 5432:your-db-host:5432 your-user@your-ssh-host

---

## Running the App

### 1. Start the backend
Open a Terminal from the root directory of the project, and run :

cd backend 

dotnet run

This will launch the server. By default, the software uses localhost with port 5174. Any server can be used. However, configuration will
have to be modified to launch the app on the wanted server.

Optionally can also run the tests (Mock tests using xUnit and Moq) by running :

dotnet test

Or:

dotnet test --logger "console;verbosity=detailed"

to debug the tests

For more details about dotnet, please check out:

https://dotnet.microsoft.com/en-us/


### 2. Start the frontend
Open a Terminal from the root directory of the project, and run:

cd frontend 

ng build --configuration production

This will build the client source code so that the user can communicate with the server and use the app through HTTP requests.


If you want to develop the frontend independently, you can also run:

ng serve

For more details about frontend configuration or Angular please check out:

https://angular.dev/


### 3. Open a browser to use the app

Once both backend and frontend are ready, navigate to the following URL to use the app:

http://localhost:5174

(Or connect to your server if you have a different configuration)

---

## Testing

You will find a test suite for the backend only for this project. The frontend does not include tests because the main goal of this project was to have a better grasp of web development, especially backend side. I learned how to use and setup modern technologies such as Angular, .NET with an introduction to the C# language, and the integration of a database.

---

## Ideas of improvement and extra features

This first version of the software gathers some basic features of an audio application. It can easily be improved, whether we are talking about speed,
rendering, available features, etc. This first version is finished and I am quite satisfied with the final result even though it definitely has room for improvement. Writing this as I am making last adjustments, I am not determined enough to significantly upgrade the app so it is close to some professional software. However, maybe one day I will lean into it again and start making new features, better the ones that are already implemented.

So here are some thoughts, or some ideas of potential improvement:
- Sort the audios in the order that we want (customzied, alphabetically, etc.)
- Make albums (regroup audios by bands, artists and whatnot)
- Possibility of attaching pictures (images) to tracks
- Play any audio or group of audios in loop
- Play any audio or group of audios with indicated speed
- Simulate Shazam's algorithm to find the name of a song
- ...

---

## Compatibility

The application has been so far tested on the following platforms:

**Operating Systems**  
- Ubuntu 22.04 LTS

**Web Browsers**
- Firefox (latest)

---

## Author 
LANDRY Jonathan

Computer Science student at University of Bordeaux, France.
Major: Software Engineering

---

## License 
This project was created as a personal initiative, outside any official academic coursework, during my Master's in Computer Science at the University of Bordeaux.

It is released under the **MIT License**.

You are free to:

- use, copy, modify, and distribute this code,  
- as long as you retain the copyright notice.

This project is provided **"as is"**, without any warranty of any kind.

© 2025 Landry Jonathan