# WebServer (WIP – Project Paused)

This project is an attempt to build a simple HTTP web server from scratch in C#, inspired by the guide "[Writing a Web Server from Scratch](https://www.codeproject.com/Articles/859108/Writing-a-Web-Server-from-Scratch)" on CodeProject.

The initial goal was to understand the internals of a web server, focusing only on `TcpListener`, raw HTTP request handling, manual routing, basic error handling, and serving static content — with no frameworks or external libraries involved.

## Implemented Features

✅ HTTP server based on `HttpListener`  
✅ Request parsing (`GET`, path, query string)  
✅ Verb + path routing (`GET /hello`, etc.)  
✅ Static content serving (`.html`, `.css`, etc.)  
✅ Custom error pages (`404`, `500`, unknown types)  
✅ Custom routing via `Route.Action(...)`  
✅ Internal redirect on error  
✅ Console logging for debug  
✅ Basic verb support (GET fully implemented, POST in progress)

## Project Status

After following the guide’s steps closely, I’ve decided to **pause development**.

### Why?

- The guide is unstructured, often unclear, and not very educational
- I realized I was **replicating code without deeply understanding it**
- I now prefer to **focus on grasping core concepts** instead of patching together a full stack
