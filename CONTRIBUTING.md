# Contributing to dotnet-feature-flags

Thank you for your interest in contributing to dotnet-feature-flags! We welcome contributions from the community and appreciate your effort to make this project better.

## How to Contribute

### 1. Fork and Clone

- Fork the repository on GitHub
- Clone your fork locally:
  ```bash
  git clone https://github.com/YOUR_USERNAME/dotnet-feature-flags.git
  cd dotnet-feature-flags
  ```

### 2. Create a Feature Branch

```bash
git checkout -b feature/your-feature-name
```

### 3. Development Setup

**Requirements:**
- .NET 10.0 SDK

**Setup steps:**
```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test
```

### 4. Code Style and Conventions

- Follow existing conventions in the codebase
- XML documentation comments are required for public members
- **KEEP ALL author headers - DO NOT remove them**

### 5. Testing

- Add unit tests for any new functionality
- Ensure all existing tests pass:
  ```bash
  dotnet test
  ```

### 6. Submit a Pull Request

- Go to the original repository on GitHub
- Create a Pull Request from your feature branch
- Include a clear description of the changes
- Reference any related issues
- Ensure CI checks pass

## Reporting Issues

Found a bug or have a feature request? Please open an issue on GitHub:
- Use clear, descriptive titles
- Provide steps to reproduce (for bugs)
- Include relevant system information

## License

By contributing to this project, you agree that your contributions will be licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
