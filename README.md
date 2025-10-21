# Web Scrambler


A full-stack web application for data encryption and decryption, developed as a bachelor's thesis project. This application provides a simple interface to encrypt/decrypt data using a scrambler algorithm, combining a .NET 8 Web API backend with a Vue.js frontend.

## Features

* **RESTful Backend:** A robust backend API built with ASP.NET Core 8 to handle all logic.
* **SPA Frontend:** A responsive and fast Single-Page Application (SPA) built with Vue.js.
* **Multiple Data Modes:** Supports encryption and decryption for:
    * Plain Text
    * Byte Arrays
    * Files (Upload/Download)
* **Unit Tested:** Core encryption logic is validated with MSTest.

## Tech Stack

### Backend
* **.NET 8**
* **ASP.NET Core Web API**
* **MSTest** (for Unit Testing)

### Frontend
* **Vue.js**
* **HTML5**
* **CSS3**

## Usage

Once both the backend and frontend are running:
1.  Open your browser to the frontend URL (e.g., `http://localhost:8080`).
2.  Select the data mode (Text, Byte, or File).
3.  Enter your text, byte string, or upload a file.
4.  Click "Scramble" to encrypt or "Descramble" to decrypt.
5.  The result will be displayed, or a file will be prompted for download.
