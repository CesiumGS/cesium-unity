#!/usr/bin/env node

/*
 * Update Samples Project Steps (see "Update the Cesium for Unity Samples Project" in
 * Documentation~/release-guide.md)
 * - Sets the com.cesium.unity dependency version in Packages/manifest.json.
 * - Sets PlayerSettings.bundleVersion in ProjectSettings/ProjectSettings.asset.
 * - Sets defaultIonAccessToken in Assets/CesiumSettings/Resources/CesiumIonServers/ion.cesium.com.asset.
 * - Commits the changes locally, but does not push.
 *
 * Cloning the samples repo, creating the ion token on ion.cesium.com, and pushing are still manual steps.
 */

const fs = require("fs");
const path = require("path");
const { execFileSync } = require("child_process");

const SAMPLES_REPO_URL = "https://github.com/CesiumGS/cesium-unity-samples.git";

function usage() {
  console.log(`Usage:
  CESIUM_ION_TOKEN=<token> npm run update-unity-samples -- --version <X.Y.Z> --samples-dir <path> [--no-commit] [--dry-run]

Required:
  --version         New com.cesium.unity version to set in the samples project, for example 1.26.0
  --samples-dir     Path to clone cesium-unity-samples into, or to an existing checkout to update

Required environment variable:
  CESIUM_ION_TOKEN  New Cesium ion access token for the samples project. Only read from the
                    environment, never accepted as a CLI argument, so it doesn't end up in your
                    shell history or process list.

Optional:
  --repo-url        Override the repository to clone/pull. Defaults to ${SAMPLES_REPO_URL}
  --no-commit       Write the files but skip the local git commit.
  --dry-run         Print planned changes without writing files, cloning, pulling, or committing.
                    The token itself is never printed.

If --samples-dir does not exist, it is cloned fresh. If it already exists as a checkout of the
samples repo, it is updated with "git checkout main && git pull --ff-only"; the script refuses to
touch it if it has uncommitted changes.

Updates the following files under --samples-dir:
  - Packages/manifest.json: sets the com.cesium.unity dependency version.
  - ProjectSettings/ProjectSettings.asset: sets PlayerSettings.bundleVersion.
  - Assets/CesiumSettings/Resources/CesiumIonServers/ion.cesium.com.asset: sets defaultIonAccessToken.

Commits the changes locally with git, but does not push. Creating the ion token on ion.cesium.com
and pushing are still manual steps.
`);
}

function parseArgs(argv) {
  const args = { dryRun: false, commit: true };

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

    if (value === "--no-commit") {
      args.commit = false;
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

function replaceDependencyVersion(contents, version) {
  // Matches the "com.cesium.unity" entry under the "dependencies" object in Packages/manifest.json.
  const pattern = /("com\.cesium\.unity"\s*:\s*")([^"]+)(")/;
  if (!pattern.test(contents)) {
    throw new Error('Could not find the "com.cesium.unity" dependency in manifest.json.');
  }

  return contents.replace(pattern, `$1${version}$3`);
}

function replaceBundleVersion(contents, version) {
  const pattern = /^(\s*bundleVersion:\s*).*$/m;
  if (!pattern.test(contents)) {
    throw new Error("Could not find bundleVersion in ProjectSettings.asset.");
  }

  return contents.replace(pattern, `$1${version}`);
}

function replaceIonToken(contents, token) {
  const pattern = /^(\s*defaultIonAccessToken:\s*).*$/m;
  if (!pattern.test(contents)) {
    throw new Error("Could not find defaultIonAccessToken in ion.cesium.com.asset.");
  }

  return contents.replace(pattern, `$1${token}`);
}

function runGit(cwd, args, dryRun) {
  if (dryRun) {
    return;
  }

  execFileSync("git", ["-C", cwd, ...args], { stdio: "inherit" });
}

function hasUncommittedChanges(samplesDir) {
  const status = execFileSync("git", ["-C", samplesDir, "status", "--porcelain"], { encoding: "utf8" }).trim();
  return status !== "";
}

function ensureSamplesCheckout(samplesDir, repoUrl, dryRun) {
  const exists = fs.existsSync(samplesDir);

  if (!exists) {
    console.log(`- git clone ${repoUrl} into ${samplesDir}`);
    if (dryRun) {
      return { ready: false };
    }

    fs.mkdirSync(path.dirname(samplesDir), { recursive: true });
    execFileSync("git", ["clone", repoUrl, samplesDir], { stdio: "inherit" });
    return { ready: true };
  }

  if (!fs.existsSync(path.join(samplesDir, ".git"))) {
    throw new Error(`${samplesDir} exists but is not a git repository. Remove it or choose a different --samples-dir.`);
  }

  if (hasUncommittedChanges(samplesDir)) {
    throw new Error(
      `${samplesDir} has uncommitted changes. Commit, stash, or discard them before running this script, or choose a different --samples-dir.`
    );
  }

  console.log(`- git -C ${samplesDir} checkout main && git pull --ff-only`);
  if (dryRun) {
    return { ready: false };
  }

  execFileSync("git", ["-C", samplesDir, "checkout", "main"], { stdio: "inherit" });
  execFileSync("git", ["-C", samplesDir, "pull", "--ff-only"], { stdio: "inherit" });
  return { ready: true };
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

  if (!args.samplesDir) {
    throw new Error("Missing required --samples-dir argument.");
  }

  const samplesDir = path.resolve(args.samplesDir);
  const repoUrl = args.repoUrl || SAMPLES_REPO_URL;

  if (args.ionToken) {
    throw new Error(
      "--ion-token is not supported, so it can't end up in your shell history or process list. Set the CESIUM_ION_TOKEN environment variable instead."
    );
  }

  const ionToken = process.env.CESIUM_ION_TOKEN;
  if (!ionToken) {
    throw new Error("Missing ion token. Set the CESIUM_ION_TOKEN environment variable before running this script.");
  }

  console.log(`Samples update plan for v${args.version}`);

  const { ready } = ensureSamplesCheckout(samplesDir, repoUrl, args.dryRun);

  if (args.dryRun && !ready) {
    console.log("- Dry run: skipping file-level preview because the checkout isn't available locally yet.");
    return;
  }

  const manifestPath = path.join(samplesDir, "Packages", "manifest.json");
  const projectSettingsPath = path.join(samplesDir, "ProjectSettings", "ProjectSettings.asset");
  const ionAssetPath = path.join(
    samplesDir,
    "Assets",
    "CesiumSettings",
    "Resources",
    "CesiumIonServers",
    "ion.cesium.com.asset"
  );

  const manifest = readText(manifestPath);
  const projectSettings = readText(projectSettingsPath);
  const ionAsset = readText(ionAssetPath);

  console.log(`- ${path.relative(samplesDir, manifestPath)}: set com.cesium.unity dependency to ${args.version}`);
  console.log(`- ${path.relative(samplesDir, projectSettingsPath)}: set bundleVersion to ${args.version}`);
  console.log(`- ${path.relative(samplesDir, ionAssetPath)}: set defaultIonAccessToken (value hidden)`);
  console.log(args.commit ? "- git commit the changes locally (not pushed)" : "- Skipping git commit (--no-commit)");

  const nextManifest = replaceDependencyVersion(manifest, args.version);
  const nextProjectSettings = replaceBundleVersion(projectSettings, args.version);
  const nextIonAsset = replaceIonToken(ionAsset, ionToken);

  if (args.dryRun) {
    return;
  }

  writeText(manifestPath, nextManifest);
  writeText(projectSettingsPath, nextProjectSettings);
  writeText(ionAssetPath, nextIonAsset);

  if (!args.commit) {
    return;
  }

  runGit(samplesDir, ["add", manifestPath, projectSettingsPath, ionAssetPath], false);
  runGit(samplesDir, ["commit", "-m", `Prepare samples for v${args.version}`], false);
}

try {
  main();
} catch (error) {
  console.error(error instanceof Error ? error.message : error);
  process.exit(1);
}
