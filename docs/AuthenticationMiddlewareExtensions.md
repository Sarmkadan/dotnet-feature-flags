# AuthenticationMiddlewareExtensions

Provides extension methods for configuring authentication middleware in an ASP.NET Core pipeline. These methods simplify the registration of authentication services and middleware components required for feature flag evaluation and user context resolution.

## API

### `UseAuthenticationMiddleware(IApplicationBuilder)`

Configures the application to use authentication middleware. This method registers the necessary authentication services and adds the authentication middleware to the pipeline.

**Parameters**
- `app`: The `IApplicationBuilder` instance to configure.

**Return Value**
- The configured `IApplicationBuilder`.

**Exceptions**
- Throws `ArgumentNullException` if `app` is `null`.

---

### `UseAuthenticationMiddleware(IApplicationBuilder, Action<AuthenticationOptions>)`

Configures the application to use authentication middleware with custom authentication options. This method allows fine-tuning of authentication behavior, such as scheme selection or token validation parameters.

**Parameters**
- `app`: The `IApplicationBuilder` instance to configure.
- `configureOptions`: An `Action` delegate to configure `AuthenticationOptions`.

**Return Value**
- The configured `IApplicationBuilder`.

**Exceptions**
- Throws `ArgumentNullException` if `app` or `configureOptions` is `null`.

---

### `UseAuthenticationMiddleware(IApplicationBuilder, string)`

Configures the application to use authentication middleware with a specific authentication scheme. This method selects the named authentication scheme for processing requests.

**Parameters**
- `app`: The `IApplicationBuilder` instance to configure.
- `scheme`: The name of the authentication scheme to use.

**Return Value**
- The configured `IApplicationBuilder`.

**Exceptions**
- Throws `ArgumentNullException` if `app` or `scheme` is `null`.

---

### `UseAuthenticationMiddleware(IApplicationBuilder, Action<AuthenticationOptions>, string)`

Configures the application to use authentication middleware with custom authentication options and a specific authentication scheme. This method combines scheme selection with option customization.

**Parameters**
- `app`: The `IApplicationBuilder` instance to configure.
- `configureOptions`: An `Action` delegate to configure `AuthenticationOptions`.
- `scheme`: The name of the authentication scheme to use.

**Return Value**
- The configured `IApplicationBuilder`.

**Exceptions**
- Throws `ArgumentNullException` if `app`, `configureOptions`, or `scheme` is `null`.

## Usage

### Basic Usage
