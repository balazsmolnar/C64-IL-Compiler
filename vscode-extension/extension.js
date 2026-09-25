// Plain CommonJS -- no TypeScript/build step needed. `vscode` is supplied
// at runtime by the VS Code extension host, not an npm dependency to
// install. See README.md for how to load this extension.
const vscode = require('vscode');
const path = require('path');

function activate(context) {
  context.subscriptions.push(
    vscode.debug.registerDebugConfigurationProvider('c64test', new C64TestConfigProvider())
  );

  context.subscriptions.push(
    vscode.debug.registerDebugAdapterDescriptorFactory('c64test', new C64TestAdapterFactory())
  );

  // Same adapter process (TestDebugger.dll --dap) for both debug types: a
  // launch config with "program" runs a whole program on VICE, one with
  // "testSelector" runs a unit test in SimpleEmulator.
  context.subscriptions.push(
    vscode.debug.registerDebugConfigurationProvider('c64vice', new C64ViceConfigProvider())
  );
  context.subscriptions.push(
    vscode.debug.registerDebugAdapterDescriptorFactory('c64vice', new C64TestAdapterFactory())
  );

  context.subscriptions.push(
    vscode.commands.registerCommand('c64vice.debugProgram', async () => {
      const folder = vscode.workspace.workspaceFolders && vscode.workspace.workspaceFolders[0];
      if (!folder) {
        vscode.window.showErrorMessage('C64 on VICE: open the C64-IL-Compiler folder as your workspace first.');
        return;
      }
      await vscode.debug.startDebugging(folder, {
        type: 'c64vice',
        name: 'Debug C64 Program on VICE',
        request: 'launch',
      });
    })
  );

  context.subscriptions.push(
    vscode.commands.registerCommand('c64test.debugTestAtCursor', async () => {
      const folder = vscode.workspace.workspaceFolders && vscode.workspace.workspaceFolders[0];
      if (!folder) {
        vscode.window.showErrorMessage('C64 Test Debugger: open the C64-IL-Compiler folder as your workspace first.');
        return;
      }
      await vscode.debug.startDebugging(folder, {
        type: 'c64test',
        name: 'Debug C64 Test',
        request: 'launch',
      });
    })
  );

  context.subscriptions.push(
    vscode.commands.registerCommand('c64test.debugAllTests', async () => {
      const folder = vscode.workspace.workspaceFolders && vscode.workspace.workspaceFolders[0];
      if (!folder) {
        vscode.window.showErrorMessage('C64 Test Debugger: open the C64-IL-Compiler folder as your workspace first.');
        return;
      }
      // testSelector: '*' -- runs every test in turn, auto-advancing past
      // each PASSED/FAILED, and only actually pausing (stopped event) when
      // one of them hits a currently-set breakpoint. See DapServer.cs's
      // run-all mode.
      await vscode.debug.startDebugging(folder, {
        type: 'c64test',
        name: 'Debug All C64 Tests',
        request: 'launch',
        testSelector: '*',
      });
    })
  );
}

// Spawns TestDebugger.dll --dap (the same engine the debug-test skill's CLI
// REPL drives) as the adapter process -- VS Code talks DAP to it over
// stdio automatically once this returns a DebugAdapterExecutable.
class C64TestAdapterFactory {
  createDebugAdapterDescriptor(session) {
    const folder = session.workspaceFolder ? session.workspaceFolder.uri.fsPath
      : (vscode.workspace.workspaceFolders && vscode.workspace.workspaceFolders[0].uri.fsPath);
    if (!folder) {
      throw new Error('C64 Test Debugger: no workspace folder to resolve TestDebugger.dll from.');
    }
    const dllPath = path.join(folder, 'TestDebugger', 'bin', 'Debug', 'TestDebugger.dll');
    return new vscode.DebugAdapterExecutable('dotnet', [dllPath, '--dap'], { cwd: folder });
  }
}

// Fills in `testSelector` from the class/method enclosing the cursor when
// the launch config doesn't already specify one -- lets you place the
// cursor inside a [Test]/[TestCase] method and just hit F5, rather than
// hand-typing "ClassName.MethodName" every time.
class C64TestConfigProvider {
  resolveDebugConfiguration(folder, config) {
    if (config.testSelector) {
      return config;
    }

    const editor = vscode.window.activeTextEditor;
    if (!editor) {
      vscode.window.showErrorMessage(
        'C64 Test Debugger: open a Test/*.cs file and place the cursor inside the test method to debug.');
      return undefined;
    }

    const selector = findEnclosingTestSelector(editor.document, editor.selection.active.line);
    if (!selector) {
      vscode.window.showErrorMessage(
        'C64 Test Debugger: could not find an enclosing class/method at the cursor -- ' +
        'set "testSelector" explicitly in launch.json instead.');
      return undefined;
    }

    config.type = config.type || 'c64test';
    config.request = config.request || 'launch';
    config.name = config.name || `Debug ${selector}`;
    config.testSelector = selector;
    return config;
  }
}

// Fills in `program` when the launch config doesn't set one: the project
// folder (Demo, Hunchback, C64Presentation) of the file open in the active
// editor, falling back to Demo.
class C64ViceConfigProvider {
  resolveDebugConfiguration(folder, config) {
    if (!config.program) {
      const known = ['Demo', 'Hunchback', 'C64Presentation'];
      const editor = vscode.window.activeTextEditor;
      let program = 'Demo';
      if (editor && folder) {
        const rel = path.relative(folder.uri.fsPath, editor.document.uri.fsPath);
        const top = rel.split(path.sep)[0];
        if (known.includes(top)) {
          program = top;
        }
      }
      config.program = program;
    }
    config.type = config.type || 'c64vice';
    config.request = config.request || 'launch';
    config.name = config.name || `Debug ${config.program} on VICE`;
    return config;
  }
}

// A simple regex-based heuristic, not a real C# parser -- good enough for
// this project's flat Test/*.cs layout (one namespace, non-nested test
// classes, one method per line). Scans upward from the cursor for the
// nearest method declaration, then further up for the nearest class
// declaration.
function findEnclosingTestSelector(document, cursorLine) {
  const methodRe = /^\s*(?:\[.*\]\s*)*(?:public|private|internal|protected)?\s*(?:static\s+)?(?:async\s+)?[\w<>\[\],.?]+\s+(\w+)\s*\(/;
  const classRe = /^\s*(?:public|internal)?\s*(?:partial\s+)?class\s+(\w+)/;

  let methodName = null;
  for (let i = cursorLine; i >= 0; i--) {
    const match = methodRe.exec(document.lineAt(i).text);
    if (match) {
      methodName = match[1];
      break;
    }
  }
  if (!methodName) {
    return null;
  }

  let className = null;
  for (let i = cursorLine; i >= 0; i--) {
    const match = classRe.exec(document.lineAt(i).text);
    if (match) {
      className = match[1];
      break;
    }
  }
  if (!className) {
    return null;
  }

  return `${className}.${methodName}`;
}

function deactivate() {}

module.exports = { activate, deactivate };
