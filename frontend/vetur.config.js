const path = require('path')

// vetur.config.js
/** @type {import('vls').VeturConfig} */
module.exports = {
  // **optional** default: `{}`
  settings: {
    "vetur.useWorkspaceDependencies": true,
    "typescript.tsdk": path.resolve(__dirname, '.yarn/sdks/typescript/bin'),
  },
}