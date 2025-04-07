# Pharmacy Queue Management System

A modern web application built with ASP.NET Core for managing pharmacy customer queues, appointments, and wait time estimates.

## Live Demo

Check out the live demo at [https://pharmacyqueue.onrender.com](https://pharmacyqueue.onrender.com)

## Features

- **Digital Queue Management**: Replace paper tickets with a digital queue system
- **Real-time Wait Time Estimations**: Provides customers with accurate wait time estimates
- **Appointment Scheduling**: Allows customers to book pharmacy appointments
- **Email Notifications**: Automated reminders and status updates
- **Admin Dashboard**: Manage queue settings, appointments, and customer data
- **Mobile-Friendly Interface**: Access from any device with responsive design

## Technology Stack

- **Backend**: ASP.NET Core (.NET 9.0)
- **Database**: MySQL with Entity Framework Core
- **Frontend**: HTML, CSS, JavaScript with ASP.NET MVC Views
- **Containerization**: Docker support for easy deployment
- **Email Service**: MailKit for sending notifications
- **UI Components**: Bootstrap 5 and Font Awesome 6.0.0

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- MySQL Server
- Docker (optional, for containerized deployment)

### Environment Setup

1. Clone the repository:
   ```
   git clone https://github.com/rajveermakkar/pharmacy-queue-management-system.git
   cd pharmacy-queue-management-system
   ```

2. Database Setup:
   - Import the database schema and initial data:
   ```
   mysql -u root -p < pharmacyqueue_backup.sql
   ```
   - Or manually create tables using Entity Framework:
   ```
   dotnet ef database update
   ```

3. Configure Environment Variables:

   #### Option 1: Using .env File (Recommended for Development)
   - Copy the example environment file:
   ```
   cp .env.example .env
   ```
   - Edit the `.env` file with your actual values:
   ```
   # Database Connection
   DB_HOST=localhost
   DB_PORT=3306
   DB_NAME=pharmacyqueue
   DB_USER=root
   DB_PASSWORD=yourpassword

   # Email Settings
   EMAIL_HOST=smtp.example.com
   EMAIL_PORT=587
   EMAIL_USERNAME=your-email@example.com
   EMAIL_PASSWORD=yourpassword
   ```

   #### Option 2: Using appsettings.json (Alternative Method)
   - Edit `PharmacyQueue/appsettings.json` to include your database connection and email settings:
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "AllowedHosts": "*",
     "ConnectionStrings": {
       "DefaultConnection": "server=localhost;port=3306;database=pharmacyqueue;user=root;password=yourpassword"
     },
     "EmailSettings": {
       "Host": "smtp.example.com",
       "Port": 587,
       "Username": "your-email@example.com",
       "Password": "yourpassword",
       "EnableSsl": true,
       "From": "pharmacy@example.com"
     },
     "AdminSettings": {
       "Email": "admin@pharmacy.com",
       "Password": "Admin@123"
     }
   }
   ```

### Running the Application

#### Using .NET CLI:
```
dotnet run --project PharmacyQueue
```

#### Using Docker:
```
docker-compose up
```

The application will be available at `http://localhost:8080`.

### Admin Access

By default, the system is configured with the following admin credentials:

- **Username**: admin@pharmacy.com
- **Password**: Admin@123

## Database Schema

The system uses the following primary data models:
- **Appointments**: Customer appointment data
- **QueueSettings**: Configuration for queue operations
- **Notifications**: Email and alert records
- **Admins**: Administrative user accounts

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Built as a final project for .NET Web Development Course
- Uses Font Awesome 6.0.0 from CDN (https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css)

## Pushing to GitHub

```bash
echo "# pharmacy-queue-management-system" >> README.md
git init
git add README.md
git commit -m "first commit"
git branch -M main
git remote add origin https://github.com/rajveermakkar/pharmacy-queue-management-system.git
git push -u origin main
```