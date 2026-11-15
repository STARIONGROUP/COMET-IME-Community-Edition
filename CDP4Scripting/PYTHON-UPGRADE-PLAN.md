# CDP4Scripting: Python upgrade plan

Status: proposal, 2026-11-15. Based on reading the plugin, the IME, the SDK API surface and the COMET-SDKP repository. Nothing in this plan has been prototyped yet; items marked **(verify)** are assumptions the spike in phase 0 must confirm.

## 1. Goals

1. Run scripts on a contemporary CPython (3.12 or later) instead of IronPython's Python 3.4 language level.
2. Let users point the IME at a local Python environment (an interpreter or a venv), or have the IME create one.
3. Let users install and import third-party libraries, including those with native extensions (numpy, pandas, openpyxl, matplotlib).
4. Keep existing scripts source-compatible where feasible: the `Command` object and direct access to `CDP4Common` objects stay, with migration notes for the known IronPython differences.
5. Give new scripts a Pythonic API by building on the existing [COMET-SDKP](https://github.com/STARIONGROUP/COMET-SDKP-Community-Edition) package, so the same scripts can also run outside the IME.

Non-goals for this plan: scripting from the Office add-in, and wrapping every model class in a Python class.

## 2. Current state

### Architecture

| Part | Where | What it does |
|---|---|---|
| Engine | `ViewModels/PythonScriptPanelViewModel.cs` | IronPython 3.4.2 on the DLR, in-process. One runtime, engine and scope per panel. |
| Panel base | `ViewModels/ScriptPanelViewModel.cs` | Editor (AvalonEdit), run/stop/save commands, completion, variables list. Holds DLR `ScriptEngine` and `ScriptRuntime` properties. |
| Model API | `Helpers/ScriptingProxy.cs` | MEF shared singleton injected into each scope as `Command`. Read-only lookups plus a few UI commands. |
| Terminals | `Helpers/OutputTerminal.cs`, `Helpers/InputTerminal.cs` | Output pane and a one-line prompt with history. |
| Ribbon | `ViewModels/ScriptingEngineRibbonPageGroupViewModel.cs` | New Python/text script, open, save all. |
| Installer | `CDP4IMEInstaller/CDP4Scripting.wxs` | Ships the IronPython and DLR assemblies. |

### Limitations

- **Python 3.4 language level, no C extensions.** numpy, pandas and similar cannot be loaded.
- **No standard library shipped.** Neither the build output nor the installer contains IronPython's `Lib` folder, so only the built-in modules import.
- **No environment concept.** The only search path added is the user's Documents folder. No pip, no venv, no settings.
- **Stop is unreliable.** `StopScript` cancels a token that is only checked when the script writes output, so a silent loop cannot be stopped.
- **Output is ASCII-encoded.** Non-ASCII text is mangled.
- **Shared mutable proxy.** `ScriptingProxy` is a singleton with a "current panel" property, so two panels running at once can write to each other's output.
- **Engine coupling.** The base view model exposes DLR types, so the engine cannot be swapped without a refactor.
- **Weak `Command` API.** Lookups are by string, `ModelCode` takes a comma-separated string, and failures print a message and return `None` instead of raising.

### COMET-SDKP

A Starion repository (LGPL 2.1, Python 3.12+, version 0.1.0 alpha, no published release, not on PyPI). It is a pip-installable package that uses `pythonnet` to load the COMET .NET SDK assemblies bundled inside it, and opens its own session to a COMET server.

It already provides: session open/close, listing models and domains, opening the active iteration, product tree computation, lookups by parameter type, transactions and value writes, type hints, documentation and a pytest suite.

Gaps for IME use:

- **Login options.** `open()` takes only URL, user name and password; no authentication scheme, token or proxy settings.
- **Data sources.** Web services only; no file-based (exchange file) access.
- **Iteration choice.** Opens the active iteration only, not a specific iteration and domain.
- **Error style.** `open()`, `close()` and `write()` return an error string, or `None` on success, although exception classes are defined.
- **Convenience layer.** No conversion of parameter values from strings, and no table or pandas exports.
- **Naming.** camelCase methods, and no `Command` equivalent.

## 3. Target design

### Decision

Run CPython **out of process**: one `python.exe` child process per script panel, taken from the user's selected environment. Inside that process, COMET-SDKP loads .NET and the SDK assemblies through `pythonnet`, and the script works against its own session to the data source.

IronPython stays as a fallback engine for when no Python environment is configured, for at least one release.

### Division of work

| Piece | Home | Reason |
|---|---|---|
| Client library: loading the SDK, sessions, lookups, value conversion, tables, writes | COMET-SDKP | Useful anywhere Python runs: notebooks, CI jobs, other tools |
| Kernel: pipe protocol with the IME, output streaming, stop handling, `Command` compatibility layer | `CDP4Scripting` | Only meaningful with the IME; it imports SDKP |
| Script panels, environment and package management UI | `CDP4Scripting` | IME features |

### Why not embed CPython in the IME process

| Concern | Embedded (Python.NET in the IME) | Out of process (this plan) |
|---|---|---|
| Native crash in a library | Takes the IME down | Only the script process dies |
| Switching environment | IME restart | Start a new process |
| Bitness | Must match the IME | Free |
| Stopping a script | Interrupt only; may not work in native code | Interrupt, then kill; always works |
| Shipping Python.NET with the IME | Yes | No; it lives in the environment |
| Scripts usable outside the IME | No | Yes, through SDKP |
| Model objects | The IME's live cache | A separate session, same object model |
| First run in a panel | Immediate | Pays for login and reading iterations |

Keeping IronPython without change was also considered and rejected: it meets none of the goals.

### How it works

```
IME process                                   Script process (python.exe from the venv)
-----------                                   -----------------------------------------
ScriptPanelViewModel
  -> IScriptEngine
       IronPythonEngine (fallback)
       CPythonProcessEngine  -- stdin/stdout/stderr -->  kernel bootstrap (from CDP4Scripting)
                             -- control pipe ------->      imports comet_sdkp (pythonnet + SDK)
                                                           opens its own session
                                                           injects Command
                                                           executes scripts, keeps globals
```

1. **Kernel per panel.** The process is started on the first run and kept alive, like a notebook kernel. Variables persist between runs, the input prompt keeps working, and the session is opened once.
2. **Start-up.** The IME launches `python.exe -u` with the kernel bootstrap and sends the connection details over a pipe: data source URI, authentication scheme, credentials or token, proxy settings, and the open iterations with their domains (from `ISession.Credentials` and `ISession.OpenIterations`). Secrets never go on the command line or into environment variables.
3. **Run.** The child refreshes its session, executes the script in the panel's globals, and streams stdout and stderr (UTF-8) to the output terminal.
4. **Control channel.** A named pipe carries structured messages: run finished or failed, variable list (name, type, text), completion requests and replies, and UI commands that must execute in the IME (`Clear`, `OpenDialog`).
5. **Stop.** Send an interrupt; if the script has not stopped after a short timeout, kill the process and start a fresh one on the next run.
6. **`Command` compatibility.** A thin Python object in the kernel offering the existing method names (`GetEngineeringModel`, `GetEngineeringModelIteration`, `GetElementDefinition`, `GetParameter`, `GetValue`, `GetSiteDirectory`, `ModelCode`), implemented on SDKP. UI commands become control messages. New scripts use SDKP directly.

### Behaviour changes users will notice

- Scripts see what is on the data source, not edits still open in an unsaved IME dialog.
- The first run in a panel takes longer, because the child logs in and reads the iterations.
- The variables panel shows text representations, not live objects.
- Old scripts may need small changes where IronPython and `pythonnet` differ. Example: a .NET string arrives as a Python `str`, so `.ToLower()` becomes `.lower()`. Number and enum conversions are stricter.
- Scripts can write to the model through SDKP's transactions. The IME shows such changes after its next refresh.

## 4. Environment and package management

- **No Python in the installer.** The IME detects existing interpreters (the `py` launcher, the registry, `PATH`) and lets the user browse to an interpreter or venv. Python 3.12 or later is required.
- **Create environment.** One action creates a COMET venv under `%LOCALAPPDATA%` from a detected interpreter and installs COMET-SDKP into it, which brings `pythonnet`.
- **SDKP source.** The installer carries a pinned SDKP wheel, so creating an environment works offline and the IME always gets a compatible version. An existing environment is checked for a compatible SDKP version.
- **No interpreter found.** Offer to download a standalone CPython build. To be decided: see open decisions.
- **Selection.** Stored in the plugin settings; the active environment and Python version are shown in the script panel. A panel picks up a changed selection the next time its process starts.
- **Packages.** A dialog runs `python -m pip` from the selected environment as a subprocess, with streamed output: install by name, list installed packages, install from `requirements.txt`.

## 5. Phases

| # | Where | Phase | Outcome |
|---|---|---|---|
| 0 | Both | Spike | The risks in section 6 are answered; go or no-go on the design. |
| 1 | IME | Engine abstraction | `IScriptEngine` (execute, interrupt, variables, reset, output events). IronPython moved behind it. DLR types removed from `ScriptPanelViewModel`. Proxy panel context made per-execution. No behaviour change. |
| 2 | SDKP | IME prerequisites | Login with scheme, token and proxy settings. Open a specific iteration and domain. Session refresh. Exceptions instead of error strings. A first tagged release. |
| 3 | IME | CPython process engine | Kernel bootstrap, process lifecycle, session handover, output streaming, control channel, stop, variables panel, `Command` compatibility layer. |
| 4 | IME | Environment management | Discovery, selection, create-venv with the bundled SDKP wheel, settings, status display. |
| 5 | IME | Package management | pip dialog and `requirements.txt` support. |
| 6 | IME | Editor | Completion from the child process (introspection, `jedi` when installed). Highlighting sheet updated for current syntax (`match`, `async`, f-strings). |
| 7 | SDKP | Convenience layer | Value conversion from strings, table exports with optional pandas, file-based data access, snake_case names with the old ones kept as aliases. |
| 8 | IME | Ship | Installer changes (SDKP wheel, IronPython kept or removed), third-party licences, user documentation, migration notes for existing scripts. |

Phase 1 carries no risk from the spike and can start in parallel with phase 0. Phase 7 is independent of the IME work and can be scheduled by demand.

## 6. Risks

For the spike:

1. **SDK versions (verify).** SDKP bundles its own SDK assemblies. Confirm they are compatible with the data sources the IME talks to, and decide how SDKP's SDK version follows the IME's.
2. **Start-up time (verify).** Measure login plus iteration read on a realistically large model. If too slow, read iterations lazily when the script first asks for them.
3. **External token logins (verify).** With OpenID authentication the child would share the IME's refresh token. If the identity provider rotates refresh tokens, the two processes could invalidate each other. Basic and local-token logins are unaffected because the child logs in itself.
4. **File-based sessions (verify).** SDKP has no file-based access yet, and it is unknown whether the IME keeps the exchange file locked, which would block a second reader. Until solved, file-based sessions fall back to IronPython.
5. **Interrupt on Windows (verify).** Confirm a clean interrupt can be delivered to the child without killing it.

Spike exit criteria: from a venv with SDKP installed, a script started by the IME walks an `Iteration`, imports numpy, is stopped while in a silent loop, and prints non-ASCII output correctly.

Ongoing:

6. **SDKP maturity.** It is an alpha with no releases. The IME pins a tagged version, and phase 2 includes producing that first release.
7. **Version coupling.** SDKP, the SDK assemblies inside it and the IME's kernel protocol must stay compatible. The kernel reports its SDKP version at start-up and the IME refuses incompatible ones with a clear message.

## 7. Open decisions

1. **Downloading Python.** Offer a one-click download of a standalone build when none is found (recommended), or require users to install Python themselves.
2. **IronPython's lifetime.** Keep as fallback for one release and then remove (recommended), or remove immediately. Removal depends on file-based sessions working in the new engine.
3. **SDKP distribution.** Bundle a wheel with the IME installer only (minimum), or also publish SDKP to PyPI so standalone users can `pip install` it (recommended).
4. **Writes from scripts.** SDKP already supports them. Allow in the IME from the start (recommended, since the user's own permissions apply), or disable until reviewed.
5. **Embedded mode.** Not planned. Reconsider only if the spike shows start-up time or compatibility problems that cannot be solved.
