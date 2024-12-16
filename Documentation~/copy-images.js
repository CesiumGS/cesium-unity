/**
 * Documentation for Cesium for Unity needs to exist under the Documentation~ folder, 
 * as folders ending in ~ are ignored by Unity. However, this causes an issue with 
 * Doxygen, as described here: https://github.com/doxygen/doxygen/issues/11273
 * 
 * The solution to this is to avoid putting the images in Doxygen's `IMAGE_PATH`,
 * which triggers the issue, instead copying them to the same relative directory as
 * the original image path in the Markdown files. The problem is that doing this from
 * the package.json script will cause issues with the difference between `cp` on Linux
 * and `copy` on Windows. Instead, we use this script to perform the copy operation
 * in a cross-platform way.
 */
const fs = require("fs");

fs.mkdirSync("./Reference/html/Documentation~/images", { recursive: true });
const images = fs.readdirSync("./images");
images.forEach(img => fs.copyFileSync("./images/" + img, "./Reference/html/Documentation~/images/" + img));
console.log(`copied ${images.length} images to Documentation~/Reference/html`);