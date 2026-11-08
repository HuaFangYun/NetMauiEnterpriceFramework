'use strict';

const fs = require('fs');
const path = require('path');
const { spawn } = require('child_process');
const sql = require('mssql');

function connectionConfig(spec, database) {
  if (spec.authType === 'windows') {
    throw new Error('Windows authentication is not implemented in the MVP. Use a SQL login.');
  }

  return {
    server: spec.server.split(',')[0],
    port: spec.server.includes(',') ? Number(spec.server.split(',')[1]) : 1433,
    user: spec.dbUser,
    password: spec.dbPassword,
    database,
    options: {
      encrypt: true,
      trustServerCertificate: spec.trustServerCertificate !== false,
    },
    pool: { max: 1, min: 0, idleTimeoutMillis: 5000 },
  };
}

function quoteIdentifier(name) {
  if (!/^[A-Za-z_][A-Za-z0-9_]*$/.test(name)) {
    throw new Error(`Invalid identifier: ${name}`);
  }
  return `[${name}]`;
}

async function withConnection(spec, database, callback) {
  const pool = new sql.ConnectionPool(connectionConfig(spec, database));
  try {
    await pool.connect();
    return await callback(pool);
  } finally {
    await pool.close();
  }
}

async function testConnection(spec) {
  return withConnection(spec, 'master', async (pool) => {
    const result = await pool.request().query('SELECT 1 AS ok');
    return result.recordset[0].ok === 1;
  });
}

async function databaseExists(pool, name) {
  const result = await pool
    .request()
    .input('name', sql.NVarChar(128), name)
    .query('SELECT 1 AS found FROM sys.databases WHERE name = @name');
  return result.recordset.length > 0;
}

async function createDatabase(spec, name) {
  const quoted = quoteIdentifier(name);
  return withConnection(spec, 'master', async (pool) => {
    if (await databaseExists(pool, name)) return false;
    await pool.request().query(`CREATE DATABASE ${quoted}`);
    return true;
  });
}

async function ensureDatabases(spec) {
  const mainCreated = await createDatabase(spec, spec.dbName);
  const auditCreated = await createDatabase(spec, spec.auditDbName);
  return { mainCreated, auditCreated };
}

function runDotnet(args, cwd, env = {}) {
  const executable = process.platform === 'win32' ? 'dotnet.exe' : 'dotnet';
  return new Promise((resolve, reject) => {
    const child = spawn(executable, args, {
      cwd,
      env: { ...process.env, ...env },
      windowsHide: true,
    });

    let output = '';
    child.stdout.on('data', (chunk) => { output += chunk.toString(); });
    child.stderr.on('data', (chunk) => { output += chunk.toString(); });
    child.on('error', reject);
    child.on('close', (code) => {
      if (code === 0) {
        resolve(output);
        return;
      }
      reject(new Error(`dotnet ${args.join(' ')} failed with exit code ${code}.\n${output.slice(-4000)}`));
    });
  });
}

async function buildSolution(spec, outputDir) {
  const solutionFile = `${spec.projectName}.sln`;
  return runDotnet(['build', solutionFile, '-v:minimal'], outputDir, {
    DOTNET_CLI_TELEMETRY_OPTOUT: '1',
    DOTNET_NOLOGO: '1',
  });
}

async function seedDatabase(spec, outputDir, options = {}) {
  const apiProject = path.join(outputDir, `${spec.projectName}.Api`, `${spec.projectName}.Api.csproj`);
  const args = ['run', '--project', apiProject, '--no-launch-profile'];
  if (options.noBuild) {
    args.push('--no-build');
  }
  args.push('--', '--seed-only');

  return runDotnet(args, outputDir, {
    ASPNETCORE_ENVIRONMENT: 'Development',
    DOTNET_CLI_TELEMETRY_OPTOUT: '1',
    DOTNET_NOLOGO: '1',
  });
}

function splitSqlBatches(text) {
  return text
    .split(/^\s*GO\s*$/im)
    .map((batch) => batch.trim())
    .filter((batch) => batch.length > 0);
}

async function deployProcedures(spec, outputDir) {
  const proceduresDir = path.join(outputDir, 'database', 'stored-procedures');
  if (!fs.existsSync(proceduresDir)) {
    return 0;
  }

  const files = fs
    .readdirSync(proceduresDir)
    .filter((file) => file.toLowerCase().endsWith('.sql'))
    .sort();

  await withConnection(spec, spec.dbName, async (pool) => {
    for (const file of files) {
      const fullPath = path.join(proceduresDir, file);
      const batches = splitSqlBatches(fs.readFileSync(fullPath, 'utf8'));
      for (const batch of batches) {
        try {
          await pool.request().batch(batch);
        } catch (error) {
          throw new Error(`Failed to deploy ${file}: ${error.message}`);
        }
      }
    }
  });

  return files.length;
}

module.exports = {
  connectionConfig,
  quoteIdentifier,
  testConnection,
  ensureDatabases,
  buildSolution,
  seedDatabase,
  deployProcedures,
  runDotnet,
};
