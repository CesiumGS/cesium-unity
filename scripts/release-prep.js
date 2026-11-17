#!/usr/bin/env node

/*
 * Release Preparation Steps
 * - Updates native~/extern/cesium-native
 *   initialize the submodule if needed, then update it by checking out main,
 *   pulling, and staging the parent repo submodule reference.
 * - Updates package.json to the target release version.
 */

const fs = require("fs");
const path = require("path");
const { execFileSync } = require("child_process");

const CESIUM_NATIVE_SUBMODULE = "native~/extern/cesium-native";

function usage() {
  console.log(`Usage:
  node scripts/release-prep.js --version <X.Y.Z> [--dry-run]

Required:
  --version         New Cesium for Unity version, for example 1.26.0

Optional:
  --dry-run         Print planned changes without writing files or staging the submodule.

The script derives the cesium-native version from the latest tag in the cesium-native repository.
It updates package.json, initializes ${CESIUM_NATIVE_SUBMODULE} if needed and updates it to main,
and leaves CHANGES.md untouched.
`);
}

function parseArgs(argv) {
  const args = { dryRun: false };

  for (let index = 2; index < argv.length; ++index) {
    const value = argv[index];

    if (value === "--help" || value === "-h") {
      args.help = true;
      continue;
    }

    if (value === "--dry-run") {
      args.dryRun = true;
      continue;
    }

    if (!value.startsWith("--")) {
      throw new Error(`Unexpected argument: ${value}`);
    }

    const key = value.slice(2).replace(/-([a-z])/g, (_, letter) => letter.toUpperCase());
    const next = argv[++index];
    if (next === undefined || next.startsWith("--")) {
      throw new Error(`Missing value for --${key}`);
    }

    args[key] = next;
  }

  return args;
}

function readText(filePath) {
  return fs.readFileSync(filePath, "utf8");
}

function writeText(filePath, contents) {
  const existing = readText(filePath);
  if (existing === contents) {
    return false;
  }

  fs.writeFileSync(filePath, contents);

  return true;
}

function latestCesiumNativeVersion() {
  // Mirrors the release-guide step that checks which cesium-native version the release will reference.
  const output = execFileSync(
    "git",
    ["ls-remote", "--tags", "--refs", "https://github.com/CesiumGS/cesium-native.git", "v*"],
    { encoding: "utf8" }
  ).trim();

  if (!output) {
    throw new Error("Could not determine the latest cesium-native tag from the remote repository.");
  }

  const versions = output
    .split(/\r?\n/)
    .map((line) => line.split("\t")[1])
    .map((tag) => /^refs\/tags\/v(\d+\.\d+\.\d+)$/.exec(tag))
    .filter(Boolean)
    .map((match) => match[1])
    .sort(compareSemver);

  if (versions.length === 0) {
    throw new Error("Could not find any semantic version tags in the cesium-native remote repository.");
  }

  return versions[versions.length - 1];
}

function previousCesiumNativeVersionFromChangelog(contents) {
  const match = contents.match(/from v(\d+\.\d+\.\d+) to v(\d+\.\d+\.\d+)\./);
  if (!match) {
    throw new Error("Could not find an existing cesium-native version note in CHANGES.md.");
  }

  return match[2];
}

function compareSemver(left, right) {
  const leftParts = left.split(".").map(Number);
  const rightParts = right.split(".").map(Number);

  for (let index = 0; index < 3; ++index) {
    if (leftParts[index] !== rightParts[index]) {
      return leftParts[index] - rightParts[index];
    }
  }

  return 0;
}

function replaceJsonField(contents, fieldName, value) {
  const pattern = new RegExp(`("${fieldName}"\\s*:\\s*")([^"]+)(")`);
  if (!pattern.test(contents)) {
    throw new Error(`Could not find ${fieldName} in file.`);
  }

  return contents.replace(pattern, `$1${value}$3`);
}

function updatePackageJson(contents, version) {
  // Maps to the release-guide step that changes the version in package.json.
  return replaceJsonField(contents, "version", version);
}

function isSubmoduleInitialized(submodulePath) {
  // Git run inside an empty submodule directory silently falls through to the parent repo.
  let toplevel;

  try {
    toplevel = execFileSync("git", ["-C", submodulePath, "rev-parse", "--show-toplevel"], {
      encoding: "utf8",
      stdio: ["ignore", "pipe", "ignore"],
    }).trim();
  } catch {
    return false;
  }

  return path.resolve(toplevel) === path.resolve(submodulePath);
}

function ensureSubmoduleInitialized(repoRoot, submodulePath) {
  if (isSubmoduleInitialized(submodulePath)) {
    return;
  }

  console.log(`Initializing ${CESIUM_NATIVE_SUBMODULE}, this may take a while...`);
  execFileSync(
    "git",
    ["-C", repoRoot, "submodule", "update", "--init", "--recursive", CESIUM_NATIVE_SUBMODULE],
    { stdio: "inherit" }
  );

  if (!isSubmoduleInitialized(submodulePath)) {
    throw new Error(
      `${CESIUM_NATIVE_SUBMODULE} is still not an initialized submodule after "git submodule update --init --recursive". ` +
        `Initialize it manually and try again.`
    );
  }
}

function runGit(cwd, args, dryRun) {
  if (dryRun) {
    return;
  }

  execFileSync("git", ["-C", cwd, ...args], { stdio: "inherit" });
}

function main() {
  const args = parseArgs(process.argv);
  if (args.help) {
    usage();
    return;
  }

  if (!args.version) {
    throw new Error("Missing required --version argument.");
  }

  if (!/^\d+\.\d+\.\d+$/.test(args.version)) {
    throw new Error(`Invalid version format: ${args.version}`);
  }

  const repoRoot = path.resolve(__dirname, "..");
  const packageJsonPath = path.join(repoRoot, "package.json");
  const cesiumNativePath = path.join(repoRoot, ...CESIUM_NATIVE_SUBMODULE.split("/"));

  const packageJson = readText(packageJsonPath);
  const changes = readText(path.join(repoRoot, "CHANGES.md"));
  const parsedPackageJson = JSON.parse(packageJson);
  const currentPackageVersion = parsedPackageJson.version;
  const cesiumNativeTo = latestCesiumNativeVersion();
  const cesiumNativeFrom = previousCesiumNativeVersionFromChangelog(changes);

  console.log(`Release prep plan for v${args.version}`);
  console.log(`- Cesium Native: v${cesiumNativeFrom} -> v${cesiumNativeTo}`);
  if (cesiumNativeTo === cesiumNativeFrom) {
    console.log("- WARNING: the latest cesium-native tag matches the changelog baseline; make sure the new native release tag has been pushed before using this script for a real release.");
  }
  console.log(`- package.json: version ${currentPackageVersion} -> ${args.version}`);
  if (!isSubmoduleInitialized(cesiumNativePath)) {
    console.log(`- ${CESIUM_NATIVE_SUBMODULE}: not initialized, so it will be cloned first`);
  }
  console.log(`- ${CESIUM_NATIVE_SUBMODULE}: git checkout main && git pull`);
  console.log(`- ${CESIUM_NATIVE_SUBMODULE}: stage the updated submodule reference in the parent repo`);
  console.log(`- CHANGES.md: no scripted changes; update it separately in the release workflow`);

  if (args.dryRun) {
    return;
  }

  ensureSubmoduleInitialized(repoRoot, cesiumNativePath);

  const nextPackageJson = updatePackageJson(packageJson, args.version);

  writeText(packageJsonPath, nextPackageJson);

  // Mirrors the release-guide submodule step: checkout main, pull, then stage the parent repo pointer.
  runGit(cesiumNativePath, ["checkout", "main"], false);
  runGit(cesiumNativePath, ["pull"], false);
  runGit(repoRoot, ["add", CESIUM_NATIVE_SUBMODULE], false);
}

try {
  main();
} catch (error) {
  console.error(error instanceof Error ? error.message : error);
  process.exit(1);
}
