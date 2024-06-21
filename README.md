# TriggeredEmailer

This console app is designed to be executed by a cronjob. The main idea is that an email is sent out at the end of the day, notifying the provider about incomplete sessions. The email includes a link to the page where the provider can update the session status and/or add notes.

This repository can be extended for other purposes related to task scheduling for the Triumph Web App.

**Note:** This project requires .NET 8 or a higher version.

### Prerequisites

- .NET8 or higher
- SendGrid API key and email
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

4. **Populate environment variable values in appsettings.json**:
Refer to my local appsettings.json setup for guidance: [My appsettings.json setup locally.](https://github.com/YIC-Triumph/TriggeredEmailer/wiki/My-appsettings.json-setup-locally)

5. Clean and rebuild the project before executing it.

6. **To run this console app**, you need to provide an argument. Use either "mailsession" or "billing_BT" or "billing_BCBA" as the argument.

7. **Run the console app using Visual Studio**:
Follow these steps as shown in the screenshots:
![image](https://github.com/YIC-Triumph/TriggeredEmailer/assets/21212665/f9ef9ae3-c7d5-4c27-b48d-d2735e160217)
![image](https://github.com/YIC-Triumph/TriggeredEmailer/assets/21212665/91b1c544-24a4-4826-b587-7b464a415004)
![image](https://github.com/YIC-Triumph/TriggeredEmailer/assets/21212665/e93c6086-248c-4c04-9b59-50b81f479f80)

