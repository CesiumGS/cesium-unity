# Reinterop~ testing & refactor plan (continuation notes)

Goal: the Reinterop~ source generator ("a bit of a mess") needs unit tests before a larger
refactor. Rather than testing internals or relying on brittle golden-file text diffs, we agreed
to build a small, restricted DSL that represents generated C++ (and eventually C#) bodies as
structured, immutable C# `record` trees. Content is *mechanically rendered* from that structure
(single printer), so tests can assert on the tree (free `record` equality) instead of comparing
strings, and declaration/definition/init can be derived from one shared model instead of being
hand-duplicated 2-3 times (a real, observed bug class in this codebase).

## Full agreed strategy (for context / future reference)

Layered test reliability, cheapest/most valuable first:
- **Tier 0** (no refactor needed): use `CSharpGeneratorDriver.RunGeneratorsAndUpdateCompilation(...)`
  and assert `updatedCompilation.GetDiagnostics()` has no errors. Validates generated C# actually
  compiles - a free correctness oracle. **Not yet implemented** (no test project exists yet).
- **Tier 1 (in progress)**: structured "signature"/body DSL so `Content` strings are derived, not
  hand-annotated. This is what we're implementing now (see below).
- **Tier 2** (future, higher investment): shell out to a real C++ compiler (`clang++ -fsyntax-only`
  or MSVC `/Zs`) against generated `.h`/`.cpp` + a small stub runtime header, to validate the
  method **bodies** (marshalling logic) actually compile. Not started.
- **Tier 3** (stretch, probably unnecessary): full compile+link+run behavioral tests, mirroring
  `Tests/TestReinterop.cs` in the parent repo but lightweight/non-Unity. Not started.

Important finding: only `RoslynSourceGenerator` (`ISourceGenerator`, legacy) is actually compiled
into `Reinterop.dll` today - [RoslynIncrementalGenerator.cs](RoslynIncrementalGenerator.cs) is
excluded via `<Compile Remove="RoslynIncrementalGenerator.cs" />` in
[Reinterop.csproj](Reinterop.csproj). Any future end-to-end/driver-based tests (Tier 0, or a full
golden-file harness) should target `RoslynSourceGenerator`, and it's worth asking the user whether
that exclusion is intentional before relying on it.

## The DSL design

Two small files define a **restricted** (not general-purpose) C++ statement/expression model - it
only covers the shapes actually used by Reinterop's generated bodies:

- [CppSyntax.cs](CppSyntax.cs): `CppExpression` (`CppIdentifier`, `CppRaw`, `CppCall`, `CppBinary`)
  and `CppStatement` (`CppVariableDeclaration`, `CppExpressionStatement`, `CppIf`, `CppThrow`,
  `CppReturn`), all `record` types for free structural equality.
- [CppPrinter.cs](CppPrinter.cs): the *only* place that renders these trees to text
  (`CppPrinter.Print(...)`). Matches the codebase's existing convention of braceless
  single-statement `if` bodies.
- [CppInterop.cs](CppInterop.cs): the first "recipe" -
  `CppInterop.CallManagedFunction(functionPointerName, callArguments, resultTypeName?, returnExpression?)`
  builds the statements for the "declare `void* reinteropException = nullptr;`, call the function
  pointer, check the exception and throw `Reinterop::ReinteropNativeException(...)` if set,
  optionally return a converted result" pattern. This exact pattern was previously hand-copied
  (with minor variations) in `Methods.cs`, `Properties.cs`, `Constructors.cs`, and `Fields.cs` -
  confirmed via `grep -r reinteropException`.

Note on naming: the user asked to avoid abbreviations in type names - use `CppExpression`/
`CppStatement`/`CppVariableDeclaration`/`CppExpressionStatement`, not `Expr`/`Stmt`/`VarDecl`/
`ExprStmt`. Keep following this convention for anything added later (e.g. prefer
`CppFunctionSignature` over `CppSig`, `CSharpExpression`/`CSharpStatement` for the future C# DSL).

Deliberately **out of scope** for the DSL (per user's own scoping): full C++ statement AST,
loops/switch (none appear in any generated body - verified), and modeling argument-conversion
expressions themselves (`GetConversionToInteropType`/`GetConversionFromInteropType` results are
still passed through as opaque strings via `CppRaw`). Only the recipe *shape* is structured.

## Status: what's already done

1. Created [CppSyntax.cs](CppSyntax.cs), [CppPrinter.cs](CppPrinter.cs), [CppInterop.cs](CppInterop.cs)
   (all new files, `internal`, no external references needed).
2. Converted two of the three branches in `Methods.cs`'s `GenerateSingleMethod` (the method
   *definition* body, i.e. the `.cpp` side) to use `CppInterop.CallManagedFunction` +
   `CppPrinter.Print(...)`:
   - The `void`-returning, non-pointer-return branch.
   - The non-void, **non**-struct-rewrite branch (`!hasStructRewrite`).
   - The struct-rewrite branch (nullable / blittable-struct return rewriting) was **left
     untouched** (still hand-built strings) - intentionally out of scope for this first pass.
   - The generic-method declaration, `addOperator` (`op_Equality`/`op_Inequality`), and the
     function-pointer field declaration/definition/init-registration code above it were **not**
     touched.
3. `get_errors` on all 4 touched/created files reports no errors (via the language service).
4. **Build verified**: `dotnet` on `PATH` may resolve to a version (e.g. 10.0.302 via homebrew)
   that doesn't match the [global.json](../global.json) pin (10.0.103), and `DOTNET_ROLL_FORWARD`
   does not override that. Just use `~/.dotnet/dotnet` directly instead (confirmed to be 10.0.103).
   `~/.dotnet/dotnet build Reinterop.csproj` builds clean (0 warnings, 0 errors) with the changes
   below. No need to touch `global.json`.
5. **`Reinterop.Tests` project created** at `Reinterop~/Reinterop.Tests/Reinterop.Tests.csproj`
   (TFM `net10.0` - only .NET 10 runtime confirmed installed locally via `~/.dotnet/dotnet
   --list-runtimes`). Plain `ProjectReference` to `../Reinterop.csproj`. NUnit +
   `Microsoft.NET.Test.Sdk` + `NUnit3TestAdapter`. `[assembly: InternalsVisibleTo("Reinterop.Tests")]`
   added via new [AssemblyAttributes.cs](AssemblyAttributes.cs) in the main project. **Gotcha**:
   the new `Reinterop.Tests/` subfolder is *inside* `Reinterop~/`, so `Reinterop.csproj`'s default
   glob picked up its `.cs` files too - had to add `<Compile Remove="Reinterop.Tests/**" />` to
   `Reinterop.csproj` to fix `CS0246` errors about `NUnit`/`Test` types. Remember this if more
   subfolders are added under `Reinterop~/` in the future.
6. **First characterization tests written and passing**: [Reinterop.Tests/CppInteropTests.cs](Reinterop.Tests/CppInteropTests.cs)
   asserts `CppPrinter.Print(CppInterop.CallManagedFunction(...))` produces the exact text the old
   hand-written string interpolation in `Methods.cs` used to produce, for both the void-return and
   value-returning shapes. `~/.dotnet/dotnet test` (run from `Reinterop~/Reinterop.Tests/`) passes:
   2/2 tests green. Also reconfirmed `~/.dotnet/dotnet build Reinterop.csproj` alone still builds
   clean (0 warnings/errors) after the `Compile Remove` fix.
7. Note: earlier in the session `global.json` was briefly `rm`'d by the user while troubleshooting
   the SDK issue, then restored - `git status` now shows it clean/untouched, no action needed.

## Next steps (in order)

1. ~~Create the `Reinterop.Tests` project~~ **DONE**.

2. ~~Write the first characterization test~~ **DONE** - see [Reinterop.Tests/CppInteropTests.cs](Reinterop.Tests/CppInteropTests.cs).

3. ~~Verify `dotnet test` passes~~ **DONE** - 2/2 passing.

4. **Extend the same recipe** to `Properties.cs`, `Constructors.cs`, and `Fields.cs` - **DONE**:
   - Extended `CppInterop.CallManagedFunction` with two new optional parameters to cover the shapes
     needed: `preDeclaredResultTypeName` (declare `Type result;` with no initializer, then call as a
     plain void statement - used for the blittable-struct-return-rewrite case) and
     `resultVariableName` (defaults to `"result"`, but e.g. `Constructors.cs`'s non-blittable-struct
     constructor names it `"handle"`). `CppVariableDeclaration.Initializer` is now nullable in
     [CppSyntax.cs](CppSyntax.cs)/[CppPrinter.cs](CppPrinter.cs) to support the no-initializer case.
   - `Properties.cs`: both the void and non-void (`hasStructRewrite` true or false) branches now use
     `CppInterop.CallManagedFunction` + `CppPrinter.Print`.
   - `Constructors.cs`: both the blittable-struct `Construct()` static function and the non-blittable
     constructor's `_handle([&]() mutable {...}())` lambda body now use the recipe.
   - `Fields.cs`: getter's default and non-Nullable-struct-rewrite branches, plus the setter, now use
     the recipe. The Nullable-with-struct-rewrite getter branch (the `resultIsValid` bool-flag pattern)
     is **intentionally left as a hand-written string template**, same scope exclusion as `Methods.cs`.
   - **Deliberate, user-approved formatting change**: `Fields.cs`'s getter/setter previously used a
     braced `if (...) { throw ...; }` style (with an inconsistent 2-space indent in the getter's
     invocation-array version) instead of the braceless `if (...) \n    throw ...;` style used
     everywhere else (`Methods.cs`/`Properties.cs`/`Constructors.cs`). Asked the user; they chose to
     **normalize** `Fields.cs` to match the braceless style used elsewhere, rather than preserve the
     old brace/indent quirk. This means `Fields.cs`'s generated C++ output changes slightly (braces
     removed, indentation normalized) - purely cosmetic/whitespace, not a semantic change, but **the
     user should re-run their manual before/after generated-output comparison for their real use case
     to confirm this is the only difference** (previous verifications were for `Methods.cs` only).
   - Added 2 more characterization tests to [Reinterop.Tests/CppInteropTests.cs](Reinterop.Tests/CppInteropTests.cs)
     for the new `preDeclaredResultTypeName` and custom `resultVariableName` shapes. All 4 tests pass
     (`~/.dotnet/dotnet test` from `Reinterop~/Reinterop.Tests/`). Main project still builds clean
     (`~/.dotnet/dotnet build Reinterop.csproj`, 0 warnings/errors).

4.5. **Redesigned `CppInterop.CallManagedFunction`'s argument API** - **DONE**. The user flagged two
   ergonomics problems with the step-4 API: (a) `callArguments` were raw strings instead of
   `CppExpression`s, and (b) `preDeclaredResultTypeName` was an awkward, uncoordinated extra
   parameter that required callers to know the out-parameter shape existed at all. Fixed by:
   - Adding `CppUnary(string Op, CppExpression Operand)` to [CppSyntax.cs](CppSyntax.cs)/
     [CppPrinter.cs](CppPrinter.cs) (renders e.g. `&result`, no space).
   - Adding an argument hierarchy to [CppSyntax.cs](CppSyntax.cs): `abstract record CppArgument`,
     `CppValueArgument(CppExpression Expression)` (a plain, already-computed value), and
     `CppOutParameterArgument(string TypeName, string Name)` (declares `TypeName Name;` with no
     initializer immediately before the call, and passes `&Name`). Static factory helpers
     `CppArgument.Value(...)` / `CppArgument.OutParameter(...)` on the base type for ergonomics.
   - Rewrote `CppInterop.CallManagedFunction`'s signature to
     `CallManagedFunction(CppExpression functionPointer, IReadOnlyList<CppArgument> arguments, string? resultTypeName = null, CppExpression? returnExpression = null, string resultVariableName = "result")`.
     `preDeclaredResultTypeName` is gone entirely - an out-parameter is now just another argument.
     The `reinteropException` out-parameter (declaration + trailing `&reinteropException` call
     argument) is now added **automatically** by the recipe; callers no longer pass it themselves.
   - Updated all call sites (`Methods.cs`, `Properties.cs`, `Constructors.cs`, `Fields.cs`) to build
     an `IReadOnlyList<CppArgument>` (filtering out the old manually-added `reinteropException`
     tuple, mapping a `pReturnValue` tuple - when struct-return-rewrite applies - to
     `CppArgument.OutParameter(...)`, and everything else to `CppArgument.Value(...)`). The
     intentionally-untouched, still-hand-written branches (`Methods.cs`'s struct-rewrite `else`,
     `Fields.cs`'s Nullable-with-struct-rewrite getter) were left as-is, unaffected by this change.
   - Rewrote all 4 tests in [Reinterop.Tests/CppInteropTests.cs](Reinterop.Tests/CppInteropTests.cs)
     for the new API; the old `PreDeclaredResultType` test became `CallManagedFunction_OutParameter_MatchesOriginalTemplate`,
     exercising `CppArgument.OutParameter(...)` directly.
   - This redesign is intended to be **100% output-preserving** (unlike step 4's deliberate Fields.cs
     brace normalization) - verified `CppType.GetConversionToInteropType` always renders the
     `pReturnValue` out-parameter as exactly `&result`/`&handle` etc., matching what the old manual
     string concatenation produced. `~/.dotnet/dotnet build Reinterop.csproj` (0 warnings/errors) and
     `~/.dotnet/dotnet test` (4/4 passing) both confirmed after the change. **The user should still
     re-run their real-repo generated-output before/after comparison to confirm zero textual diff.**

5. **Add Tier 0** to the test project once it exists: build a real `CSharpCompilation` +
   `CSharpGeneratorDriver` running `RoslynSourceGenerator`, and assert
   `updatedCompilation.GetDiagnostics()` has no errors, for a handful of representative
   `[Reinterop]`/`ExposeToCPP` input snippets (plain method, property, constructor, field, event,
   enum, inheritance, `ReinteropNativeImplementation`). This is the cheapest broad safety net and
   should happen before or alongside further DSL expansion. Will need to pin
   `Microsoft.CodeAnalysis.CSharp` to the same 3.8.0 version as `Reinterop.csproj` in
   `Reinterop.Tests.csproj` at that point.

6. **Mirror the pattern for the reverse direction** (C++ implements a C# partial method): add a
   `CppImplementationInvoker.TranslateExceptionsToOutParameter(...)` recipe for the
   `try { ... } catch (ReinteropNativeException&) {...} catch (std::exception&) {...} catch (...) {...}`
   pattern duplicated in `MethodsImplementedInCpp.cs` and `CustomDelegateGenerator.cs`.

7. **Mirror Layer 1 for C#**: `CSharpExpression`/`CSharpStatement` + a `CSharpInterop.CallNativeFunction`
   recipe for the equivalent pattern on the C# side (`GeneratedCSharpPartialMethodDefinitions.cs`,
   `Interop.CreateCSharpDelegateInit`, `CustomDelegateGenerator.cs`) - the
   `IntPtr reinteropException = IntPtr.Zero; <call>; if (...) throw ...;` idiom.

8. **Fluent builder** (`context.Type(...).Method(...).Returns(...).Parameter(...).Body(...).AddTo(result)`)
   tying declaration + definition + init-registration together from one signature, once the recipes
   it composes already exist and are tested. Lowest priority - do this last.

9. Only after the above exists should the actual "clean up the mess" refactor of the wider
   codebase begin, running the growing test suite continuously.

## Things to remember

- User preference: no abbreviations in type names (`Statement` not `Stmt`, `Expression` not `Expr`).
- Keep the DSL restricted/closed - don't generalize beyond what's actually used; verified there are
  no loops/switches in any generated body.
- As of the step-4.5 redesign, `CppInterop.CallManagedFunction` appends the trailing
  `&reinteropException` call argument **itself** - callers must NOT include it in `arguments`.
  (The still-hand-written, unconverted branches in `Methods.cs`/`Fields.cs` are the only remaining
  places that manage `reinteropException` manually, via their own untouched string templates.)
- Use `~/.dotnet/dotnet` (not plain `dotnet`) for build/test commands in this repo - see above.
