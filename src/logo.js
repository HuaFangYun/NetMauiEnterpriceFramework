'use strict';

const fs = require('fs-extra');
const path = require('path');

const ALLOWED = new Set(['.png', '.jpg', '.jpeg', '.svg']);

async function resolveLogo(spec, outputDir) {
  if (!spec.logoPath) return null;

  const ext = path.extname(spec.logoPath).toLowerCase();
  if (!ALLOWED.has(ext)) {
    throw new Error('Logo must be PNG, JPG, or SVG.');
  }

  const targetDir = path.join(outputDir, 'src', `${spec.projectName}.Shared`, 'wwwroot', 'images');
  const targetFile = path.join(targetDir, `logo${ext}`);
  await fs.ensureDir(targetDir);
  await fs.copy(spec.logoPath, targetFile, { overwrite: true });
  return targetFile;
}

module.exports = { resolveLogo };
