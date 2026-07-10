# ValidationException

Exception thrown when validation of feature flag configuration or webhook payload fails, carrying a dictionary of field-specific error messages.

## API

### `public Dictionary<string, string> Errors`

Gets the collection of validation errors where the key is the name of the invalid field or property and the value is the corresponding error message.

### `public ValidationException(string message) : base(message)`

Initializes a new instance of the `ValidationException` class with a specified error message.

- **Parameters**
  - `message` (string): The message that describes the error.

### `public ValidationException(string message, Dictionary<string, string> errors) : base(message)`

Initializes a new instance of the `ValidationException` class with a specified error message and a collection of validation errors.

- **Parameters**
  - `message` (string): The message that describes the error.
  - `errors` (Dictionary<string, string>): The dictionary of field-specific error messages.

### `public ValidationException(string message, Exception innerException) : base(message, innerException)`

Initializes a new instance of the `ValidationException` class with a specified error message and a reference to the inner exception that is the cause of this exception.

- **Parameters**
  - `message` (string): The message that describes the error.
  - `innerException` (Exception): The exception that is the cause of the current exception.

### `public WebhookValidationException(string message) : base(message)`

Initializes a new instance of the `WebhookValidationException` class with a specified error message. This exception is used specifically for webhook payload validation failures.

- **Parameters**
  - `message` (string): The message that describes the error.

### `public WebhookValidationException(string message, Dictionary<string, string> errors) : base(message)`

Initializes a new instance of the `WebhookValidationException` class with a specified error message and a collection of validation errors. This exception is used specifically for webhook payload validation failures.

- **Parameters**
  - `message` (string): The message that describes the error.
  - `errors` (Dictionary<string, string>): The dictionary of field-specific error messages.

## Usage
