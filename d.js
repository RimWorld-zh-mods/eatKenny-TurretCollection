const path = require('path');
const fs = require('fs-extra');
const rm = require('rimraf');

const dirsToCopy = ['About', 'Assemblies', 'Defs', 'Languages', 'Sounds', 'Textures'];
const filesToCopy = ['LICENSE', 'README.md'];

const targetModDir =
  '/mnt/d/Games/SteamLibrary/steamapps/common/RimWorld/Mods/duduluu-Turret-Collection';

for (let dir of dirsToCopy) {
  let sourceDir = path.resolve(__dirname, dir);
  let targetDir = path.join(targetModDir, dir);
  rm(targetDir, err => {
    fs.copy(sourceDir, targetDir);
  });
}
for (let file of filesToCopy) {
  let sourceFile = path.resolve(__dirname, file);
  let targetFile = path.join(targetModDir, file);
  rm(targetFile, err => {
    fs.copy(sourceFile, targetFile);
  });
}
