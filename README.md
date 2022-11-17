# MultiTenant.Demo

Multi-Tenant Demo API, using AWS Cognito as identity provider, with a pre-token generation triggered Lambda.
This demo loosely follows the AWS SaaS Multi-Tenant workshop (See On-line Resources).



## On-line Resources

* [Google](https://www.google.com) - Use it, till you love it!
* [Saas Multi-Tenant Workshop](https://security.workshop.josharj.people.aws.dev/en/setup.html)


* FluentResults - https://www.nuget.org/
```powershell
Install-Package FluentResults -Version 3.15.0
```

## Gotcha's

* Environment Variables

When changes are made to environment variables, the IDE needs to be restarted. When Visual Studio opens, it caches the current environment variables.

* Cognito

Only the IDToken can be "changed" with Lambda Triggers. In-order to get the Tenant Code from the Token, you have to pass the ID Token, NOT the Access Token, to the API.

