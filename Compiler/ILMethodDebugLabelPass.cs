using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace Compiler;

// Only runs when CompilerContext.EmitDebugInfo is set (see that property's
// comment for why this must be opt-in). Reads the portable PDB sitting
// alongside the compiled assembly (produced by a normal SDK-style build --
// no <DebugType> csproj setting needed, portable is the net10.0 default) and,
// for each non-hidden sequence point, gives the matching ILOperation (found
// by IL offset -- ILOperation.Position is set from the same byte offset a
// sequence point's Offset is keyed on, see ILMethodCodePass) a label -- reusing
// one already assigned by ILMethodLabelPass (a branch/switch target) rather
// than colliding with it, assigning a fresh one only when Label is still
// null. Must run after ILMethodLabelPass, before the optimizer group (an
// optimizer marking a labeled operation Optimized is fine -- ILMethodEmitPass
// still emits the label, just pointing at whatever real instruction follows).
//
// One instance is constructed once in Program.cs and reused for every method
// (ILCodePass's per-method loop calls the same pass instances repeatedly), so
// the PDB is opened lazily on first use and cached for the rest of the run --
// this deliberately doesn't reopen the file for every method.
class ILMethodDebugLabelPass : ICompilerMethodPass
{
    private MetadataReader _pdbReader;
    private bool _pdbLoadAttempted;

    public void Execute(CompilerMethodContext context)
    {
        if (!context.CompilerContext.EmitDebugInfo)
            return;
        if (context.Method.IsAbstract)
            return;

        EnsurePdbLoaded(context.CompilerContext);
        if (_pdbReader == null)
            return;

        var entityHandle = MetadataTokens.EntityHandle(context.Method.MetadataToken);
        if (entityHandle.Kind != HandleKind.MethodDefinition)
            return;

        var methodHandle = (MethodDefinitionHandle)entityHandle;
        MethodDebugInformation debugInfo;
        try
        {
            var debugHandle = methodHandle.ToDebugInformationHandle();
            debugInfo = _pdbReader.GetMethodDebugInformation(debugHandle);
        }
        catch (BadImageFormatException)
        {
            // Compiler-generated method (e.g. a lambda/iterator shim) with no
            // real PDB row -- nothing to label.
            return;
        }

        var methodLabel = context.Method.GetLabel();
        var byPosition = context.Lines.ToDictionary(l => l.Position);

        foreach (var sp in debugInfo.GetSequencePoints())
        {
            if (sp.IsHidden)
                continue;
            if (!byPosition.TryGetValue(sp.Offset, out var line))
                continue;

            // "_dbg" marker (not just "_{offset}", which ILMethodLabelPass
            // already uses for branch targets) -- a plain "{methodLabel}_
            // {offset}" collides with another method's own bare entry-point
            // label whenever that method happens to be literally named
            // "{ThisMethodName}_{offset}" (e.g. Test/BranchTest.cs has both
            // Branch_Multiple_Conditions and Branch_Multiple_Conditions_2 --
            // offset 2 in the former collided with the latter's entry label
            // once sequence points made "_2" a plausible generated suffix).
            line.Label ??= $"{methodLabel}_dbg{line.Position}";

            var doc = _pdbReader.GetDocument(sp.Document);
            var sourceFile = Path.GetFileName(_pdbReader.GetString(doc.Name));
            context.CompilerContext.DebugSequencePoints.Add(
                new DebugSequencePoint(methodLabel, line.Label, sourceFile, sp.StartLine));
        }

        foreach (var scopeHandle in _pdbReader.GetLocalScopes(methodHandle.ToDebugInformationHandle()))
        {
            var scope = _pdbReader.GetLocalScope(scopeHandle);
            foreach (var localHandle in scope.GetLocalVariables())
            {
                var local = _pdbReader.GetLocalVariable(localHandle);
                var name = _pdbReader.GetString(local.Name);
                context.CompilerContext.DebugLocals.Add(
                    new DebugLocal(methodLabel, local.Index, name));
            }
        }
    }

    private void EnsurePdbLoaded(CompilerContext context)
    {
        if (_pdbLoadAttempted)
            return;
        _pdbLoadAttempted = true;

        var pdbPath = Path.ChangeExtension(context.Assembly.Location, ".pdb");
        if (!File.Exists(pdbPath))
            return;

        var provider = MetadataReaderProvider.FromPortablePdbStream(File.OpenRead(pdbPath));
        _pdbReader = provider.GetMetadataReader();
    }
}
