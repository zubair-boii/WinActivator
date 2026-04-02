# WinActivator

A lightweight Windows activation utility built with WPF (.NET Framework 4.7.2), focused on simplicity, speed, and a clean user experience.

---

## Notes
- I know the code is not the best. This is my first attempt at a working desktop app.
- Any contributions or suggestions are welcome as this project evolve

---

- # 🚀 Features
* Clean and modern WPF UI
* Embedded script execution (no external files required)
* Automatic extraction and execution of activation scripts
* Runs with elevated privileges when required
* Minimal dependencies (no heavy external libraries)
* Portable and easy to use

---

 ![UI](/images/1.png)

---

## 🛠️ How It Works

WinActivator embeds activation scripts directly into the application as resources.
At runtime, the app:

1. Extracts the script to a secure local directory
2. Executes it using `cmd.exe`
3. Handles elevation (UAC) if required
4. Cleans up any temporary files after execution

This approach avoids bundling loose `.cmd` files and keeps everything self-contained.

---

## 📦 Requirements


* Windows 7 or higher
* .NET Framework 4.7.2 or higher

---

## ▶️ Usage

1. Launch the application
2. Select your desired activation method
3. Click the activate button
4. Accept the UAC prompt (if shown)
5. Wait for the process to complete

---

## ⚠️ Notes

* Antivirus or Windows Defender may flag script-based activation behavior
* Ensure real-time protection is configured appropriately if execution fails
* Some activation methods may require an internet connection

---

## 🧱 Project Structure

```
WinActivator/
│
├── Scripts/              # Embedded activation scripts
├── Components/           # Reusable UI components
├── MainWindow.xaml       # Main UI
├── MainWindow.xaml.cs    # UI logic
└── App.xaml              # Application entry point
```

---
## 🔧 Development

To build the project:

1. Open the solution in Visual Studio
2. Restore any required dependencies
3. Build in Release or Debug mode

---

## 🧹 Cleanup Behavior

The application attempts to remove any extracted script files after execution.
In cases where elevated processes are used, cleanup may be delayed or handled by the script itself.

---

## 📄 License

This project is provided for educational and experimental purposes.
Use responsibly.

---

## 👤 Author

Developed by Zubair

---

## ⭐ Contribution

Feel free to fork the repository and improve the project.
Pull requests are welcome.
