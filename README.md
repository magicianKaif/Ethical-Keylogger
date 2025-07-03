# Ethical Keylogger

> **WARNING:**  
> This software is provided strictly for educational, ethical, and authorized security testing purposes.  
> **Do not use or run this program without explicit, informed consent from all parties involved. Unauthorized keylogging is illegal and unethical.**

## Description

Ethical Keylogger is a Windows-based keylogging tool designed for demonstrating the dangers of malicious software and to aid in authorized penetration testing and monitoring with full consent.  
It records keystrokes on the target machine and periodically sends the log via email.

## Features

- Hooks into Windows keyboard events to log keystrokes
- Stores logs locally in a text file
- Periodically sends keystroke logs to a specified email address (via SMTP)
- Runs invisibly in the background (hides console window)
- Optionally persists across reboots via Windows Registry startup entry
- Uses environment variables for email credentials (for safety)

## Usage

### 1. Build

Requires .NET Framework (4.x) or .NET Core/6+ with Windows Forms compatibility.

```bash
dotnet build
```

### 2. Configure Email Settings

Set the following environment variables before running the executable:

- `EKL_EMAIL_TO` – recipient email address
- `EKL_EMAIL_FROM` – sender Gmail address
- `EKL_SMTP_PASS` – Gmail app password (not your main password)

Example (Windows Command Line):

```cmd
set EKL_EMAIL_TO=recipient@example.com
set EKL_EMAIL_FROM=youraddress@gmail.com
set EKL_SMTP_PASS=your-gmail-app-password
EthicalKeyLogger.exe
```

> **Gmail Notice:**  
> You must use an “App Password” for Gmail (not your main password).  
> See: https://support.google.com/accounts/answer/185833

### 3. Run

Execute the compiled program. The keylogger will:

- Start logging keystrokes to `keystrokes.txt`
- Send the log file via email every 10 minutes (if any new activity)
- Attempt to add itself to Windows startup for persistence

### 4. Stop

To terminate, end the process from Task Manager or command line.

## Important Code Highlights

- **Environment Variables**: Credentials are read from environment variables, not hardcoded.
- **Buffered Logging**: Keystrokes are buffered and written to disk in batches to minimize performance impact.
- **No User Interface**: Runs silently in the background using `ApplicationContext`.

## Ethical Use

- **You are solely responsible** for ensuring you have proper authorization before using this tool.
- For research, demonstration, or with explicit permission only.

## Legal Disclaimer

This project is for ethical security research and education.  
The author assumes no liability and is not responsible for any misuse or damage caused by this software.  
Always comply with applicable laws and obtain all necessary permissions before deployment.

## License

MIT License. See [LICENSE](LICENSE) for details.

---

**Author:** magicianKaif  
**Telegram:** [magician_slime](https://t.me/magician_slime)
