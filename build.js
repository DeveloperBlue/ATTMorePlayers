/* Script for building and packaging the mod */

const fs = require('fs');
const path = require('path');
const archiver = require('archiver');

const archive = archiver('zip', {
    gzip : true,
    zlib: { level : 9}
})

/* */

const files = [
    {
        file : "bin/Debug/netstandard2.1/ATTMorePlayers.dll",
        name : "MorePlayers.dll"
    },
    "icon.png",
    "manifest.json",
    "README.md"
]

console.log("");

let manifest

try {
    manifest = JSON.parse(fs.readFileSync('manifest.json', 'utf8'));
} catch (e) {
    console.error(`Failed to load manifest.json`);
    console.error(e);
    process.exit();
}

console.log(`Building distribution for ${manifest.name} - ${manifest.version_number}`);
console.log(`Bundling ${files.length} files.`);
files.forEach((file) => {
    console.log(`\t${file}`)
})
console.log("");

const outputStream = fs.createWriteStream(`dist/${manifest.name}-${manifest.version_number}.zip`)
archive.on("error", (e) => {
    console.error(e);
    process.exit();
})

archive.pipe(outputStream)
files.forEach((filePair) => {
    if (typeof(filePair) == "string") {
        archive.file(filePair, {name : filePair});
    } else {
        archive.file(filePair.file, {name : filePair.name});
    } 
})

archive.finalize();

console.log(`Finished building distribution for ${manifest.name} - ${manifest.version_number}`);