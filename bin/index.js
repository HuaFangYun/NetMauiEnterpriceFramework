#!/usr/bin/env node
'use strict';

const { run } = require('../src/cli');

run().catch((error) => {
  console.error(error instanceof Error ? error.message : String(error));
  process.exitCode = 1;
});
