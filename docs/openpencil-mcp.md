# OpenPencil MCP

This repo retains an OpenPencil MCP configuration for historical design work. It is paused by default; enable it only for an explicit OpenPencil task:

```json
"openpencil": {
  "type": "remote",
  "url": "http://127.0.0.1:3100/mcp",
  "enabled": false,
  "timeout": 30000
}
```

Use this guide when OpenCode cannot see the `openpencil_*` tools, when the MCP server is not listening, or when a design task needs to drive an `.op` file from OpenCode.

## Start The MCP Server

On Windows, the OpenPencil CLI is normally installed at `C:\Program Files\OpenPencil\op.exe`.

Start a headless MCP server for a design file:

```powershell
& "C:\Program Files\OpenPencil\op.exe" start --headless --file "designs/marketplace-search.op" --port 3100
```

For another design, replace the `--file` value with the relevant file under `designs/`.

## Verify The Server

Check that OpenPencil reports the MCP server as running:

```powershell
& "C:\Program Files\OpenPencil\op.exe" status --port 3100
```

Expected shape:

```json
{"running":true,"port":3100,"url":"http://127.0.0.1:3100"}
```

Check that the TCP port responds:

```powershell
Test-NetConnection -ComputerName 127.0.0.1 -Port 3100
```

Expected signal:

```text
TcpTestSucceeded : True
```

Read the active document through the CLI:

```powershell
& "C:\Program Files\OpenPencil\op.exe" page list --port 3100 --pretty
& "C:\Program Files\OpenPencil\op.exe" layout --port 3100 --depth 2 --pretty
```

Run the OpenPencil design linter:

```powershell
& "C:\Program Files\OpenPencil\op.exe" design:lint --port 3100 --pretty
```

## Verify OpenCode Integration

After the MCP server is running, OpenCode should expose native tools named like `openpencil_get_document_info`, `openpencil_get_editor_state`, and `openpencil_batch_get`.

If those tools are not available after starting the server, restart OpenCode while the MCP server remains running.

Useful smoke checks from OpenCode:

- `openpencil_get_document_info`
- `openpencil_get_editor_state`
- `openpencil_snapshot_layout`

## Troubleshooting

- `GET http://127.0.0.1:3100/mcp` returning `400 Bad Request` is not a failure by itself. The MCP endpoint expects the MCP/JSON-RPC protocol, not a browser-style GET request.
- If `Test-NetConnection` fails, the MCP server is not listening on port `3100`. Start it again with `op.exe start --headless`.
- If another process uses port `3100`, stop that process or choose another port and update `opencode.json` to match.
- If OpenPencil Desktop is open but OpenCode still has no tools, do not assume Desktop exposed MCP. Start the server explicitly with `op.exe start --headless` and restart OpenCode.
- If the CLI is not on `PATH`, invoke it with the full path: `C:\Program Files\OpenPencil\op.exe`.
