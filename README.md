# AspNet NG + API with Cookie Auth

-   AspNet project created with `dotnet new angular -o <proj_name>`
-   Added _custom `AddCookie` authentication_ (use API to auth/login + handle redirect 401 + allow CORS + SameSite=None with FIX to host Angular Pages in separate domain from API)
-   Upgrade solution to latest Angular v10
-   Added Docker container to isolate Node/Angular env with VSCode Remote .devcontainer
