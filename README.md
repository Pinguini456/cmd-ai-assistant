# Cmd AI Assistant

[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![OpenAI](https://img.shields.io/badge/OpenAI-2.9.1-green.svg)](https://www.nuget.org/packages/OpenAI/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

A free, lightweight, extensible command-line AI assistant powered by OpenAI GPT-4o. Execute file operations, run shell commands, and automate tasks through natural language conversation.

## Features

- **Natural Language Interface** - Interact with your system using conversational prompts
- **File Operations** - Read and write files through AI-driven commands
- **Shell Command Execution** - Execute cmd.exe commands with automatic output capture
- **Tool-Calling Architecture** - Extensible agent pattern for adding custom capabilities
- **Secure Authentication** - Uses environment variables for API key management

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- GitHub account with access to GitHub Models
- Windows operating system (for shell command execution)

## Installation

### Clone the Repository

```bash
git clone https://github.com/Pinguini456/cmd-ai-assistant.git
cd cmd-ai-assistant
```

### Build the Project

```bash
dotnet build
dotnet publish -c Release -o ./publish
```

## Configuration

The application requires a GitHub Token to access the Azure OpenAI models API.

### Setting Up Your GitHub Token

1. Go to [GitHub Settings > Tokens](https://github.com/settings/tokens)
2. Generate a new personal access token (classic) with the *read:packages* permission
3. Set the token as an environment variable:

```powershell
# PowerShell
$env:GITHUB_TOKEN = "your_github_token_here"

# Command Prompt
set GITHUB_TOKEN=your_github_token_here

# Permanently (System Environment Variable)
setx GITHUB_TOKEN "your_github_token_here"
```

## Usage

### Basic Syntax

```bash
cmdaiassistant -p "<your prompt>"
```

### Example Commands

**Read a file:**
```bash
cmdaiassistant -p "Read the contents of C:\\path\\to\\file.txt"
```

**Create a new file:**
```bash
cmdaiassistant -p "Create a new file at C:\\temp\\hello.txt with content 'Hello, World!'"
```

**Execute a shell command:**
```bash
cmdaiassistant -p "List all files in the current directory"
```

**Combined workflow:**
```bash
cmdaiassistant -p "Read my config.json file, then create a backup at config.json.bak"
```

## Available Tools

The AI assistant has access to the following tools:

| Tool | Description | Parameters |
|------|-------------|------------|
| `Read` | Read file contents from disk | `file_path` - Absolute or relative path |
| `Write` | Write content to a file | `file_path`, `content` |
| `Bash` | Execute Windows command prompt commands | `command` - cmd.exe compatible command |

## Architecture

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   User Input    │───▶│   GPT-4o Model  │────▶│  Tool Execution │
│   (Prompt)      │     │   (Planning)    │     │  (Read/Write/   │
│                 │     │                 │     │   Bash)         │
└─────────────────┘     └─────────────────┘     └─────────────────┘
                               ▲                         │
                               │                         │
                               │          ┌──────────────┘
                               │          │
                               │          ▼
                               │ ┌─────────────────┐
                               └─│  Tool Results   │
                                 │  (Feedback)     │
                                 └─────────────────┘
```

The application implements an iterative tool-calling loop:
1. User provides a natural language prompt
2. The model decides which tool(s) to invoke
3. Tools execute on the local system
4. Results are fed back to the model
5. Process continues until the model provides a final response

## Technical Details

- **Framework:** .NET 9.0
- **OpenAI SDK:** Version 2.9.1
- **Model:** GPT-4o via GitHub Models API
- **Endpoint:** `https://models.inference.ai.azure.com`

## Security Considerations

- **Never commit your GitHub token** to version control
- The application executes shell commands with the same privileges as the running user
- Use caution when allowing AI-generated commands to execute on your system
- Review file write operations before confirming sensitive changes

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [OpenAI SDK for .NET](https://github.com/openai/openai-dotnet)
- [GitHub Models](https://github.com/marketplace/models) for providing the GPT-4o endpoint

## Support

If you encounter any issues or have questions, please [open an issue](https://github.com/Pinguini456/cmd-ai-assistant/issues) on GitHub.
