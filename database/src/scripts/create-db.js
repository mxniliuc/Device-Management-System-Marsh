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

    await Promise.all([
      users.createIndex({ name: 1, location: 1 }, { name: 'users_name_location' }),

      devices.createIndex({ name: 1, manufacturer: 1 }, { unique: true, name: 'devices_name_manufacturer_unique' }),
      devices.createIndex({ assignedToUserId: 1 }, { name: 'devices_assignedToUserId' }),
      devices.createIndex({ manufacturer: 1, type: 1, os: 1 }, { name: 'devices_make_type_os' })
    ]);
  } finally {
    await client.close();
  }
}

run().catch((err) => {
  // eslint-disable-next-line no-console
  console.error(err);
  process.exitCode = 1;
});

