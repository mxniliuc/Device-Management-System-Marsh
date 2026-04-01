require('dotenv').config();

const { MongoClient } = require('mongodb');

function getMongoUri() {
  return process.env.MONGODB_URI || 'mongodb://127.0.0.1:27017/device_management';
}

function getDbNameFromUri(uri) {
  const url = new URL(uri);
  const pathname = url.pathname || '';
  const dbFromPath = pathname.replace(/^\//, '');
  if (!dbFromPath) {
    throw new Error('MONGODB_URI must include a database URI');
  }
  return dbFromPath;
}

async function run() {
  const uri = getMongoUri();
  const dbName = getDbNameFromUri(uri);

  const client = new MongoClient(uri);
  await client.connect();

  try {
    const db = client.db(dbName);

    const users = db.collection('users');
    const devices = db.collection('devices');

  } finally {
    await client.close();
  }
}

run().catch((err) => {
  // eslint-disable-next-line no-console
  console.error(err);
  process.exitCode = 1;
});

