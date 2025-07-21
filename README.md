# Meal Planning Application v2.0

A comprehensive ASP.NET Core 9.0 meal planning application with AI-powered meal generation, subscription management, and user-friendly interface.

## Features

- **AI-Powered Meal Planning**: Generate personalized meal plans using OpenAI and OpenRouter APIs
- **User Authentication**: Complete identity management with ASP.NET Core Identity
- **Subscription System**: Stripe integration for payment processing
- **Responsive UI**: Modern Tailwind CSS styling with dark/light mode support
- **Recipe Management**: Save, view, and organize meal plans with detailed recipes
- **Dietary Preferences**: Support for various dietary restrictions and preferences
- **Multi-language Support**: Internationalization support for multiple languages
- **Security Features**: Rate limiting, XSS protection, and security headers

## Technologies Used

- **Backend**: ASP.NET Core 9.0, Entity Framework Core
- **Frontend**: Razor Pages, Tailwind CSS, JavaScript
- **Database**: SQL Server with Entity Framework migrations
- **Authentication**: ASP.NET Core Identity
- **Payments**: Stripe API integration
- **AI Services**: OpenAI API, OpenRouter API
- **Mapping**: Geoapify API for location services

## Setup Instructions

### Prerequisites

- .NET 9.0 SDK
- SQL Server (LocalDB or full SQL Server)
- Node.js (for Tailwind CSS compilation)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/adityadev54/Meal-Planning-v.2.git
   cd Meal-Planning-v.2
   ```

2. **Install dependencies**
   ```bash
   dotnet restore
   npm install
   ```

3. **Configure appsettings.json**
   - Copy `appsettings.json` and update the following:
     - Connection string for your SQL Server instance
     - OpenAI API key (`OpenAISettings:ApiKey`)
     - OpenRouter API key (`OpenRouter:ApiKey`)
     - Geoapify API key (`Geoapify:ApiKey`)
     - Stripe keys for payment processing (`Stripe` section)

4. **Database Setup**
   ```bash
   dotnet ef database update
   ```

5. **Build Tailwind CSS**
   ```bash
   npm run build-css
   ```

6. **Run the application**
   ```bash
   dotnet run
   ```

The application will be available at `https://localhost:5001` or `http://localhost:5000`.

## Configuration

### Required API Keys

- **OpenAI API Key**: For AI meal plan generation
- **OpenRouter API Key**: Alternative AI service
- **Geoapify API Key**: For location-based services
- **Stripe Keys**: For payment processing (test and live keys)

### Database Configuration

Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "GMDbContextConnection": "Data Source=YourServer;Initial Catalog=GetMovingMealsDb;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False"
  }
}
```

## Features Overview

### Meal Planning
- AI-generated personalized meal plans
- Dietary restriction support
- Recipe instructions and ingredient lists
- Grocery list generation

### User Management
- Registration and authentication
- User profiles and preferences
- Subscription management
- Activity tracking

### Payment System
- Stripe integration
- Multiple subscription tiers
- Trial periods
- Payment history

### Admin Features
- User management
- Subscription monitoring
- System analytics
- Content management

## Development

### Running in Development Mode

```bash
dotnet run --environment Development
```

### Database Migrations

To create a new migration:
```bash
dotnet ef migrations add MigrationName
```

To update the database:
```bash
dotnet ef database update
```

### Building for Production

```bash
dotnet publish -c Release -o ./publish
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## Security

- All sensitive data should be stored in environment variables or secure configuration
- API keys are not included in the repository
- User input is validated and sanitized
- HTTPS is enforced in production

## License

This project is proprietary software. All rights reserved.

## Support

For support and questions, please contact the development team.
