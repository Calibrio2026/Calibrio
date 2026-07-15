const port = Number.parseInt(process.argv[2] || "3001", 10);
process.env.PORT = String(port);
process.env.HOST = "0.0.0.0";
process.env.NODE_ENV = "production";
await import("./server.js");
