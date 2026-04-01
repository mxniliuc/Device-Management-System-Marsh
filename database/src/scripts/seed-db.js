require('dotenv').config();

const { MongoClient, ObjectId } = require('mongodb');

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

async function upsertByFilter(col, filter, doc) {
  const now = new Date();
  const update = {
    $set: { ...doc, updatedAt: now },
    $setOnInsert: { createdAt: now }
  };
  const res = await col.findOneAndUpdate(filter, update, { upsert: true, returnDocument: 'after' });

  if (res?.value) return res.value;

  const upsertedId = res?.lastErrorObject?.upserted;
  if (upsertedId) {
    return await col.findOne({ _id: upsertedId });
  }

  return await col.findOne(filter);
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

    const seededUsers = await Promise.all([
      upsertByFilter(
        users,
        { email: 'alex.johnson@corp.example' },
        { name: 'Alex Johnson', role: 'IT Admin', location: 'London', email: 'alex.johnson@corp.example' }
      ),
      upsertByFilter(
        users,
        { email: 'priya.singh@corp.example' },
        { name: 'Priya Singh', role: 'Finance', location: 'Bucharest', email: 'priya.singh@corp.example' }
      ),
      upsertByFilter(
        users,
        { email: 'marco.rossi@corp.example' },
        { name: 'Marco Rossi', role: 'Sales', location: 'Milan', email: 'marco.rossi@corp.example' }
      ),
      upsertByFilter(
        users,
        { email: 'nina.mueller@corp.example' },
        { name: 'Nina Müller', role: 'HR', location: 'Berlin', email: 'nina.mueller@corp.example' }
      )
    ]);

    const userByEmail = new Map(seededUsers.filter(Boolean).map((u) => [u.email, u]));

    const devicesToSeed = [
      {
        name: 'iPhone 15 Pro',
        manufacturer: 'Apple',
        type: 'phone',
        os: 'iOS',
        osVersion: '18.1',
        processor: 'A17 Pro',
        ramGb: 8,
        description: 'A high-performance Apple smartphone suitable for daily business use.',
        location: 'London',
        assignedToUserId: new ObjectId(userByEmail.get('alex.johnson@corp.example')._id),
        assignedAt: new Date(Date.now() - 12 * 24 * 60 * 60 * 1000)
      },
      {
        name: 'Galaxy Tab S10',
        manufacturer: 'Samsung',
        type: 'tablet',
        os: 'Android',
        osVersion: '15',
        processor: 'Snapdragon 8 Gen 4',
        ramGb: 12,
        description: 'A large-screen Android tablet ideal for presentations and travel.',
        location: 'Milan',
        assignedToUserId: new ObjectId(userByEmail.get('marco.rossi@corp.example')._id),
        assignedAt: new Date(Date.now() - 3 * 24 * 60 * 60 * 1000)
      },
      {
        name: 'Pixel 9',
        manufacturer: 'Google',
        type: 'phone',
        os: 'Android',
        osVersion: '15',
        processor: 'Tensor G5',
        ramGb: 12,
        description: 'A modern Android smartphone with a clean OS experience and strong security features.',
        location: 'Berlin',
        assignedToUserId: new ObjectId(userByEmail.get('nina.mueller@corp.example')._id),
        assignedAt: new Date(Date.now() - 25 * 24 * 60 * 60 * 1000)
      },
      {
        name: 'iPad Air',
        manufacturer: 'Apple',
        type: 'tablet',
        os: 'iPadOS',
        osVersion: '18.0',
        processor: 'M2',
        ramGb: 8,
        description: 'A lightweight Apple tablet suitable for note-taking and productivity.',
        location: 'Bucharest',
        assignedToUserId: null,
        assignedAt: null
      }
    ];

    await Promise.all(
      devicesToSeed.map((d) =>
        upsertByFilter(devices, { name: d.name, manufacturer: d.manufacturer }, d)
      )
    );
  } finally {
    await client.close();
  }
}

run().catch((err) => {
  // eslint-disable-next-line no-console
  console.error(err);
  process.exitCode = 1;
});

