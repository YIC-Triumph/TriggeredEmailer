# TriggeredEmailer

This console app is designed to be executed by a cronjob. The main idea is that an email is sent out at the end of the day, notifying the provider about incomplete sessions. The email includes a link to the page where the provider can update the session status and/or add notes.

This repository can be extended for other purposes related to task scheduling for the Triumph Web App.

**Note:** This project requires .NET 8 or a higher version.

### Prerequisites

- .NET8 or higher
- SendGrid account
- SQL Server Connection string for Triumph Web App

### Installation

Follow these steps to set up and run the project on your machine:

1. **Clone this repository:**
    ```bash
    git clone https://github.com/YIC-Triumph/TriggeredEmailer.git
    cd TriggeredEmailer
    ```

2. **Ensure .NET 8 SDK is installed:**
   Make sure that you have the .NET 8 SDK installed on your machine. If not, download and install it from [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download).

3. **Open the solution file:**
   Open the `.sln` file using either Visual Studio or Visual Studio Code.

4. **Create `appsettings.json`:**
   Create an `appsettings.json` file in the `./TriggeredEmailer` project directory to set up environment variables. Use the following template:

    ```json
    {
        "ConnectionStrings": {
            "DBConnectionString": ""
        },
        "AppSettings": {
            "EmailFrom": "",
            "SendGridApiKey": "",
            "Domain": "http://localhost:50629/"
        }
    }
    ```

   Replace the placeholder values with your specific configurations.

5. Clean and rebuild the project before executing it.

6. **Execute the project:**
    Run the project using your preferred development environment or by executing the necessary commands.

