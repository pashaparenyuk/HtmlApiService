# Project Overview

This repository contains two projects: an API built with ASP.NET Core and a client application using Vue.js.

## API (HtmlApiService)

The API utilizes the Hangfire service to manage task creation and scheduling. Hangfire facilitates the processing of files uploaded by users. 

### Getting Started
1. Run or publish the `HtmlApiService`.
2. Initialize and run the `html-pdf-app` (execute `npm init` and `npm run serve`).

**Note:** API endpoints are hardcoded in the Vue app (`html-pdf-app`) and are set to the default address `http://localhost:5007`.

## Vue Client Application (html-pdf-app)

The client application is a Vue project designed for simplicity. Users can upload files, and the dynamic status changes, download, and delete functionalities are available. The focus is on functionality, and no CSS styling has been applied.

### Getting Started
1. Initialize and run the Vue project using `npm init` and `npm run serve`.

**Note:** Ensure that the API is running at the specified address (`http://localhost:5007`).

Feel free to explore, contribute, and provide feedback!
