# Usage

* [Features](#features)
* [Prerequisites](#prerequisites)
* [Installation](#installation)
* [Getting Started](#getting-started)

# Features

- **DOTNET STANDARD 2.0**
  - Web API

# Prerequisites:
 * [.Net Core SDK 3.0](https://www.microsoft.com/net/download/windows)
 * [VSCode](https://code.visualstudio.com/) (ideally), or VS2019

# Installation:
 * Install-Package ttn.Web.JwtToken

# Getting Started

run api project with hot-reload.
 
 ```console
 cd sample
dotnet watch run
 ```

controller

 ```csharp
 [Authorize(AuthenticationSchemes = "your_scheme_name")]
 public class AccountController : Controller
{
  // any action of this Controller is identity required
}
 ```

action

```csharp
[Authorize(AuthenticationSchemes = Consts.AuthenticationScheme)]
[HttpPost]
public async Task<IActionResult> yourAction([FromForm] string refreshToken)
{
  // ...
  return Ok("I am sign in");
}
```
