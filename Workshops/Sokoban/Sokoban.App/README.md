# Sokoban.App

This is the React and Vite frontend app for the Sokoban workshop.

The main workshop README is here:

```text
../README.md
```

## Requirements

- Node.js
- npm
- Docker Desktop, only if running the Docker version

## Install

Run this once after cloning the project or after deleting `node_modules`:

```powershell
npm install
```

## Run

Start the Vite development server:

```powershell
npm run dev
```

Open the local URL printed by Vite, usually:

```text
http://localhost:5173
```

## Build

Create production files in `dist`:

```powershell
npm run build
```

## Preview Build

Serve the built `dist` folder locally with Vite:

```powershell
npm run preview
```

## Docker

Build and run the production container:

```powershell
docker build -t sokoban-app .
docker run -d --name sokoban-app -p 8080:80 sokoban-app
```

Open:

```text
http://localhost:8080
```

Rebuild after code changes:

```powershell
docker stop sokoban-app
docker rm sokoban-app
docker build -t sokoban-app .
docker run -d --name sokoban-app -p 8080:80 sokoban-app
```

Stop and remove the container:

```powershell
docker stop sokoban-app
docker rm sokoban-app
```
