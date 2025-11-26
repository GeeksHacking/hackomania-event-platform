# HackOMania Event Platform

????

## Getting started

[Aspire](https://aspire.dev) is used to bootstrap the project. These can be installed [here](https://aspire.dev).

[.NET](https://dot.net) is used for the API. It can be installed [here](https://dot.net/download).

Along with Aspire, you also need NodeJS and PNPM. These can be installed using [mise](https://mise.jdx.dev).

After installing the above, you can simply run:

```shell
export Parameters__github_client_id=""
export Parameters__github_client_secret=""
aspire run -d
```

And visit the dashboard link with everything setup!

## Development

For simplicity, a `justfile` is provided with common scripts.

`just dev` starts the processes.

`just codegen` generates new Kiota API clients based on the OpenAPI schema.