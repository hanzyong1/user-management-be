# User Management API

## Overview
This is a .NET Core Web API for user authentication and profile management  
It supports secure login using JWT, profile updates, and profile image uploads to Azure Blob Storage

## Tech Stack
- ASP.NET Core Web API
- MS SQL
- Azure Blob Storage
- Scalar Api documentation

## Features

### Authentication
- User login with email and password
- JWT-based authentication in HTTP-only cookies
- Refresh token support
- User registration

### Profile Management
- Get user profile details
- Update profile details
- Upload profile picture to Azure Blob Storage

## Setup Instructions

1. Update configurations in:
   - `appsettings.json`

2. Setup database (choose one):

   **Option A: Run migrations**
   - In Package Manager Console:
     ```
     update-database
     ```

   **Option B: Import existing database**
   - Import the provided `.bacpac` file using SQL Server Management Studio (SSMS)
   - Ensure the connection string in `appsettings.json` matches the restored database

3. Run the application in `http`

## Database Seeding
- The database is automatically seeded on application startup
- To disable seeding, comment out the seed logic in `Program.cs`

## Assumptions
- It is assumed that users will input valid and correctly formatted data (e.g. valid email format)
- Azure services are used for storage, and valid credentials are required to run the application

## Notes
- This application uses Azure Blob Storage for image handling
- The application requires an authenticated Azure CLI session (`az login`) with access to the specified Blob Storage account
- The Azure account used must match the configured **Service URL** and **Container Name**
- Users may configure and use their own Azure credentials by updating the Blob Storage settings in `appsettings.json`
- Ensure that the configured account has the necessary permissions to upload and manage blobs