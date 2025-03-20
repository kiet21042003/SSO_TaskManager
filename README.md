# Task Manager with SSO Integration

A modern task management web application built with ASP.NET Core, featuring Single Sign-On (SSO) authentication through Auth0.

## Features

- **Single Sign-On (SSO)**: Secure authentication using Auth0
- **Task Management**:
  - Create, update, and delete tasks
  - Drag-and-drop task status updates
  - Task categorization by status (Todo, In Progress, Completed, Cancelled)
- **User Profile**:
  - Customizable user profile with avatar
  - Profile information management
- **Modern UI/UX**:
  - Responsive design
  - Interactive dashboard
  - Real-time task status updates
  - Toast notifications

## Technologies

- **Backend**:
  - ASP.NET Core 6.0
  - Entity Framework Core
  - SQL Server
  - Auth0 Authentication

- **Frontend**:
  - Bootstrap 5
  - jQuery
  - Font Awesome
  - Toastify.js
  - Chart.js

## Prerequisites

- .NET 6.0 SDK
- SQL Server
- Auth0 Account
- Visual Studio 2022 or VS Code

## Configuration

1. Clone the repository
2. Update `appsettings.json` with your Auth0 credentials:
```json
{
  "Auth0": {
    "Domain": "your-auth0-domain",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret"
  }
}
```
3. Update the database connection string in `appsettings.json`
4. Run database migrations:
```bash
dotnet ef database update
```

## Running the Application

1. Navigate to the project directory:
```bash
cd SSO_Demo/TaskManager/src
```

2. Run the application:
```bash
dotnet run
```

3. Access the application at `https://localhost:5001`

## Project Structure

- `Controllers/`: Application controllers
- `Models/`: Data models and view models
- `Views/`: Razor views and partial views
- `Data/`: Database context and migrations
- `wwwroot/`: Static files (CSS, JavaScript, images)
- `ViewComponents/`: Reusable UI components

## Features in Detail

### Task Management
- Create new tasks with title, description, and status
- Update task status via drag-and-drop interface
- Delete tasks
- View task statistics in dashboard

### User Profile
- Update profile information
- Upload profile picture
- View task statistics and progress

### Authentication
- SSO login through Auth0
- Secure API endpoints
- Role-based access control

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Auth0 for authentication services
- Bootstrap team for the UI framework
- All other open-source contributors 